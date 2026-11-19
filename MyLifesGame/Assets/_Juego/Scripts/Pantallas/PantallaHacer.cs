using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pantalla "Hacer": elegir una de las 8 acciones. El cronómetro empieza al momento.</summary>
public class PantallaHacer : Pantalla
{
    private readonly Button botonAmigos;

    public PantallaHacer(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla Hacer")
    {
        TextoEnFila("¿Qué vas a hacer?", 100, 64, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        TextoEnFila("Elige una acción: el cronómetro empieza al momento.", 50, 30, Paleta.TextoSuave);

        RectTransform filaAmigos = UIFabrica.Fila(columna, "Con amigos", 90);
        botonAmigos = UIFabrica.Boton(filaAmigos, "", Paleta.Gris, Paleta.Texto, 32, () =>
        {
            this.juego.ConAmigos = !this.juego.ConAmigos;
            Actualizar();
        });
        UIFabrica.Anclar((RectTransform)botonAmigos.transform, 0, 0, 1, 1);

        foreach (DefinicionAccion accion in juego.Tabla.acciones)
        {
            DefinicionAccion a = accion;
            RectTransform fila = UIFabrica.Fila(columna, a.nombre, 115);
            Button b = UIFabrica.Boton(fila, a.nombre + "   ·   zona óptima " + Rango(a), Paleta.Tarjeta, Paleta.Texto, 36,
                () => this.juego.EmpezarAccion(a));
            UIFabrica.Anclar((RectTransform)b.transform, 0, 0, 1, 1);
        }

        UIFabrica.Relleno(columna);
        RectTransform filaVolver = UIFabrica.Fila(columna, "Volver", 110);
        Button volver = UIFabrica.Boton(filaVolver, "Volver", Paleta.Gris, Paleta.Texto, 38, () => this.juego.Volver());
        UIFabrica.Anclar((RectTransform)volver.transform, 0, 0, 1, 1);
    }

    public override void Actualizar()
    {
        UIFabrica.CambiarTexto(botonAmigos, juego.ConAmigos
            ? "Con amigos: SÍ (suma Social en Videojuegos)"
            : "Con amigos: NO (toca para jugar con amigos)");
        botonAmigos.image.color = juego.ConAmigos ? Paleta.DeArea(Area.Social) : Paleta.Gris;
    }

    public static string Rango(DefinicionAccion a)
    {
        if (a.zonaMin <= 0) return "hasta " + Jornada.Duracion(a.zonaMax);
        return Jornada.Duracion(a.zonaMin) + " - " + Jornada.Duracion(a.zonaMax);
    }
}
