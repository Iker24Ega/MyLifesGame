# Fase 1: acciones, áreas y curvas del equilibrio (borrador v0.4)

> **17-nov (tarde):** minipreguntas de un toque tras acciones largas y días especiales (9.1).

> **Cambios v0.4 (17-nov, respuestas de Iker):** Estudiar suma +8 de Mente por hora y el esfuerzo mental rinde la mitad a partir de 6 h al día; el desgaste de Energía baja a −1,5 por hora despierto; la regla 6 (pantallas antes de dormir) pasa a ser personal; nueva sección 9 de **calibración personal**. Día de ejemplo re-simulado.

> **17-nov:** día de ejemplo recalculado con todas las reglas (sección 6) y tres ajustes propuestos a partir de la simulación.

> **Cambios v0.3 (16-nov):** Iker acepta las siete reglas de realismo (sección 8), con un periodo de gracia de 48 h antes de que un área empiece a bajar y un control diario opcional de agua y hábitos básicos. Las reglas 1 y 3 sustituyen en parte a la sección 1.1 y a los valores directos de Ánimo de la 2.2.

> **Cambios v0.2 (16-nov, respuestas de Iker):** onboarding con valores iniciales manuales; tramos de las áreas Poco < 30, Zona buena 30-80, Alto > 80; entrenar da Energía en el momento y no al día siguiente; trabajar cansa ya en una jornada normal; videojuegos con zona óptima más corta; recompensas aparcadas; acciones combinadas apuntadas para el futuro. Día de ejemplo recalculado.

> Propuesta del socio para que Iker la revise y la cambie. **Todos los números son hipótesis de juego, no ciencia ni consejo médico.** Se ajustan jugando en el prototipo, sin tocar código (viven en una tabla de datos).
> Relacionado: documento de bases, secciones 3, 4.1-4.6, 5.13 y 5.14.

Leyenda: ✅ decidido por Iker · 💡 propuesta del socio · ❓ pregunta para Iker

---

## 1. Cómo funcionan las áreas

- ✅ Cada área va de **0 a 100**.
- ✅ **Valores iniciales reales:** el primer día el jugador indica cómo está en cada área en un **onboarding** (cómo hacerlo, por definir), porque no todo el mundo empieza en 50 en todo. 💡 Si se lo salta, se empieza en 50.
- ✅ Cada área tiene tres tramos en su barra:
  - **Poco:** menos de 30.
  - **Zona buena:** de 30 a 80.
  - **Alto:** más de 80.
- ✅ En las **acciones** (cronómetro) se mantiene "Poco / Zona óptima / Demasiado", porque pasarse de dosis sí es malo. En las **áreas** el tercer tramo se llama "Alto", porque tener mucho Físico o Ánimo no es malo en sí.

### 1.1 Desgaste con el tiempo

Las áreas bajan solas poco a poco, para que haya que cuidarlas. ✅ **Desde la v0.3, Físico, Mente y Social solo empiezan a bajar tras 48 h seguidas sin cuidarlas, y el Ánimo lo marca el equilibrio** (sección 8, reglas 1 y 3). Nunca bajan de 10 por desgaste, y durante el horario de sueño y los modos de pausa no hay desgaste (4.5).

| Área | Desgaste propuesto | Motivo |
| --- | --- | --- |
| Físico | −8 al día | Sin moverte, se nota en días |
| Mente | −6 al día | Más lento |
| Social | −10 al día | Iker: "3 días sin quedar" debe notarse claramente |
| Ánimo | Tiende a 50, 3 puntos al día | Ni sube ni baja indefinidamente solo |
| Energía | −1,5 por hora despierto (v0.4; antes −2) | Es la batería del día: se gasta y se recarga durmiendo |

---

## 2. Las 8 acciones de la v1

Son las de la imagen "Hacer" (5.14). Cada acción tiene tres tramos de duración, como la barra del cronómetro:
- **Poco:** efecto proporcional al tiempo, sin bonus.
- **Zona óptima:** efecto completo y bonus de cierre "Dosis óptima".
- **Demasiado:** los beneficios caen a una cuarta parte y empiezan a restar Energía y, a veces, Ánimo.

La recompensa siempre es proporcional al tiempo real (4.4): 10 minutos dan el efecto de 10 minutos.

