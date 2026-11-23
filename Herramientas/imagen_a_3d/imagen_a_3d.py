#!/usr/bin/env python
"""
Imagen → modelo 3D por lotes con TRELLIS (gratis, en Hugging Face), para My Life's Game.

Adaptado de img2threejs (integrations/mesh3d/generate_reference_mesh.py, licencia Apache-2.0).

Qué hace:
  - Lee las imágenes de Flow de  _descargas/flow_3d/
  - Genera un .glb por imagen en  _descargas/modelos3d/  con el mismo nombre
  - Si existe "<nombre>_espalda.png", la usa como segunda vista del mismo modelo (mejor espalda)
  - Las imágenes "textura_*" no pasan a 3D: se copian a  _descargas/modelos3d/texturas/
  - Se salta lo que ya está hecho, así que se puede relanzar cuando se acaba el cupo gratis
  - Deja un informe en  _descargas/modelos3d/informe.json

Uso (lo normal es hacer doble clic en Generar3D.bat):
    python imagen_a_3d.py                 # todo lo pendiente
    python imagen_a_3d.py arbol edificio_gimnasio    # solo esas piezas
    python imagen_a_3d.py --rehacer arbol --semilla 7  # repetir una pieza con otra semilla
"""
from __future__ import annotations

import argparse
import json
import shutil
import struct
import sys
import time
from pathlib import Path

RAIZ = Path(__file__).resolve().parents[2]          # carpeta del repositorio MyLifesGame
ENTRADA = RAIZ / "_descargas" / "flow_3d"
SALIDA = RAIZ / "_descargas" / "modelos3d"
TOKEN_ARCHIVO = Path(__file__).resolve().parent / "hf_token.txt"
EXTENSIONES = {".png", ".jpg", ".jpeg", ".webp"}

# Espacios con la misma API de TRELLIS; si uno falla (caído o cambiado) se prueba el siguiente
ESPACIOS = ["trellis-community/TRELLIS", "JeffreyXiang/TRELLIS"]


class CupoAgotado(Exception):
    pass


def leer_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Imagen → 3D por lotes (TRELLIS)")
    p.add_argument("piezas", nargs="*", help="nombres sin extensión; vacío = todas las pendientes")
    p.add_argument("--rehacer", action="store_true", help="vuelve a generar aunque ya exista el .glb")
    p.add_argument("--semilla", type=int, default=0)
    p.add_argument("--simplificar", type=float, default=0.95,
                   help="fracción de triángulos que se quitan (0.9-0.98). Más alto = más ligero")
    p.add_argument("--textura", type=int, default=1024, choices=[512, 1024, 2048])
    p.add_argument("--espacio", default=None, help="forzar un espacio de Hugging Face concreto")
    return p.parse_args()


def leer_token() -> str | None:
    if TOKEN_ARCHIVO.exists():
        t = TOKEN_ARCHIVO.read_text(encoding="utf-8").strip()
        if t:
            return t
    try:
        from huggingface_hub import get_token
        return get_token()
    except Exception:
        return None


def agrupar(imagenes: list[Path]) -> dict[str, list[Path]]:
    """{nombre: [vista principal, vistas extra...]}. "_espalda", "_lado", "_detras" son vistas extra."""
    sufijos = ("_espalda", "_lado", "_detras", "_back", "_side")
    grupos: dict[str, list[Path]] = {}
    extras: list[tuple[str, Path]] = []
    for img in sorted(imagenes):
        base = img.stem
        suf = next((s for s in sufijos if base.lower().endswith(s)), None)
        if suf:
            extras.append((base[: -len(suf)], img))
        else:
            grupos.setdefault(base, []).insert(0, img)
    for base, img in extras:
        if base in grupos:
            grupos[base].append(img)
        else:
            print(f"  aviso: {img.name} no tiene imagen principal '{base}', se ignora")
    return grupos


def es_cupo(e: Exception) -> bool:
    t = str(e).lower()
    return any(k in t for k in ("quota", "exceeded", "zerogpu", "gpu task", "rate limit", "too many requests"))


def generar(cliente, vistas: list[Path], args) -> Path:
    from gradio_client import handle_file

    principal = handle_file(str(vistas[0]))
    extra = [{"image": handle_file(str(p)), "caption": None} for p in vistas[1:]]
    # /start_session es obligatorio: crea la carpeta temporal de la sesión (sin él da FileNotFoundError)
    cliente.predict(api_name="/start_session")
    res = cliente.predict(
        image=principal,
        multiimages=extra,
        seed=args.semilla,
        ss_guidance_strength=7.5,
        ss_sampling_steps=12,
        slat_guidance_strength=3.0,
        slat_sampling_steps=12,
        multiimage_algo="stochastic",
        mesh_simplify=args.simplificar,
        texture_size=args.textura,
        api_name="/generate_and_extract_glb",
    )
    for item in res:
        cand = item.get("video") if isinstance(item, dict) else item
        if isinstance(cand, str) and cand.lower().endswith(".glb"):
            return Path(cand)
    raise RuntimeError(f"la respuesta no trae .glb: {res!r}")


def info_glb(ruta: Path) -> dict:
    """Lee la cabecera JSON del GLB (solo biblioteca estándar): triángulos aproximados, materiales, compresión."""
    datos = ruta.read_bytes()
    magia, _v, _l = struct.unpack_from("<III", datos, 0)
    if magia != 0x46546C67:
        return {"valido": False}
    largo, tipo = struct.unpack_from("<II", datos, 12)
    gltf = json.loads(datos[20:20 + largo])
    tris = 0
    for malla in gltf.get("meshes", []):
        for prim in malla.get("primitives", []):
            idx = prim.get("indices")
            if idx is not None:
                tris += gltf["accessors"][idx]["count"] // 3
    req = gltf.get("extensionsRequired", [])
    return {
        "valido": True,
        "triangulos": tris,
        "materiales": len(gltf.get("materials", [])),
        "comprimido": any("draco" in e.lower() or "meshopt" in e.lower() for e in req),
        "mb": round(len(datos) / 1048576, 2),
    }


