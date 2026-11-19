using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base de todas las pantallas: un fondo a pantalla completa con una columna de contenido
/// y, si se pide, la barra de navegación inferior (Ciudad · Hoy · Hacer · Avatar · Álbum).
/// </summary>
public abstract class Pantalla
{
    public const float AltoBarra = 170f;

    protected readonly Juego juego;
    protected readonly RectTransform columna;
    protected readonly RectTransform raizRT;
    private readonly GameObject raiz;
    private readonly Aparecer aparecer;

    /// <param name="pestana">Índice de la pestaña resaltada (0-4), o -1 para no mostrar barra.</param>
    protected Pantalla(Juego juego, Transform canvas, string nombre, int pestana = -1, bool transparente = false)
    {
        this.juego = juego;
        Image fondo = UIFabrica.Caja(canvas, nombre, transparente ? new Color(0, 0, 0, 0) : Paleta.Fondo, false);
        fondo.raycastTarget = !transparente;
        UIFabrica.Anclar(fondo.rectTransform, 0, 0, 1, 1);
        raiz = fondo.gameObject;
        raizRT = fondo.rectTransform;
        raiz.AddComponent<CanvasGroup>();
        aparecer = raiz.AddComponent<Aparecer>();
        columna = UIFabrica.Columna(fondo.transform, "Columna", 48, 22);
        if (pestana >= 0)
        {
            columna.offsetMin = new Vector2(0, AltoBarra);
            BarraNavegacion.Crear(fondo.transform, juego, pestana);
        }
    }

    public void Mostrar() { raiz.SetActive(true); aparecer.Lanzar(); AlMostrar(); }
    public void Ocultar() { raiz.SetActive(false); }
    public bool Visible { get { return raiz.activeSelf; } }
    public virtual void Actualizar() { }
    protected virtual void AlMostrar() { }

    protected Text TextoEnFila(string texto, float alto, int tam, Color color,
        TextAnchor alineacion = TextAnchor.MiddleLeft, FontStyle estilo = FontStyle.Normal)
    {
        RectTransform fila = UIFabrica.Fila(columna, "Fila", alto);
        Text t = UIFabrica.Texto(fila, texto, tam, color, alineacion, estilo);
        UIFabrica.Anclar(t.rectTransform, 0, 0, 1, 1);
        return t;
    }

    protected Image Tarjeta(Transform padre)
    {
        Image fondo = UIFabrica.Caja(padre, "Tarjeta", Paleta.Tarjeta);
        UIFabrica.Anclar(fondo.rectTransform, 0, 0, 1, 1);
        UIFabrica.Sombra(fondo);
        return fondo;
    }

    protected Button BotonEnFila(string texto, float alto, Color fondo, Color colorTexto, int tam, System.Action alPulsar)
    {
        RectTransform fila = UIFabrica.Fila(columna, texto, alto);
        Button b = UIFabrica.Boton(fila, texto, fondo, colorTexto, tam, alPulsar);
        UIFabrica.Anclar((RectTransform)b.transform, 0, 0, 1, 1);
        return b;
    }
}