### 2.1 Tabla de dosis

| Acción | Poco | Zona óptima | Demasiado (a partir de) | Lugar habitual |
| --- | --- | --- | --- | --- |
| Entrenar | < 30 min | 30-90 min | > 2 h | Gimnasio |
| Pasear | < 20 min | 20-90 min (de 90 a 150 se aplana) | > 2 h 30 min | Parque / calle |
| Estudiar | < 30 min | 30 min - 2 h seguidas | > 3 h seguidas | Biblioteca / casa |
| Trabajar | — | hasta 8 h al día | > 9 h al día | Trabajo / casa |
| Quedar con amigos | < 30 min | 30 min - 4 h | > 5 h | Cafetería / casa de amigos |
| Videojuegos | < 30 min | 30 min - 1 h 30 min | > 2 h | Casa / recreativos |
| Descansar (siesta, sofá) | < 15 min | 15-45 min | > 1 h 30 min | Casa |
| Meditar | < 5 min | 5-30 min | > 1 h (sin castigo, solo se aplana) | Casa / parque |

### 2.2 Efecto por hora en cada área

Puntos por hora en la zona óptima. Entre paréntesis, el efecto por hora a partir de "Demasiado".

| Acción | Físico | Mente | Social | Energía | Ánimo |
| --- | --- | --- | --- | --- | --- |
| Entrenar | +15 (+4) | — | — | +4 (−20) | +6 (−4) |
| Pasear | +5 (+1) | — | — | +6 (−8) | +5 (+1) |
| Estudiar | — | +8 (+3) | — | −5 (−10) | — (−5) |
| Trabajar | — | +4 (+1) | — | −5 (−12) | — (−4) |
| Quedar con amigos | — | — | +20 (+5) | −3 (−8) | +8 (+2) |
| Videojuegos | — (−3) | — (−3) | — | −2 (−8) | +8 (+1) |
| Descansar | — | — | — | +25 (+5) | +4 (−4) |
| Meditar | — | +10 (+10) | — | +5 (+5) | +20 (+20 hasta tope) |

**Matices propuestos:**
- ✅ **Entrenar en la zona óptima da Energía en el momento**, no al día siguiente (Iker: "cuando entreno noto que tengo energía"). Pasado "Demasiado", la resta con fuerza.
- ✅ **Videojuegos:** interruptor "con amigos" al empezar. Con amigos añade **Social +6 por hora**; jugando solo, Social no se toca.
- ✅ **Meditar** no castiga pasarse: a partir de 1 h deja de sumar Mente y Ánimo extra, pero no resta. Así se ve que no todo exceso es malo.
- ✅ **Trabajar cansa ya en una jornada normal** (8 h consumen a cualquiera) y a partir de 9 h cansa mucho más. Cuenta como acción porque también incluye trabajar en tus propios proyectos.
- ✅ **Pasear:** el paseo normal da Energía y uno de 2-3 h la resta (decisión de Iker, 5.13).
- ✅ **Fatiga mental (v0.4):** a partir de 6 h de Trabajar y Estudiar sumadas en el día, lo que suman a Mente se reduce a la mitad. Texto: "Mucho esfuerzo mental hoy: rinde menos".

### 2.3 Ejemplo para comprobar la tabla

Entrenar 1 h 05 min, en la zona óptima:
- Físico +16, Ánimo +6, Energía +4.
- Bonus "Dosis óptima" y la misión "Entrenar 30 minutos" cumplida.

Entrenar 2 h 30 min:
- Primeras 2 h: Físico +30, Ánimo +12, Energía +8.
- Último tramo (30 min en "Demasiado"): Físico +2, Ánimo −2, Energía −10.
- Total: Físico +32, Ánimo +10, Energía −2 (−7 contando el desgaste normal de estar despierto), sin bonus de dosis óptima y con el aviso "Sesión larga: cansancio".
- 💡 Lectura: pasarse no da casi más Físico y se come toda la Energía ganada. No compensa.

---

## 3. Lo que suma solo, sin cronómetro

### 3.1 Dormir (con el horario de sueño configurado)

| Horas | Efecto propuesto |
| --- | --- |
| < 6 h | La Energía del día empieza como máximo en 60 y Ánimo −5 |
| 6-7 h | Energía recarga +10 por hora |
| 7-9 h (zona buena) | Energía recarga +10 por hora y +5 de Ánimo al despertar |
| > 10 h | Sin más recarga y Ánimo −3 |

