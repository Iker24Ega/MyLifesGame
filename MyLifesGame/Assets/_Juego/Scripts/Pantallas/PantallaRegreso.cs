using System.Collections.Generic;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pantalla de regreso tras horas fuera: tu avatar te esperaba y te pregunta qué has hecho. Sin castigos.</summary>
public class PantallaRegreso : Pantalla
{
    private static readonly string[] Ids = { "trabajar", "estudiar", "entrenar", "quedar", "pasear", "descansar" };
    private static readonly string[] Nombres = { "Trabajar", "Estudiar", "Entrenar", "Con amigos", "Pasear", "Descansar" };

    private readonly Text banner;
    private readonly AvatarDibujo avatar;
    private readonly Button[] botones = new Button[6];
    private readonly Text[] duraciones = new Text[6];
    private readonly bool[] elegidas = new bool[6];
    private readonly int[] minutos = new int[6];

    public PantallaRegreso(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla regreso")
    {
        banner = TextoEnFila("", 80, 46, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        TextoEnFila("Tranquilo, no pierdes nada: tus barras estaban en pausa.", 46, 28, Paleta.TextoSuave, TextAnchor.MiddleCenter);

        RectTransform filaAvatar = UIFabrica.Fila(columna, "Avatar", 420);
        avatar = new AvatarDibujo(filaAvatar);
        Text burbuja = UIFabrica.Texto(filaAvatar, "¿Qué has hecho?", 40, Paleta.Texto, TextAnchor.UpperLeft, FontStyle.Bold);
        UIFabrica.Anclar(burbuja.rectTransform, 0.05f, 0.75f, 0.6f, 0.95f);

        TextoEnFila("Cuéntame qué ha pasado (cuenta un poco menos que en directo):", 50, 28, Paleta.Texto);
        for (int i = 0; i < 6; i++)
        {
            int indice = i;
            RectTransform fila = UIFabrica.Fila(columna, Nombres[i], 84);
            botones[i] = UIFabrica.Boton(fila, Nombres[i], Paleta.Tarjeta, Paleta.Texto, 32, () => { elegidas[indice] = !elegidas[indice]; Actualizar(); });
            UIFabrica.Anclar((RectTransform)botones[i].transform, 0, 0, 0.5f, 1);
            Button menos = UIFabrica.Boton(fila, "−", Paleta.Gris, Paleta.Texto, 40, () => { minutos[indice] = Mathf.Max(15, minutos[indice] - 15); Actualizar(); });
            UIFabrica.Anclar((RectTransform)menos.transform, 0.53f, 0.1f, 0.65f, 0.9f);
            duraciones[i] = UIFabrica.Texto(fila, "", 32, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(duraciones[i].rectTransform, 0.65f, 0, 0.86f, 1);
            Button mas = UIFabrica.Boton(fila, "+", Paleta.Gris, Paleta.Texto, 40, () => { minutos[indice] = Mathf.Min(600, minutos[indice] + 15); Actualizar(); });
            UIFabrica.Anclar((RectTransform)mas.transform, 0.88f, 0.1f, 1f, 0.9f);
        }

        UIFabrica.Relleno(columna);
        BotonEnFila("Listo", 130, Paleta.Verde, Paleta.Blanco, 50, Confirmar);
        BotonEnFila("Mejor luego", 80, Paleta.Gris, Paleta.Texto, 30, () => this.juego.TerminarRegreso(new List<KeyValuePair<string, int>>()));
    }

    protected override void AlMostrar()
    {
        int horas = Mathf.RoundToInt(juego.MinutosAusencia / 60f);
        banner.text = "Llevas " + horas + " h fuera";
        for (int i = 0; i < 6; i++) { elegidas[i] = false; minutos[i] = 60; }
        avatar.Actualizar(TipoEstadoAvatar.Descuidado, Catalogos.BuscarPrenda(juego.Progreso.Datos.ropaPuesta).colorHex);
    }

    public override void Actualizar()
    {
        for (int i = 0; i < 6; i++)
        {
            botones[i].image.color = elegidas[i] ? Paleta.TramoBueno : Paleta.Tarjeta;
            duraciones[i].text = Jornada.Duracion(minutos[i]);
        }
    }

    private void Confirmar()
    {
        var lista = new List<KeyValuePair<string, int>>();
        for (int i = 0; i < 6; i++) if (elegidas[i]) lista.Add(new KeyValuePair<string, int>(Ids[i], minutos[i]));
        juego.TerminarRegreso(lista);
    }
}
