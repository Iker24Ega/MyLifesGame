# Imagen → 3D por lotes

Convierte las imágenes de Flow en modelos 3D con TRELLIS (gratis, en Hugging Face), sin subirlas una a una.
Adaptado de img2threejs (Apache-2.0).

## Uso

1. Guarda las imágenes de Flow en `_descargas\flow_3d\`, con el nombre de `prompts-flow-3d.md` (por ejemplo `arbol.png`).
2. Doble clic en **Generar3D.bat**.
   - La primera vez prepara Python y te pide tu token de Hugging Face (huggingface.co/settings/tokens, tipo *Read*). Con token tienes más usos gratis.
3. Los modelos salen en `_descargas\modelos3d\` con el mismo nombre (`arbol.glb`). Unity los recoge solo.

## Detalles

- `personaje_base_espalda.png` se usa como segunda vista de `personaje_base` (mejor espalda).
- Las `textura_*` no pasan a 3D: se copian a `_descargas\modelos3d\texturas\`.
- Lo ya generado se salta. Si se acaba el cupo gratis, vuelve a lanzarlo más tarde y sigue donde lo dejó.
- Repetir una pieza con otra semilla: abre una terminal en esta carpeta y escribe
  `.venv\Scripts\python imagen_a_3d.py --rehacer arbol --semilla 7`
- `informe.json` (en `modelos3d`) apunta triángulos y peso de cada modelo.
