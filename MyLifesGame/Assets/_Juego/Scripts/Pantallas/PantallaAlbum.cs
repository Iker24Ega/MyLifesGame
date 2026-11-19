using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pestaña "Álbum": los logros, desbloqueados o con candado.</summary>
public class PantallaAlbum : Pantalla
{
    private readonly Text cabecera;
    private readonly Image[] insignias;
    private readonly Text[] textos;
    private readonly Image[] tarjetas;

    public PantallaAlbum(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla álbum", 4)
    {
        cabecera = TextoEnFila("", 100, 60, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        TextoEnFila("Más adelante se clasificarán por categoría y dificultad.", 44, 26, Paleta.TextoSuave);

        int n = Catalogos.Logros.Length;
        insignias = new Image[n];
        textos = new Text[n];
        tarjetas = new Image[n];
        for (int fila = 0; fila < (n + 1) / 2; fila++)
        {
            RectTransform rt = UIFabrica.Fila(columna, "Fila " + fila, 200);
            for (int c = 0; c < 2; c++)
            {
                int i = fila * 2 + c;
                if (i >= n) break;
                Image t = UIFabrica.Caja(rt, "Logro " + i, Paleta.Tarjeta);
                UIFabrica.Anclar(t.rectTransform, c * 0.5f + 0.01f, 0, (c + 1) * 0.5f - 0.01f, 1);
                tarjetas[i] = t;
                insignias[i] = UIFabrica.Caja(t.transform, "Insignia", Paleta.Gris);
                insignias[i].sprite = UIFabrica.Circulo; insignias[i].type = Image.Type.Simple;
                insignias[i].rectTransform.anchorMin = insignias[i].rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
                insignias[i].rectTransform.sizeDelta = new Vector2(90, 90);
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
            insignias[i].color = tiene ? Paleta.DeArea(l.area) : Paleta.Gris;
            tarjetas[i].color = tiene ? Paleta.Tarjeta : Paleta.Hex("EDE6DB");
            textos[i].text = tiene
                ? "<b>" + l.nombre + "</b>\n<size=22>" + l.descripcion + "</size>"
                : "<b>? ? ?</b>\n<size=22>" + l.descripcion + "</size>";
        }
        cabecera.text = "Logros  " + conseguidos + "/" + Catalogos.Logros.Length;
    }
}
