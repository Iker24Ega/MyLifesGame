#!/usr/bin/env python
"""
Recorta el fondo blanco de las imágenes de Flow y las deja como sprites (PNG con transparencia) para la ciudad.

Lee  _descargas/flow_3d/  y escribe  MyLifesGame/Assets/_Juego/Arte/Resources/Sprites/<nombre>.png
- Quita el fondo blanco conectado con los bordes (no el blanco de dentro del dibujo: ventanas, farolas...).
- Suaviza el borde y quita el halo blanco.
- Recorta al contenido y limita el tamaño (máx. 512 px) para que la web no pese.

Uso: python recortar_sprites.py [nombres...]   (sin nombres: todas las que sirven para la ciudad)
"""
from __future__ import annotations

import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

RAIZ = Path(__file__).resolve().parents[2]
ENTRADA = RAIZ / "_descargas" / "flow_3d"
SALIDA = RAIZ / "MyLifesGame" / "Assets" / "_Juego" / "Arte" / "SpritesFlow"  # fuera de Resources: no entra en el juego
NO_SPRITE = ("personaje", "textura_", "mancuerna", "libro", "mando", "botella", "mochila",
             "gorra", "gorro", "gafas", "auriculares", "reloj")
MAXIMO = 512


def recortar(origen: Path, destino: Path, maximo: int = MAXIMO) -> tuple[int, int]:
    im = Image.open(origen).convert("RGB")
    a = np.asarray(im).astype(np.int16)
    minimo = a.min(axis=2)
    rango = a.max(axis=2) - minimo
    candidato = (minimo > 226) & (rango < 22)          # casi blanco y sin color
    mascara = Image.fromarray(np.where(candidato, 255, 0).astype(np.uint8)).convert("L").copy()
    w, h = mascara.size
    paso = 8
    bordes = [(x, 0) for x in range(0, w, paso)] + [(x, h - 1) for x in range(0, w, paso)] + \
             [(0, y) for y in range(0, h, paso)] + [(w - 1, y) for y in range(0, h, paso)]
    for p in bordes:
        if mascara.getpixel(p) == 255:
            ImageDraw.floodfill(mascara, p, 128)
    fondo = np.asarray(mascara) == 128
    alfa = np.where(fondo, 0, 255).astype(np.uint8)
    # Borde suave: se difumina 1,2 px y se recupera el interior opaco
    suave = np.asarray(Image.fromarray(alfa, "L").filter(ImageFilter.GaussianBlur(1.2))).astype(np.float32)
    alfa_f = np.minimum(suave, alfa.astype(np.float32))
    # Erosión ligera del borde (el antialias contra el blanco deja halo claro)
    alfa_f = np.clip((alfa_f - 40) * (255 / 215), 0, 255)
    k = alfa_f[..., None] / 255.0
    rgb = a.astype(np.float32)
    with np.errstate(divide="ignore", invalid="ignore"):
        limpio = np.where(k > 0.02, (rgb - (1 - k) * 255) / np.maximum(k, 1e-3), rgb)
    limpio = np.clip(limpio, 0, 255)
    rgba = np.dstack([limpio, alfa_f]).astype(np.uint8)
    salida = Image.fromarray(rgba, "RGBA")
    caja = salida.getchannel("A").point(lambda v: 255 if v > 10 else 0).getbbox()
    if caja is None:
        raise ValueError("no queda nada tras quitar el fondo")
    x0, y0, x1, y1 = caja
    salida = salida.crop((max(0, x0 - 2), max(0, y0 - 2), min(w, x1 + 2), min(h, y1 + 2)))
    if max(salida.size) > maximo:
        f = maximo / max(salida.size)
        salida = salida.resize((max(1, round(salida.width * f)), max(1, round(salida.height * f))), Image.LANCZOS)
    destino.parent.mkdir(parents=True, exist_ok=True)
    salida.save(destino, optimize=True)
    return salida.size


def main() -> None:
    nombres = set(sys.argv[1:])
    for img in sorted(ENTRADA.iterdir()):
        if img.suffix.lower() not in (".jpg", ".jpeg", ".png", ".webp"):
            continue
        n = img.stem
        if nombres and n not in nombres:
            continue
        if not nombres and (n.startswith(NO_SPRITE) or n.endswith("_espalda")):
            continue
        dst = SALIDA / f"{n}.png"
        if not nombres and dst.exists() and dst.stat().st_mtime >= img.stat().st_mtime:
            continue
        try:
            tam = recortar(img, dst, 1024 if n.startswith("sala_") else MAXIMO)  # las salas ocupan toda la pantalla
            print(f"{n}: {tam[0]}x{tam[1]}")
        except Exception as e:
            print(f"{n}: ERROR {e}")


if __name__ == "__main__":
    main()