### 3.2 Pasos del día (fuera de las acciones, 5.13)

| Pasos | Efecto propuesto |
| --- | --- |
| Menos de 6.000 | +1 de Físico por cada 1.000 pasos |
| 6.000-12.000 (zona buena) | +1 de Físico y +0,5 de Energía por cada 1.000 pasos |
| 12.000-18.000 | Se aplana: no suma más |
| Más de 18.000 | Energía −5 (cansancio) |

✅ **Sin doble conteo:** los pasos dados durante una acción con cronómetro no suman aquí.

---

## 4. Check-in de la mañana (pantalla de 5.14)

| Pregunta | Efecto propuesto |
| --- | --- |
| ¿Cómo has dormido? (5 caras) | Fija la Energía de salida: 40 / 50 / 60 / 75 / 85. Si contradice el horario de sueño, manda la respuesta del jugador |
| ¿Cómo te sientes? (Agotado → A tope) | Ajusta Ánimo: −10 / −5 / 0 / +5 / +10 |
| ¿Qué quieres cuidar hoy? (1-2 áreas) | Las misiones del día se proponen sobre esas áreas y dan XP ×1,25 en ellas |

💡 Si el jugador salta el check-in, se usa el horario de sueño y nada más.

---

## 5. Recompensas (⏸ aparcado por Iker, 16-nov)

⏸ Iker prefiere mirarlo más adelante: aún no tiene claro cómo hacerlo. Lo de abajo queda como propuesta guardada, **no decidida**.

- 💡 **Dos recompensas que salen de lo mismo:**
  - **XP** para subir de nivel: 1 XP por minuto en la zona óptima, 0,5 en "Poco" y 0,25 en "Demasiado".
  - **Monedas** para comprar ropa: la mitad de la XP ganada.
- 💡 **Misiones del día:** bonus fijo de XP y monedas al cumplirlas, como en las imágenes (+15 a +50 XP).
- 💡 **Tope diario** de XP y monedas (por ejemplo, 600 XP) para que no compense pasarse el día "farmeando".
- 💡 **Registro posterior** (lo que olvidaste iniciar): 70 % del efecto y de la recompensa, sin bonus de dosis óptima (4.4).

---

## 6. Día de ejemplo con las reglas v0.3 (recalculado 17-nov)

Simulado minuto a minuto con todas las reglas de la sección 8. Se parte de: llevas varios días sin quedar (Social 25, en "Poco"); check-in "He dormido bien" (Energía 75) y "Me siento bien" (Ánimo 55). Físico 55, Mente 50.

### 6.1 Día A: día completo y bastante equilibrado (re-simulado con v0.4)

| Momento | Qué pasa | Físico | Mente | Social | Energía | Ánimo | Regla que actúa |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 08:00 | Check-in | 55 | 50 | 25 | 75 | 55 | |
| 08:15 | Ducha, desayuno y 2 vasos de agua | 55 | 50 | 25 | 82 | 58 | 7: acciones rápidas |
| 09:00-13:00 | Trabajar 4 h | 55 | 66 | 25 | 55 | 58 | |
| 13:00-13:30 | Pasear 30 min | 58 | 66 | 25 | 59 | 60 | 4: "Desconectar tras trabajar recarga más" |
| 14:00 | Comida y 2 vasos de agua | 58 | 66 | 25 | 64 | 60 | 7 |
| 14:00 | Revisión del equilibrio | 58 | 66 | 25 | 63 | 58 | 1: "Tener Social bajo te pesa" (−3) |
| 15:00-17:00 | Estudiar 2 h | 58 | 82 | 25 | 48 | 58 | |
| 18:00-19:05 | Entrenar 1 h 05 | 74 | 82 | 25 | 50 | 62 | |
| 20:00 | Revisión del equilibrio | 74 | 82 | 25 | 48 | 59 | 1: Social sigue bajo (−3) |
| 20:00-22:00 | Quedar con amigos 2 h | 74 | 82 | 65 | 39 | 69 | |
| 22:00-22:45 | Videojuegos 45 min (solo) | 74 | 82 | 65 | 37 | 73 | |
| Noche | Cena y 2 vasos (6 en total) | 74 | 82 | 65 | 41 | 74 | 7 |
| Todo el día | 7.500 pasos fuera de acciones | 81 | 82 | 65 | 42 | 74 | 5.13 |
| 23:00-07:00 | Dormir 8 h | 81 | 82 | 65 | 85 | 79 | 6 con sensibilidad "Algo": te levantas con 85 como máximo |

