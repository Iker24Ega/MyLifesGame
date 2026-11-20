using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pestaña "Avatar": cómo está tu avatar, tu nivel y monedas, y la tienda de sudaderas.</summary>
public class PantallaAvatar : Pantalla
{
    private readonly Text cabecera, estado, aviso, textoXp, monedas;
    private readonly Image barraXp;
    private readonly Image[] tarjetasPrenda = new Image[6];
    private readonly FormasUI[] dibujosPrenda = new FormasUI[6];
    private readonly AvatarDibujo avatar;
    private readonly Button[] botonesPrenda = new Button[6];
    private readonly Text[] textosPrenda = new Text[6];
    private readonly Button botonPantallas;

    public PantallaAvatar(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla avatar", 3)
    {
        // Cabecera: nivel, barra de XP y monedas
        RectTransform cab = UIFabrica.Fila(columna, "Cabecera", 96);
        Image pildoraNivel = UIFabrica.Caja(cab, "Nivel", Paleta.Coral);
        UIFabrica.Anclar(pildoraNivel.rectTransform, 0, 0.12f, 0.2f, 0.88f);
        cabecera = UIFabrica.Texto(pildoraNivel.transform, "", 36, Paleta.Blanco, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(cabecera.rectTransform, 0, 0, 1, 1);
        Image fondoXp = UIFabrica.Caja(cab, "Fondo XP", Paleta.Gris);
        UIFabrica.Anclar(fondoXp.rectTransform, 0.23f, 0.36f, 0.66f, 0.64f);
        barraXp = UIFabrica.Caja(fondoXp.transform, "XP", Paleta.Verde);
        barraXp.type = Image.Type.Filled; barraXp.fillMethod = Image.FillMethod.Horizontal;
        UIFabrica.Anclar(barraXp.rectTransform, 0, 0, 1, 1);
        textoXp = UIFabrica.Texto(cab, "", 22, Paleta.TextoSuave, TextAnchor.UpperCenter);
        UIFabrica.Anclar(textoXp.rectTransform, 0.23f, 0f, 0.66f, 0.34f);
        Image pildoraMonedas = UIFabrica.Caja(cab, "Monedas", Paleta.Hex("FFE7A8"));
        UIFabrica.Anclar(pildoraMonedas.rectTransform, 0.69f, 0.12f, 1f, 0.88f);
        Image moneda = UIFabrica.Caja(pildoraMonedas.transform, "Moneda", Paleta.Hex("E3B341"));
        moneda.sprite = UIFabrica.Circulo; moneda.type = Image.Type.Simple;
        moneda.rectTransform.anchorMin = moneda.rectTransform.anchorMax = new Vector2(0, 0.5f);
        moneda.rectTransform.sizeDelta = new Vector2(44, 44);
        moneda.rectTransform.anchoredPosition = new Vector2(40, 0);
        monedas = UIFabrica.Texto(pildoraMonedas.transform, "", 34, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(monedas.rectTransform, 0.25f, 0, 0.98f, 1);

        RectTransform filaAvatar = UIFabrica.Fila(columna, "Avatar", 560);
        avatar = new AvatarDibujo(filaAvatar);
        estado = TextoEnFila("", 64, 30, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);

        TextoEnFila("Tienda de sudaderas", 54, 36, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        for (int fila = 0; fila < 2; fila++)
        {
            RectTransform rf = UIFabrica.Fila(columna, "Tienda " + fila, 276);
            for (int c = 0; c < 3; c++)
            {
                int i = fila * 3 + c;
                if (i >= Catalogos.Prendas.Length) break;
                int indice = i;
                Prenda p = Catalogos.Prendas[i];
                Image t = UIFabrica.Caja(rf, p.nombre, Paleta.Tarjeta);
                UIFabrica.Anclar(t.rectTransform, c / 3f + 0.008f, 0, (c + 1) / 3f - 0.008f, 1);
                UIFabrica.Sombra(t, 5f, 0.1f);
                tarjetasPrenda[i] = t;
                RectTransform zd = UIFabrica.Crear("Dibujo", t.transform);
                zd.anchorMin = zd.anchorMax = new Vector2(0.5f, 0.66f);
                zd.sizeDelta = new Vector2(130, 130);
                dibujosPrenda[i] = FormasUI.En(zd.gameObject);
                dibujosPrenda[i].raycastTarget = false;
                textosPrenda[i] = UIFabrica.Texto(t.transform, "", 24, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
                UIFabrica.Anclar(textosPrenda[i].rectTransform, 0.04f, 0.27f, 0.96f, 0.42f);
                botonesPrenda[i] = UIFabrica.Boton(t.transform, "", Paleta.Verde, Paleta.Blanco, 24, () => this.juego.ElegirPrenda(Catalogos.Prendas[indice]));
                UIFabrica.Anclar((RectTransform)botonesPrenda[i].transform, 0.1f, 0.05f, 0.9f, 0.24f);
            }
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

    private static Color ColorEstado(TipoEstadoAvatar t)
    {
        switch (t)
        {
            case TipoEstadoAvatar.EnForma: return Paleta.Verde;
            case TipoEstadoAvatar.Cansado: return Paleta.Hex("D9963B");
            case TipoEstadoAvatar.Descuidado: return Paleta.Coral;
            default: return Paleta.DeArea(Area.Mente);
        }
    }

    public void MostrarAviso(string texto) { aviso.text = texto; }

    protected override void AlMostrar() { aviso.text = ""; }

    public override void Actualizar()
    {
        Progreso pr = juego.Progreso;
        cabecera.text = "Nv. " + pr.Nivel;
        barraXp.fillAmount = pr.XpEnNivel / (float)Progreso.XpPorNivel;
        textoXp.text = pr.XpEnNivel + " / " + Progreso.XpPorNivel + " XP";
        monedas.text = pr.Datos.monedas.ToString();
        string causa;
        TipoEstadoAvatar tipo = EstadoAvatar.Calcular(juego.Jornada.Estado, juego.VieneDeAusencia, out causa);
        avatar.Actualizar(tipo, Catalogos.BuscarPrenda(pr.Datos.ropaPuesta).colorHex);
        estado.text = "<color=#" + ColorUtility.ToHtmlStringRGB(ColorEstado(tipo)) + ">● " + EstadoAvatar.Nombre(tipo) + "</color>   <size=26><color=#8A7565>" + causa + "</color></size>";

        for (int i = 0; i < Catalogos.Prendas.Length && i < 6; i++)
        {
            Prenda p = Catalogos.Prendas[i];
            bool tiene = pr.Comprada(p.id);
            bool puesta = pr.Datos.ropaPuesta == p.id;
            bool alcanzable = pr.Nivel >= p.nivelMinimo;
            Iconos.Sudadera(dibujosPrenda[i], Paleta.Hex(p.colorHex), !tiene && !alcanzable);
            string corto = p.nombre.Replace("Sudadera ", "");
            textosPrenda[i].text = char.ToUpper(corto[0]) + corto.Substring(1);
            tarjetasPrenda[i].color = puesta ? Paleta.Hex("E4F2E1") : Paleta.Tarjeta;
            string accion = puesta ? "Puesta ✓" : tiene ? "Poner" : alcanzable ? "● " + p.precio : "Nivel " + p.nivelMinimo;
            UIFabrica.CambiarTexto(botonesPrenda[i], accion);
            botonesPrenda[i].image.color = puesta ? Paleta.Verde : tiene ? Paleta.DeArea(Area.Mente) : alcanzable ? Paleta.Hex("E3B341") : Paleta.Gris;
        }
        SensibilidadPantallas s = juego.Jornada.Datos.pantallas;
        UIFabrica.CambiarTexto(botonPantallas, "Pantallas antes de dormir: " + (s == SensibilidadPantallas.Nada ? "nada" : s == SensibilidadPantallas.Algo ? "algo" : "mucho"));
    }
}
