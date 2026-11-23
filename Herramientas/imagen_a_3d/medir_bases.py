#!/usr/bin/env python
"""
Mide el grosor del "zócalo" (la peana de tierra/césped/acera) de cada modelo generado por TRELLIS.
Unity lo usa para hundir esa peana en el suelo y que los edificios no parezcan islas flotantes.

Escribe MyLifesGame/Assets/_Juego/Arte/Resources/Modelos3D/bases.txt  ->  "nombre fraccion" (fracción de la altura)
"""
import json, struct, sys
from pathlib import Path
import numpy as np

RAIZ = Path(__file__).resolve().parents[2]
CARPETA = RAIZ / "MyLifesGame" / "Assets" / "_Juego" / "Arte" / "Resources" / "Modelos3D"


def matriz(nodo):
    if "matrix" in nodo:
        return np.array(nodo["matrix"], dtype=np.float64).reshape(4, 4).T
    t = np.array(nodo.get("translation", [0, 0, 0]), dtype=np.float64)
    x, y, z, w = nodo.get("rotation", [0, 0, 0, 1])
    s = np.array(nodo.get("scale", [1, 1, 1]), dtype=np.float64)
    r = np.array([[1 - 2 * (y*y + z*z), 2 * (x*y - z*w), 2 * (x*z + y*w)],
                  [2 * (x*y + z*w), 1 - 2 * (x*x + z*z), 2 * (y*z - x*w)],
                  [2 * (x*z - y*w), 2 * (y*z + x*w), 1 - 2 * (x*x + y*y)]])
    m = np.eye(4); m[:3, :3] = r * s; m[:3, 3] = t
    return m


def posiciones(ruta: Path) -> np.ndarray:
    d = ruta.read_bytes()
    largo = struct.unpack_from("<I", d, 12)[0]
    g = json.loads(d[20:20 + largo])
    off = 20 + largo
    blargo = struct.unpack_from("<I", d, off)[0]
    binario = d[off + 8: off + 8 + blargo]
    pts = []

    def visitar(i, padre):
        n = g["nodes"][i]
        m = padre @ matriz(n)
        if "mesh" in n:
            for prim in g["meshes"][n["mesh"]]["primitives"]:
                acc = g["accessors"][prim["attributes"]["POSITION"]]
                bv = g["bufferViews"][acc["bufferView"]]
                ini = bv.get("byteOffset", 0) + acc.get("byteOffset", 0)
                paso = bv.get("byteStride", 12)
                cuenta = acc["count"]
                bruto = np.frombuffer(binario, dtype=np.uint8, count=paso * (cuenta - 1) + 12, offset=ini)
                v = np.lib.stride_tricks.as_strided(bruto.view(np.float32), shape=(cuenta, 3), strides=(paso, 4)).astype(np.float64)
                v = np.c_[v, np.ones(cuenta)] @ m.T
                pts.append(v[:, :3])
        for h in n.get("children", []):
            visitar(h, m)

    escena = g["scenes"][g.get("scene", 0)]
    for i in escena["nodes"]:
        visitar(i, np.eye(4))
    return np.vstack(pts)


def zocalo(p: np.ndarray) -> float:
    """Altura (fracción) de la cara de arriba de la peana.
    En la peana hay muchos vértices abajo (cara inferior), casi ninguno en los laterales (hueco)
    y otro pico en la cara de arriba (césped, acera, detalles). Sin hueco claro = sin peana."""
    y = p[:, 1]
    h = (y - y.min()) / max(y.max() - y.min(), 1e-9)
    c, _ = np.histogram(h, bins=60, range=(0, 0.3))
    mx = c[3:30].max()
    if mx == 0:
        return 0.0
    hueco = 0
    for k in range(1, 30):
        if c[k] < 0.2 * mx:
            hueco += 1
        else:
            if hueco >= 3 and c[k] >= 0.25 * mx:
                return round(k * 0.005, 3)
            hueco = 0
    return 0.0


def main():
    lineas = []
    for glb in sorted(CARPETA.glob("*.glb")):
        try:
            f = zocalo(posiciones(glb))
        except Exception as e:
            print(f"{glb.stem}: error {e}")
            continue
        print(f"{glb.stem}: zócalo {f:.3f}")
        lineas.append(f"{glb.stem} {f}")
    (CARPETA / "bases.txt").write_text("\n".join(lineas) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
