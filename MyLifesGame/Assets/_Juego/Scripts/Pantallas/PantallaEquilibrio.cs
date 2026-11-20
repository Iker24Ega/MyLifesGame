using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>"Tu equilibrio": radar de las cinco áreas (con el avatar en el centro) y una fila por área.</summary>
public class PantallaEquilibrio : Pantalla
{
    private static readonly Color ColorFondo = Paleta.Hex("EDE2D2");
    private static readonly Color ColorZona = new Color(0.62f, 0.83f, 0.66f, 0.9f);
    private static readonly Color ColorValores = new Color(0.91f, 0.38f, 0.30f, 0.38f);
    private static readonly Color ColorBorde = Paleta.Hex("A8553F");

    private readonly FormasUI radar;
    private readonly BarraTramos[] barras = new BarraTramos[5];
    private readonly Text[] estados = new Text[5];
    private readonly Text[] valores = new Text[5];
    private readonly Text sugerencia;

    public PantallaEquilibrio(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla equilibrio", 1)
    {
        RectTransform cabecera = UIFabrica.Fila(columna, "Cabecera", 90);
        Button volver = UIFabrica.Boton(cabecera, "<", Paleta.Tarjeta, Paleta.Texto, 44, () => this.juego.IrAPestana(1));
        UIFabrica.Anclar((RectTransform)volver.transform, 0, 0.05f, 0.11f, 0.95f);
        Text titulo = UIFabrica.Texto(cabecera, "Tu equilibrio", 56, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(titulo.rectTransform, 0.12f, 0, 0.88f, 1);

        RectTransform filaRadar = UIFabrica.Fila(columna, "Radar", 640);
        RectTransform zona = UIFabrica.Crear("Zona radar", filaRadar);
        zona.anchorMin = zona.anchorMax = new Vector2(0.5f, 0.5f);
        zona.sizeDelta = new Vector2(470, 470);
        radar = FormasUI.En(zona.gameObject);
        radar.raycastTarget = false;

        // Retrato del avatar en el centro
        Image retrato = UIFabrica.Caja(zona, "Retrato", Paleta.Blanco);
        retrato.sprite = UIFabrica.Circulo; retrato.type = Image.Type.Simple;
        retrato.rectTransform.anchorMin = retrato.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        retrato.rectTransform.sizeDelta = new Vector2(96, 96);
        UIFabrica.Sombra(retrato, 4f, 0.2f);
        Image cara = UIFabrica.Caja(retrato.transform, "Cara", Paleta.Piel);
        cara.sprite = UIFabrica.Circulo; cara.type = Image.Type.Simple;
        UIFabrica.Anclar(cara.rectTransform, 0.1f, 0.1f, 0.9f, 0.9f);
        Image pelo = UIFabrica.Caja(cara.transform, "Pelo", Paleta.Pelo);
        UIFabrica.Anclar(pelo.rectTransform, 0.05f, 0.62f, 0.95f, 1.02f);

        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            float ang = Mathf.PI / 2f - i * 2f * Mathf.PI / 5f;
            Vector2 pos = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 272f;
            FormasUI icono = Iconos.Crear(zona, area, 66f);
            icono.rectTransform.anchorMin = icono.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icono.rectTransform.anchoredPosition = pos;
            Text nombre = UIFabrica.Texto(zona, Areas.Nombre(area), 30, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            nombre.rectTransform.anchorMin = nombre.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            nombre.rectTransform.sizeDelta = new Vector2(220, 40);
            nombre.rectTransform.anchoredPosition = pos + new Vector2(0, pos.y >= 0 ? 48 : -48);
        }

        TextoEnFila("Amarillo: Poco (< 30) · Verde: Zona buena · Azul: Alto (> 80)", 40, 24, Paleta.TextoSuave, TextAnchor.MiddleCenter);

        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            RectTransform fila = UIFabrica.Fila(columna, Areas.Nombre(area), 108);
            Image t = Tarjeta(fila);
            FormasUI icono = Iconos.Crear(t.transform, area, 64f);
            icono.rectTransform.anchorMin = icono.rectTransform.anchorMax = new Vector2(0, 0.5f);
            icono.rectTransform.anchoredPosition = new Vector2(56, 0);
            Text nombre = UIFabrica.Texto(t.transform, Areas.Nombre(area), 34, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFabrica.Anclar(nombre.rectTransform, 0.13f, 0.52f, 0.55f, 0.95f);
            valores[i] = UIFabrica.Texto(t.transform, "", 36, Paleta.Texto, TextAnchor.MiddleRight, FontStyle.Bold);
            UIFabrica.Anclar(valores[i].rectTransform, 0.75f, 0.52f, 0.96f, 0.95f);
            barras[i] = new BarraTramos(t.transform, new[] { 0.3f, 0.8f, 1f },
                new[] { Paleta.TramoPoco, Paleta.TramoBueno, Paleta.TramoAlto });
            UIFabrica.Anclar(barras[i].Raiz, 0.13f, 0.12f, 0.6f, 0.5f);
            estados[i] = UIFabrica.Texto(t.transform, "", 26, Paleta.TextoSuave, TextAnchor.MiddleRight);
            UIFabrica.Anclar(estados[i].rectTransform, 0.6f, 0.08f, 0.96f, 0.5f);
        }

        RectTransform filaSug = UIFabrica.Fila(columna, "Sugerencia", 120);
        Image ts = UIFabrica.Caja(filaSug, "Tarjeta sugerencia", Paleta.Hex("FBE3C8"));
        UIFabrica.Anclar(ts.rectTransform, 0, 0, 1, 1);
        UIFabrica.Sombra(ts);
        sugerencia = UIFabrica.Texto(ts.transform, "", 30, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(sugerencia.rectTransform, 0.05f, 0, 0.66f, 1);
        Button misiones = UIFabrica.Boton(ts.transform, "Ver misiones", Paleta.Coral, Paleta.Blanco, 30, () => this.juego.IrAPestana(1));
        UIFabrica.Anclar((RectTransform)misiones.transform, 0.68f, 0.18f, 0.97f, 0.82f);
        UIFabrica.Relleno(columna);
    }

    private static Vector2 Punta(int i, float r)
    {
        float ang = Mathf.PI / 2f - i * 2f * Mathf.PI / 5f;
        return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
    }

    public override void Actualizar()
    {
        EstadoAreas e = juego.Jornada.Estado;
        var v = new float[5];
        Area masBaja = Area.Fisico;
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            v[i] = Mathf.Clamp01(e[area] / 100f);
            barras[i].Valor(v[i]);
            valores[i].text = Mathf.RoundToInt(e[area]).ToString();
            switch (e.Tramo(area))
            {
                case TramoArea.Poco: estados[i].text = "Baja: conviene cuidarla"; estados[i].color = Paleta.Coral; break;
                case TramoArea.Alto: estados[i].text = "Alta"; estados[i].color = Paleta.DeArea(Area.Mente); break;
                default: estados[i].text = "En zona buena"; estados[i].color = Paleta.Verde; break;
            }
            if (area != Area.Animo && e[area] < e[masBaja]) masBaja = area;
        }

        radar.Limpiar();
        var exterior = new Vector2[5];
        for (int i = 0; i < 5; i++) exterior[i] = Punta(i, 1f);
        radar.Poligono(exterior, ColorFondo);
        for (int i = 0; i < 5; i++)
        {
            int j = (i + 1) % 5;
            radar.Poligono(new[] { Punta(i, 0.3f), Punta(i, 0.8f), Punta(j, 0.8f), Punta(j, 0.3f) }, ColorZona);
        }
        var puntos = new Vector2[5];
        for (int i = 0; i < 5; i++) puntos[i] = Punta(i, Mathf.Max(0.05f, v[i]));
        radar.Poligono(puntos, ColorValores, Vector2.zero);
        for (int i = 0; i < 5; i++) radar.Linea(puntos[i], puntos[(i + 1) % 5], 0.025f, ColorBorde);
        for (int i = 0; i < 5; i++)
        {
            radar.Circulo(puntos[i], 0.06f, ColorBorde);
            radar.Circulo(puntos[i], 0.035f, Color.white);
        }

        switch (masBaja)
        {
            case Area.Fisico: sugerencia.text = "Hoy te vendría bien moverte un poco"; break;
            case Area.Mente: sugerencia.text = "Un rato de estudio o lectura te vendría bien"; break;
            case Area.Social: sugerencia.text = "Hoy te vendría bien quedar con alguien"; break;
            default: sugerencia.text = "Descansa un poco o da un paseo corto"; break;
        }
    }
}