La Energía incluye el desgaste de 1,5 por hora despierto, también en los ratos sin acción. Ninguna área llevaba 48 h sin cuidarse salvo Social, que se cuida hoy, así que no hay desgaste por la regla 3.

### 6.2 Día B: jornada completa de trabajo y gimnasio por la tarde (re-simulado con v0.4)

| Momento | Qué pasa | Físico | Mente | Social | Energía | Ánimo | Regla que actúa |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 08:00 | Check-in | 55 | 50 | 25 | 75 | 55 | |
| 08:15 | Ducha, desayuno y 2 vasos de agua | 55 | 50 | 25 | 82 | 58 | 7 |
| 09:00-17:00 | Trabajar 8 h | 55 | 78 | 25 | 29 | 58 | Fatiga mental a partir de 6 h |
| 18:00-19:05 | Entrenar 1 h 05 | 63 | 78 | 25 | 28 | 60 | 2: "Ibas sin energía: rinde menos" |

Una jornada completa deja la Energía justo en el límite de "Poco" (29). Si a una persona el trabajo la cansa menos, la calibración personal (9) lo corrige.

### 6.3 Lo que enseñó la simulación y cómo se resolvió (17-nov)

1. ✅ **El Ánimo está controlado:** de 55 a 79 en un buen día (en la v0.2 llegaba a 96). Tener Social en "Poco" lo frena durante el día hasta que quedas.
2. ✅ **Mente subía demasiado rápido** (de 50 a 90 en un día). Resuelto: Estudiar +8 por hora y fatiga mental a partir de 6 h. Ahora llega a 82 en el día A.
3. ✅ **La jornada completa vaciaba la Energía.** Resuelto: desgaste de −1,5 por hora. Aun así, 8 h de trabajo te dejan en el límite (29); cada persona lo ajusta con la calibración (9).
4. ✅ **La regla de las pantallas no se notaba.** Resuelto: ahora limita la Energía al despertar y es un ajuste personal (regla 6).

---

## 7. Respuestas de Iker (16-nov) y lo que queda abierto

1. ✅ **Las 8 acciones valen para empezar.** Se irán añadiendo más en el futuro (candidatas: cocinar o comer bien, leer, tareas de casa, redes sociales).
2. ✅ **Las zonas óptimas encajan**, salvo videojuegos: 2 h al día es mucho. Nueva zona: 30 min - 1 h 30 min, "Demasiado" a partir de 2 h.
3. ✅ El tercer tramo de las áreas se llama **"Alto"**.
4. ⏸ **Recompensas:** se miran más adelante (5).
5. ✅ **Trabajar cuenta como acción**, también para trabajar en tus propios proyectos.
6. 💡 **Acciones combinadas, apuntadas para el futuro** (Iker: "muy buena idea"). Ejemplos: pasear con amigos o jugar una pachanga con los amigos (ejercicio + social). En la v1, una acción cada vez.

**Abierto:**
- ✅ **Onboarding al final** (Iker, 16-nov): interactivo y ligero, al estilo Duolingo, que te hace participar y completa tu perfil sin ser pesado antes de usar la app. Se diseña cuando la demo esté hecha y las bases bien construidas. Mientras tanto, en el prototipo se empieza en 50 o se meten los valores a mano.
- ❓ Cómo se combinan los efectos en las acciones combinadas: ¿se suman los de las dos acciones, o tienen su propia fila en la tabla?
- ❓ Ánimo sigue subiendo rápido en el día de ejemplo (de 60 a 96). ¿Lo bajamos ya o esperamos a probarlo? (Ver propuesta 8.1.)

---

## 8. Reglas para acercar el modelo a la realidad (✅ aceptadas por Iker, 16-nov)

El modelo deja de ser una calculadora fija: lo que haces depende de **cómo estás**, de **lo que ya has hecho** y de **cuánto llevas sin hacer algo**.

