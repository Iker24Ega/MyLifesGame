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
    private readonly FormasUI anilloPasos;
    private readonly FormasUI[] iconosMision = new FormasUI[4];
    private readonly Image[] barrasMision = new Image[4];
    private readonly Button[] botonesVelocidad = new Button[3];
    private readonly Button botonAgua, botonComer, botonDucha;
    private readonly Image[] franjasMision = new Image[4];
    private readonly Text[] textosMision = new Text[4];
    private readonly Text[] premiosMision = new Text[4];
    private readonly RectTransform[] filasMision = new RectTransform[4];

    public PantallaHoy(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla Hoy", 1, true)
    {
        // Como en la imagen de referencia: la ciudad 3D se ve arriba y el contenido va en una hoja clara debajo.
        Image hoja = UIFabrica.Caja(raizRT, "Hoja", new Color(Paleta.Fondo.r, Paleta.Fondo.g, Paleta.Fondo.b, 0.97f));
        hoja.raycastTarget = true;
        UIFabrica.Anclar(hoja.rectTransform, 0, 0, 1, 0.75f);
        UIFabrica.Sombra(hoja, 12f, 0.18f);
        hoja.transform.SetSiblingIndex(0);

        // Reloj (pastilla sobre la ciudad) y velocidad del tiempo
        RectTransform cabecera = UIFabrica.Fila(columna, "Cabecera", 100);
        Image pastilla = UIFabrica.Caja(cabecera, "Pastilla fecha", Paleta.Tarjeta);
        UIFabrica.Anclar(pastilla.rectTransform, 0, 0.08f, 0.48f, 0.92f);
        UIFabrica.Sombra(pastilla, 6f, 0.18f);
        reloj = UIFabrica.Texto(pastilla.transform, "", 44, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(reloj.rectTransform, 0, 0, 1, 1);
        for (int i = 0; i < 3; i++)
        {
            int indice = i;
            botonesVelocidad[i] = UIFabrica.Boton(cabecera, NombresVelocidad[i], Paleta.Gris, Paleta.Texto, 34,
                () => this.juego.Velocidad = Velocidades[indice]);
            UIFabrica.Anclar((RectTransform)botonesVelocidad[i].transform, 0.52f + 0.16f * i, 0.12f, 0.66f + 0.16f * i, 0.88f);
        }

        UIFabrica.Fila(columna, "Ventana a la ciudad", 260);

        // Tarjeta de pasos con anillo
        RectTransform filaPasos = UIFabrica.Fila(columna, "Pasos", 290);
        Image tarjetaPasos = Tarjeta(filaPasos);
        RectTransform zonaAnillo = UIFabrica.Crear("Anillo", tarjetaPasos.transform);
        zonaAnillo.anchorMin = zonaAnillo.anchorMax = new Vector2(0, 0.5f);
        zonaAnillo.sizeDelta = new Vector2(240, 240);
        zonaAnillo.anchoredPosition = new Vector2(160, 0);
        Image hueco = UIFabrica.Caja(zonaAnillo, "Hueco", Paleta.Fondo);
        hueco.sprite = UIFabrica.Circulo; hueco.type = Image.Type.Simple;
        UIFabrica.Anclar(hueco.rectTransform, 0.1f, 0.1f, 0.9f, 0.9f);
        RectTransform capa = UIFabrica.Crear("Capa anillo", zonaAnillo);
        UIFabrica.Anclar(capa, 0, 0, 1, 1);
        anilloPasos = FormasUI.En(capa.gameObject);
        anilloPasos.raycastTarget = false;
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
            UIFabrica.Anclar(franjasMision[i].rectTransform, 0, 0.1f, 0.014f, 0.9f);
            iconosMision[i] = Iconos.Crear(t.transform, Area.Fisico, 60f);
            iconosMision[i].rectTransform.anchorMin = iconosMision[i].rectTransform.anchorMax = new Vector2(0, 0.5f);
            iconosMision[i].rectTransform.anchoredPosition = new Vector2(60, 4);
            textosMision[i] = UIFabrica.Texto(t.transform, "", 28, Paleta.Texto);
            UIFabrica.Anclar(textosMision[i].rectTransform, 0.13f, 0.22f, 0.78f, 1);
            Image fondoBarra = UIFabrica.Caja(t.transform, "Fondo barra", Paleta.Gris);
            UIFabrica.Anclar(fondoBarra.rectTransform, 0.13f, 0.1f, 0.76f, 0.2f);
            barrasMision[i] = UIFabrica.Caja(fondoBarra.transform, "Barra", Paleta.Verde);
            barrasMision[i].type = Image.Type.Filled;
            barrasMision[i].fillMethod = Image.FillMethod.Horizontal;
            UIFabrica.Anclar(barrasMision[i].rectTransform, 0, 0, 1, 1);
            premiosMision[i] = UIFabrica.Texto(t.transform, "", 30, Paleta.Texto, TextAnchor.MiddleRight, FontStyle.Bold);
            UIFabrica.Anclar(premiosMision[i].rectTransform, 0.75f, 0, 0.97f, 1);
        }

        // Acciones rápidas
        RectTransform rapidas = UIFabrica.Fila(columna, "Acciones rápidas", 90);
        botonAgua = UIFabrica.Boton(rapidas, "Agua", Paleta.Hex("D7E8F7"), Paleta.Texto, 30, () => this.juego.HacerRapida(AccionRapida.Agua));
        botonComer = UIFabrica.Boton(rapidas, "Comer bien", Paleta.Hex("DCEFD9"), Paleta.Texto, 30, () => this.juego.HacerRapida(AccionRapida.ComerBien));
        botonDucha = UIFabrica.Boton(rapidas, "Ducha", Paleta.Hex("E9E0F5"), Paleta.Texto, 30, () => this.juego.HacerRapida(AccionRapida.Ducha));
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
        RectTransform filaRegistro = UIFabrica.Fila(columna, "Registro", 96);
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
        anilloPasos.Limpiar();
        float g(float v) { return Mathf.Clamp01(v / PasosEscala) * 360f; }
        anilloPasos.Arco(Vector2.zero, 0.9f, 1f, 0f, g(6000), Paleta.TramoPoco);
        anilloPasos.Arco(Vector2.zero, 0.9f, 1f, g(6000), g(10000), Paleta.TramoBueno);
        anilloPasos.Arco(Vector2.zero, 0.9f, 1f, g(10000), 360f, Paleta.TramoAlto);
        anilloPasos.Arco(Vector2.zero, 0.74f, 0.86f, 0f, 360f, Paleta.Gris);
        if (pasos > 0) anilloPasos.Arco(Vector2.zero, 0.74f, 0.86f, 0f, g(pasos), pasos >= 6000 ? Paleta.Verde : Paleta.Coral);
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
            Iconos.Dibujar(iconosMision[i], (Area)m.area, true);
            barrasMision[i].fillAmount = m.hecha ? 1f : Mathf.Clamp01(m.minutosHechos / (float)Mathf.Max(1, m.minutos));
            barrasMision[i].color = m.hecha ? Paleta.Verde : Paleta.DeArea((Area)m.area);
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
            if (!string.IsNullOrEmpty(e.nota)) reg.Append(": ").Append(e.nota);
            reg.Append('\n');
        }
        registro.text = reg.ToString();
    }
}