def conectar(token, espacio_forzado):
    from gradio_client import Client
    espacios = [espacio_forzado] if espacio_forzado else ESPACIOS
    ultimo = None
    for esp in espacios:
        try:
            c = Client(esp, token=token, verbose=False)
            print(f"Conectado a {esp} ({'con cuenta' if token else 'anónimo: menos usos gratis'})")
            return c, esp
        except Exception as e:  # espacio caído, renombrado o en pausa
            ultimo = e
            print(f"  {esp} no disponible: {str(e)[:120]}")
    raise RuntimeError(f"ningún espacio de TRELLIS responde ({ultimo})")


def main() -> None:
    args = leer_args()
    if not ENTRADA.exists():
        ENTRADA.mkdir(parents=True)
        print(f"He creado {ENTRADA}. Guarda ahí las imágenes de Flow y vuelve a lanzarlo.")
        return
    SALIDA.mkdir(parents=True, exist_ok=True)

    imagenes = [p for p in ENTRADA.iterdir() if p.suffix.lower() in EXTENSIONES]
    texturas = [p for p in imagenes if p.stem.lower().startswith("textura")]
    for t in texturas:
        dst = SALIDA / "texturas" / t.name
        if not dst.exists() or args.rehacer:
            dst.parent.mkdir(exist_ok=True)
            shutil.copy2(t, dst)
            print(f"textura copiada: {t.name}")

    grupos = agrupar([p for p in imagenes if p not in texturas])
    if args.piezas:
        grupos = {k: v for k, v in grupos.items() if k in args.piezas}
        faltan = set(args.piezas) - set(grupos)
        for f in faltan:
            print(f"  aviso: no encuentro la imagen '{f}' en {ENTRADA}")

    pendientes = {k: v for k, v in grupos.items() if args.rehacer or not (SALIDA / f"{k}.glb").exists()}
    # Orden de prioridad (el cupo gratis da para pocas piezas al día): primero lo que más se ve en la ciudad
    prioridad = ("edificio_", "relleno_", "casa_relleno", "parcela_", "fuente", "puente", "farola", "arbol_",
                 "macizo", "valla", "maceta", "buzon", "mesa_", "personaje_", "mochila", "gorra", "gorro", "gafas",
                 "auriculares", "reloj", "mancuerna", "libro", "mando", "botella", "sala_")
    def orden(nombre: str) -> int:
        return next((i for i, p in enumerate(prioridad) if nombre.startswith(p)), len(prioridad))
    pendientes = dict(sorted(pendientes.items(), key=lambda kv: (orden(kv[0]), kv[0])))
    print(f"{len(grupos)} piezas con imagen, {len(pendientes)} por generar.")
    if not pendientes:
        return

    informe_ruta = SALIDA / "informe.json"
    informe = json.loads(informe_ruta.read_text(encoding="utf-8")) if informe_ruta.exists() else {}
    cliente, espacio = conectar(leer_token(), args.espacio)

    hechos = 0
    for i, (nombre, vistas) in enumerate(pendientes.items(), 1):
        print(f"[{i}/{len(pendientes)}] {nombre}  ({len(vistas)} vista{'s' if len(vistas) > 1 else ''}) ...", flush=True)
        t0 = time.time()
        for intento in (1, 2):
            try:
                glb = generar(cliente, vistas, args)
                destino = SALIDA / f"{nombre}.glb"
                shutil.copy(glb, destino)
                try:  # TRELLIS hornea sombras y apaga colores: se aclaran para el estilo de Flow
                    from corregir_color_glb import corregir_glb
                    corregir_glb(destino, copia_original=SALIDA.parent / "modelos3d_originales" / destino.name)
                except Exception as e:
                    print(f"    (sin corrección de color: {e})")
                if not nombre.startswith("personaje"):
                    try:  # texturas a 512 px: la web pesa mucho y a la distancia de la ciudad no se nota
                        from aligerar_glb import aligerar
                        aligerar(destino)
                    except Exception as e:
                        print(f"    (sin aligerar: {e})")
                datos = info_glb(destino)
                datos.update({"vistas": [v.name for v in vistas], "semilla": args.semilla,
                              "espacio": espacio, "segundos": round(time.time() - t0)})
                informe[nombre] = datos
                informe_ruta.write_text(json.dumps(informe, indent=2, ensure_ascii=False), encoding="utf-8")
                print(f"    OK  {datos.get('triangulos', '?')} triángulos, {datos.get('mb', '?')} MB, {datos['segundos']} s")
                hechos += 1
                break
            except Exception as e:
                if es_cupo(e):
                    print("\nSe ha acabado el cupo gratis de GPU por ahora. Vuelve a lanzarlo en unas horas:"
                          " seguirá por donde lo ha dejado.")
                    print(f"Hechas en esta tanda: {hechos}.")
                    return
                if intento == 1:
                    print(f"    falló ({str(e)[:160]}). Reintento...")
                    time.sleep(5)
                else:
                    print(f"    ERROR con {nombre}: {str(e)[:300]}")
    print(f"\nTerminado: {hechos} modelos nuevos en {SALIDA}")


if __name__ == "__main__":
    try:
        main()
    except ModuleNotFoundError as e:
        sys.exit(f"Falta una librería ({e.name}). Lánzalo con Generar3D.bat, que la instala.")
