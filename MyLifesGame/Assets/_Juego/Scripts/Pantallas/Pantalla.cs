using UnityEngine;
using UnityEngine.UI;

/// <summary>Base de todas las pantallas: un fondo a pantalla completa con una columna de contenido.</summary>
public abstract class Pantalla
{
    protected readonly Juego juego;
    protected readonly RectTransform columna;
    private readonly GameObject raiz;

    protected Pantalla(Juego juego, Transform canvas, string nombre)
    {
        this.juego = juego;
        Image fondo = UIFabrica.Caja(canvas, nombre, Paleta.Fondo, false);
        UIFabrica.Anclar(fondo.rectTransform, 0, 0, 1, 1);
        raiz = fondo.gameObject;
        columna = UIFabrica.Columna(fondo.transform, "Columna", 56, 24);
    }

    public void Mostrar() { raiz.SetActive(true); }
    public void Ocultar() { raiz.SetActive(false); }
    public virtual void Actualizar() { }

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
        return fondo;
    }
}
