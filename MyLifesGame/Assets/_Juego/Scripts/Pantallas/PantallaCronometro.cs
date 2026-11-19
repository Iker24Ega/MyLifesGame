using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Acción en curso: cronómetro y barra de dosis (Poco / Zona óptima / Demasiado).</summary>
public class PantallaCronometro : Pantalla
{
    private readonly Text nombre, lugar, tiempo, estado, info;
    private readonly RectTransform filaBarra;
    private BarraTramos barra;
    private float escala = 1f;

    public PantallaCronometro(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla cronómetro")
    {
        nombre = TextoEnFila("", 110, 72, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        lugar = TextoEnFila("", 60, 36, Paleta.TextoSuave, TextAnchor.MiddleCenter);
        UIFabrica.Fila(columna, "Hueco", 60);
        tiempo = TextoEnFila("", 220, 150, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        estado = TextoEnFila("", 60, 40, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        filaBarra = UIFabrica.Fila(columna, "Barra de dosis", 110);

        RectTransform etiquetas = UIFabrica.Fila(columna, "Etiquetas", 50);
        Text poco = UIFabrica.Texto(etiquetas, "Poco", 28, Paleta.TextoSuave, TextAnchor.MiddleLeft);
        Text optima = UIFabrica.Texto(etiquetas, "Zona óptima", 28, Paleta.TextoSuave, TextAnchor.MiddleCenter);
        Text demasiado = UIFabrica.Texto(etiquetas, "Demasiado", 28, Paleta.TextoSuave, TextAnchor.MiddleRight);
        UIFabrica.Anclar(poco.rectTransform, 0, 0, 0.33f, 1);
        UIFabrica.Anclar(optima.rectTransform, 0.33f, 0, 0.66f, 1);
        UIFabrica.Anclar(demasiado.rectTransform, 0.66f, 0, 1, 1);

        info = TextoEnFila("", 60, 30, Paleta.TextoSuave, TextAnchor.MiddleCenter);
        UIFabrica.Relleno(columna);

        RectTransform filaTerminar = UIFabrica.Fila(columna, "Terminar", 150);
        Button terminar = UIFabrica.Boton(filaTerminar, "Terminar", Paleta.Coral, Paleta.Blanco, 56, () => this.juego.TerminarAccion());
        UIFabrica.Anclar((RectTransform)terminar.transform, 0, 0, 1, 1);
        RectTransform filaCancelar = UIFabrica.Fila(columna, "Cancelar", 100);
        Button cancelar = UIFabrica.Boton(filaCancelar, "Cancelar (no cuenta)", Paleta.Gris, Paleta.Texto, 34, () => this.juego.CancelarAccion());
        UIFabrica.Anclar((RectTransform)cancelar.transform, 0, 0, 1, 1);
    }

    /// <summary>Prepara la pantalla para una acción concreta (la barra depende de sus tramos).</summary>
    public void Preparar(DefinicionAccion a)
    {
        nombre.text = a.nombre;
        lugar.text = a.lugar;
        info.text = "Zona óptima: " + PantallaHacer.Rango(a) + " · Demasiado desde " + Jornada.Duracion(a.demasiadoDesde);

        escala = a.demasiadoDesde * 1.4f;
        if (barra != null) Object.Destroy(barra.Raiz.gameObject);
        barra = new BarraTramos(filaBarra,
            new[] { a.zonaMin / escala, a.zonaMax / escala, a.demasiadoDesde / escala, 1f },
            new[] { Paleta.TramoPoco, Paleta.TramoBueno, Paleta.TramoLarga, Paleta.TramoDemasiado });
        UIFabrica.Anclar(barra.Raiz, 0, 0, 1, 1);
    }

    public override void Actualizar()
    {
        DefinicionAccion a = juego.AccionEnCurso;
        if (a == null) return;

        float segundos = (juego.MinutoJuego - juego.InicioAccion) * 60f;
        int total = Mathf.FloorToInt(segundos);
        tiempo.text = (total / 3600).ToString("00") + ":" + (total / 60 % 60).ToString("00") + ":" + (total % 60).ToString("00");

        int minutos = juego.MinutosAccion;
        barra.Valor(minutos / escala);
        switch (a.Tramo(minutos))
        {
            case TramoDosis.Poco: estado.text = "Poco"; estado.color = Paleta.TextoSuave; break;
            case TramoDosis.ZonaOptima: estado.text = "Zona óptima"; estado.color = Paleta.Verde; break;
            case TramoDosis.Larga: estado.text = "Algo larga (sin bonus)"; estado.color = Paleta.Texto; break;
            default: estado.text = "Demasiado: cansa más de lo que aporta"; estado.color = Paleta.Coral; break;
        }
    }
}