Regla para todas: cada efecto tiene que poder explicarse en **una línea** en la pantalla de resultado. Si no se puede explicar, no entra. Los números son de partida (💡) y se ajustan jugando.

1. ✅ **El Ánimo sale del equilibrio** (Iker: "te la compro al 100 %").
   - 💡 Cada 6 h despierto: Ánimo +3 si las otras cuatro áreas están en zona buena o alta, y −3 por cada área en "Poco".
   - 💡 Para compensar, lo que suman directamente las acciones al Ánimo baja más o menos a la mitad: entrenar +4, pasear +3, quedar con amigos +5, videojuegos +5, meditar +12 por hora.
   - Texto: "Tu equilibrio te sube el ánimo" / "Tener Social bajo te pesa".
2. ✅ **El efecto depende de cómo estás.**
   - 💡 Energía en "Poco" (menos de 30): entrenar, estudiar y trabajar dan la mitad de beneficio y gastan 1,5 veces más Energía.
   - 💡 Ánimo en "Poco": quedar con amigos y meditar suben el Ánimo 1,5 veces más.
   - Texto: "Ibas sin energía: rinde menos" / "Lo necesitabas: te ha sentado mejor".
3. ✅ **Periodo de gracia de 48 h** (Iker: uno o dos días de descanso a la semana vienen bien).
   - Un área **no baja** hasta pasadas **48 h seguidas** sin cuidarla. Después baja: Físico −8, Mente −6 y Social −10 al día.
   - Descansar uno o dos días a la semana no penaliza nada.
4. ✅ **Descanso activo** (Iker: "ahí has dado en el clavo").
   - 💡 Si llevas 2 h o más de Trabajar o Estudiar en las últimas 3 h, la primera media hora de Pasear, Meditar o Descansar recarga 1,5 veces más Energía.
   - Texto: "Desconectar después de estudiar recarga más".
5. ✅ **Días seguidos y descanso.**
   - 💡 Entrenar 4 días o más seguidos: desde el cuarto, Físico rinde un 25 % menos y cada sesión gasta −5 de Energía extra, hasta que haya un día sin entrenar.
   - 💡 El equilibrio también se mira **por semana** (pestaña "Semana"): por ejemplo, entrenar 3-5 días a la semana es la zona buena semanal.
   - Texto: "Llevas 5 días seguidos: tu cuerpo pide descanso".
6. ✅ **La hora importa, pero a cada uno le afecta distinto** (Iker, 17-nov: a él le afecta bastante; a otros, no).
   - ✅ Es un **ajuste personal**, "Pantallas antes de dormir: ¿cómo te afectan?": Nada / Algo / Mucho.
   - 💡 Efecto de videojuegos o entrenar en la última hora antes de tu horario de sueño: con "Algo", te levantas con la Energía a 85 como máximo; con "Mucho", a 75; con "Nada", sin efecto. (La v0.3 restaba un 15 % de recarga y no se notaba, porque 8 h de sueño llenaban igual la Energía.)
   - ✅ **Manda siempre la respuesta del check-in** ("¿Cómo has dormido?"): cada día el jugador dice cómo se siente de descansado de verdad, y eso pesa más que la regla.
   - Texto: "Pantallas antes de dormir: te levantas con menos energía".
7. ✅ **Acciones rápidas sin cronómetro**, también para llevar un **control diario** de lo que se bebe y de los hábitos básicos, si el jugador quiere.
   - 💡 **Agua:** contador de vasos con un toque y meta diaria que pone el propio jugador. Cada vaso, Energía +1, hasta la meta.
   - 💡 **Comer bien:** Energía +3, Ánimo +1 (hasta 3 veces al día).
   - 💡 **Ducharse:** Ánimo +2, Energía +2 (una vez al día).
   - Se añaden desde un acceso rápido en "Hoy" o en "Hacer", sin cronómetro.

💡 **Orden de construcción en el prototipo:** primero las reglas 1, 2 y 3, que son la base; después 4, 5 y 7, y por último la 6. Todas forman parte del diseño desde ya.

✅ Día de ejemplo rehecho con estas reglas (sección 6, 17-nov).

---

## 9. Calibración personal (✅ idea de Iker, 17-nov)

