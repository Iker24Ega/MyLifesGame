using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Acción en curso: anillo de dosis (Poco / Zona óptima / Demasiado) con el tiempo dentro y efectos en vivo.</summary>
public class PantallaCronometro : Pantalla
{
    private readonly Text nombre, lugar, tiempo, estado, info;
    private readonly FormasUI anillo;
    private readonly FormasUI icono;
    private readonly Text[] flechas = new Text[5];
    private float escala = 1f;
    private DefinicionAccion accion;

    public PantallaCronometro(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla cronómetro")
    {
        nombre = TextoEnFila("", 100, 66, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        lugar = TextoEnFila("", 50, 34, Paleta.TextoSuave, TextAnchor.MiddleCenter);

        RectTransform filaAnillo = UIFabrica.Fila(columna, "Anillo", 640);
        RectTransform zona = UIFabrica.Crear("Zona anillo", filaAnillo);
        zona.anchorMin = zona.anchorMax = new Vector2(0.5f, 0.5f);
        zona.sizeDelta = new Vector2(600, 600);
        Image fondo = UIFabrica.Caja(zona, "Fondo", Paleta.Tarjeta);
        fondo.sprite = UIFabrica.Circulo; fondo.type = Image.Type.Simple;
        UIFabrica.Anclar(fondo.rectTransform, 0.06f, 0.06f, 0.94f, 0.94f);
        UIFabrica.Sombra(fondo, 8f, 0.12f);
        RectTransform capaAnillo = UIFabrica.Crear("Capa anillo", zona);
        UIFabrica.Anclar(capaAnillo, 0, 0, 1, 1);
        anillo = FormasUI.En(capaAnillo.gameObject);
        anillo.raycastTarget = false;
        icono = Iconos.Crear(zona, Area.Fisico, 110f);
        icono.rectTransform.anchorMin = icono.rectTransform.anchorMax = new Vector2(0.5f, 0.72f);
        tiempo = UIFabrica.Texto(zona, "", 104, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(tiempo.rectTransform, 0, 0.36f, 1, 0.6f);
        estado = UIFabrica.Texto(zona, "", 34, Paleta.Verde, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(estado.rectTransform, 0.1f, 0.22f, 0.9f, 0.36f);

        info = TextoEnFila("", 50, 26, Paleta.TextoSuave, TextAnchor.MiddleCenter);

        // Efectos en vivo
        RectTransform filaVivo = UIFabrica.Fila(columna, "En vivo", 150);
        Image tv = Tarjeta(filaVivo);
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            FormasUI ic = Iconos.Crear(tv.transform, area, 64f);
            ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2((i + 0.5f) / 5f, 0.62f);
            flechas[i] = UIFabrica.Texto(tv.transform, "", 26, Paleta.TextoSuave, TextAnchor.MiddleCenter, FontStyle.Bold);
            flechas[i].rectTransform.anchorMin = flechas[i].rectTransform.anchorMax = new Vector2((i + 0.5f) / 5f, 0.2f);
            flechas[i].rectTransform.sizeDelta = new Vector2(180, 40);
        }

        UIFabrica.Relleno(columna);
        BotonEnFila("Terminar", 140, Paleta.Coral, Paleta.Blanco, 56, () => this.juego.TerminarAccion());
        BotonEnFila("Cancelar (no cuenta)", 90, Paleta.Gris, Paleta.Texto, 32, () => this.juego.CancelarAccion());
    }

    /// <summary>Prepara la pantalla para una acción concreta.</summary>
    public void Preparar(DefinicionAccion a)
    {
        accion = a;
        nombre.text = a.nombre;
        lugar.text = a.lugar;
        info.text = "Zona óptima: " + PantallaHacer.Rango(a) + " · Demasiado desde " + Jornada.Duracion(a.demasiadoDesde);
        escala = a.demasiadoDesde * 1.35f;
        Iconos.Dibujar(icono, Iconos.AreaDeAccion(a.id), true);
        foreach (Area area in Areas.Todas)
        {
            float v = a.efectoHora.Get(area);
            int i = (int)area;
            flechas[i].text = v > 0.01f ? "+ sube" : v < -0.01f ? "− baja" : "·";
            flechas[i].color = v > 0.01f ? Paleta.Verde : v < -0.01f ? Paleta.Coral : Paleta.TextoSuave;
        }
    }

    public override void Actualizar()
    {
        DefinicionAccion a = juego.AccionEnCurso;
        if (a == null) return;
        if (a != accion) Preparar(a);

        float segundos = (juego.MinutoJuego - juego.InicioAccion) * 60f;
        int total = Mathf.FloorToInt(segundos);
        tiempo.text = (total / 3600).ToString("00") + ":" + (total / 60 % 60).ToString("00") + ":" + (total % 60).ToString("00");
        int minutos = juego.MinutosAccion;

        // Anillo: tramos de la dosis y marcador
        anillo.Limpiar();
        float g(float m) { return Mathf.Clamp01(m / escala) * 360f; }
        anillo.Arco(Vector2.zero, 0.88f, 0.98f, 0f, g(a.zonaMin), Paleta.TramoPoco);
        anillo.Arco(Vector2.zero, 0.88f, 0.98f, g(a.zonaMin), g(a.zonaMax), Paleta.TramoBueno);
        anillo.Arco(Vector2.zero, 0.88f, 0.98f, g(a.zonaMax), g(a.demasiadoDesde), Paleta.TramoLarga);
        anillo.Arco(Vector2.zero, 0.88f, 0.98f, g(a.demasiadoDesde), 360f, Paleta.TramoDemasiado);
        float progreso = g(minutos);
        anillo.Arco(Vector2.zero, 0.74f, 0.84f, 0f, 360f, Paleta.Fondo);
        anillo.Arco(Vector2.zero, 0.74f, 0.84f, 0f, Mathf.Max(1f, progreso), Paleta.Coral);
        float rad = progreso * Mathf.Deg2Rad;
        Vector2 p = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * 0.93f;
        anillo.Circulo(p, 0.085f, Paleta.Texto);
        anillo.Circulo(p, 0.06f, Color.white);

        switch (a.Tramo(minutos))
        {
            case TramoDosis.Poco: estado.text = "Poco"; estado.color = Paleta.TextoSuave; break;
            case TramoDosis.ZonaOptima: estado.text = "Zona óptima"; estado.color = Paleta.Verde; break;
            case TramoDosis.Larga: estado.text = "Algo larga (sin bonus)"; estado.color = Paleta.Texto; break;
            default: estado.text = "Demasiado: cansa más de lo que aporta"; estado.color = Paleta.Coral; break;
        }
    }
}
