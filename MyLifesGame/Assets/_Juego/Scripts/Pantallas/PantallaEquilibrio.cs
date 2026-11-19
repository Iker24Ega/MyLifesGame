using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>"Tu equilibrio": radar de las cinco áreas y una fila por área con su barra de tramos.</summary>
public class PantallaEquilibrio : Pantalla
{
    private readonly GraficoRadar radar;
    private readonly RectTransform[] puntos = new RectTransform[5];
    private readonly BarraTramos[] barras = new BarraTramos[5];
    private readonly Text[] estados = new Text[5];
    private readonly Text[] valores = new Text[5];
    private readonly Text sugerencia;

    public PantallaEquilibrio(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla equilibrio", 1)
    {
        RectTransform cabecera = UIFabrica.Fila(columna, "Cabecera", 90);
        Button volver = UIFabrica.Boton(cabecera, "<", Paleta.Gris, Paleta.Texto, 44, () => this.juego.IrAPestana(1));
        UIFabrica.Anclar((RectTransform)volver.transform, 0, 0.05f, 0.11f, 0.95f);
        Text titulo = UIFabrica.Texto(cabecera, "Tu equilibrio", 56, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(titulo.rectTransform, 0.12f, 0, 0.88f, 1);

        // Radar
        RectTransform filaRadar = UIFabrica.Fila(columna, "Radar", 560);
        RectTransform zona = UIFabrica.Crear("Zona radar", filaRadar);
        zona.anchorMin = zona.anchorMax = new Vector2(0.5f, 0.5f);
        zona.sizeDelta = new Vector2(430, 430);
        radar = zona.gameObject.AddComponent<GraficoRadar>();
        radar.raycastTarget = false;
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            Image p = UIFabrica.Caja(zona, "Punto " + i, Paleta.Blanco);
            p.sprite = UIFabrica.Circulo; p.type = Image.Type.Simple;
            p.rectTransform.anchorMin = p.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            p.rectTransform.sizeDelta = new Vector2(26, 26);
            puntos[i] = p.rectTransform;

            float ang = Mathf.PI / 2f - i * 2f * Mathf.PI / 5f;
            Vector2 pos = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 262f;
            Image insignia = UIFabrica.Caja(zona, "Insignia " + i, Paleta.DeArea(area));
            insignia.sprite = UIFabrica.Circulo; insignia.type = Image.Type.Simple;
            insignia.rectTransform.anchorMin = insignia.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            insignia.rectTransform.sizeDelta = new Vector2(56, 56);
            insignia.rectTransform.anchoredPosition = pos;
            Text nombre = UIFabrica.Texto(zona, Areas.Nombre(area), 30, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            nombre.rectTransform.anchorMin = nombre.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            nombre.rectTransform.sizeDelta = new Vector2(200, 40);
            nombre.rectTransform.anchoredPosition = pos + new Vector2(0, pos.y >= 0 ? 46 : -46);
        }

        Text leyenda = TextoEnFila("Amarillo: Poco (< 30) · Verde: Zona buena · Azul: Alto (> 80)", 40, 24, Paleta.TextoSuave, TextAnchor.MiddleCenter);
        leyenda.supportRichText = true;

        // Filas
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            RectTransform fila = UIFabrica.Fila(columna, Areas.Nombre(area), 120);
            Image t = Tarjeta(fila);
            Text nombre = UIFabrica.Texto(t.transform, Areas.Nombre(area), 34, Paleta.DeArea(area), TextAnchor.MiddleLeft, FontStyle.Bold);
            UIFabrica.Anclar(nombre.rectTransform, 0.04f, 0.5f, 0.5f, 0.95f);
            valores[i] = UIFabrica.Texto(t.transform, "", 34, Paleta.Texto, TextAnchor.MiddleRight, FontStyle.Bold);
            UIFabrica.Anclar(valores[i].rectTransform, 0.6f, 0.5f, 0.96f, 0.95f);
            barras[i] = new BarraTramos(t.transform, new[] { 0.3f, 0.8f, 1f },
                new[] { Paleta.TramoPoco, Paleta.TramoBueno, Paleta.TramoAlto });
            UIFabrica.Anclar(barras[i].Raiz, 0.04f, 0.25f, 0.55f, 0.6f);
            estados[i] = UIFabrica.Texto(t.transform, "", 26, Paleta.TextoSuave, TextAnchor.MiddleRight);
            UIFabrica.Anclar(estados[i].rectTransform, 0.57f, 0.05f, 0.96f, 0.5f);
        }

        // Sugerencia
        RectTransform filaSug = UIFabrica.Fila(columna, "Sugerencia", 120);
        Image ts = UIFabrica.Caja(filaSug, "Tarjeta sugerencia", Paleta.Hex("FBE3C8"));
        UIFabrica.Anclar(ts.rectTransform, 0, 0, 1, 1);
        sugerencia = UIFabrica.Texto(ts.transform, "", 30, Paleta.Texto);
        UIFabrica.Anclar(sugerencia.rectTransform, 0.04f, 0, 0.66f, 1);
        Button misiones = UIFabrica.Boton(ts.transform, "Ver misiones", Paleta.Coral, Paleta.Blanco, 30, () => this.juego.IrAPestana(1));
        UIFabrica.Anclar((RectTransform)misiones.transform, 0.68f, 0.18f, 0.97f, 0.82f);
        UIFabrica.Relleno(columna);
    }

    public override void Actualizar()
    {
        EstadoAreas e = juego.Jornada.Estado;
        var v01 = new float[5];
        Area masBaja = Area.Fisico;
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            v01[i] = e[area] / 100f;
            barras[i].Valor(v01[i]);
            valores[i].text = Mathf.RoundToInt(e[area]).ToString();
            switch (e.Tramo(area))
            {
                case TramoArea.Poco: estados[i].text = "Baja: conviene cuidarla"; estados[i].color = Paleta.Coral; break;
                case TramoArea.Alto: estados[i].text = "Alta"; estados[i].color = Paleta.DeArea(Area.Mente); break;
                default: estados[i].text = "En zona buena"; estados[i].color = Paleta.Verde; break;
            }
            if (area != Area.Animo && e[area] < e[masBaja]) masBaja = area;
        }
        radar.PonerValores(v01);
        foreach (Area area in Areas.Todas) puntos[(int)area].anchoredPosition = radar.Punto((int)area, v01[(int)area]) - radar.rectTransform.rect.center;

        switch (masBaja)
        {
            case Area.Fisico: sugerencia.text = "Hoy te vendría bien moverte un poco"; break;
            case Area.Mente: sugerencia.text = "Un rato de estudio o lectura te vendría bien"; break;
            case Area.Social: sugerencia.text = "Hoy te vendría bien quedar con alguien"; break;
            default: sugerencia.text = "Descansa un poco o da un paseo corto"; break;
        }
    }
}
