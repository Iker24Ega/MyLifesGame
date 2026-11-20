using MyLifesGame.Motor;
using UnityEngine;

/// <summary>Iconos oficiales de las cinco áreas: pesa, libro, burbujas, rayo y cara sonriente (decisión 5.14).</summary>
public static class Iconos
{
    /// <summary>Crea un icono redondo de área dentro de "padre", de tamaño "tam" píxeles.</summary>
    public static FormasUI Crear(Transform padre, Area area, float tam, bool conFondo = true)
    {
        RectTransform rt = UIFabrica.Crear("Icono " + Areas.Nombre(area), padre);
        rt.sizeDelta = new Vector2(tam, tam);
        var f = FormasUI.En(rt.gameObject);
        f.raycastTarget = false;
        Dibujar(f, area, conFondo);
        return f;
    }

    public static void Dibujar(FormasUI f, Area area, bool conFondo)
    {
        f.Limpiar();
        Color fondo = Paleta.DeArea(area);
        Color trazo = conFondo ? Color.white : fondo;
        if (conFondo) f.Circulo(Vector2.zero, 1f, fondo);
        DibujarTrazo(f, area, trazo, conFondo);
    }

    private static void DibujarTrazo(FormasUI f, Area area, Color trazo, bool conFondo = true)
    {
        switch (area)
        {
            case Area.Fisico:
                f.Rect(Vector2.zero, new Vector2(1.0f, 0.14f), 0f, trazo);
                f.Rect(new Vector2(-0.4f, 0), new Vector2(0.16f, 0.66f), 0f, trazo);
                f.Rect(new Vector2(0.4f, 0), new Vector2(0.16f, 0.66f), 0f, trazo);
                f.Rect(new Vector2(-0.58f, 0), new Vector2(0.12f, 0.42f), 0f, trazo);
                f.Rect(new Vector2(0.58f, 0), new Vector2(0.12f, 0.42f), 0f, trazo);
                break;
            case Area.Mente:
                f.Poligono(new[] { new Vector2(-0.62f, -0.36f), new Vector2(-0.62f, 0.44f), new Vector2(-0.05f, 0.34f), new Vector2(-0.05f, -0.46f) }, trazo);
                f.Poligono(new[] { new Vector2(0.05f, -0.46f), new Vector2(0.05f, 0.34f), new Vector2(0.62f, 0.44f), new Vector2(0.62f, -0.36f) }, trazo);
                break;
            case Area.Social:
                f.Circulo(new Vector2(-0.18f, 0.14f), 0.38f, trazo);
                f.Poligono(new[] { new Vector2(-0.42f, -0.12f), new Vector2(-0.5f, -0.42f), new Vector2(-0.2f, -0.2f) }, trazo);
                f.Circulo(new Vector2(0.3f, -0.12f), 0.3f, conFondo ? new Color(1, 1, 1, 0.8f) : trazo);
                f.Poligono(new[] { new Vector2(0.42f, -0.34f), new Vector2(0.56f, -0.52f), new Vector2(0.28f, -0.4f) }, conFondo ? new Color(1, 1, 1, 0.8f) : trazo);
                break;
            case Area.Energia:
                f.Poligono(new[] { new Vector2(0.16f, 0.66f), new Vector2(-0.34f, -0.06f), new Vector2(0.08f, -0.06f) }, trazo);
                f.Poligono(new[] { new Vector2(-0.08f, 0.06f), new Vector2(0.34f, 0.06f), new Vector2(-0.16f, -0.66f) }, trazo);
                break;
            default:
                f.Arco(Vector2.zero, 0.52f, 0.66f, 0f, 360f, trazo);
                f.Circulo(new Vector2(-0.2f, 0.14f), 0.09f, trazo);
                f.Circulo(new Vector2(0.2f, 0.14f), 0.09f, trazo);
                f.Arco(new Vector2(0, 0.02f), 0.24f, 0.34f, 120f, 240f, trazo);
                break;
        }
    }

    /// <summary>Área principal de cada acción, para su icono.</summary>
    public static Area AreaDeAccion(string id)
    {
        switch (id)
        {
            case "entrenar": return Area.Fisico;
            case "estudiar": case "trabajar": return Area.Mente;
            case "quedar": return Area.Social;
            case "pasear": case "descansar": return Area.Energia;
            default: return Area.Animo;
        }
    }

