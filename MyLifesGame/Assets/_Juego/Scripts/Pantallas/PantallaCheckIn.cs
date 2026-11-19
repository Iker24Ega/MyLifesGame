using System.Collections.Generic;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Check-in de la mañana: cómo has dormido, cómo te sientes, qué quieres cuidar y si vienes arrastrando algo.</summary>
public class PantallaCheckIn : Pantalla
{
    private static readonly string[] Sueno = { "Fatal", "Mal", "Regular", "Bien", "Genial" };
    private static readonly string[] Sentir = { "Agotado", "Cansado", "Normal", "Bien", "A tope" };

    private readonly Button[] botonesSueno = new Button[5];
    private readonly Button[] botonesSentir = new Button[5];
    private readonly Button[] botonesFoco = new Button[5];
    private readonly Button botonArrastro;
    private readonly Text titulo;
    private int sueno = 3, sentir = 3;
    private readonly List<Area> focos = new List<Area>();
    private bool arrastro;

    public PantallaCheckIn(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla check-in")
    {
        titulo = TextoEnFila("Buenos días", 110, 72, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        TextoEnFila("Cuéntame cómo empiezas el día. Son unos toques.", 50, 30, Paleta.TextoSuave);

        TextoEnFila("¿Cómo has dormido?", 60, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        FilaOpciones(Sueno, botonesSueno, i => { sueno = i; Actualizar(); });
        TextoEnFila("¿Cómo te sientes?", 60, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        FilaOpciones(Sentir, botonesSentir, i => { sentir = i; Actualizar(); });

        TextoEnFila("¿Qué quieres cuidar hoy? (hasta 2)", 60, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        var nombres = new string[5];
        foreach (Area a in Areas.Todas) nombres[(int)a] = Areas.Nombre(a);
        FilaOpciones(nombres, botonesFoco, i =>
        {
            Area a = (Area)i;
            if (focos.Contains(a)) focos.Remove(a);
            else { if (focos.Count >= 2) focos.RemoveAt(0); focos.Add(a); }
            Actualizar();
        });

        TextoEnFila("¿Vienes arrastrando algo? (opcional)", 60, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        botonArrastro = BotonEnFila("", 90, Paleta.Gris, Paleta.Texto, 30, () => { arrastro = !arrastro; Actualizar(); });

        UIFabrica.Relleno(columna);
        BotonEnFila("Empezar el día", 140, Paleta.Coral, Paleta.Blanco, 52, () => this.juego.CompletarCheckIn(sueno, sentir, focos, arrastro));
        BotonEnFila("Saltar por hoy", 90, Paleta.Gris, Paleta.Texto, 32, () => this.juego.CompletarCheckIn(3, 2, new List<Area>(), false));
    }

    private void FilaOpciones(string[] textos, Button[] botones, System.Action<int> alElegir)
    {
        RectTransform fila = UIFabrica.Fila(columna, "Opciones", 96);
        for (int i = 0; i < textos.Length; i++)
        {
            int indice = i;
            botones[i] = UIFabrica.Boton(fila, textos[i], Paleta.Tarjeta, Paleta.Texto, 28, () => alElegir(indice));
            UIFabrica.Anclar((RectTransform)botones[i].transform, i / 5f + 0.004f, 0, (i + 1) / 5f - 0.004f, 1);
        }
    }

    protected override void AlMostrar()
    {
        sueno = 3; sentir = 3; focos.Clear(); arrastro = false;
        titulo.text = "Buenos días · Día " + juego.Dia;
    }

    public override void Actualizar()
    {
        for (int i = 0; i < 5; i++)
        {
            botonesSueno[i].image.color = i == sueno ? Paleta.Verde : Paleta.Tarjeta;
            botonesSentir[i].image.color = i == sentir ? Paleta.Verde : Paleta.Tarjeta;
            botonesFoco[i].image.color = focos.Contains((Area)i) ? Paleta.DeArea((Area)i) : Paleta.Tarjeta;
        }
        UIFabrica.CambiarTexto(botonArrastro, arrastro
            ? "SÍ: noche larga o día pesado (el esfuerzo cansará un poco más)"
            : "No, día normal (toca si vienes arrastrando algo)");
        botonArrastro.image.color = arrastro ? Paleta.TramoDemasiado : Paleta.Gris;
    }
}
