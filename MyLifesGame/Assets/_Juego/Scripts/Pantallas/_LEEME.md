# Pantallas

Interfaz del juego, creada desde código al pulsar Play (Juego.cs arranca solo, no hace falta montar nada en la escena).

- `Juego.cs`: reloj del juego, conexión con el motor y cambio de pantallas.
- `PantallaPrincipal.cs`, `PantallaHacer.cs`, `PantallaCronometro.cs`, `PantallaResultado.cs`: las pantallas del prototipo.
- `UIFabrica.cs`, `BarraTramos.cs`, `Paleta.cs`: piezas reutilizables y colores.

Las pantallas **solo muestran** lo que calcula el Motor; nunca calculan reglas por su cuenta.
