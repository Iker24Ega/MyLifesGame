using System.Text;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Pantalla "Hacer": rejilla de acciones con icono; al elegir una se ve su zona óptima e impacto y se empieza.</summary>
public class PantallaHacer : Pantalla
{
    private readonly Image[] tarjetas;
    private readonly Text detalleTitulo, detalleZona, detalleImpacto;
    private readonly Button botonAmigos, botonEmpezar;
    private DefinicionAccion elegida;

    public PantallaHacer(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla Hacer", -1, true)
    {
        ComoHoja(0.93f);
        RectTransform cabecera = UIFabrica.Fila(columna, "Cabecera", 100);
        Text titulo = UIFabrica.Texto(cabecera, "¿Qué vas a hacer?", 60, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(titulo.rectTransform, 0, 0, 0.85f, 1);
        Button cerrar = UIFabrica.Boton(cabecera, "X", Paleta.Tarjeta, Paleta.Texto, 40, () => this.juego.Volver());
        UIFabrica.Anclar((RectTransform)cerrar.transform, 0.87f, 0.1f, 1f, 0.9f);
        TextoEnFila("Elige una acción y empieza el cronómetro.", 44, 28, Paleta.TextoSuave);

        int n = juego.Tabla.acciones.Count;
        tarjetas = new Image[n];
        for (int f = 0; f < (n + 1) / 2; f++)
        {
            RectTransform fila = UIFabrica.Fila(columna, "Fila " + f, 170);
            for (int c = 0; c < 2; c++)
            {
                int i = f * 2 + c;
                if (i >= n) break;
                DefinicionAccion a = juego.Tabla.acciones[i];
                Button b = UIFabrica.Boton(fila, "", Paleta.Tarjeta, Paleta.Texto, 10, () => Elegir(a));
                UIFabrica.Anclar((RectTransform)b.transform, c * 0.5f + 0.01f, 0, (c + 1) * 0.5f - 0.01f, 1);
                tarjetas[i] = (Image)b.targetGraphic;
                FormasUI icono = Iconos.Crear(b.transform, Iconos.AreaDeAccion(a.id), 78f);
                icono.rectTransform.anchorMin = icono.rectTransform.anchorMax = new Vector2(0.2f, 0.56f);
                Text nombre = UIFabrica.Texto(b.transform, a.nombre, 34, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
                UIFabrica.Anclar(nombre.rectTransform, 0.38f, 0.45f, 0.98f, 0.9f);
                Text zona = UIFabrica.Texto(b.transform, Rango(a), 22, Paleta.TextoSuave, TextAnchor.MiddleLeft);
                UIFabrica.Anclar(zona.rectTransform, 0.38f, 0.22f, 0.98f, 0.48f);
                int k = 0;
                foreach (Area area in Areas.Todas)
                {
                    if (a.efectoHora.Get(area) <= 0 && !(area == Area.Social && a.socialConAmigosPorHora > 0)) continue;
                    Image punto = UIFabrica.Caja(b.transform, "Punto", Paleta.DeArea(area));
                    punto.sprite = UIFabrica.Circulo; punto.type = Image.Type.Simple;
                    punto.rectTransform.anchorMin = punto.rectTransform.anchorMax = new Vector2(0.38f, 0.13f);
                    punto.rectTransform.sizeDelta = new Vector2(18, 18);
                    punto.rectTransform.anchoredPosition = new Vector2(9 + k * 24, 0);
                    k++;
                }
            }
        }

        // Detalle de la acción elegida
        RectTransform filaDetalle = UIFabrica.Fila(columna, "Detalle", 230);
        Image detalle = Tarjeta(filaDetalle);
        detalleTitulo = UIFabrica.Texto(detalle.transform, "", 36, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(detalleTitulo.rectTransform, 0.05f, 0.68f, 0.95f, 0.95f);
        detalleZona = UIFabrica.Texto(detalle.transform, "", 26, Paleta.Verde, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(detalleZona.rectTransform, 0.05f, 0.42f, 0.95f, 0.68f);
        detalleImpacto = UIFabrica.Texto(detalle.transform, "", 26, Paleta.Texto, TextAnchor.MiddleLeft);
        UIFabrica.Anclar(detalleImpacto.rectTransform, 0.05f, 0.08f, 0.62f, 0.42f);
        botonAmigos = UIFabrica.Boton(detalle.transform, "", Paleta.Gris, Paleta.Texto, 24, () => { this.juego.ConAmigos = !this.juego.ConAmigos; Actualizar(); });
        UIFabrica.Anclar((RectTransform)botonAmigos.transform, 0.64f, 0.1f, 0.96f, 0.4f);

        UIFabrica.Relleno(columna);
        botonEmpezar = BotonEnFila("Empezar", 140, Paleta.Coral, Paleta.Blanco, 54, () => { if (elegida != null) this.juego.EmpezarAccion(elegida); });
    }

    protected override void AlMostrar()
    {
        Elegir(juego.Tabla.acciones[0]);
    }

    private void Elegir(DefinicionAccion a)
    {
        elegida = a;
        Actualizar();
    }

    public override void Actualizar()
    {
        if (elegida == null) return;
        for (int i = 0; i < tarjetas.Length; i++)
        {
            bool sel = juego.Tabla.acciones[i] == elegida;
            tarjetas[i].color = sel ? Paleta.Hex("FFF1E2") : Paleta.Tarjeta;
            var borde = tarjetas[i].GetComponent<Outline>();
            if (borde == null) borde = tarjetas[i].gameObject.AddComponent<Outline>();
            borde.effectColor = Paleta.Coral;
            borde.effectDistance = new Vector2(4, -4);
            borde.enabled = sel;
        }
        detalleTitulo.text = elegida.nombre + "  ·  " + elegida.lugar;
        detalleZona.text = "Zona óptima: " + Rango(elegida) + "   ·   Demasiado desde " + Jornada.Duracion(elegida.demasiadoDesde);
        var sb = new StringBuilder();
        foreach (Area area in Areas.Todas)
        {
            float v = elegida.efectoHora.Get(area);
            if (Mathf.Abs(v) < 0.01f) continue;
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(Paleta.DeArea(area))).Append(">")
              .Append(Areas.Nombre(area)).Append(v > 0 ? " +" : " −").Append("</color>   ");
        }
        detalleImpacto.text = sb.ToString();
        bool amigos = elegida.socialConAmigosPorHora > 0;
        botonAmigos.gameObject.SetActive(amigos);
        UIFabrica.CambiarTexto(botonAmigos, juego.ConAmigos ? "Con amigos: SÍ" : "Con amigos: NO");
        botonAmigos.image.color = juego.ConAmigos ? Paleta.DeArea(Area.Social) : Paleta.Gris;
        UIFabrica.CambiarTexto(botonEmpezar, "Empezar: " + elegida.nombre);
    }

    public static string Rango(DefinicionAccion a)
    {
        if (a.zonaMin <= 0) return "hasta " + Jornada.Duracion(a.zonaMax);
        return Jornada.Duracion(a.zonaMin) + " - " + Jornada.Duracion(a.zonaMax);
    }
}
