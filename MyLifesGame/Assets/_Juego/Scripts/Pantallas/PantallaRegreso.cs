using System.Collections.Generic;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pantalla de regreso tras horas fuera: tu avatar te esperaba y te pregunta qué has hecho. Sin castigos.</summary>
public class PantallaRegreso : Pantalla
{
    private static readonly string[] Ids = { "trabajar", "estudiar", "entrenar", "quedar", "pasear", "descansar" };
    private static readonly string[] Nombres = { "Trabajar", "Estudiar", "Entrenar", "Con amigos", "Pasear", "Descansar" };

    private readonly Text banner;
    private readonly AvatarDibujo avatar;
    private readonly Button[] botones = new Button[6];
    private readonly Text[] duraciones = new Text[6];
    private readonly Text[] marcas = new Text[6];
    private readonly bool[] elegidas = new bool[6];
    private readonly int[] minutos = new int[6];

    public PantallaRegreso(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla regreso")
    {
        banner = TextoEnFila("", 80, 46, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        TextoEnFila("Tranquilo, no pierdes nada: tus barras estaban en pausa.", 46, 28, Paleta.TextoSuave, TextAnchor.MiddleCenter);

        RectTransform filaAvatar = UIFabrica.Fila(columna, "Avatar", 440);
        avatar = new AvatarDibujo(filaAvatar);
        Image globo = UIFabrica.Caja(filaAvatar, "Globo", Paleta.Blanco);
        UIFabrica.Anclar(globo.rectTransform, 0.6f, 0.66f, 0.97f, 0.93f);
        UIFabrica.Sombra(globo, 4f, 0.12f);
        Text burbuja = UIFabrica.Texto(globo.transform, "¡Hola! ¿Qué has hecho?", 30, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(burbuja.rectTransform, 0.05f, 0, 0.95f, 1);

        TextoEnFila("Cuéntame qué ha pasado (cuenta un poco menos que en directo):", 50, 28, Paleta.Texto);
        for (int i = 0; i < 6; i++)
        {
            int indice = i;
            RectTransform fila = UIFabrica.Fila(columna, Nombres[i], 92);
            botones[i] = UIFabrica.Boton(fila, Nombres[i], Paleta.Tarjeta, Paleta.Texto, 32, () => { elegidas[indice] = !elegidas[indice]; Actualizar(); });
            UIFabrica.Anclar((RectTransform)botones[i].transform, 0, 0, 1, 1);
            Text tb = botones[i].GetComponentInChildren<Text>();
            tb.alignment = TextAnchor.MiddleLeft;
            UIFabrica.Anclar(tb.rectTransform, 0.13f, 0, 0.5f, 1);
            FormasUI ic = Iconos.Crear(botones[i].transform, Iconos.AreaDeAccion(Ids[i]), 60f);
            ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0, 0.5f);
            ic.rectTransform.anchoredPosition = new Vector2(52, 0);
            marcas[i] = UIFabrica.Texto(botones[i].transform, "", 34, Paleta.Verde, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(marcas[i].rectTransform, 0.44f, 0, 0.52f, 1);
            Button menos = UIFabrica.Boton(botones[i].transform, "−", Paleta.Gris, Paleta.Texto, 40, () => { minutos[indice] = Mathf.Max(15, minutos[indice] - 15); Actualizar(); });
            RectTransform rm = (RectTransform)menos.transform;
            rm.anchorMin = rm.anchorMax = new Vector2(0.6f, 0.5f); rm.sizeDelta = new Vector2(70, 70);
            menos.image.sprite = UIFabrica.Circulo; menos.image.type = Image.Type.Simple;
            duraciones[i] = UIFabrica.Texto(botones[i].transform, "", 32, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(duraciones[i].rectTransform, 0.64f, 0, 0.86f, 1);
            Button mas = UIFabrica.Boton(botones[i].transform, "+", Paleta.Gris, Paleta.Texto, 40, () => { minutos[indice] = Mathf.Min(600, minutos[indice] + 15); Actualizar(); });
            RectTransform rp = (RectTransform)mas.transform;
            rp.anchorMin = rp.anchorMax = new Vector2(0.92f, 0.5f); rp.sizeDelta = new Vector2(70, 70);
            mas.image.sprite = UIFabrica.Circulo; mas.image.type = Image.Type.Simple;
        }

        UIFabrica.Relleno(columna);
        BotonEnFila("Listo", 130, Paleta.Verde, Paleta.Blanco, 50, Confirmar);
        BotonEnFila("Mejor luego", 80, Paleta.Gris, Paleta.Texto, 30, () => this.juego.TerminarRegreso(new List<KeyValuePair<string, int>>()));
    }

    protected override void AlMostrar()
    {
        int horas = Mathf.RoundToInt(juego.MinutosAusencia / 60f);
        banner.text = "Llevas " + horas + " h fuera";
        for (int i = 0; i < 6; i++) { elegidas[i] = false; minutos[i] = 60; }
        avatar.Actualizar(TipoEstadoAvatar.Descuidado, Catalogos.BuscarPrenda(juego.Progreso.Datos.ropaPuesta).colorHex);
    }

    public override void Actualizar()
    {
        for (int i = 0; i < 6; i++)
        {
            botones[i].image.color = elegidas[i] ? Paleta.Hex("E4F2E1") : Paleta.Tarjeta;
            marcas[i].text = elegidas[i] ? "✓" : "";
            duraciones[i].text = Jornada.Duracion(minutos[i]);
        }
    }

    private void Confirmar()
    {
        var lista = new List<KeyValuePair<string, int>>();
        for (int i = 0; i < 6; i++) if (elegidas[i]) lista.Add(new KeyValuePair<string, int>(Ids[i], minutos[i]));
        juego.TerminarRegreso(lista);
    }
}
