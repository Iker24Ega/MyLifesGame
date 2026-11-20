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
        RectTransform cab = UIFabrica.Fila(columna, "Cabecera", 170);
        titulo = UIFabrica.Texto(cab, "Buenos días", 66, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(titulo.rectTransform, 0, 0.4f, 0.8f, 1);
        Text sub = UIFabrica.Texto(cab, "Cuéntame cómo empiezas el día. Son unos toques.", 30, Paleta.TextoSuave);
        UIFabrica.Anclar(sub.rectTransform, 0, 0, 0.8f, 0.4f);
        RectTransform zonaSol = UIFabrica.Crear("Sol", cab);
        zonaSol.anchorMin = zonaSol.anchorMax = new Vector2(1, 0.5f);
        zonaSol.sizeDelta = new Vector2(150, 150);
        zonaSol.anchoredPosition = new Vector2(-75, 0);
        FormasUI sol = FormasUI.En(zonaSol.gameObject);
        sol.raycastTarget = false;
        Iconos.Sol(sol);

        TextoEnFila("¿Cómo has dormido?", 60, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        FilaOpciones(Sueno, botonesSueno, 0, i => { sueno = i; Actualizar(); });
        TextoEnFila("¿Cómo te sientes?", 60, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        FilaOpciones(Sentir, botonesSentir, 1, i => { sentir = i; Actualizar(); });

        TextoEnFila("¿Qué quieres cuidar hoy? (hasta 2)", 60, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        var nombres = new string[5];
        foreach (Area a in Areas.Todas) nombres[(int)a] = Areas.Nombre(a);
        FilaOpciones(nombres, botonesFoco, 2, i =>
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

    /// <summary>Fila de cinco tarjetas con dibujo (tipo 0: caritas dormidas, 1: caritas, 2: iconos de área) y texto debajo.</summary>
    private void FilaOpciones(string[] textos, Button[] botones, int tipo, System.Action<int> alElegir)
    {
        RectTransform fila = UIFabrica.Fila(columna, "Opciones", 170);
        for (int i = 0; i < textos.Length; i++)
        {
            int indice = i;
            botones[i] = UIFabrica.Boton(fila, textos[i], Paleta.Tarjeta, Paleta.Texto, 26, () => alElegir(indice));
            UIFabrica.Anclar((RectTransform)botones[i].transform, i / 5f + 0.008f, 0, (i + 1) / 5f - 0.008f, 1);
            Text t = botones[i].GetComponentInChildren<Text>();
            UIFabrica.Anclar(t.rectTransform, 0.02f, 0.02f, 0.98f, 0.32f);
            RectTransform z = UIFabrica.Crear("Dibujo", botones[i].transform);
            z.anchorMin = z.anchorMax = new Vector2(0.5f, 0.64f);
            z.sizeDelta = new Vector2(86, 86);
            FormasUI f = FormasUI.En(z.gameObject);
            f.raycastTarget = false;
            if (tipo == 2) Iconos.Dibujar(f, (Area)i, true);
            else Iconos.Carita(f, i, tipo == 0 ? ColorSueno[i] : ColorSentir[i], tipo == 0 && i <= 1);
        }
    }

    private static readonly Color[] ColorSueno = { Paleta.Hex("C9D3E6"), Paleta.Hex("D5DCEB"), Paleta.Hex("F3DE9A"), Paleta.Hex("BFE3C5"), Paleta.Hex("9FD3A8") };
    private static readonly Color[] ColorSentir = { Paleta.Hex("F2B8A2"), Paleta.Hex("F6D2A8"), Paleta.Hex("F3DE9A"), Paleta.Hex("BFE3C5"), Paleta.Hex("9FD3A8") };

    protected override void AlMostrar()
    {
        sueno = 3; sentir = 3; focos.Clear(); arrastro = false;
        titulo.text = "Buenos días · Día " + juego.Dia;
    }

    private static void Marcar(Button b, bool elegido, Color color)
    {
        b.image.color = elegido ? Color.Lerp(Paleta.Tarjeta, color, 0.28f) : Paleta.Tarjeta;
        Outline o = b.GetComponent<Outline>() ?? b.gameObject.AddComponent<Outline>();
        o.effectColor = color;
        o.effectDistance = new Vector2(5, -5);
        o.enabled = elegido;
    }

    public override void Actualizar()
    {
        for (int i = 0; i < 5; i++)
        {
            Marcar(botonesSueno[i], i == sueno, Paleta.Verde);
            Marcar(botonesSentir[i], i == sentir, Paleta.Verde);
            Marcar(botonesFoco[i], focos.Contains((Area)i), Paleta.DeArea((Area)i));
        }
        UIFabrica.CambiarTexto(botonArrastro, arrastro
            ? "SÍ: noche larga o día pesado (el esfuerzo cansará un poco más)"
            : "No, día normal (toca si vienes arrastrando algo)");
        botonArrastro.image.color = arrastro ? Paleta.TramoDemasiado : Paleta.Gris;
    }
}