    /// <summary>Carita de 0 (fatal) a 4 (genial) sobre un círculo de color.</summary>
    public static void Carita(FormasUI f, int nivel, Color fondo, bool dormida = false)
    {
        f.Limpiar();
        f.Circulo(Vector2.zero, 1f, fondo);
        Color trazo = Paleta.Texto;
        if (dormida)
        {
            f.Arco(new Vector2(-0.32f, 0.22f), 0.1f, 0.18f, 100f, 260f, trazo);
            f.Arco(new Vector2(0.32f, 0.22f), 0.1f, 0.18f, 100f, 260f, trazo);
        }
        else
        {
            f.Circulo(new Vector2(-0.32f, 0.2f), 0.11f, trazo);
            f.Circulo(new Vector2(0.32f, 0.2f), 0.11f, trazo);
        }
        switch (nivel)
        {
            case 0: f.Arco(new Vector2(0, -0.62f), 0.32f, 0.42f, -60f, 60f, trazo); break;
            case 1: f.Arco(new Vector2(0, -0.78f), 0.4f, 0.49f, -40f, 40f, trazo); break;
            case 2: f.Rect(new Vector2(0, -0.32f), new Vector2(0.5f, 0.1f), 0f, trazo); break;
            case 3: f.Arco(new Vector2(0, 0.02f), 0.38f, 0.47f, 130f, 230f, trazo); break;
            default:
                f.Arco(new Vector2(0, -0.08f), 0f, 0.46f, 90f, 270f, trazo);
                f.Arco(new Vector2(0, -0.08f), 0f, 0.3f, 120f, 240f, Paleta.Coral);
                break;
        }
        if (nivel >= 3)
        {
            Color mejilla = new Color(1f, 0.55f, 0.5f, 0.45f);
            f.Circulo(new Vector2(-0.6f, -0.12f), 0.13f, mejilla);
            f.Circulo(new Vector2(0.6f, -0.12f), 0.13f, mejilla);
        }
    }

