# Plan maestro: de la mini demo a un juego de móvil bonito y usable

> Objetivo (Iker, 19-nov): que el juego se vea y se use como un juego de móvil normal, al nivel de las imágenes de referencia de Flow (`docs/` y sección 5.11-5.15 de las bases). Claude trabaja en autónomo, por fases, con commits automáticos y pequeños informes al terminar cada bloque.

## Cómo se trabaja

- **Commits automáticos.** Claude hace commit en el PC de Iker y lo sube a GitHub (mismos commits, GitHub Desktop queda sincronizado solo).
- **Unity automático.** Un script de editor (`Assets/_Juego/Editor/AutomatizacionClaude.cs`) refresca y compila solo, hace capturas de cada pantalla y deja un informe en `MyLifesGame/Automatizacion/`. Así Claude comprueba lo que construye sin pedir clics.
- **Informes cortos** al terminar cada fase o bloque grande: qué se ha hecho, capturas y siguiente paso.
- **Iker no tiene que hacer nada** salvo, al principio: un clic en Unity para activar la automatización y descargar los packs gratuitos de arte (ver "Recursos").

## Recursos de arte (gratuitos, licencia CC0)

| Recurso | Uso |
| --- | --- |
| Kenney City Kit (Suburban) y City Kit (Commercial) | Casas, comercios, árboles y calles de la ciudad |
| Quaternius Universal Base Characters (parte gratuita) + Universal Animation Library (parte gratuita) | Avatar 3D con animaciones (reposo, andar...) |
| Fuente Nunito (Google Fonts, OFL) | Tipografía redondeada de toda la interfaz |

## Fases

### F0. Infraestructura (automatización)
- [ ] Commits y subida a GitHub desde Claude.
- [ ] Script de editor: refresco y compilación automáticos, informe de errores.
- [ ] Capturas automáticas de todas las pantallas en modo Play.
- [ ] Importación automática de los packs de arte a `Assets/_Juego/Arte/`.
**Listo cuando:** Claude puede cambiar código, compilar, ver capturas y hacer commit sin ayuda.

### F1. Sistema visual de la interfaz
- [ ] Tipografía Nunito en toda la interfaz.
- [ ] Componentes: tarjetas con sombra suave, botones tipo píldora con animación al pulsar, chips, barras redondeadas de tres tramos, iconos de las cinco áreas (pesa, libro, burbujas, rayo con corazón, cara sonriente).
- [ ] Transiciones suaves entre pantallas y avisos animados.
- [ ] Cabecera de juego (retrato, nivel, XP, monedas) y barra inferior como en las imágenes.
- [ ] Zona segura del móvil (notch).
**Listo cuando:** cualquier pantalla parece de un juego de móvil acabado, sin "botones planos".

### F2. Pantallas al nivel de las imágenes
- [ ] Hoy: anillo de pasos con zonas, foco, misiones con franja de área, acciones rápidas.
- [ ] Tu equilibrio: radar y filas de tres tramos (formato de la imagen de referencia).
- [ ] Hacer: rejilla 2x4 con iconos, lugar enlazado, duración, impacto esperado.
- [ ] Cronómetro: anillo grande, barra de dosis, flechas en vivo de las áreas.
- [ ] Resultado: barras animadas, confeti, recompensas y minipregunta.
- [ ] Check-in, Regreso, Avatar (categorías y tienda) y Álbum (cartas coleccionables).
**Listo cuando:** cada pantalla se parece a su imagen de Flow.

### F3. Ciudad 3D bonita
- [ ] Ciudad con el kit de Kenney: casas, comercios, calles, árboles y plaza.
- [ ] Luz cálida de tarde, sombras suaves y postproceso URP (bloom suave, color cálido, viñeta).
- [ ] Cámara inclinada con zoom limitado y arrastre con el dedo.
- [ ] Pines medallón con icono, carteles y anillo del avatar; huellas del recorrido.
- [ ] Modo construir: solares, vista previa del edificio, anillos de distancia.
- [ ] Barrios bloqueados por nivel.
**Listo cuando:** la ciudad recuerda a la imagen "ciudad 3D" de referencia.

### F4. Avatar 3D de verdad
- [ ] Personaje con animaciones (reposo, andar, entrenar, sentarse).
- [ ] Ropa por colores y piezas; la tienda cambia el avatar.
- [ ] Estados (en forma, normal, cansado, descuidado) con postura, animación y ambiente.
- [ ] Retrato del avatar en la interfaz.

### F5. Escenas de acción
- [ ] Interiores sencillos (gimnasio, casa, biblioteca, cafetería) para el cronómetro.

### F6. Pulido de juego
- [ ] Sonido y música suaves, vibración y partículas.
- [ ] Mini tutorial ligero y ajustes.
- [ ] Rendimiento en móvil.

### F7. Móvil
- [ ] Versión web para probar en el iPhone (Safari).
- [ ] Más adelante: notificaciones y pasos reales (Build Automation + TestFlight).

## Registro de informes

(Se añade una línea por bloque terminado.)
