using System.Text;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Resultado de la acción: cuánto ha cambiado cada área y por qué.</summary>
public class PantallaResultado : Pantalla
{
    private readonly Text subtitulo, tramo, notas;
    private readonly Text[] cambios = new Text[5];
    private readonly Text[] nuevos = new Text[5];

    public PantallaResultado(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla resultado")
    {
        TextoEnFila("Acción terminada", 110, 72, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        subtitulo = TextoEnFila("", 60, 40, Paleta.TextoSuave, TextAnchor.MiddleCenter);
        tramo = TextoEnFila("", 60, 40, Paleta.Verde, TextAnchor.MiddleCenter, FontStyle.Bold);

        RectTransform filaTarjeta = UIFabrica.Fila(columna, "Cambios", 560);
        Image tarjeta = Tarjeta(filaTarjeta);
        RectTransform col = UIFabrica.Columna(tarjeta.transform, "Columna", 32, 10);
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
            Text n = UIFabrica.Texto(fila, Areas.Nombre(area), 40, Paleta.Texto);
            UIFabrica.Anclar(n.rectTransform, 0.07f, 0, 0.45f, 1);
            cambios[i] = UIFabrica.Texto(fila, "", 46, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(cambios[i].rectTransform, 0.45f, 0, 0.7f, 1);
            nuevos[i] = UIFabrica.Texto(fila, "", 36, Paleta.TextoSuave, TextAnchor.MiddleRight);
            UIFabrica.Anclar(nuevos[i].rectTransform, 0.7f, 0, 1f, 1);
        }

        notas = TextoEnFila("", 300, 32, Paleta.Texto, TextAnchor.UpperLeft);
        UIFabrica.Relleno(columna);
        RectTransform filaContinuar = UIFabrica.Fila(columna, "Continuar", 150);
        Button continuar = UIFabrica.Boton(filaContinuar, "Continuar", Paleta.Verde, Paleta.Blanco, 56, () => this.juego.IrAPrincipal());
        UIFabrica.Anclar((RectTransform)continuar.transform, 0, 0, 1, 1);
    }

    public void Preparar(ResultadoAccion r)
    {
        subtitulo.text = r.accion.nombre + " · " + Jornada.Duracion(r.minutos);
        switch (r.tramo)
        {
            case TramoDosis.Poco: tramo.text = "Poco tiempo: efecto proporcional"; tramo.color = Paleta.TextoSuave; break;
            case TramoDosis.ZonaOptima: tramo.text = "¡Dosis óptima!"; tramo.color = Paleta.Verde; break;
            case TramoDosis.Larga: tramo.text = "Algo larga: sin bonus"; tramo.color = Paleta.Texto; break;
            default: tramo.text = "Te has pasado"; tramo.color = Paleta.Coral; break;
        }

        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            int d = Mathf.RoundToInt(r.cambios[i]);
            if (d > 0) { cambios[i].text = "+" + d; cambios[i].color = Paleta.Verde; }
            else if (d < 0) { cambios[i].text = "−" + (-d); cambios[i].color = Paleta.Coral; }
            else { cambios[i].text = "0"; cambios[i].color = Paleta.TextoSuave; }
            nuevos[i].text = "ahora " + Mathf.RoundToInt(juego.Jornada.Estado[area]);
        }

        var sb = new StringBuilder();
        foreach (string nota in r.notas)
            if (nota != "Dosis óptima") sb.Append("• ").Append(nota).Append('\n');
        if (sb.Length == 0) sb.Append("• Sin efectos especiales esta vez.");
        notas.text = sb.ToString();
    }
}