    /// <summary>Sol de mañana con rayos.</summary>
    public static void Sol(FormasUI f)
    {
        f.Limpiar();
        Color rayo = Paleta.Hex("F6C453");
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f * Mathf.Deg2Rad;
            Vector2 d = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
            f.Linea(d * 0.66f, d * 0.95f, 0.1f, rayo);
        }
        f.Circulo(Vector2.zero, 0.55f, Paleta.Hex("FFD66B"));
        f.Circulo(new Vector2(-0.18f, 0.06f), 0.06f, Paleta.Texto);
        f.Circulo(new Vector2(0.18f, 0.06f), 0.06f, Paleta.Texto);
        f.Arco(new Vector2(0, -0.04f), 0.18f, 0.24f, 120f, 240f, Paleta.Texto);
    }

    /// <summary>Sudadera con capucha del color indicado (para la tienda).</summary>
    public static void Sudadera(FormasUI f, Color c, bool bloqueada)
    {
        f.Limpiar();
        if (bloqueada) c = Color.Lerp(c, Paleta.Gris, 0.75f);
        Color osc = Color.Lerp(c, Color.black, 0.18f);
        f.Rect(new Vector2(-0.55f, -0.12f), new Vector2(0.3f, 0.92f), -18f, osc);
        f.Rect(new Vector2(0.55f, -0.12f), new Vector2(0.3f, 0.92f), 18f, osc);
        f.Circulo(new Vector2(0, 0.42f), 0.36f, osc);
        f.Rect(new Vector2(0, -0.12f), new Vector2(0.92f, 1.0f), 0f, c);
        f.Circulo(new Vector2(0, 0.38f), 0.2f, Color.Lerp(osc, Color.black, 0.25f));
        f.Rect(new Vector2(0, -0.46f), new Vector2(0.56f, 0.22f), 0f, osc);
        f.Linea(new Vector2(-0.1f, 0.24f), new Vector2(-0.12f, -0.06f), 0.05f, Paleta.Blanco);
        f.Linea(new Vector2(0.1f, 0.24f), new Vector2(0.12f, -0.06f), 0.05f, Paleta.Blanco);
        f.Rect(new Vector2(0, -0.6f), new Vector2(0.92f, 0.08f), 0f, osc);
    }

    /// <summary>Medalla de logro: con el icono del área si está conseguido; con candado si no.</summary>
    public static void Medalla(FormasUI f, Area area, bool conseguido)
    {
        f.Limpiar();
        if (!conseguido)
        {
            f.Circulo(Vector2.zero, 1f, Paleta.Hex("DDD3C5"));
            f.Circulo(Vector2.zero, 0.8f, Paleta.Hex("E9E1D5"));
            Color c = Paleta.Hex("B3A694");
            f.Arco(new Vector2(0, 0.12f), 0.2f, 0.32f, -90f, 90f, c);
            f.Rect(new Vector2(-0.26f, 0.06f), new Vector2(0.12f, 0.14f), 0f, c);
            f.Rect(new Vector2(0.26f, 0.06f), new Vector2(0.12f, 0.14f), 0f, c);
            f.Rect(new Vector2(0, -0.22f), new Vector2(0.72f, 0.5f), 0f, c);
            f.Circulo(new Vector2(0, -0.2f), 0.08f, Paleta.Hex("E9E1D5"));
            return;
        }
        Color col = Paleta.DeArea(area);
        // Cintas
        f.Rect(new Vector2(-0.3f, -0.75f), new Vector2(0.3f, 0.7f), 18f, Color.Lerp(col, Color.black, 0.2f));
        f.Rect(new Vector2(0.3f, -0.75f), new Vector2(0.3f, 0.7f), -18f, Color.Lerp(col, Color.black, 0.2f));
        f.Circulo(Vector2.zero, 1f, Paleta.Hex("E3B341"));
        f.Circulo(Vector2.zero, 0.86f, Paleta.Hex("F6D26B"));
        f.Circulo(Vector2.zero, 0.72f, col);
        // Icono del área encima, a menor escala
        IconoEscalado(f, area, 0.62f);
        f.Arco(Vector2.zero, 0.62f, 0.72f, 300f, 340f, new Color(1, 1, 1, 0.35f));
    }

    private static void IconoEscalado(FormasUI f, Area area, float k)
    {
        // Reutiliza el dibujo del icono en un FormasUI auxiliar escalando coordenadas.
        f.Escala = k;
        DibujarTrazo(f, area, Color.white);
        f.Escala = 1f;
    }

    /// <summary>Mano que señala hacia abajo (para guiar al jugador), con borde oscuro.</summary>
    public static void Mano(FormasUI f)
    {
        f.Limpiar();
        Color borde = Paleta.Texto, piel = Paleta.Hex("FFE2C6");
        for (int capa = 0; capa < 2; capa++)
        {
            Color c = capa == 0 ? borde : piel;
            float g = capa == 0 ? 0.08f : 0f;
            f.Rect(new Vector2(-0.05f, -0.42f), new Vector2(0.28f + g, 0.8f + g), 0f, c);          // índice
            f.Circulo(new Vector2(-0.05f, -0.82f), 0.14f + g * 0.5f, c);                               // yema
            f.Rect(new Vector2(0.18f, 0.22f), new Vector2(0.78f + g, 0.62f + g), 0f, c);           // palma
            f.Circulo(new Vector2(0.18f, 0.52f), 0.39f + g * 0.5f, c);
            f.Rect(new Vector2(-0.32f, 0.12f), new Vector2(0.22f + g, 0.48f + g), -28f, c);       // pulgar
            f.Rect(new Vector2(0.18f, 0.86f), new Vector2(0.62f + g, 0.24f + g), 0f, capa == 0 ? borde : Paleta.Coral); // puño de la manga
        }
        f.Linea(new Vector2(0.15f, 0.0f), new Vector2(0.15f, 0.28f), 0.04f, new Color(0.29f, 0.2f, 0.15f, 0.35f));
        f.Linea(new Vector2(0.38f, 0.0f), new Vector2(0.38f, 0.28f), 0.04f, new Color(0.29f, 0.2f, 0.15f, 0.35f));
    }
}
