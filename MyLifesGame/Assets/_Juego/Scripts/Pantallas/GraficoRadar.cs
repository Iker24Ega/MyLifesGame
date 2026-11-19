using UnityEngine;
using UnityEngine.UI;

/// <summary>Radar de cinco puntas: fondo, anillo verde de "zona buena" (30-80) y el polígono con tus valores.</summary>
public class GraficoRadar : MaskableGraphic
{
    private readonly float[] valores = { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
    private static readonly Color ColorFondo = new Color(0.91f, 0.86f, 0.78f, 1f);
    private static readonly Color ColorZonaBuena = new Color(0.62f, 0.83f, 0.66f, 0.85f);
    private static readonly Color ColorValores = new Color(0.91f, 0.38f, 0.30f, 0.45f);
    private static readonly Color ColorBorde = new Color(0.55f, 0.32f, 0.25f, 1f);

    public void PonerValores(float[] areas01)
    {
        for (int i = 0; i < 5; i++) valores[i] = Mathf.Clamp01(areas01[i]);
        SetVerticesDirty();
    }

    /// <summary>Posición (en coordenadas locales) de la punta i a una fracción r del radio.</summary>
    public Vector2 Punto(int i, float r)
    {
        float radio = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;
        float ang = Mathf.PI / 2f - i * 2f * Mathf.PI / 5f;
        return rectTransform.rect.center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radio * r;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Anillo(vh, 0f, 1f, ColorFondo);
        Anillo(vh, 0.3f, 0.8f, ColorZonaBuena);

        // Polígono de valores (abanico desde el centro)
        int centro = vh.currentVertCount;
        vh.AddVert(rectTransform.rect.center, ColorValores, Vector2.zero);
        for (int i = 0; i < 5; i++) vh.AddVert(Punto(i, valores[i]), ColorValores, Vector2.zero);
        for (int i = 0; i < 5; i++) vh.AddTriangle(centro, centro + 1 + i, centro + 1 + (i + 1) % 5);

        // Borde del polígono
        for (int i = 0; i < 5; i++) Linea(vh, Punto(i, valores[i]), Punto((i + 1) % 5, valores[(i + 1) % 5]), 5f, ColorBorde);
    }

    private void Anillo(VertexHelper vh, float r0, float r1, Color c)
    {
        for (int i = 0; i < 5; i++)
        {
            int j = (i + 1) % 5;
            int b = vh.currentVertCount;
            vh.AddVert(Punto(i, r0), c, Vector2.zero);
            vh.AddVert(Punto(i, r1), c, Vector2.zero);
            vh.AddVert(Punto(j, r1), c, Vector2.zero);
            vh.AddVert(Punto(j, r0), c, Vector2.zero);
            vh.AddTriangle(b, b + 1, b + 2);
            vh.AddTriangle(b, b + 2, b + 3);
        }
    }

    private static void Linea(VertexHelper vh, Vector2 a, Vector2 bP, float grosor, Color c)
    {
        Vector2 dir = (bP - a).normalized;
        Vector2 n = new Vector2(-dir.y, dir.x) * grosor * 0.5f;
        int b = vh.currentVertCount;
        vh.AddVert(a - n, c, Vector2.zero);
        vh.AddVert(a + n, c, Vector2.zero);
        vh.AddVert(bP + n, c, Vector2.zero);
        vh.AddVert(bP - n, c, Vector2.zero);
        vh.AddTriangle(b, b + 1, b + 2);
        vh.AddTriangle(b, b + 2, b + 3);
    }
}