✅ **Idea de Iker:** cada persona es diferente. El juego no va a acertar desde el principio, así que cada uno lo regula con el uso diario: lo que le consume trabajar, andar o entrenar, o simplemente no hacer nada, para que sea lo más real posible. Lo más importante es que **cada día pueda decir cómo se siente de verdad**.

💡 **Cómo hacerlo sin que deje de ser un juego:**
1. **El check-in diario manda** (ya decidido): lo que el jugador dice cada mañana sobre su descanso y su ánimo fija sus valores de partida, por encima de lo que calcula el juego.
2. **Unos pocos ajustes personales** (Nada / Algo / Mucho), no toda la tabla: "Pantallas antes de dormir", "Cuánto me cansa trabajar", "Cuánto me cansa entrenar", "Cuánto me afecta no quedar con gente". Cada opción multiplica el efecto (por ejemplo ×0,5 / ×1 / ×1,5).
3. **Calibración automática que propone, no impone:** el juego compara lo que calculaba (por ejemplo, Energía 45 al despertar) con lo que el jugador dice en el check-in (75). Si la diferencia se repite durante varios días, pregunta: "Parece que el trabajo te cansa menos de lo que calculaba. ¿Lo ajusto?". El jugador acepta o no.
4. **Con límites:** los ajustes se mueven entre ×0,5 y ×1,5. Así nadie puede poner "entrenar 5 h no cansa" y el equilibrio sigue teniendo sentido.

⚠️ Riesgo: si el jugador puede tocarlo todo, el juego pierde su gracia y puede "engañarse" a sí mismo. Por eso se ajustan pocas cosas, con límites y con propuestas razonadas del propio juego.

### 9.1 Minipreguntas de un toque (✅ idea de Iker, 17-nov)

✅ **Idea de Iker:** la calibración no tiene que ser solo automática. Hay días mejores y peores: si vienes de un fin de semana con cena que se alargó, el lunes de trabajo pesa mucho más; si estás algo enfermo, te encuentras fatal. El juego tiene que poder enterarse con **minipreguntas** después de jornadas largas, entrenamientos largos, viajes, etc. ✅ **Condición clave:** sin rellenar textos, porque si usar la app se vuelve pesado se deja de usar.

💡 **Cómo funcionan:**
- **Dónde:** en la propia pantalla de **Resultado** de la acción, sin pantalla nueva. Una línea: "¿Qué tal ha ido?" con tres caras: **Mejor de lo normal / Normal / Peor**.
- **Un toque y listo.** Si no tocas nada, cuenta como "Normal". Siempre se puede saltar.
- **Por qué (opcional, otro toque):** si eliges "Peor" o "Mejor", aparecen unos chips para decir el motivo, sin escribir: "Dormí poco", "Noche larga", "Me encuentro mal", "Estrés", "Muy concentrado", "Con energía". Se puede elegir uno o ninguno.
- **Qué cambia:** solo esa acción. "Peor" = gasta 1,5 veces más Energía; "Mejor" = gasta la mitad. Texto en el resultado: "Hoy te ha costado más: lo tengo en cuenta".
- **Cuándo salen:** solo en momentos que lo merecen, no después de todo:
  - Acciones largas: trabajar 6 h o más, entrenar fuera de la zona óptima, quedar 4 h o más, y viajes cuando existan.
  - El primer día tras un fin de semana, un viaje o una pausa.
  - Como mucho **2 minipreguntas al día**.
- **"Me encuentro mal"** ofrece activar el **modo enfermo** (4.5), que pausa el desgaste y no castiga nada. Es un dato sensible: se guarda solo en el móvil y no se usa para nada más.
- **También en el check-in:** una línea opcional "¿Vienes arrastrando algo?" con los mismos chips. Si eliges, por ejemplo, "Noche larga", ese día las acciones de esfuerzo cansan un 25 % más ("Día pesado").

💡 **Cómo alimentan la calibración:** el juego guarda las respuestas (en el móvil) y busca patrones. Si "Peor" aparece casi todos los lunes después de trabajar, puede proponer: "Los lunes el trabajo te cuesta más. ¿Lo tengo en cuenta?". Siempre propone; tú decides.

💡 **Cuándo:** el check-in que manda ya está en el prototipo. Los ajustes personales y la calibración automática, después de probar el prototipo unas semanas, porque necesitan datos reales de uso.

