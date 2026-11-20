using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gráfico de interfaz que dibuja formas simples (círculos, anillos, arcos, rectángulos, polígonos, líneas)
/// en coordenadas de -1 a 1 dentro de su rectángulo. Se usa para iconos, el radar y los anillos de progreso.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class FormasUI : MaskableGraphic
{
    private enum Tipo { Circulo, Arco, Rect, Poligono, Linea }

    private struct Forma
    {
        public Tipo tipo;
        public Vector2 a, b;
        public float r0, r1, ang0, ang1, rot;
        public Color color;
        public Vector2[] puntos;
        public bool abanicoDesdeA;
    }

    private readonly List<Forma> formas = new List<Forma>();

    /// <summary>Añade un FormasUI (con su CanvasRenderer) al objeto.</summary>
    public static FormasUI En(GameObject go)
    {
        if (go.GetComponent<CanvasRenderer>() == null) go.AddComponent<CanvasRenderer>();
        return go.AddComponent<FormasUI>();
    }

    /// <summary>Escala que se aplica a las formas que se añadan a partir de ahora (alrededor del centro).</summary>
    public float Escala = 1f;

    public int NumFormas { get { return formas.Count; } }
    public int UltimosVertices { get; private set; }

    public void Limpiar() { formas.Clear(); SetVerticesDirty(); }

    public void Circulo(Vector2 c, float r, Color col)
    {
        formas.Add(new Forma { tipo = Tipo.Circulo, a = c * Escala, r1 = r * Escala, color = col }); SetVerticesDirty();
    }

    /// <summary>Anillo o arco entre los radios r0 y r1. Ángulos en grados, en el sentido del reloj desde arriba.</summary>
    public void Arco(Vector2 c, float r0, float r1, float ang0, float ang1, Color col)
    {
        formas.Add(new Forma { tipo = Tipo.Arco, a = c * Escala, r0 = r0 * Escala, r1 = r1 * Escala, ang0 = ang0, ang1 = ang1, color = col }); SetVerticesDirty();
    }

    public void Rect(Vector2 c, Vector2 tam, float rotGrados, Color col)
    {
        formas.Add(new Forma { tipo = Tipo.Rect, a = c * Escala, b = tam * Escala, rot = rotGrados, color = col }); SetVerticesDirty();
    }

    /// <summary>Polígono convexo (o en estrella respecto a "centro" si se indica).</summary>
    public void Poligono(Vector2[] puntos, Color col, Vector2? centro = null)
    {
        if (!Mathf.Approximately(Escala, 1f))
        {
            var p = new Vector2[puntos.Length];
            for (int i = 0; i < p.Length; i++) p[i] = puntos[i] * Escala;
            puntos = p;
        }
        formas.Add(new Forma { tipo = Tipo.Poligono, puntos = puntos, color = col, abanicoDesdeA = centro.HasValue, a = (centro ?? Vector2.zero) * Escala });
        SetVerticesDirty();
    }

    public void Linea(Vector2 desde, Vector2 hasta, float grosor, Color col)
    {
        formas.Add(new Forma { tipo = Tipo.Linea, a = desde * Escala, b = hasta * Escala, r1 = grosor * Escala, color = col }); SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        float esc = Mathf.Min(r.width, r.height) * 0.5f;
        Vector2 o = r.center;
        foreach (Forma f in formas)
        {
            switch (f.tipo)
            {
                case Tipo.Circulo: Abanico(vh, o + f.a * esc, 0f, f.r1 * esc, 0f, 360f, f.color); break;
                case Tipo.Arco: Abanico(vh, o + f.a * esc, f.r0 * esc, f.r1 * esc, f.ang0, f.ang1, f.color); break;
                case Tipo.Rect:
                {
                    Vector2 h = f.b * 0.5f;
                    Quaternion q = Quaternion.Euler(0, 0, -f.rot);
                    var p = new Vector2[4];
                    p[0] = (Vector2)(q * new Vector3(-h.x, -h.y)); p[1] = (Vector2)(q * new Vector3(-h.x, h.y));
                    p[2] = (Vector2)(q * new Vector3(h.x, h.y)); p[3] = (Vector2)(q * new Vector3(h.x, -h.y));
                    Quad(vh, o + (f.a + p[0]) * esc, o + (f.a + p[1]) * esc, o + (f.a + p[2]) * esc, o + (f.a + p[3]) * esc, f.color);
                    break;
                }
                case Tipo.Poligono:
                {
                    int b = vh.currentVertCount;
                    if (f.abanicoDesdeA)
                    {
                        vh.AddVert(o + f.a * esc, f.color, Vector2.zero);
                        foreach (Vector2 pt in f.puntos) vh.AddVert(o + pt * esc, f.color, Vector2.zero);
                        int n = f.puntos.Length;
                        for (int i = 0; i < n; i++) vh.AddTriangle(b, b + 1 + i, b + 1 + (i + 1) % n);
                    }
                    else
                    {
                        foreach (Vector2 pt in f.puntos) vh.AddVert(o + pt * esc, f.color, Vector2.zero);
                        for (int i = 1; i < f.puntos.Length - 1; i++) vh.AddTriangle(b, b + i, b + i + 1);
                    }
                    break;
                }
                case Tipo.Linea:
                {
                    Vector2 a = o + f.a * esc, c = o + f.b * esc;
                    Vector2 dir = (c - a).normalized;
                    Vector2 n = new Vector2(-dir.y, dir.x) * f.r1 * esc * 0.5f;
                    Quad(vh, a - n, a + n, c + n, c - n, f.color);
                    break;
                }
            }
        }
        UltimosVertices = vh.currentVertCount;
    }

    private static void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color col)
    {
        int i = vh.currentVertCount;
        vh.AddVert(a, col, Vector2.zero); vh.AddVert(b, col, Vector2.zero);
        vh.AddVert(c, col, Vector2.zero); vh.AddVert(d, col, Vector2.zero);
        vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
    }

    private static void Abanico(VertexHelper vh, Vector2 c, float r0, float r1, float ang0, float ang1, Color col)
    {
        int pasos = Mathf.Max(6, Mathf.CeilToInt(Mathf.Abs(ang1 - ang0) / 6f));
        for (int i = 0; i < pasos; i++)
        {
            float a0 = Mathf.Deg2Rad * Mathf.Lerp(ang0, ang1, i / (float)pasos);
            float a1 = Mathf.Deg2Rad * Mathf.Lerp(ang0, ang1, (i + 1) / (float)pasos);
            Vector2 d0 = new Vector2(Mathf.Sin(a0), Mathf.Cos(a0));
            Vector2 d1 = new Vector2(Mathf.Sin(a1), Mathf.Cos(a1));
            Quad(vh, c + d0 * r0, c + d0 * r1, c + d1 * r1, c + d1 * r0, col);
        }
    }
}
