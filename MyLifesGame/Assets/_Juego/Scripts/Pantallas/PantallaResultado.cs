using System.Text;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Resultado de la acción: cambios por área y por qué, recompensas y la minipregunta "¿Qué tal ha ido?".</summary>
public class PantallaResultado : Pantalla
{
    private static readonly string[] MotivosPeor = { "Dormí poco", "Noche larga", "Me encuentro mal", "Estrés" };
    private static readonly string[] MotivosMejor = { "Muy concentrado", "Con energía", "Buen ambiente", "Descansado" };

    private readonly Text subtitulo, tramo, recompensas, notas, pregunta;
    private readonly Text[] cambios = new Text[5];
    private readonly Text[] nuevos = new Text[5];
    private readonly RectTransform filaValoracion, filaMotivos;
    private readonly Button[] botonesMotivo = new Button[5];
    private ResultadoAccion resultado;
    private int valoracionPendiente;

    public PantallaResultado(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla resultado")
    {
        TextoEnFila("Acción terminada", 100, 66, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        subtitulo = TextoEnFila("", 50, 38, Paleta.TextoSuave, TextAnchor.MiddleCenter);
        tramo = TextoEnFila("", 50, 38, Paleta.Verde, TextAnchor.MiddleCenter, FontStyle.Bold);

        RectTransform filaTarjeta = UIFabrica.Fila(columna, "Cambios", 470);
        Image tarjeta = Tarjeta(filaTarjeta);
        RectTransform col = UIFabrica.Columna(tarjeta.transform, "Columna", 26, 6);
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            RectTransform fila = UIFabrica.Fila(col, Areas.Nombre(area), 78);
            FormasUI ic = Iconos.Crear(fila, area, 62f);
            ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0, 0.5f);
            ic.rectTransform.anchoredPosition = new Vector2(34, 0);
            Text n = UIFabrica.Texto(fila, Areas.Nombre(area), 38, Paleta.Texto);
            UIFabrica.Anclar(n.rectTransform, 0.12f, 0, 0.45f, 1);
            cambios[i] = UIFabrica.Texto(fila, "", 44, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(cambios[i].rectTransform, 0.45f, 0, 0.7f, 1);
            nuevos[i] = UIFabrica.Texto(fila, "", 32, Paleta.TextoSuave, TextAnchor.MiddleRight);
            UIFabrica.Anclar(nuevos[i].rectTransform, 0.7f, 0, 1f, 1);
        }

        recompensas = TextoEnFila("", 50, 34, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        notas = TextoEnFila("", 230, 28, Paleta.Texto, TextAnchor.UpperLeft);

        // Minipregunta (9.1)
        pregunta = TextoEnFila("¿Qué tal ha ido?", 46, 32, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        filaValoracion = UIFabrica.Fila(columna, "Valoración", 84);
        string[] opciones = { "Peor de lo normal", "Normal", "Mejor de lo normal" };
        for (int i = 0; i < 3; i++)
        {
            int v = i - 1;
            Button b = UIFabrica.Boton(filaValoracion, opciones[i], Paleta.Tarjeta, Paleta.Texto, 28, () => Valorar(v));
            UIFabrica.Anclar((RectTransform)b.transform, i / 3f + 0.005f, 0, (i + 1) / 3f - 0.005f, 1);
        }
        filaMotivos = UIFabrica.Fila(columna, "Motivos", 84);
        for (int i = 0; i < 5; i++)
        {
            int indice = i;
            botonesMotivo[i] = UIFabrica.Boton(filaMotivos, "", Paleta.Gris, Paleta.Texto, 24, () => ElegirMotivo(indice));
            UIFabrica.Anclar((RectTransform)botonesMotivo[i].transform, i / 5f + 0.004f, 0, (i + 1) / 5f - 0.004f, 1);
        }

        UIFabrica.Relleno(columna);
        BotonEnFila("Continuar", 130, Paleta.Verde, Paleta.Blanco, 52, () => this.juego.IrAPestana(0));
    }

    public void Preparar(ResultadoAccion r, bool preguntar)
    {
        resultado = r;
        valoracionPendiente = 0;
        subtitulo.text = r.accion.nombre + " · " + Jornada.Duracion(r.minutos);
        switch (r.tramo)
        {
            case TramoDosis.Poco: tramo.text = "Poco tiempo: efecto proporcional"; tramo.color = Paleta.TextoSuave; break;
            case TramoDosis.ZonaOptima: tramo.text = r.posterior ? "En zona óptima (registrado después)" : "¡Dosis óptima!"; tramo.color = Paleta.Verde; break;
            case TramoDosis.Larga: tramo.text = "Algo larga: sin bonus"; tramo.color = Paleta.Texto; break;
            default: tramo.text = "Te has pasado"; tramo.color = Paleta.Coral; break;
        }
        recompensas.text = "+" + r.xp + " XP   ·   +" + r.monedas + " monedas";
        pregunta.text = "¿Qué tal ha ido?";
        pregunta.gameObject.transform.parent.gameObject.SetActive(preguntar);
        filaValoracion.gameObject.SetActive(preguntar);
        filaMotivos.gameObject.SetActive(false);
        Refrescar();
    }

    private void Refrescar()
    {
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            int d = Mathf.RoundToInt(resultado.cambios[i]);
            if (d > 0) { cambios[i].text = "+" + d; cambios[i].color = Paleta.Verde; }
            else if (d < 0) { cambios[i].text = "−" + (-d); cambios[i].color = Paleta.Coral; }
            else { cambios[i].text = "0"; cambios[i].color = Paleta.TextoSuave; }
            nuevos[i].text = "ahora " + Mathf.RoundToInt(juego.Jornada.Estado[area]);
        }

        var sb = new StringBuilder();
        foreach (string nota in resultado.notas)
            if (nota != "Dosis óptima") sb.Append("• ").Append(nota).Append('\n');
        foreach (string m in resultado.mensajes) sb.Append("<b>★ ").Append(m).Append("</b>\n");
        if (sb.Length == 0) sb.Append("• Sin efectos especiales esta vez.");
        notas.text = sb.ToString();
    }

    private void Valorar(int v)
    {
        if (v == 0)
        {
            filaValoracion.gameObject.SetActive(false);
            pregunta.text = "¿Qué tal ha ido? Normal";
            return;
        }
        valoracionPendiente = v;
        string[] motivos = v < 0 ? MotivosPeor : MotivosMejor;
        for (int i = 0; i < 4; i++) UIFabrica.CambiarTexto(botonesMotivo[i], motivos[i]);
        UIFabrica.CambiarTexto(botonesMotivo[4], "Sin motivo");
        filaValoracion.gameObject.SetActive(false);
        filaMotivos.gameObject.SetActive(true);
        pregunta.text = v < 0 ? "Peor de lo normal. ¿Por qué? (opcional)" : "Mejor de lo normal. ¿Por qué? (opcional)";
    }

    private void ElegirMotivo(int indice)
    {
        string motivo = "";
        if (indice < 4) motivo = (valoracionPendiente < 0 ? MotivosPeor : MotivosMejor)[indice];
        juego.Valorar(resultado, valoracionPendiente, motivo);
        filaMotivos.gameObject.SetActive(false);
        pregunta.text = "¡Gracias! Lo tengo en cuenta.";
        if (motivo == "Me encuentro mal") pregunta.text = "Cuídate. (El modo enfermo, que pausa todo, llegará pronto.)";
        Refrescar();
    }
}
