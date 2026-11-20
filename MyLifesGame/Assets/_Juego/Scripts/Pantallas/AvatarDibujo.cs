using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retrato del avatar en la interfaz: el avatar 3D del estudio (el mismo que el de la ciudad) sobre una
/// pequeña habitación cuyo ambiente cambia según su estado. El cuerpo nunca cambia (regla de diseño 5.6).
/// </summary>
public class AvatarDibujo
{
    private readonly Image fondo, suelo;
    private readonly FormasUI ventana;
    private readonly Text extra;

    public AvatarDibujo(Transform padre)
    {
        fondo = UIFabrica.Caja(padre, "Avatar", Paleta.Tarjeta);
        UIFabrica.Anclar(fondo.rectTransform, 0, 0, 1, 1);
        fondo.gameObject.AddComponent<RectMask2D>();

        RectTransform zv = UIFabrica.Crear("Ventana", fondo.transform);
        UIFabrica.Anclar(zv, 0.06f, 0.5f, 0.3f, 0.9f);
        ventana = FormasUI.En(zv.gameObject);
        ventana.raycastTarget = false;

        suelo = UIFabrica.Caja(fondo.transform, "Suelo", Paleta.Hex("E8D9C2"), false);
        UIFabrica.Anclar(suelo.rectTransform, 0, 0, 1, 0.24f);

        Image sombra = UIFabrica.Caja(fondo.transform, "Sombra", new Color(0.29f, 0.2f, 0.15f, 0.16f));
        sombra.sprite = UIFabrica.Circulo; sombra.type = Image.Type.Simple;
        sombra.rectTransform.anchorMin = sombra.rectTransform.anchorMax = new Vector2(0.5f, 0.17f);
        sombra.rectTransform.sizeDelta = new Vector2(230, 50);

        var retrato = new GameObject("Retrato 3D", typeof(RectTransform)).AddComponent<RawImage>();
        retrato.transform.SetParent(fondo.transform, false);
        retrato.texture = EstudioAvatar.Textura;
        retrato.raycastTarget = false;
        RectTransform rt = retrato.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(0, 0);
        rt.offsetMax = new Vector2(0, -6);
        var ajuste = retrato.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        ajuste.aspectRatio = 768f / 960f;

        extra = UIFabrica.Texto(fondo.transform, "", 64, Paleta.TextoSuave, TextAnchor.UpperRight, FontStyle.Bold);
        UIFabrica.Anclar(extra.rectTransform, 0.6f, 0.62f, 0.94f, 0.95f);
    }

    public void Actualizar(TipoEstadoAvatar estado, string colorSudadera)
    {
        EstudioAvatar.Actualizar(estado, colorSudadera);
        Color cielo;
        switch (estado)
        {
            case TipoEstadoAvatar.EnForma: fondo.color = Paleta.Hex("FFEFD2"); cielo = Paleta.Hex("A9D8F2"); extra.text = "★"; extra.color = Paleta.Hex("E3B341"); break;
            case TipoEstadoAvatar.Cansado: fondo.color = Paleta.Hex("EAE2D6"); cielo = Paleta.Hex("C9C2D8"); extra.text = "Zz"; extra.color = Paleta.TextoSuave; break;
            case TipoEstadoAvatar.Descuidado: fondo.color = Paleta.Hex("DDD6E4"); cielo = Paleta.Hex("8E94B5"); extra.text = "?"; extra.color = Paleta.TextoSuave; break;
            default: fondo.color = Paleta.Hex("F8EEDF"); cielo = Paleta.Hex("BFE0F2"); extra.text = ""; break;
        }
        suelo.color = Color.Lerp(fondo.color, Paleta.Hex("C9A27A"), 0.35f);
        ventana.Limpiar();
        ventana.Rect(Vector2.zero, new Vector2(1.9f, 1.9f), 0f, Paleta.Blanco);
        ventana.Rect(Vector2.zero, new Vector2(1.66f, 1.66f), 0f, cielo);
        ventana.Circulo(new Vector2(0.4f, 0.4f), 0.22f, estado == TipoEstadoAvatar.Descuidado ? Paleta.Hex("E8E4D8") : Paleta.Hex("FFD66B"));
        ventana.Rect(Vector2.zero, new Vector2(0.08f, 1.66f), 0f, Paleta.Blanco);
        ventana.Rect(Vector2.zero, new Vector2(1.66f, 0.08f), 0f, Paleta.Blanco);
    }
}
