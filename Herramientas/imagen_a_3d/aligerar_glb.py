#!/usr/bin/env python
"""
Reduce las texturas de los .glb a 512 px (la web pesa mucho: Unity guarda cada textura sin comprimir).
A la distancia de la cámara de la ciudad no se nota. El personaje se deja en 1024 (se ve de cerca).

Uso: python aligerar_glb.py [carpeta]   (por defecto Assets/_Juego/Arte/Resources/Modelos3D)
"""
import io, json, struct, sys
from pathlib import Path
from PIL import Image

RAIZ = Path(__file__).resolve().parents[2]
CARPETA = RAIZ / "MyLifesGame" / "Assets" / "_Juego" / "Arte" / "Resources" / "Modelos3D"
LADO = 512
RESPETAR = ("personaje",)


def aligerar(ruta: Path) -> bool:
    d = ruta.read_bytes()
    largo = struct.unpack_from("<I", d, 12)[0]
    g = json.loads(d[20:20 + largo])
    ini = 20 + largo
    blargo = struct.unpack_from("<I", d, ini)[0]
    binario = d[ini + 8: ini + 8 + blargo]
    vistas_img = {im["bufferView"]: im for im in g.get("images", []) if "bufferView" in im}
    if not vistas_img:
        return False
    cambios = False
    nuevas = []
    for i, bv in enumerate(g["bufferViews"]):
        trozo = binario[bv.get("byteOffset", 0): bv.get("byteOffset", 0) + bv["byteLength"]]
        if i in vistas_img:
            img = Image.open(io.BytesIO(trozo))
            if max(img.size) > LADO:
                img = img.convert("RGBA" if img.mode in ("RGBA", "LA", "P") else "RGB")
                img = img.resize((LADO, LADO), Image.LANCZOS)
                buf = io.BytesIO()
                img.save(buf, "PNG", optimize=True)
                trozo = buf.getvalue()
                vistas_img[i]["mimeType"] = "image/png"
                cambios = True
        nuevas.append(trozo)
    if not cambios:
        return False
    salida = bytearray()
    for bv, trozo in zip(g["bufferViews"], nuevas):
        while len(salida) % 4:
            salida += b"\0"
        bv["byteOffset"] = len(salida)
        bv["byteLength"] = len(trozo)
        salida += trozo
    while len(salida) % 4:
        salida += b"\0"
    g["buffers"][0]["byteLength"] = len(salida)
    js = json.dumps(g, separators=(",", ":")).encode()
    js += b" " * ((4 - len(js) % 4) % 4)
    total = 12 + 8 + len(js) + 8 + len(salida)
    datos = struct.pack("<III", 0x46546C67, 2, total) + struct.pack("<II", len(js), 0x4E4F534A) + js \
        + struct.pack("<II", len(salida), 0x004E4942) + bytes(salida)
    ruta.write_bytes(datos)
    return True


def main():
    carpeta = Path(sys.argv[1]) if len(sys.argv) > 1 else CARPETA
    for glb in sorted(carpeta.glob("*.glb")):
        if glb.stem.startswith(RESPETAR):
            continue
        antes = glb.stat().st_size
        if aligerar(glb):
            print(f"{glb.stem}: {antes // 1024} KB -> {glb.stat().st_size // 1024} KB")


if __name__ == "__main__":
    main()
