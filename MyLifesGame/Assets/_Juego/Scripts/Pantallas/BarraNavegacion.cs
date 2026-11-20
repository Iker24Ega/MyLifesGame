using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra inferior como la de las imágenes de referencia: Ciudad · Hoy · Hacer (botón central elevado) · Avatar · Álbum,
/// cada pestaña con su icono encima del nombre y la activa resaltada en una pastilla.
/// </summary>
public static class BarraNavegacion
{
    private static readonly string[] Nombres = { "Ciudad", "Hoy", "Hacer", "Avatar", "Álbum" };

    public static void Crear(Transform padre, Juego juego, int activa)
    {
        Image fondo = UIFabrica.Caja(padre, "Barra de navegación", Paleta.Tarjeta, true);
        RectTransform rt = fondo.rectTransform;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(-24, Pantalla.AltoBarra - 12);
        rt.anchoredPosition = new Vector2(0, 8);
        UIFabrica.Sombra(fondo, 10f, 0.18f);

        for (int i = 0; i < 5; i++)
        {
            int indice = i;
            float x0 = i / 5f, x1 = (i + 1) / 5f;
            if (i == 2)
            {
                // Botón central elevado
                Image aro = UIFabrica.Caja(rt, "Aro Hacer", Paleta.Tarjeta);
                aro.sprite = UIFabrica.Circulo; aro.type = Image.Type.Simple;
                aro.rectTransform.anchorMin = aro.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                aro.rectTransform.sizeDelta = new Vector2(196, 196);
                aro.rectTransform.anchoredPosition = new Vector2(0, 34);
                UIFabrica.Sombra(aro, 6f, 0.15f);
                Button hacer = BotonVacio(aro.transform, Paleta.Coral, () => juego.IrAHacer());
                ((Image)hacer.targetGraphic).sprite = UIFabrica.Circulo;
                ((Image)hacer.targetGraphic).type = Image.Type.Simple;
                UIFabrica.Anclar((RectTransform)hacer.transform, 0.07f, 0.07f, 0.93f, 0.93f);
                RectTransform zi = UIFabrica.Crear("Más", hacer.transform);
                UIFabrica.Anclar(zi, 0.3f, 0.42f, 0.7f, 0.82f);
                FormasUI mas = FormasUI.En(zi.gameObject); mas.raycastTarget = false;
                mas.Rect(Vector2.zero, new Vector2(1.3f, 0.3f), 0f, Paleta.Blanco);
                mas.Rect(Vector2.zero, new Vector2(0.3f, 1.3f), 0f, Paleta.Blanco);
                Text th = UIFabrica.Texto(hacer.transform, "Hacer", 34, Paleta.Blanco, TextAnchor.MiddleCenter, FontStyle.Bold);
                UIFabrica.Anclar(th.rectTransform, 0, 0.12f, 1, 0.42f);
                th.raycastTarget = false;
                continue;
            }
            bool esActiva = i == activa;
            Color color = esActiva ? Paleta.Hex("6B4A35") : Paleta.TextoSuave;
            Button b = BotonVacio(rt, esActiva ? Paleta.Fondo : new Color(0, 0, 0, 0), () => juego.IrAPestana(indice));
            RectTransform brt = (RectTransform)b.transform;
            brt.anchorMin = new Vector2(x0, 0.08f);
            brt.anchorMax = new Vector2(x1, 0.92f);
            brt.offsetMin = new Vector2(8, 0);
            brt.offsetMax = new Vector2(-8, 0);
            RectTransform zi2 = UIFabrica.Crear("Icono", b.transform);
            zi2.anchorMin = zi2.anchorMax = new Vector2(0.5f, 0.64f);
            zi2.sizeDelta = new Vector2(62, 62);
            FormasUI ic = FormasUI.En(zi2.gameObject); ic.raycastTarget = false;
            Icono(ic, i, color, esActiva ? Paleta.Fondo : Paleta.Tarjeta);
            Text t = UIFabrica.Texto(b.transform, Nombres[i], 30, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(t.rectTransform, 0, 0.04f, 1, 0.36f);
            t.raycastTarget = false;
        }
    }

    private static Button BotonVacio(Transform padre, Color fondo, System.Action alPulsar)
    {
        Image img = UIFabrica.Caja(padre, "Botón", fondo);
        Button b = img.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        img.gameObject.AddComponent<BotonAnimado>();
        b.onClick.AddListener(() => alPulsar());
        return b;
    }

    /// <summary>Iconos de pestaña: casa, calendario, persona y cuadro.</summary>
    private static void Icono(FormasUI f, int i, Color c, Color hueco)
    {
        switch (i)
        {
            case 0:
                f.Poligono(new[] { new Vector2(-0.95f, 0.05f), new Vector2(0f, 0.9f), new Vector2(0.95f, 0.05f) }, c);
                f.Rect(new Vector2(0, -0.42f), new Vector2(1.3f, 0.95f), 0f, c);
                f.Rect(new Vector2(0, -0.58f), new Vector2(0.36f, 0.62f), 0f, hueco);
                break;
            case 1:
                f.Rect(new Vector2(0, -0.08f), new Vector2(1.7f, 1.6f), 0f, c);
                f.Rect(new Vector2(0, -0.22f), new Vector2(1.4f, 1.0f), 0f, hueco);
                f.Rect(new Vector2(-0.45f, 0.78f), new Vector2(0.18f, 0.4f), 0f, c);
                f.Rect(new Vector2(0.45f, 0.78f), new Vector2(0.18f, 0.4f), 0f, c);
                for (int k = 0; k < 6; k++)
                    f.Rect(new Vector2(-0.42f + (k % 3) * 0.42f, -0.05f - (k / 3) * 0.38f), new Vector2(0.22f, 0.2f), 0f, c);
                break;
            case 3:
                f.Circulo(new Vector2(0, 0.42f), 0.44f, c);
                f.Arco(new Vector2(0, -0.95f), 0f, 0.95f, -90f, 90f, c);
                break;
            default:
                f.Rect(Vector2.zero, new Vector2(1.8f, 1.5f), 0f, c);
                f.Rect(Vector2.zero, new Vector2(1.5f, 1.2f), 0f, hueco);
                f.Circulo(new Vector2(-0.35f, 0.22f), 0.18f, c);
                f.Poligono(new[] { new Vector2(-0.75f, -0.6f), new Vector2(-0.1f, 0.05f), new Vector2(0.3f, -0.3f), new Vector2(0.75f, -0.6f) }, c);
                break;
        }
    }
}
