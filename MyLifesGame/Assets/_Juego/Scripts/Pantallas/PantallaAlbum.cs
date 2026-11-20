using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pestaña "Álbum": los logros, desbloqueados o con candado.</summary>
public class PantallaAlbum : Pantalla
{
    private readonly Text cabecera;
    private readonly FormasUI[] insignias;
    private readonly Image barra;
    private readonly Text[] textos;
    private readonly Image[] tarjetas;

    public PantallaAlbum(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla álbum", 4, true)
    {
        ComoHoja(0.93f, 0.12f);
        RectTransform cab = UIFabrica.Fila(columna, "Cabecera", 110);
        cabecera = UIFabrica.Texto(cab, "", 60, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(cabecera.rectTransform, 0, 0.3f, 0.7f, 1);
        Image fondoBarra = UIFabrica.Caja(cab, "Fondo barra", Paleta.Gris);
        UIFabrica.Anclar(fondoBarra.rectTransform, 0, 0.06f, 1, 0.22f);
        barra = UIFabrica.Caja(fondoBarra.transform, "Barra", Paleta.Hex("E3B341"));
        barra.type = Image.Type.Filled; barra.fillMethod = Image.FillMethod.Horizontal;
        UIFabrica.Anclar(barra.rectTransform, 0, 0, 1, 1);
        TextoEnFila("Tu colección de medallas. Cada una cuenta una buena costumbre.", 44, 26, Paleta.TextoSuave);

        int n = Catalogos.Logros.Length;
        insignias = new FormasUI[n];
        textos = new Text[n];
        tarjetas = new Image[n];
        for (int fila = 0; fila < (n + 1) / 2; fila++)
        {
            RectTransform rt = UIFabrica.Fila(columna, "Fila " + fila, 230);
            for (int c = 0; c < 2; c++)
            {
                int i = fila * 2 + c;
                if (i >= n) break;
                Image t = UIFabrica.Caja(rt, "Logro " + i, Paleta.Tarjeta);
                UIFabrica.Anclar(t.rectTransform, c * 0.5f + 0.01f, 0, (c + 1) * 0.5f - 0.01f, 1);
                tarjetas[i] = t;
                UIFabrica.Sombra(t, 5f, 0.1f);
                RectTransform zm = UIFabrica.Crear("Medalla", t.transform);
                zm.anchorMin = zm.anchorMax = new Vector2(0.5f, 0.68f);
                zm.sizeDelta = new Vector2(112, 112);
                insignias[i] = FormasUI.En(zm.gameObject);
                insignias[i].raycastTarget = false;
                textos[i] = UIFabrica.Texto(t.transform, "", 26, Paleta.Texto, TextAnchor.MiddleCenter);
                UIFabrica.Anclar(textos[i].rectTransform, 0.05f, 0.02f, 0.95f, 0.45f);
            }
        }
        UIFabrica.Relleno(columna);
    }

    public override void Actualizar()
    {
        Progreso pr = juego.Progreso;
        int conseguidos = 0;
        for (int i = 0; i < Catalogos.Logros.Length; i++)
        {
            Logro l = Catalogos.Logros[i];
            bool tiene = pr.Tiene(l.id);
            if (tiene) conseguidos++;
            Iconos.Medalla(insignias[i], l.area, tiene);
            tarjetas[i].color = tiene ? Paleta.Tarjeta : Paleta.Hex("EDE6DB");
            textos[i].text = tiene
                ? "<b>" + l.nombre + "</b>\n<size=22>" + l.descripcion + "</size>"
                : "<b>? ? ?</b>\n<size=22>" + l.descripcion + "</size>";
        }
        cabecera.text = "Álbum  " + conseguidos + "/" + Catalogos.Logros.Length;
        barra.fillAmount = conseguidos / (float)Catalogos.Logros.Length;
    }
}
