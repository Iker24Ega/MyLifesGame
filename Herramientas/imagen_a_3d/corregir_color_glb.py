#!/usr/bin/env python
"""
Corrige el color de las texturas de un GLB generado por TRELLIS.

TRELLIS hornea en la textura las sombras de la imagen y tiende a apagar los colores; el estilo de Flow es
luminoso y pastel. Esto aclara las sombras (gamma), sube un poco la saturación y reescribe el GLB.
Guarda el original en _descargas/modelos3d_originales/ la primera vez.

Uso:  python corregir_color_glb.py archivo.glb [más.glb ...] [--gamma 0.72] [--saturacion 1.15]
"""
from __future__ import annotations

import argparse
import io
import json
import shutil
import struct
from pathlib import Path


def corregir_imagen(datos: bytes, mime: str, gamma: float, saturacion: float) -> tuple[bytes, str]:
    from PIL import Image, ImageEnhance

    im = Image.open(io.BytesIO(datos))
    alfa = im.getchannel("A") if im.mode == "RGBA" else None
    rgb = im.convert("RGB")
    tabla = [round(255 * ((i / 255) ** gamma)) for i in range(256)]
    rgb = rgb.point(tabla * 3)
    rgb = ImageEnhance.Color(rgb).enhance(saturacion)
    if alfa is not None:
        rgb.putalpha(alfa)
    salida = io.BytesIO()
    rgb.save(salida, format="PNG", optimize=True)
    return salida.getvalue(), "image/png"


def corregir_glb(ruta: Path, gamma: float = 0.72, saturacion: float = 1.15, copia_original: Path | None = None) -> int:
    datos = ruta.read_bytes()
    magia, version, _ = struct.unpack_from("<III", datos, 0)
    if magia != 0x46546C67:
        raise ValueError(f"{ruta} no es un GLB")
    largo_json, _ = struct.unpack_from("<II", datos, 12)
    gltf = json.loads(datos[20:20 + largo_json])
    pos = 20 + largo_json
    largo_bin, _ = struct.unpack_from("<II", datos, pos)
    binario = datos[pos + 8: pos + 8 + largo_bin]
    if gltf.get("extras", {}).get("colorCorregido"):
        return 0

    vistas = gltf["bufferViews"]
    trozos = [binario[v.get("byteOffset", 0): v.get("byteOffset", 0) + v["byteLength"]] for v in vistas]
    n = 0
    for img in gltf.get("images", []):
        if "bufferView" not in img:
            continue
        i = img["bufferView"]
        trozos[i], img["mimeType"] = corregir_imagen(trozos[i], img.get("mimeType", "image/png"), gamma, saturacion)
        n += 1
    if n == 0:
        return 0

    # Reconstruir el búfer binario con las nuevas longitudes (alineado a 4 bytes)
    nuevo = bytearray()
    for v, t in zip(vistas, trozos):
        while len(nuevo) % 4:
            nuevo.append(0)
        v["byteOffset"] = len(nuevo)
        v["byteLength"] = len(t)
        nuevo += t
    while len(nuevo) % 4:
        nuevo.append(0)
    gltf["buffers"][0]["byteLength"] = len(nuevo)
    gltf.setdefault("extras", {})["colorCorregido"] = {"gamma": gamma, "saturacion": saturacion}

    js = json.dumps(gltf, separators=(",", ":")).encode("utf-8")
    js += b" " * ((4 - len(js) % 4) % 4)
    total = 12 + 8 + len(js) + 8 + len(nuevo)
    salida = struct.pack("<III", 0x46546C67, version, total) + struct.pack("<II", len(js), 0x4E4F534A) + js \
        + struct.pack("<II", len(nuevo), 0x004E4942) + bytes(nuevo)

    if copia_original is not None and not copia_original.exists():
        copia_original.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ruta, copia_original)
    ruta.write_bytes(salida)
    return n


def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("glb", nargs="+", type=Path)
    p.add_argument("--gamma", type=float, default=0.72)
    p.add_argument("--saturacion", type=float, default=1.15)
    a = p.parse_args()
    for g in a.glb:
        orig = g.parent.parent / "modelos3d_originales" / g.name
        n = corregir_glb(g, a.gamma, a.saturacion, orig)
        print(f"{g.name}: {n} textura(s) corregida(s)" if n else f"{g.name}: ya estaba corregido")


if __name__ == "__main__":
    main()
