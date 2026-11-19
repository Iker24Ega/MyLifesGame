using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra horizontal dividida en tramos de colores (Poco / Zona buena / Alto, o la barra de dosis)
/// con un marcador redondo que indica el valor actual.
/// </summary>
public class BarraTramos
{
    public RectTransform Raiz { get; private set; }
    private readonly RectTransform marcador;

    /// <param name="cortes">Fin de cada tramo, de 0 a 1 (el último debe ser 1).</param>
    public BarraTramos(Transform padre, float[] cortes, Color[] colores)
    {
        Raiz = UIFabrica.Crear("Barra", padre);
        float inicio = 0f;
        for (int i = 0; i < cortes.Length; i++)
        {
            Image tramo = UIFabrica.Caja(Raiz, "Tramo " + i, colores[i], false);
            UIFabrica.Anclar(tramo.rectTransform, inicio, 0.3f, cortes[i], 0.7f);
            inicio = cortes[i];
        }
        Image m = UIFabrica.Caja(Raiz, "Marcador", Paleta.Blanco);
        m.sprite = UIFabrica.Circulo;
        m.type = Image.Type.Simple;
        var borde = m.gameObject.AddComponent<Outline>();
        borde.effectColor = Paleta.Texto;
        borde.effectDistance = new Vector2(3, -3);
        marcador = m.rectTransform;
        marcador.sizeDelta = new Vector2(44, 44);
        Valor(0f);
    }

    /// <summary>Mueve el marcador (0 = izquierda, 1 = derecha).</summary>
    public void Valor(float t)
    {
        float x = Mathf.Clamp01(t);
        marcador.anchorMin = new Vector2(x, 0.5f);
        marcador.anchorMax = new Vector2(x, 0.5f);
        marcador.anchoredPosition = Vector2.zero;
    }
}
