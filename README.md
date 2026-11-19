# My Life's Game

Juego para móvil que convierte tu vida real en un juego. Declaras lo que haces (entrenar, estudiar, trabajar, quedar con amigos, descansar...), lo cronometras en tiempo real y cada acción afecta a cinco áreas de tu vida: **Físico, Mente, Social, Energía y Ánimo**. La gracia no está en hacer más, sino en mantener el **equilibrio**: cada acción tiene su dosis buena y pasarse también pasa factura.

Tu avatar vive en una ciudad 3D inventada donde tú decides qué edificio es tu gimnasio, tu cafetería o tu casa, y refleja cómo te va: se le ve en forma, cansado o descuidado según lo que haces de verdad.

> Proyecto de Iker Egaña. Estado: **mini demo jugable** (Fase 1): motor del equilibrio, todas las pantallas principales en versión mínima y una ciudad 3D de formas básicas.

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

## Cómo probar la demo

1. Abre `MyLifesGame/` con Unity Hub (Unity 6.6).
2. En la pestaña **Game**, elige una resolución vertical (1080 x 1920).
3. Pulsa **Play**. No hace falta montar nada en la escena: `Juego.cs` lo crea todo.

Qué hay en la demo:

- **Check-in de la mañana**: sueño, ánimo, áreas a cuidar y "¿vienes arrastrando algo?".
- **Ciudad** (3D): el avatar camina solo al edificio que tocas; modo construir para decidir qué es cada edificio, con pines por área.
- **Hoy**: reloj con velocidades (x1, x60, x600), pasos del día (simulados), foco, misiones según tus áreas, agua, comidas y ducha.
- **Tu equilibrio**: radar de las cinco áreas y filas con tramos.
- **Hacer → Cronómetro → Resultado**: barra de dosis, cambios explicados, XP y monedas (provisionales) y la minipregunta "¿Qué tal ha ido?".
- **Regreso tras horas fuera** (se puede simular desde Avatar): sin castigos, registras lo que hiciste.
- **Avatar**: estado (en forma, normal, cansado, descuidado) y tienda de sudaderas.
- **Álbum**: logros.
- La partida **se guarda sola** (`partida.json` en la carpeta de datos de la app). "Nueva partida" en Avatar la reinicia.
- Cada acción queda en la consola con el prefijo `[Diario]`.

Reglas del equilibrio que ya aplica el motor: dosis por acción, Ánimo por equilibrio, rendimiento según cómo estás, periodo de gracia de 48 h, descanso activo, días seguidos entrenando, pantallas antes de dormir, fatiga mental, acciones rápidas, pasos, sueño, check-in, minipreguntas y registro posterior.

## Hoja de ruta de la Fase 1

1. ✅ Motor del equilibrio en C# (áreas, acciones, dosis, reglas) y simulación del día de ejemplo.
2. ✅ Pantallas en versión mínima: check-in, Ciudad, Hoy, Equilibrio, Hacer, cronómetro, Resultado, Regreso, Avatar y Álbum.
3. ✅ Guardado local.
4. Notificaciones locales.
5. Contador de pasos real (sensor del móvil).
6. Probar la versión web en el iPhone (Safari).

La ciudad 3D llega después de comprobar que el bucle engancha.
