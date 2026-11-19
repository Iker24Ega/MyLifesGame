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
}
