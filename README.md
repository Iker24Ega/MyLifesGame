# My Life's Game

Juego para móvil que convierte tu vida real en un juego. Declaras lo que haces (entrenar, estudiar, trabajar, quedar con amigos, descansar...), lo cronometras en tiempo real y cada acción afecta a cinco áreas de tu vida: **Físico, Mente, Social, Energía y Ánimo**. La gracia no está en hacer más, sino en mantener el **equilibrio**: cada acción tiene su dosis buena y pasarse también pasa factura.

Tu avatar vive en una ciudad 3D inventada donde tú decides qué edificio es tu gimnasio, tu cafetería o tu casa, y refleja cómo te va: se le ve en forma, cansado o descuidado según lo que haces de verdad.

> Proyecto de Iker Egaña. Estado: **Fase 1, prototipo del bucle** (sin ciudad todavía).

## Tecnología

- **Motor:** Unity 6 (C#), plantilla Universal 3D (URP).
- **Editor de código:** VS Code con las extensiones Unity y C# Dev Kit.
- **Plataforma objetivo:** iOS y Android. Durante el desarrollo se prueba en el editor y con la versión web en el navegador del móvil.

## Estructura

```
Assets/
  _Juego/
    Scripts/
      Motor/      Motor del equilibrio: reglas y cálculos, C# puro, sin nada de Unity
      Pantallas/  Interfaz: Hacer, cronómetro, Resultado, Hoy...
    Datos/        Tabla de acciones y efectos (JSON), ajustable sin tocar código
    Escenas/      Escenas de Unity
    Arte/         Imágenes, modelos y sonidos
docs/             Documentos de diseño
```

**Regla principal del código:** el **motor calcula** y las **pantallas solo muestran**. Así se puede cambiar el aspecto del juego (o pasar a la ciudad 3D) sin tocar las reglas del equilibrio.

## Documentación de diseño

- [`docs/bases-del-proyecto.md`](docs/bases-del-proyecto.md): visión, mecánicas, mundo, decisiones y hoja de ruta.
- [`docs/fase-1-acciones-y-curvas.md`](docs/fase-1-acciones-y-curvas.md): acciones, áreas, dosis, reglas de realismo y calibración personal.

Las versiones de trabajo de estos documentos viven en el Project de Claude "My Life's Game"; las de aquí son copias que se actualizan de vez en cuando.

## Hoja de ruta de la Fase 1

1. Motor del equilibrio en C# (áreas, acciones, dosis, reglas) y simulación del día de ejemplo.
2. Pantallas 2D: Hacer, cronómetro, Resultado, Hoy.
3. Guardado local.
4. Notificaciones locales.
5. Contador de pasos.

La ciudad 3D llega después de comprobar que el bucle engancha.
