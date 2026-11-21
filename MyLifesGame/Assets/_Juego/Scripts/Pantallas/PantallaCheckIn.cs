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

    private readonly RawImage escena;

    private readonly Text textoSueno, fecha, zzz;
    private readonly Image[] marcoSueno = new Image[5];
    private readonly Image[] checkFoco = new Image[5];

    public PantallaCheckIn(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla check-in", -1, true)
    {
        // Como la imagen de Flow: dormitorio 3D arriba (el avatar sale de la cama y se estira) y una hoja con las preguntas
        ComoHoja(0.56f, 0f);
        escena = FondoEscena();

        // Fecha arriba a la izquierda y "Zzz" sobre el avatar
        Image pildoraFecha = UIFabrica.Caja(raizRT, "Fecha", Paleta.Tarjeta);
        pildoraFecha.rectTransform.anchorMin = pildoraFecha.rectTransform.anchorMax = new Vector2(0, 1);
        pildoraFecha.rectTransform.pivot = new Vector2(0, 1);
        pildoraFecha.rectTransform.sizeDelta = new Vector2(250, 76);
        pildoraFecha.rectTransform.anchoredPosition = new Vector2(40, -60);
        UIFabrica.Sombra(pildoraFecha, 4f, 0.15f);
        fecha = UIFabrica.Texto(pildoraFecha.transform, "", 32, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(fecha.rectTransform, 0, 0, 1, 1);
        zzz = UIFabrica.Texto(raizRT, "Z z z", 64, new Color(0.45f, 0.38f, 0.5f, 0.85f), TextAnchor.MiddleCenter, FontStyle.Bold);
        zzz.rectTransform.anchorMin = zzz.rectTransform.anchorMax = new Vector2(0.72f, 0.86f);
        zzz.rectTransform.sizeDelta = new Vector2(260, 120);
        zzz.raycastTarget = false;

        // Cabecera de la hoja: puntos de página, título y subtítulo
        RectTransform cab = UIFabrica.Fila(columna, "Cabecera", 100);
        for (int i = 0; i < 3; i++)
        {
            Image punto = UIFabrica.Caja(cab, "Punto", i == 0 ? Paleta.Texto : Paleta.Gris);
            punto.sprite = UIFabrica.Circulo; punto.type = Image.Type.Simple;
            punto.rectTransform.anchorMin = punto.rectTransform.anchorMax = new Vector2(1, 1);
            punto.rectTransform.sizeDelta = new Vector2(16, 16);
            punto.rectTransform.anchoredPosition = new Vector2(-70 + i * 26, -6);
        }
        titulo = UIFabrica.Texto(cab, "Buenos días", 58, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(titulo.rectTransform, 0, 0.38f, 1, 1);
        Text sub = UIFabrica.Texto(cab, "Cuéntame cómo empiezas el día", 32, Paleta.Texto, TextAnchor.MiddleCenter);
        UIFabrica.Anclar(sub.rectTransform, 0, 0, 1, 0.38f);

        // ¿Cómo has dormido? Cinco caritas sueltas; la elegida, en un recuadro blanco
        TextoEnFila("¿Cómo has dormido?", 44, 34, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        RectTransform filaCaras = UIFabrica.Fila(columna, "Caritas", 98);
        for (int i = 0; i < 5; i++)
        {
            int indice = i;
            botonesSueno[i] = UIFabrica.Boton(filaCaras, "", new Color(1, 1, 1, 0), Paleta.Texto, 10, () => { sueno = indice; Actualizar(); });
            RectTransform rb = (RectTransform)botonesSueno[i].transform;
            rb.anchorMin = rb.anchorMax = new Vector2((i + 0.5f) / 5f, 0.5f);
            rb.sizeDelta = new Vector2(104, 104);
            marcoSueno[i] = UIFabrica.Caja(rb, "Marco", Paleta.Blanco);
            UIFabrica.Anclar(marcoSueno[i].rectTransform, 0, 0, 1, 1);
            Outline o = marcoSueno[i].gameObject.AddComponent<Outline>();
            o.effectColor = Paleta.Hex("D9CFC0"); o.effectDistance = new Vector2(4, -4);
            RectTransform z = UIFabrica.Crear("Carita", rb);
            z.anchorMin = z.anchorMax = new Vector2(0.5f, 0.5f);
            z.sizeDelta = new Vector2(78, 78);
            FormasUI f = FormasUI.En(z.gameObject);
            f.raycastTarget = false;
            Iconos.Carita(f, i, ColorSueno[i], i <= 1);
        }
        textoSueno = TextoEnFila("", 34, 26, Paleta.TextoSuave, TextAnchor.MiddleCenter, FontStyle.Bold);

        // ¿Cómo te sientes? Píldoras de texto
        TextoEnFila("¿Cómo te sientes?", 44, 34, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        RectTransform filaSentir = UIFabrica.Fila(columna, "Sentir", 66);
        for (int i = 0; i < 5; i++)
        {
            int indice = i;
            botonesSentir[i] = UIFabrica.Boton(filaSentir, Sentir[i], Paleta.Hex("EFE5D6"), Paleta.Texto, 27, () => { sentir = indice; Actualizar(); });
            UIFabrica.Anclar((RectTransform)botonesSentir[i].transform, i / 5f + 0.006f, 0.05f, (i + 1) / 5f - 0.006f, 0.95f);
        }

        // ¿Qué quieres cuidar hoy? Píldoras del color de cada área, con un check en las elegidas
        TextoEnFila("¿Qué quieres cuidar hoy?", 44, 34, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        RectTransform filaFoco = UIFabrica.Fila(columna, "Focos", 68);
        foreach (Area ar in Areas.Todas)
        {
            int i = (int)ar;
            botonesFoco[i] = UIFabrica.Boton(filaFoco, Areas.Nombre(ar), Paleta.DeArea(ar), Paleta.Blanco, 27, () =>
            {
                Area a = (Area)i;
                if (focos.Contains(a)) focos.Remove(a);
                else { if (focos.Count >= 2) focos.RemoveAt(0); focos.Add(a); }
                Actualizar();
            });
            UIFabrica.Anclar((RectTransform)botonesFoco[i].transform, i / 5f + 0.006f, 0.05f, (i + 1) / 5f - 0.006f, 0.9f);
            checkFoco[i] = UIFabrica.Caja(botonesFoco[i].transform, "Check", Paleta.Hex("8C8279"));
            checkFoco[i].sprite = UIFabrica.Circulo; checkFoco[i].type = Image.Type.Simple;
            checkFoco[i].rectTransform.anchorMin = checkFoco[i].rectTransform.anchorMax = new Vector2(1, 1);
            checkFoco[i].rectTransform.sizeDelta = new Vector2(34, 34);
            checkFoco[i].rectTransform.anchoredPosition = new Vector2(-6, -2);
            Text tick = UIFabrica.Texto(checkFoco[i].transform, "\u2713", 24, Paleta.Blanco, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(tick.rectTransform, 0, 0, 1, 1);
        }

        botonArrastro = BotonEnFila("", 46, new Color(1, 1, 1, 0), Paleta.TextoSuave, 26, () => { arrastro = !arrastro; Actualizar(); });

        UIFabrica.Relleno(columna);
        Button empezar = BotonEnFila("Empezar el día", 100, Paleta.Coral, Paleta.Blanco, 48, () => this.juego.CompletarCheckIn(sueno, sentir, focos, arrastro));
        Text te = empezar.GetComponentInChildren<Text>();
        UIFabrica.Anclar(te.rectTransform, 0.12f, 0, 1, 1);
        RectTransform zs = UIFabrica.Crear("Sol", empezar.transform);
        zs.anchorMin = zs.anchorMax = new Vector2(0.24f, 0.5f);
        zs.sizeDelta = new Vector2(62, 62);
        FormasUI sol = FormasUI.En(zs.gameObject);
        sol.raycastTarget = false;
        Iconos.Sol(sol);
        BotonEnFila("Saltar por hoy", 54, new Color(1, 1, 1, 0), Paleta.TextoSuave, 28, () => this.juego.CompletarCheckIn(3, 2, new List<Area>(), false));
    }

    private static readonly Color[] ColorSueno = { Paleta.Hex("F2B0A0"), Paleta.Hex("F6D39A"), Paleta.Hex("F3DE9A"), Paleta.Hex("F6D873"), Paleta.Hex("F6CF5A") };
    private static readonly string[] Dias = { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };
    private static readonly string[] Meses = { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };

    protected override void AlMostrar()
    {
        sueno = 3; sentir = 3; focos.Clear(); arrastro = false;
        AjustarFondoEscena(escena);
        EstudioAvatar.PonerEscena("checkin", -3.8f, 1.2f);
        EstudioAvatar.PonerAccion("estirarse");
        // La cama y el avatar en la parte de arriba (la hoja tapa la de abajo)
        EstudioAvatar.EncuadrarEscena(new Vector3(4.6f, -3.6f, -1.4f), 1.05f, 15f);
        titulo.text = "Buenos días";
        System.DateTime hoy = System.DateTime.Now;
        fecha.text = Dias[(int)hoy.DayOfWeek] + " " + hoy.Day + " " + Meses[hoy.Month - 1];
    }

    public override void Actualizar()
    {
        for (int i = 0; i < 5; i++)
        {
            marcoSueno[i].enabled = i == sueno;
            marcoSueno[i].GetComponent<Outline>().enabled = i == sueno;
            bool s2 = i == sentir;
            botonesSentir[i].image.color = s2 ? Paleta.Hex("CFEBD3") : Paleta.Hex("EFE5D6");
            Outline o = botonesSentir[i].GetComponent<Outline>() ?? botonesSentir[i].gameObject.AddComponent<Outline>();
            o.effectColor = Paleta.Verde; o.effectDistance = new Vector2(3, -3); o.enabled = s2;
            bool f = focos.Contains((Area)i);
            botonesFoco[i].image.color = f ? Paleta.DeArea((Area)i) : Color.Lerp(Paleta.DeArea((Area)i), Color.white, 0.45f);
            checkFoco[i].gameObject.SetActive(f);
        }
        textoSueno.text = "Día " + juego.Dia + " · has dormido " + Sueno[sueno].ToLowerInvariant();
        UIFabrica.CambiarTexto(botonArrastro, arrastro
            ? "Vienes arrastrando algo: sí (el esfuerzo cansará un poco más)"
            : "¿Vienes arrastrando algo de ayer? No");
    }
}
