using UnityEngine;
using UnityEngine.UI;

/// <summary>Barra inferior: Ciudad · Hoy · Hacer (botón central) · Avatar · Álbum.</summary>
public static class BarraNavegacion
{
    private static readonly string[] Nombres = { "Ciudad", "Hoy", "Hacer", "Avatar", "Álbum" };

    public static void Crear(Transform padre, Juego juego, int activa)
    {
        Image fondo = UIFabrica.Caja(padre, "Barra de navegación", Paleta.Tarjeta, false);
        RectTransform rt = fondo.rectTransform;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, Pantalla.AltoBarra);
        rt.anchoredPosition = Vector2.zero;
        Image linea = UIFabrica.Caja(rt, "Línea", Paleta.Gris, false);
        linea.rectTransform.anchorMin = new Vector2(0, 1);
        linea.rectTransform.anchorMax = new Vector2(1, 1);
        linea.rectTransform.sizeDelta = new Vector2(0, 3);

        for (int i = 0; i < 5; i++)
        {
            int indice = i;
            float x0 = i / 5f, x1 = (i + 1) / 5f;
            if (i == 2)
            {
                Button hacer = UIFabrica.Boton(rt, "+\nHacer", Paleta.Coral, Paleta.Blanco, 36, () => juego.IrAHacer());
                ((Image)hacer.targetGraphic).sprite = UIFabrica.Circulo;
                ((Image)hacer.targetGraphic).type = Image.Type.Simple;
                RectTransform hrt = (RectTransform)hacer.transform;
                hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.5f);
                hrt.sizeDelta = new Vector2(170, 170);
                hrt.anchoredPosition = new Vector2(0, 25);
                continue;
            }
            bool esActiva = i == activa;
            Button b = UIFabrica.Boton(rt, Nombres[i], esActiva ? Paleta.Fondo : Paleta.Tarjeta,
                esActiva ? Paleta.Coral : Paleta.TextoSuave, 32, () => juego.IrAPestana(indice));
            RectTransform brt = (RectTransform)b.transform;
            brt.anchorMin = new Vector2(x0, 0.1f);
            brt.anchorMax = new Vector2(x1, 0.9f);
            brt.offsetMin = new Vector2(6, 0);
            brt.offsetMax = new Vector2(-6, 0);
        }
    }
}
