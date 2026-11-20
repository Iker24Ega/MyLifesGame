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
        columna.gameObject.AddComponent<ZonaSegura>();
    }

    /// <summary>
    /// Convierte la pantalla en una hoja sobre la ciudad 3D (como en las imágenes de referencia):
    /// fondo transparente con un velo suave y una tarjeta grande que empieza en "arriba" (0-1 del alto).
    /// La pantalla debe crearse con transparente = true.
    /// </summary>
    protected void ComoHoja(float arriba, float velo = 0.22f)
    {
        Image capa = UIFabrica.Caja(raizRT, "Velo", new Color(0.2f, 0.14f, 0.1f, velo), false);
        UIFabrica.Anclar(capa.rectTransform, 0, 0, 1, 1);
        capa.raycastTarget = true;
        capa.transform.SetSiblingIndex(0);
        Image hoja = UIFabrica.Caja(raizRT, "Hoja", Paleta.Fondo);
        UIFabrica.Anclar(hoja.rectTransform, 0.025f, 0.015f, 0.975f, arriba);
        UIFabrica.Sombra(hoja, 14f, 0.25f);
        hoja.transform.SetSiblingIndex(1);
        Image asa = UIFabrica.Caja(hoja.transform, "Asa", Paleta.Gris);
        asa.rectTransform.anchorMin = asa.rectTransform.anchorMax = new Vector2(0.5f, 1);
        asa.rectTransform.sizeDelta = new Vector2(120, 12);
        asa.rectTransform.anchoredPosition = new Vector2(0, -24);
        columna.anchorMax = new Vector2(1, arriba);
        columna.offsetMax = new Vector2(-30, -30);
        columna.offsetMin = new Vector2(30, columna.offsetMin.y + 20);
    }

    /// <summary>Si es true, la pantalla usa la cámara de escena 3D del estudio (y no se apaga al mostrarla).</summary>
    public bool UsaEscena { get; protected set; }

    /// <summary>Sala 3D a pantalla completa detrás de todo (dormitorio, gimnasio, vestidor...).</summary>
    protected RawImage FondoEscena()
    {
        UsaEscena = true;
        var img = new GameObject("Sala 3D", typeof(RectTransform)).AddComponent<RawImage>();
        img.transform.SetParent(raizRT, false);
        img.transform.SetAsFirstSibling();
        img.texture = EstudioAvatar.TexturaEscena;
        img.raycastTarget = false;
        UIFabrica.Anclar(img.rectTransform, 0, 0, 1, 1);
        return img;
    }

    /// <summary>Recorta la textura de la sala para que llene la pantalla sin deformarse.</summary>
    protected void AjustarFondoEscena(RawImage img)
    {
        Rect rc = raizRT.rect;
        if (rc.height < 1f || img == null) return;
        float aspecto = rc.width / rc.height;
        float tex = (float)EstudioAvatar.TexturaEscena.width / EstudioAvatar.TexturaEscena.height;
        img.uvRect = aspecto < tex
            ? new Rect((1f - aspecto / tex) * 0.5f, 0, aspecto / tex, 1)
            : new Rect(0, (1f - tex / aspecto) * 0.5f, 1, tex / aspecto);
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
