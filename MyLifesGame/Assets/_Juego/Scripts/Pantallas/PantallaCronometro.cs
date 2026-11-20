using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Acción en curso, como la imagen de referencia "Acción en curso": la sala 3D a pantalla completa con el avatar
/// haciendo la acción, tarjeta con el tiempo arriba, barra de dosis (Poco / Zona óptima / Demasiado) con marcador,
/// efectos en vivo sobre las cinco áreas abajo y los botones Cancelar / Terminar.
/// </summary>
public class PantallaCronometro : Pantalla
{
    private readonly Text haciendo, tiempo, lugar, estado, info;
    private readonly Text[] etiquetasDosis = new Text[3];
    private readonly FormasUI anillo, icono, barra;
    private readonly FormasUI[] flechas = new FormasUI[5];
    private readonly RawImage escena;
    private float escala = 1f, barraW = 6f;
    private DefinicionAccion accion;

    public PantallaCronometro(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla cronómetro")
    {
        // Sala 3D a pantalla completa, detrás de todo
        escena = FondoEscena();

        // Tarjeta superior: anillo pequeño + qué haces + tiempo + lugar
        RectTransform filaTiempo = UIFabrica.Fila(columna, "Tiempo", 250);
        Image tt = UIFabrica.Caja(filaTiempo, "Tarjeta tiempo", Paleta.Tarjeta);
        UIFabrica.Anclar(tt.rectTransform, 0.06f, 0, 0.94f, 1);
        UIFabrica.Sombra(tt, 10f, 0.18f);
        RectTransform za = UIFabrica.Crear("Anillo", tt.transform);
        za.anchorMin = za.anchorMax = new Vector2(0, 0.5f);
        za.sizeDelta = new Vector2(190, 190);
        za.anchoredPosition = new Vector2(130, 0);
        anillo = FormasUI.En(za.gameObject);
        anillo.raycastTarget = false;
        icono = Iconos.Crear(za, Area.Fisico, 92f);
        haciendo = UIFabrica.Texto(tt.transform, "", 40, Paleta.TextoSuave, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(haciendo.rectTransform, 0.3f, 0.7f, 0.98f, 0.92f);
        tiempo = UIFabrica.Texto(tt.transform, "", 104, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(tiempo.rectTransform, 0.29f, 0.26f, 0.99f, 0.74f);
        lugar = UIFabrica.Texto(tt.transform, "", 34, Paleta.TextoSuave, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(lugar.rectTransform, 0.3f, 0.06f, 0.98f, 0.28f);

        // Barra de dosis
        RectTransform filaDosis = UIFabrica.Fila(columna, "Dosis", 170);
        Image td = UIFabrica.Caja(filaDosis, "Tarjeta dosis", new Color(1f, 0.99f, 0.97f, 0.94f));
        UIFabrica.Anclar(td.rectTransform, 0.06f, 0, 0.94f, 1);
        UIFabrica.Sombra(td, 8f, 0.15f);
        RectTransform zb = UIFabrica.Crear("Barra", td.transform);
        UIFabrica.Anclar(zb, 0.05f, 0.52f, 0.95f, 0.84f);
        barra = FormasUI.En(zb.gameObject);
        barra.raycastTarget = false;
        string[] nombres = { "Poco", "Zona óptima", "Demasiado" };
        for (int i = 0; i < 3; i++)
        {
            etiquetasDosis[i] = UIFabrica.Texto(td.transform, nombres[i], 30, Paleta.TextoSuave, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(etiquetasDosis[i].rectTransform, 0.05f + i * 0.3f, 0.26f, 0.35f + i * 0.3f, 0.48f);
        }
        estado = UIFabrica.Texto(td.transform, "", 28, Paleta.Verde, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(estado.rectTransform, 0.05f, 0.03f, 0.95f, 0.26f);

        UIFabrica.Relleno(columna);

        // Efectos en vivo sobre las cinco áreas
        RectTransform filaVivo = UIFabrica.Fila(columna, "En vivo", 190);
        Image tv = UIFabrica.Caja(filaVivo, "Tarjeta vivo", new Color(1f, 0.99f, 0.97f, 0.95f));
        UIFabrica.Anclar(tv.rectTransform, 0, 0, 1, 1);
        UIFabrica.Sombra(tv, 10f, 0.18f);
        foreach (Area area in Areas.Todas)
        {
            int i = (int)area;
            FormasUI ic = Iconos.Crear(tv.transform, area, 96f);
            ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2((i + 0.5f) / 5f, 0.6f);
            RectTransform zf = UIFabrica.Crear("Flecha", tv.transform);
            zf.anchorMin = zf.anchorMax = new Vector2((i + 0.5f) / 5f, 0.6f);
            zf.sizeDelta = new Vector2(40, 40);
            zf.anchoredPosition = new Vector2(40, 40);
            flechas[i] = FormasUI.En(zf.gameObject);
            flechas[i].raycastTarget = false;
            Text n = UIFabrica.Texto(tv.transform, Areas.Nombre(area), 28, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            n.rectTransform.anchorMin = n.rectTransform.anchorMax = new Vector2((i + 0.5f) / 5f, 0.17f);
            n.rectTransform.sizeDelta = new Vector2(200, 40);
        }
        info = TextoEnFila("", 44, 26, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);

        // Botones
        RectTransform filaBotones = UIFabrica.Fila(columna, "Botones", 130);
        Button cancelar = UIFabrica.Boton(filaBotones, "Cancelar", Paleta.Tarjeta, Paleta.Texto, 40, () => this.juego.CancelarAccion());
        UIFabrica.Anclar((RectTransform)cancelar.transform, 0, 0, 0.36f, 1);
        Button terminar = UIFabrica.Boton(filaBotones, "Terminar", Paleta.Coral, Paleta.Blanco, 52, () => this.juego.TerminarAccion());
        UIFabrica.Anclar((RectTransform)terminar.transform, 0.39f, 0, 1, 1);
    }

    private static string Gerundio(DefinicionAccion a)
    {
        switch (a.id)
        {
            case "entrenar": return "Entrenando";
            case "estudiar": return "Estudiando";
            case "trabajar": return "Trabajando";
            case "quedar": return "Con amigos";
            case "pasear": return "Paseando";
            case "videojuegos": return "Jugando";
            case "descansar": return "Descansando";
            case "meditar": return "Meditando";
            default: return a.nombre;
        }
    }

    /// <summary>Prepara la pantalla para una acción concreta.</summary>
    public void Preparar(DefinicionAccion a)
    {
        accion = a;
        haciendo.text = Gerundio(a);
        lugar.text = a.lugar;
        info.text = "Zona óptima: " + PantallaHacer.Rango(a) + " · Demasiado desde " + Jornada.Duracion(a.demasiadoDesde);
        escala = a.demasiadoDesde * 1.35f;
        Iconos.Dibujar(icono, Iconos.AreaDeAccion(a.id), true);
        foreach (Area area in Areas.Todas)
        {
            float v = a.efectoHora.Get(area);
            FormasUI f = flechas[(int)area];
            f.Limpiar();
            if (v > 0.01f)
            {
                f.Circulo(Vector2.zero, 1f, Paleta.Verde);
                f.Poligono(new[] { new Vector2(0, 0.62f), new Vector2(0.55f, -0.05f), new Vector2(-0.55f, -0.05f) }, Color.white);
                f.Rect(new Vector2(0, -0.3f), new Vector2(0.3f, 0.55f), 0f, Color.white);
            }
            else if (v < -0.01f)
            {
                f.Circulo(Vector2.zero, 1f, Paleta.Coral);
                f.Poligono(new[] { new Vector2(0, -0.62f), new Vector2(-0.55f, 0.05f), new Vector2(0.55f, 0.05f) }, Color.white);
                f.Rect(new Vector2(0, 0.3f), new Vector2(0.3f, 0.55f), 0f, Color.white);
            }
        }
        AjustarEscena();
    }

    /// <summary>Recorta la textura de la sala para que llene la pantalla sin deformarse.</summary>
    private void AjustarEscena()
    {
        Rect rc = raizRT.rect;
        if (rc.height < 1f) return;
        float aspecto = rc.width / rc.height;
        float tex = (float)EstudioAvatar.TexturaEscena.width / EstudioAvatar.TexturaEscena.height;
        escena.uvRect = aspecto < tex
            ? new Rect((1f - aspecto / tex) * 0.5f, 0, aspecto / tex, 1)
            : new Rect(0, (1f - tex / aspecto) * 0.5f, 1, tex / aspecto);
    }

    public override void Actualizar()
    {
        DefinicionAccion a = juego.AccionEnCurso;
        if (a == null) return;
        if (a != accion) Preparar(a);
        AjustarEscena();
        EstudioAvatar.PonerAccion(a.id);
        EstudioAvatar.PonerEscena(a.id);

        float segundos = (juego.MinutoJuego - juego.InicioAccion) * 60f;
        int total = Mathf.FloorToInt(segundos);
        tiempo.text = (total / 3600).ToString("00") + ":" + (total / 60 % 60).ToString("00") + ":" + (total % 60).ToString("00");
        int minutos = juego.MinutosAccion;

        // Anillo pequeño: progreso hacia "demasiado"
        anillo.Limpiar();
        float g(float m) { return Mathf.Clamp01(m / escala) * 360f; }
        float progreso = g(minutos);
        anillo.Arco(Vector2.zero, 0.8f, 0.98f, 0f, 360f, Paleta.Fondo);
        anillo.Arco(Vector2.zero, 0.8f, 0.98f, 0f, Mathf.Max(2f, progreso), Paleta.Coral);

        // Barra de dosis horizontal con marcador
        barra.Limpiar();
        Rect rb = barra.rectTransform.rect;
        float W = rb.height > 1f ? rb.width / rb.height : 6f;   // FormasUI usa la mitad del lado corto como unidad
        barraW = W;
        float x(float m) { return -W + 2f * W * Mathf.Clamp01(m / escala); }
        float h = 0.95f;
        barra.Rect(new Vector2((x(0) + x(a.zonaMin)) * 0.5f, 0), new Vector2(x(a.zonaMin) - x(0), h), 0f, Paleta.TramoPoco);
        barra.Rect(new Vector2((x(a.zonaMin) + x(a.zonaMax)) * 0.5f, 0), new Vector2(x(a.zonaMax) - x(a.zonaMin), h), 0f, Paleta.TramoBueno);
        barra.Rect(new Vector2((x(a.zonaMax) + x(a.demasiadoDesde)) * 0.5f, 0), new Vector2(x(a.demasiadoDesde) - x(a.zonaMax), h), 0f, Paleta.TramoLarga);
        barra.Rect(new Vector2((x(a.demasiadoDesde) + x(escala)) * 0.5f, 0), new Vector2(x(escala) - x(a.demasiadoDesde), h), 0f, Paleta.TramoDemasiado);
        barra.Circulo(new Vector2(-W, 0), h * 0.5f, Paleta.TramoPoco);
        barra.Circulo(new Vector2(W, 0), h * 0.5f, Paleta.TramoDemasiado);
        float px = x(minutos);
        barra.Circulo(new Vector2(px, 0), 1.0f, Paleta.Texto);
        barra.Circulo(new Vector2(px, 0), 0.9f, Color.white);
        barra.Circulo(new Vector2(px, 0), 0.55f, Paleta.Coral);
        // Etiquetas centradas bajo cada tramo
        PonerEtiqueta(0, (x(0) + x(a.zonaMin)) * 0.5f);
        PonerEtiqueta(1, (x(a.zonaMin) + x(a.zonaMax)) * 0.5f);
        PonerEtiqueta(2, (x(a.demasiadoDesde) + x(escala)) * 0.5f);

        TramoDosis tramo = a.Tramo(minutos);
        etiquetasDosis[0].color = tramo == TramoDosis.Poco ? Paleta.Texto : Paleta.TextoSuave;
        etiquetasDosis[1].color = tramo == TramoDosis.ZonaOptima ? Paleta.Verde : Paleta.TextoSuave;
        etiquetasDosis[2].color = tramo == TramoDosis.Demasiado ? Paleta.Coral : Paleta.TextoSuave;
        switch (tramo)
        {
            case TramoDosis.Poco: estado.text = "Todavía poco: sigue un rato más"; estado.color = Paleta.TextoSuave; break;
            case TramoDosis.ZonaOptima: estado.text = "Zona óptima: máximo efecto"; estado.color = Paleta.Verde; break;
            case TramoDosis.Larga: estado.text = "Algo larga (sin bonus)"; estado.color = Paleta.Texto; break;
            default: estado.text = "Demasiado: cansa más de lo que aporta"; estado.color = Paleta.Coral; break;
        }
    }

    private void PonerEtiqueta(int i, float centro)
    {
        // centro en -1..1 dentro de la barra (que ocupa 0.05-0.95 de la tarjeta)
        float u = 0.05f + (centro / barraW + 1f) * 0.5f * 0.9f;
        UIFabrica.Anclar(etiquetasDosis[i].rectTransform, u - 0.16f, 0.26f, u + 0.16f, 0.48f);
    }
}
