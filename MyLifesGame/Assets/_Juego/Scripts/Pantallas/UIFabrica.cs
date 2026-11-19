using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Ayudas para crear la interfaz desde código (sin montar nada en la escena).</summary>
public static class UIFabrica
{
    private static Font fuente;
    private static Sprite redondeado;
    private static Sprite circulo;

    public static Font Fuente
    {
        get { if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return fuente; }
    }

    public static Sprite Redondeado
    {
        get { if (redondeado == null) redondeado = CrearSpriteRedondeado(64, 24); return redondeado; }
    }

    public static Sprite Circulo
    {
        get { if (circulo == null) circulo = CrearSpriteRedondeado(64, 32); return circulo; }
    }

    // Genera una textura con esquinas redondeadas y la convierte en un sprite "9-slice".
    private static Sprite CrearSpriteRedondeado(int tam, int radio)
    {
        var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        var pixeles = new Color32[tam * tam];
        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float px = x + 0.5f, py = y + 0.5f;
                float cx = Mathf.Clamp(px, radio, tam - radio);
                float cy = Mathf.Clamp(py, radio, tam - radio);
                float d = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                byte a = (byte)(Mathf.Clamp01(radio - d + 0.5f) * 255);
                pixeles[y * tam + x] = new Color32(255, 255, 255, a);
            }
        }
        tex.SetPixels32(pixeles);
        tex.Apply();
        float borde = Mathf.Min(radio, tam / 2 - 1);
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(borde, borde, borde, borde));
    }

    public static RectTransform Crear(string nombre, Transform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return (RectTransform)go.transform;
    }

    /// <summary>Coloca un elemento en proporciones de su padre (0 a 1).</summary>
    public static void Anclar(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static Image Caja(Transform padre, string nombre, Color color, bool redondeada = true)
    {
        RectTransform rt = Crear(nombre, padre);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        if (redondeada)
        {
            img.sprite = Redondeado;
            img.type = Image.Type.Sliced;
        }
        return img;
    }

    public static Text Texto(Transform padre, string texto, int tam, Color color,
        TextAnchor alineacion = TextAnchor.MiddleLeft, FontStyle estilo = FontStyle.Normal)
    {
        RectTransform rt = Crear("Texto", padre);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = Fuente;
        t.text = texto;
        t.fontSize = tam;
        t.color = color;
        t.alignment = alineacion;
        t.fontStyle = estilo;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static Button Boton(Transform padre, string texto, Color fondo, Color colorTexto, int tam, Action alPulsar)
    {
        Image img = Caja(padre, "Botón " + texto, fondo);
        var b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(() => alPulsar());
        Text t = Texto(img.transform, texto, tam, colorTexto, TextAnchor.MiddleCenter, FontStyle.Bold);
        Anclar(t.rectTransform, 0.03f, 0f, 0.97f, 1f);
        return b;
    }

    public static void CambiarTexto(Button boton, string texto)
    {
        boton.GetComponentInChildren<Text>().text = texto;
    }

    /// <summary>Contenedor que apila a sus hijos de arriba abajo.</summary>
    public static RectTransform Columna(Transform padre, string nombre, int margen, int separacion)
    {
        RectTransform rt = Crear(nombre, padre);
        Anclar(rt, 0, 0, 1, 1);
        var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(margen, margen, margen, margen);
        v.spacing = separacion;
        v.childControlHeight = true;
        v.childControlWidth = true;
        v.childForceExpandHeight = false;
        v.childForceExpandWidth = true;
        return rt;
    }

    /// <summary>Hueco de una altura fija dentro de una columna.</summary>
    public static RectTransform Fila(Transform columna, string nombre, float alto)
    {
        RectTransform rt = Crear(nombre, columna);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = alto;
        le.minHeight = alto;
        return rt;
    }

    /// <summary>Hueco que ocupa todo el espacio libre de la columna.</summary>
    public static RectTransform Relleno(Transform columna)
    {
        RectTransform rt = Crear("Relleno", columna);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.flexibleHeight = 1;
        return rt;
    }
}
