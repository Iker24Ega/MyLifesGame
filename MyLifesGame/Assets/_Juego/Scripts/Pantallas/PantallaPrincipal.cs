using System.Text;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pantalla principal: reloj, las cinco áreas, acciones rápidas, registro y botón Hacer.</summary>
public class PantallaPrincipal : Pantalla
{
    private static readonly float[] Velocidades = { 1f / 60f, 1f, 10f };
    private static readonly string[] NombresVelocidad = { "x1", "x60", "x600" };

    private readonly Text reloj;
    private readonly Button[] botonesVelocidad = new Button[3];
    private readonly BarraTramos[] barras = new BarraTramos[5];
    private readonly Text[] valores = new Text[5];
    private readonly Button botonAgua, botonComer, botonDucha;
    private readonly Text registro;

    public PantallaPrincipal(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla principal")
    {
        // Cabecera: reloj y velocidad del tiempo
        RectTransform cabecera = UIFabrica.Fila(columna, "Cabecera", 110);
        reloj = UIFabrica.Texto(cabecera, "", 60, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(reloj.rectTransform, 0, 0, 0.5f, 1);
        for (int i = 0; i < 3; i++)
        {
            int indice = i;
            botonesVelocidad[i] = UIFabrica.Boton(cabecera, NombresVelocidad[i], Paleta.Gris, Paleta.Texto, 36,
                () => this.juego.Velocidad = Velocidades[indice]);
            UIFabrica.Anclar((RectTransform)botonesVelocidad[i].transform, 0.52f + 0.16f * i, 0.12f, 0.66f + 0.16f * i, 0.88f);
        }
        TextoEnFila("Velocidad del tiempo: x1 = real · x60 = 1 min por segundo · x600 = 10 min por segundo", 50, 24, Paleta.TextoSuave);

        // Tarjeta con las cinco áreas
        RectTransform filaTarjeta = UIFabrica.Fila(columna, "Tu equilibrio", 680);
        Image tarjeta = Tarjeta(filaTarjeta);
        RectTransform col = UIFabrica.Columna(tarjeta.transform, "Columna", 32, 10);
        Text titulo = UIFabrica.Texto(UIFabrica.Fila(col, "Título", 60), "Tu equilibrio", 44, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(titulo.rectTransform, 0, 0, 1, 1);
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            RectTransform fila = UIFabrica.Fila(col, Areas.Nombre(area), 90);
            Image punto = UIFabrica.Caja(fila, "Punto", Paleta.DeArea(area));
            punto.sprite = UIFabrica.Circulo;
            punto.type = Image.Type.Simple;
            punto.rectTransform.anchorMin = new Vector2(0, 0.5f);
            punto.rectTransform.anchorMax = new Vector2(0, 0.5f);
            punto.rectTransform.sizeDelta = new Vector2(34, 34);
            punto.rectTransform.anchoredPosition = new Vector2(20, 0);
            Text nombre = UIFabrica.Texto(fila, Areas.Nombre(area), 38, Paleta.Texto);
            UIFabrica.Anclar(nombre.rectTransform, 0.06f, 0, 0.28f, 1);
            barras[i] = new BarraTramos(fila, new[] { 0.3f, 0.8f, 1f },
                new[] { Paleta.TramoPoco, Paleta.TramoBueno, Paleta.TramoAlto });
            UIFabrica.Anclar(barras[i].Raiz, 0.29f, 0, 0.86f, 1);
            valores[i] = UIFabrica.Texto(fila, "", 40, Paleta.Texto, TextAnchor.MiddleRight, FontStyle.Bold);
            UIFabrica.Anclar(valores[i].rectTransform, 0.87f, 0, 1f, 1);
        }
        Text leyenda = UIFabrica.Texto(UIFabrica.Fila(col, "Leyenda", 40),
            "Amarillo: Poco (< 30) · Verde: Zona buena · Azul: Alto (> 80)", 24, Paleta.TextoSuave);
        UIFabrica.Anclar(leyenda.rectTransform, 0, 0, 1, 1);

        // Acciones rápidas
        RectTransform rapidas = UIFabrica.Fila(columna, "Acciones rápidas", 100);
        botonAgua = UIFabrica.Boton(rapidas, "Agua", Paleta.Tarjeta, Paleta.Texto, 34, () => this.juego.HacerRapida(AccionRapida.Agua));
        botonComer = UIFabrica.Boton(rapidas, "Comer bien", Paleta.Tarjeta, Paleta.Texto, 34, () => this.juego.HacerRapida(AccionRapida.ComerBien));
        botonDucha = UIFabrica.Boton(rapidas, "Ducha", Paleta.Tarjeta, Paleta.Texto, 34, () => this.juego.HacerRapida(AccionRapida.Ducha));
        UIFabrica.Anclar((RectTransform)botonAgua.transform, 0f, 0f, 0.32f, 1f);
        UIFabrica.Anclar((RectTransform)botonComer.transform, 0.34f, 0f, 0.66f, 1f);
        UIFabrica.Anclar((RectTransform)botonDucha.transform, 0.68f, 0f, 1f, 1f);

        // Registro de lo último que ha pasado
        RectTransform filaRegistro = UIFabrica.Fila(columna, "Registro", 330);
        Image fondoRegistro = Tarjeta(filaRegistro);
        registro = UIFabrica.Texto(fondoRegistro.transform, "", 26, Paleta.Texto, TextAnchor.UpperLeft);
        UIFabrica.Anclar(registro.rectTransform, 0.04f, 0.05f, 0.96f, 0.95f);

        UIFabrica.Relleno(columna);

        // Botones principales
        RectTransform botones = UIFabrica.Fila(columna, "Botones", 150);
        Button hacer = UIFabrica.Boton(botones, "+  Hacer", Paleta.Coral, Paleta.Blanco, 56, () => this.juego.IrAHacer());
        Button dormir = UIFabrica.Boton(botones, "Dormir 8 h", Paleta.Gris, Paleta.Texto, 36, () => this.juego.Dormir());
        UIFabrica.Anclar((RectTransform)hacer.transform, 0f, 0f, 0.64f, 1f);
        UIFabrica.Anclar((RectTransform)dormir.transform, 0.67f, 0f, 1f, 1f);
    }

    public override void Actualizar()
    {
        Jornada dia = juego.Jornada;
        reloj.text = "Día " + juego.Dia + " · " + Jornada.Hora(juego.MinutoEntero);
        for (int i = 0; i < 3; i++)
            botonesVelocidad[i].image.color = Mathf.Approximately(juego.Velocidad, Velocidades[i]) ? Paleta.Coral : Paleta.Gris;

        foreach (Area area in Areas.Todas)
        {
            float v = dia.Estado[area];
            barras[(int)area].Valor(v / 100f);
            valores[(int)area].text = Mathf.RoundToInt(v).ToString();
        }

        UIFabrica.CambiarTexto(botonAgua, "Agua " + dia.VasosAgua + "/" + dia.MetaAgua);
        UIFabrica.CambiarTexto(botonComer, "Comer bien " + Mathf.Min(dia.ComidasBuenas, Ajustes.MaxComidasBuenas) + "/" + Ajustes.MaxComidasBuenas);
        UIFabrica.CambiarTexto(botonDucha, dia.DuchaHecha ? "Ducha (hecha)" : "Ducha");

        var sb = new StringBuilder("<b>Últimos cambios</b>\n");
        int desde = Mathf.Max(0, dia.Eventos.Count - 6);
        for (int i = dia.Eventos.Count - 1; i >= desde; i--)
        {
            Evento e = dia.Eventos[i];
            sb.Append(Jornada.Hora(e.minutoInicio)).Append("  ").Append(e.texto);
            if (!string.IsNullOrEmpty(e.nota)) sb.Append(" → ").Append(e.nota);
            sb.Append('\n');
        }
        registro.text = sb.ToString();
    }
}
