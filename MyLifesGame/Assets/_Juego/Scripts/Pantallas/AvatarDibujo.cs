using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dibujo 2D sencillo del avatar (cabeza, pelo, ojos, sudadera, mochila) que cambia de
/// postura y ambiente según su estado. El cuerpo nunca cambia (regla de diseño 5.6).
/// </summary>
public class AvatarDibujo
{
    private readonly Image fondo, cuerpo, adorno;
    private readonly RectTransform figura;
    private readonly Text extra;

    public AvatarDibujo(Transform padre)
    {
        fondo = UIFabrica.Caja(padre, "Avatar", Paleta.Tarjeta);
        UIFabrica.Anclar(fondo.rectTransform, 0, 0, 1, 1);

        figura = UIFabrica.Crear("Figura", fondo.transform);
        figura.anchorMin = figura.anchorMax = new Vector2(0.5f, 0.45f);
        figura.sizeDelta = new Vector2(260, 420);

        Image mochila = Pieza("Mochila", Paleta.Mochila, new Vector2(0, -40), new Vector2(230, 200), false);
        mochila.rectTransform.anchoredPosition = new Vector2(0, -30);
        cuerpo = Pieza("Sudadera", Paleta.Hex("2FA3A0"), new Vector2(0, -70), new Vector2(200, 220), false);
        Pieza("Vaqueros", Paleta.Vaquero, new Vector2(0, -205), new Vector2(150, 90), false);
        Pieza("Cuello", Paleta.Piel, new Vector2(0, 40), new Vector2(60, 40), false);
        Pieza("Cabeza", Paleta.Piel, new Vector2(0, 110), new Vector2(150, 150), true);
        Image pelo = Pieza("Pelo", Paleta.Pelo, new Vector2(0, 165), new Vector2(156, 70), false);
        pelo.rectTransform.anchoredPosition = new Vector2(0, 160);
        Pieza("Ojo izquierdo", Paleta.Texto, new Vector2(-30, 110), new Vector2(18, 22), true);
        Pieza("Ojo derecho", Paleta.Texto, new Vector2(30, 110), new Vector2(18, 22), true);
        Pieza("Sonrisa", Paleta.Coral, new Vector2(0, 75), new Vector2(46, 10), false);

        adorno = Pieza("Medalla", Paleta.Hex("E3B341"), new Vector2(60, -40), new Vector2(40, 40), true);
        extra = UIFabrica.Texto(fondo.transform, "", 64, Paleta.TextoSuave, TextAnchor.UpperRight, FontStyle.Bold);
        UIFabrica.Anclar(extra.rectTransform, 0.55f, 0.6f, 0.92f, 0.95f);
    }

    private Image Pieza(string nombre, Color color, Vector2 posicion, Vector2 tam, bool redonda)
    {
        Image img = UIFabrica.Caja(figura, nombre, color);
        if (redonda) { img.sprite = UIFabrica.Circulo; img.type = Image.Type.Simple; }
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = tam;
        rt.anchoredPosition = posicion;
        return img;
    }

    public void Actualizar(TipoEstadoAvatar estado, string colorSudadera)
    {
        cuerpo.color = Paleta.Hex(colorSudadera);
        adorno.gameObject.SetActive(estado == TipoEstadoAvatar.EnForma);
        switch (estado)
        {
            case TipoEstadoAvatar.EnForma:
                fondo.color = Paleta.Hex("FFF1D6");
                figura.localRotation = Quaternion.identity;
                figura.anchoredPosition = new Vector2(0, 10);
                extra.text = "";
                break;
            case TipoEstadoAvatar.Cansado:
                fondo.color = Paleta.Hex("E9E3DA");
                figura.localRotation = Quaternion.Euler(0, 0, -6);
                figura.anchoredPosition = new Vector2(0, -15);
                extra.text = "Zz";
                break;
            case TipoEstadoAvatar.Descuidado:
                fondo.color = Paleta.Hex("DCD6E3");
                figura.localRotation = Quaternion.Euler(0, 0, 4);
                figura.anchoredPosition = new Vector2(0, -35);
                extra.text = "?";
                break;
            default:
                fondo.color = Paleta.Tarjeta;
                figura.localRotation = Quaternion.identity;
                figura.anchoredPosition = Vector2.zero;
                extra.text = "";
                break;
        }
    }
}
