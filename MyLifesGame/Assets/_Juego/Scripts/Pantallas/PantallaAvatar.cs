using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pestaña "Avatar": cómo está tu avatar, tu nivel y monedas, y la tienda de sudaderas.</summary>
public class PantallaAvatar : Pantalla
{
    private readonly Text cabecera, estado, aviso;
    private readonly AvatarDibujo avatar;
    private readonly Button[] botonesPrenda = new Button[6];
    private readonly Text[] textosPrenda = new Text[6];
    private readonly Button botonPantallas;

    public PantallaAvatar(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla avatar", 3)
    {
        cabecera = TextoEnFila("", 80, 44, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        RectTransform filaAvatar = UIFabrica.Fila(columna, "Avatar", 520);
        avatar = new AvatarDibujo(filaAvatar);
        estado = TextoEnFila("", 70, 34, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);

        TextoEnFila("Tienda de sudaderas  (monedas provisionales para la demo)", 50, 30, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        for (int i = 0; i < Catalogos.Prendas.Length && i < 6; i++)
        {
            int indice = i;
            Prenda p = Catalogos.Prendas[i];
            RectTransform fila = UIFabrica.Fila(columna, p.nombre, 78);
            Image t = Tarjeta(fila);
            Image muestra = UIFabrica.Caja(t.transform, "Color", Paleta.Hex(p.colorHex));
            muestra.sprite = UIFabrica.Circulo; muestra.type = Image.Type.Simple;
            muestra.rectTransform.anchorMin = muestra.rectTransform.anchorMax = new Vector2(0, 0.5f);
            muestra.rectTransform.sizeDelta = new Vector2(52, 52);
            muestra.rectTransform.anchoredPosition = new Vector2(46, 0);
            textosPrenda[i] = UIFabrica.Texto(t.transform, "", 28, Paleta.Texto);
            UIFabrica.Anclar(textosPrenda[i].rectTransform, 0.11f, 0, 0.68f, 1);
            botonesPrenda[i] = UIFabrica.Boton(t.transform, "", Paleta.Verde, Paleta.Blanco, 26, () => this.juego.ElegirPrenda(Catalogos.Prendas[indice]));
            UIFabrica.Anclar((RectTransform)botonesPrenda[i].transform, 0.7f, 0.12f, 0.98f, 0.88f);
        }
        aviso = TextoEnFila("", 44, 28, Paleta.Coral, TextAnchor.MiddleCenter, FontStyle.Bold);

        // Ajustes de la demo
        RectTransform filaAjustes = UIFabrica.Fila(columna, "Ajustes demo", 80);
        Button ausencia = UIFabrica.Boton(filaAjustes, "Simular 6 h fuera", Paleta.Gris, Paleta.Texto, 26, () => this.juego.SimularAusencia(6 * 60));
        botonPantallas = UIFabrica.Boton(filaAjustes, "", Paleta.Gris, Paleta.Texto, 24, () => this.juego.CambiarSensibilidadPantallas());
        Button nueva = UIFabrica.Boton(filaAjustes, "Nueva partida", Paleta.TramoDemasiado, Paleta.Texto, 26, () => this.juego.NuevaPartida());
        UIFabrica.Anclar((RectTransform)ausencia.transform, 0f, 0f, 0.32f, 1f);
        UIFabrica.Anclar((RectTransform)botonPantallas.transform, 0.34f, 0f, 0.66f, 1f);
        UIFabrica.Anclar((RectTransform)nueva.transform, 0.68f, 0f, 1f, 1f);
        UIFabrica.Relleno(columna);
    }

    public void MostrarAviso(string texto) { aviso.text = texto; }

    protected override void AlMostrar() { aviso.text = ""; }

    public override void Actualizar()
    {
        Progreso pr = juego.Progreso;
        cabecera.text = "Nv. " + pr.Nivel + "  (" + pr.XpEnNivel + "/" + Progreso.XpPorNivel + " XP)   ·   " + pr.Datos.monedas + " monedas";
        string causa;
        TipoEstadoAvatar tipo = EstadoAvatar.Calcular(juego.Jornada.Estado, juego.VieneDeAusencia, out causa);
        avatar.Actualizar(tipo, Catalogos.BuscarPrenda(pr.Datos.ropaPuesta).colorHex);
        estado.text = "Estado: " + EstadoAvatar.Nombre(tipo) + "  ·  " + causa;

        for (int i = 0; i < Catalogos.Prendas.Length && i < 6; i++)
        {
            Prenda p = Catalogos.Prendas[i];
            bool tiene = pr.Comprada(p.id);
            bool puesta = pr.Datos.ropaPuesta == p.id;
            textosPrenda[i].text = p.nombre + (tiene ? "" : "\n<size=22><color=#8A7565>" + p.precio + " monedas · nivel " + p.nivelMinimo + "</color></size>");
            string accion = puesta ? "Puesta" : tiene ? "Poner" : "Comprar";
            UIFabrica.CambiarTexto(botonesPrenda[i], accion);
            botonesPrenda[i].image.color = puesta ? Paleta.Gris : tiene ? Paleta.DeArea(Area.Mente) : Paleta.Verde;
        }
        SensibilidadPantallas s = juego.Jornada.Datos.pantallas;
        UIFabrica.CambiarTexto(botonPantallas, "Pantallas antes de dormir: " + (s == SensibilidadPantallas.Nada ? "nada" : s == SensibilidadPantallas.Algo ? "algo" : "mucho"));
    }
}
