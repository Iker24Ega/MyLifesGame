using System.Globalization;
using System.Text;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pestaña "Hoy": reloj, pasos del día, foco, misiones, acciones rápidas y registro.</summary>
public class PantallaHoy : Pantalla
{
    private static readonly float[] Velocidades = { 1f / 60f, 1f, 10f };
    private static readonly string[] NombresVelocidad = { "x1", "x60", "x600" };
    private const int PasosEscala = 12000;

    private readonly Text reloj, pasosNumero, foco, registro, tituloMisiones;
    private readonly Image anilloProgreso;
    private readonly Button[] botonesVelocidad = new Button[3];
    private readonly Button botonAgua, botonComer, botonDucha;
    private readonly Image[] franjasMision = new Image[4];
    private readonly Text[] textosMision = new Text[4];
    private readonly Text[] premiosMision = new Text[4];
    private readonly RectTransform[] filasMision = new RectTransform[4];

    public PantallaHoy(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla Hoy", 1)
    {
        // Reloj y velocidad del tiempo
        RectTransform cabecera = UIFabrica.Fila(columna, "Cabecera", 100);
        reloj = UIFabrica.Texto(cabecera, "", 54, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(reloj.rectTransform, 0, 0, 0.5f, 1);
        for (int i = 0; i < 3; i++)
        {
            int indice = i;
            botonesVelocidad[i] = UIFabrica.Boton(cabecera, NombresVelocidad[i], Paleta.Gris, Paleta.Texto, 34,
                () => this.juego.Velocidad = Velocidades[indice]);
            UIFabrica.Anclar((RectTransform)botonesVelocidad[i].transform, 0.52f + 0.16f * i, 0.12f, 0.66f + 0.16f * i, 0.88f);
        }

        // Tarjeta de pasos con anillo
        RectTransform filaPasos = UIFabrica.Fila(columna, "Pasos", 290);
        Image tarjetaPasos = Tarjeta(filaPasos);
        RectTransform zonaAnillo = UIFabrica.Crear("Anillo", tarjetaPasos.transform);
        zonaAnillo.anchorMin = zonaAnillo.anchorMax = new Vector2(0, 0.5f);
        zonaAnillo.sizeDelta = new Vector2(240, 240);
        zonaAnillo.anchoredPosition = new Vector2(160, 0);
        Image baseAnillo = UIFabrica.Caja(zonaAnillo, "Base", Paleta.Gris);
        baseAnillo.sprite = UIFabrica.Circulo; baseAnillo.type = Image.Type.Simple;
        UIFabrica.Anclar(baseAnillo.rectTransform, 0, 0, 1, 1);
        anilloProgreso = UIFabrica.Caja(zonaAnillo, "Progreso", Paleta.Verde);
        anilloProgreso.sprite = UIFabrica.Circulo;
        anilloProgreso.type = Image.Type.Filled;
        anilloProgreso.fillMethod = Image.FillMethod.Radial360;
        anilloProgreso.fillOrigin = (int)Image.Origin360.Top;
        anilloProgreso.fillClockwise = true;
        UIFabrica.Anclar(anilloProgreso.rectTransform, 0, 0, 1, 1);
        Image hueco = UIFabrica.Caja(zonaAnillo, "Hueco", Paleta.Tarjeta);
        hueco.sprite = UIFabrica.Circulo; hueco.type = Image.Type.Simple;
        UIFabrica.Anclar(hueco.rectTransform, 0.14f, 0.14f, 0.86f, 0.86f);
        pasosNumero = UIFabrica.Texto(zonaAnillo, "", 40, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(pasosNumero.rectTransform, 0, 0, 1, 1);

        Text tituloPasos = UIFabrica.Texto(tarjetaPasos.transform, "Pasos de hoy", 40, Paleta.Texto, TextAnchor.UpperLeft, FontStyle.Bold);
        UIFabrica.Anclar(tituloPasos.rectTransform, 0.33f, 0.62f, 0.98f, 0.92f);
        Text detalle = UIFabrica.Texto(tarjetaPasos.transform,
            "Zona buena desde 6.000. Suman Físico y algo de Energía. Solo cuentan los pasos fuera de tus acciones.",
            24, Paleta.TextoSuave, TextAnchor.UpperLeft);
        UIFabrica.Anclar(detalle.rectTransform, 0.33f, 0.32f, 0.98f, 0.62f);
        Button sumar = UIFabrica.Boton(tarjetaPasos.transform, "+1.000 pasos (simulado)", Paleta.Verde, Paleta.Blanco, 28,
            () => this.juego.SumarPasos(1000));
        UIFabrica.Anclar((RectTransform)sumar.transform, 0.33f, 0.07f, 0.97f, 0.3f);

        // Foco del día
        foco = TextoEnFila("", 50, 30, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);

        // Misiones
        tituloMisiones = TextoEnFila("Misiones de hoy", 50, 38, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        for (int i = 0; i < 4; i++)
        {
            filasMision[i] = UIFabrica.Fila(columna, "Misión " + i, 92);
            Image t = Tarjeta(filasMision[i]);
            franjasMision[i] = UIFabrica.Caja(t.transform, "Franja", Paleta.Gris, false);
            UIFabrica.Anclar(franjasMision[i].rectTransform, 0, 0.1f, 0.018f, 0.9f);
            textosMision[i] = UIFabrica.Texto(t.transform, "", 30, Paleta.Texto);
            UIFabrica.Anclar(textosMision[i].rectTransform, 0.04f, 0, 0.78f, 1);
            premiosMision[i] = UIFabrica.Texto(t.transform, "", 30, Paleta.Texto, TextAnchor.MiddleRight, FontStyle.Bold);
            UIFabrica.Anclar(premiosMision[i].rectTransform, 0.75f, 0, 0.97f, 1);
        }

        // Acciones rápidas
        RectTransform rapidas = UIFabrica.Fila(columna, "Acciones rápidas", 90);
        botonAgua = UIFabrica.Boton(rapidas, "Agua", Paleta.Tarjeta, Paleta.Texto, 30, () => this.juego.HacerRapida(AccionRapida.Agua));
        botonComer = UIFabrica.Boton(rapidas, "Comer bien", Paleta.Tarjeta, Paleta.Texto, 30, () => this.juego.HacerRapida(AccionRapida.ComerBien));
        botonDucha = UIFabrica.Boton(rapidas, "Ducha", Paleta.Tarjeta, Paleta.Texto, 30, () => this.juego.HacerRapida(AccionRapida.Ducha));
        UIFabrica.Anclar((RectTransform)botonAgua.transform, 0f, 0f, 0.32f, 1f);
        UIFabrica.Anclar((RectTransform)botonComer.transform, 0.34f, 0f, 0.66f, 1f);
        UIFabrica.Anclar((RectTransform)botonDucha.transform, 0.68f, 0f, 1f, 1f);

        // Equilibrio y dormir
        RectTransform botones = UIFabrica.Fila(columna, "Botones", 95);
        Button verEquilibrio = UIFabrica.Boton(botones, "Ver mi equilibrio", Paleta.DeArea(Area.Mente), Paleta.Blanco, 32, () => this.juego.IrAEquilibrio());
        Button dormir = UIFabrica.Boton(botones, "Dormir 8 h", Paleta.Gris, Paleta.Texto, 32, () => this.juego.Dormir());
        UIFabrica.Anclar((RectTransform)verEquilibrio.transform, 0f, 0f, 0.6f, 1f);
        UIFabrica.Anclar((RectTransform)dormir.transform, 0.63f, 0f, 1f, 1f);

        // Registro
        RectTransform filaRegistro = UIFabrica.Fila(columna, "Registro", 150);
        Image fondoRegistro = Tarjeta(filaRegistro);
        registro = UIFabrica.Texto(fondoRegistro.transform, "", 22, Paleta.Texto, TextAnchor.UpperLeft);
        UIFabrica.Anclar(registro.rectTransform, 0.03f, 0.04f, 0.97f, 0.96f);
        UIFabrica.Relleno(columna);
    }

    public override void Actualizar()
    {
        Jornada dia = juego.Jornada;
        reloj.text = "Día " + juego.Dia + " · " + Jornada.Hora(juego.MinutoEntero);
        for (int i = 0; i < 3; i++)
            botonesVelocidad[i].image.color = Mathf.Approximately(juego.Velocidad, Velocidades[i]) ? Paleta.Coral : Paleta.Gris;

        int pasos = dia.Datos.pasosHoy;
        anilloProgreso.fillAmount = Mathf.Clamp01(pasos / (float)PasosEscala);
        anilloProgreso.color = pasos >= 6000 ? Paleta.Verde : Paleta.TramoPoco;
        pasosNumero.text = pasos.ToString("N0", CultureInfo.GetCultureInfo("es-ES")) + "\n<size=24>pasos</size>";

        if (dia.Datos.focos.Count == 0) foco.text = "Hoy cuidas: (sin foco elegido en el check-in)";
        else
        {
            var sb = new StringBuilder("Hoy cuidas: ");
            for (int i = 0; i < dia.Datos.focos.Count; i++)
            {
                Area a = (Area)dia.Datos.focos[i];
                if (i > 0) sb.Append(" · ");
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(Paleta.DeArea(a))).Append('>').Append(Areas.Nombre(a)).Append("</color>");
            }
            foco.text = sb.ToString();
        }

        var misiones = juego.Progreso.Datos.misiones;
        int hechas = 0;
        for (int i = 0; i < 4; i++)
        {
            bool hay = i < misiones.Count;
            filasMision[i].gameObject.SetActive(hay);
            if (!hay) continue;
            MisionDia m = misiones[i];
            if (m.hecha) hechas++;
            franjasMision[i].color = Paleta.DeArea((Area)m.area);
            string estado = m.hecha ? "<color=#5BAA6B><b>HECHA</b></color>  " : "";
            string progreso = m.hecha ? "" : "  (" + Mathf.Min(m.minutosHechos, m.minutos) + "/" + m.minutos + " min)";
            string motivo = string.IsNullOrEmpty(m.motivo) ? "" : "\n<size=22><color=#8A7565>" + m.motivo + "</color></size>";
            textosMision[i].text = estado + m.titulo + progreso + motivo;
            premiosMision[i].text = "+" + m.xp + " XP";
        }
        tituloMisiones.text = "Misiones de hoy  " + hechas + "/" + misiones.Count;

        UIFabrica.CambiarTexto(botonAgua, "Agua " + dia.Datos.vasosAgua + "/" + dia.Datos.metaAgua);
        UIFabrica.CambiarTexto(botonComer, "Comer bien " + Mathf.Min(dia.Datos.comidasBuenas, Ajustes.MaxComidasBuenas) + "/" + Ajustes.MaxComidasBuenas);
        UIFabrica.CambiarTexto(botonDucha, dia.Datos.duchaHecha ? "Ducha (hecha)" : "Ducha");

        var reg = new StringBuilder("<b>Últimos cambios</b>\n");
        int desde = Mathf.Max(0, dia.Eventos.Count - 3);
        for (int i = dia.Eventos.Count - 1; i >= desde; i--)
        {
            Evento e = dia.Eventos[i];
            reg.Append(Jornada.Hora(e.minutoInicio)).Append("  ").Append(e.texto);
            if (!string.IsNullOrEmpty(e.nota)) reg.Append(" → ").Append(e.nota);
            reg.Append('\n');
        }
        registro.text = reg.ToString();
    }
}
