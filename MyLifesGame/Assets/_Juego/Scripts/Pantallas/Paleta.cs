using MyLifesGame.Motor;
using UnityEngine;

/// <summary>Colores del juego (los del diseño: fondo crema, áreas con su color).</summary>
public static class Paleta
{
    public static readonly Color Fondo = Hex("F6EFE3");
    public static readonly Color Tarjeta = Hex("FFF9F0");
    public static readonly Color Texto = Hex("4A3426");
    public static readonly Color TextoSuave = Hex("8A7565");
    public static readonly Color Coral = Hex("E8604C");
    public static readonly Color Verde = Hex("5BAA6B");
    public static readonly Color Gris = Hex("D9CFC2");
    public static readonly Color Blanco = Color.white;

    // Tramos de las barras
    public static readonly Color TramoPoco = Hex("F3DE9A");
    public static readonly Color TramoBueno = Hex("9FD3A8");
    public static readonly Color TramoAlto = Hex("A9CBEA");
    public static readonly Color TramoLarga = Hex("C9E6C4");
    public static readonly Color TramoDemasiado = Hex("F2A36B");

    public static Color DeArea(Area area)
    {
        switch (area)
        {
            case Area.Fisico: return Hex("E8604C");
            case Area.Mente: return Hex("4A7BD0");
            case Area.Social: return Hex("F2B33D");
            case Area.Energia: return Hex("5BAA6B");
            default: return Hex("9B6BD1");
        }
    }

    public static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString("#" + hex, out c);
        return c;
    }
}
