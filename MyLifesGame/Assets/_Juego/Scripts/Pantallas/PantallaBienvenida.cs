using System.Collections.Generic;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Primera partida, al estilo Duolingo: el avatar te saluda, eliges sudadera y contestas cinco preguntas
/// rápidas que fijan los valores iniciales de las cinco áreas. Se puede saltar en cualquier momento.
/// </summary>
public class PantallaBienvenida : Pantalla
{
    private struct Pregunta
    {
        public Area area; public string texto; public string[] opciones; public float[] valores;
    }

    private static readonly Pregunta[] Preguntas =
    {
        new Pregunta { area = Area.Fisico, texto = "¿Cuánto te mueves normalmente?", opciones = new[] { "Casi nada", "Algo", "Bastante" }, valores = new[] { 30f, 50f, 68f } },
        new Pregunta { area = Area.Mente, texto = "¿Cuánto lees, estudias o aprendes?", opciones = new[] { "Poco", "A ratos", "Mucho" }, valores = new[] { 32f, 50f, 68f } },
        new Pregunta { area = Area.Social, texto = "¿Cuánto ves a tus amigos o familia?", opciones = new[] { "Muy poco", "Algunas veces", "A menudo" }, valores = new[] { 28f, 48f, 66f } },
        new Pregunta { area = Area.Energia, texto = "¿Qué tal duermes últimamente?", opciones = new[] { "Mal", "Regular", "Bien" }, valores = new[] { 55f, 70f, 85f } },
        new Pregunta { area = Area.Animo, texto = "¿Cómo estás de ánimo estos días?", opciones = new[] { "Bajo", "Normal", "Bien" }, valores = new[] { 34f, 50f, 66f } },
    };
    private static readonly string[] PrendasRegalo = { "teal", "coral", "mostaza" };
    private const int Pasos = 8; // 0 hola, 1 sudadera, 2-6 preguntas, 7 resumen

    private readonly Image barra;
    private readonly RectTransform contenido;
    private readonly Button saltar;
    private int paso;
    private readonly float[] valores = { 50, 50, 50, 75, 50 };
    private string prenda = "teal";
    private AvatarDibujo avatar;

    public PantallaBienvenida(Juego juego, Transform canvas) : base(juego, canvas, "Pantalla bienvenida")
    {
        // Barra de progreso arriba y botón "Saltar" (como Duolingo)
        RectTransform cab = UIFabrica.Fila(columna, "Cabecera", 80);
        Image fondoBarra = UIFabrica.Caja(cab, "Fondo barra", Paleta.Gris);
        UIFabrica.Anclar(fondoBarra.rectTransform, 0, 0.36f, 0.76f, 0.64f);
        barra = UIFabrica.Caja(fondoBarra.transform, "Barra", Paleta.Verde);
        barra.type = Image.Type.Filled; barra.fillMethod = Image.FillMethod.Horizontal;
        UIFabrica.Anclar(barra.rectTransform, 0, 0, 1, 1);
        saltar = UIFabrica.Boton(cab, "Saltar", Paleta.Fondo, Paleta.TextoSuave, 30, () => Terminar(true));
        UIFabrica.Anclar((RectTransform)saltar.transform, 0.8f, 0.1f, 1f, 0.9f);

        contenido = UIFabrica.Columna(UIFabrica.Fila(columna, "Contenido", 1500), "Pasos", 0, 24);
        UIFabrica.Relleno(columna);
    }

    protected override void AlMostrar() { IrAPaso(0); }

    /// <summary>Muestra el paso indicado (público para las capturas automáticas).</summary>
    public void IrAPaso(int p)
    {
        paso = Mathf.Clamp(p, 0, Pasos - 1);
        barra.fillAmount = (paso + 1) / (float)Pasos;
        for (int i = contenido.childCount - 1; i >= 0; i--) { GameObject h = contenido.GetChild(i).gameObject; h.SetActive(false); h.transform.SetParent(null, false); Object.Destroy(h); }
        avatar = null;
        if (paso == 0) PasoHola();
        else if (paso == 1) PasoSudadera();
        else if (paso <= 6) PasoPregunta(Preguntas[paso - 2]);
        else PasoResumen();
    }

    private Text Texto(string t, int alto, int tam, Color c, FontStyle estilo = FontStyle.Normal)
    {
        Text x = UIFabrica.Texto(UIFabrica.Fila(contenido, "Texto", alto), t, tam, c, TextAnchor.MiddleCenter, estilo);
        UIFabrica.Anclar(x.rectTransform, 0, 0, 1, 1);
        return x;
    }

    private void Avatar(int alto)
    {
        RectTransform f = UIFabrica.Fila(contenido, "Avatar", alto);
        avatar = new AvatarDibujo(f);
        avatar.UsarSala();
        UsaEscena = true;
        avatar.Actualizar(TipoEstadoAvatar.EnForma, Catalogos.BuscarPrenda(prenda).colorHex);
    }

    private void Boton(string t, Color fondo, System.Action alPulsar)
    {
        Button b = UIFabrica.Boton(UIFabrica.Fila(contenido, "Botón", 140), t, fondo, Paleta.Blanco, 50, alPulsar);
        UIFabrica.Anclar((RectTransform)b.transform, 0, 0, 1, 1);
    }

    private void PasoHola()
    {
        Avatar(760);
        Texto("¡Hola! Soy tu avatar", 100, 62, Paleta.Texto, FontStyle.Bold);
        Texto("Lo que hagas en tu vida real me hace crecer: moverte, aprender, quedar con gente, descansar... Vamos a conocernos en un minuto.", 170, 34, Paleta.TextoSuave);
        Boton("¡Vamos!", Paleta.Verde, () => IrAPaso(1));
    }

    private void PasoSudadera()
    {
        Avatar(640);
        Texto("Elige tu sudadera", 90, 56, Paleta.Texto, FontStyle.Bold);
        Texto("Regalo de bienvenida. Podrás comprar más con monedas.", 60, 30, Paleta.TextoSuave);
        RectTransform fila = UIFabrica.Fila(contenido, "Sudaderas", 300);
        for (int i = 0; i < PrendasRegalo.Length; i++)
        {
            Prenda p = Catalogos.BuscarPrenda(PrendasRegalo[i]);
            bool elegida = prenda == p.id;
            Button b = UIFabrica.Boton(fila, "", elegida ? Paleta.Hex("E4F2E1") : Paleta.Tarjeta, Paleta.Texto, 28, () => { prenda = p.id; IrAPaso(1); });
            UIFabrica.Anclar((RectTransform)b.transform, i / 3f + 0.01f, 0, (i + 1) / 3f - 0.01f, 1);
            if (elegida)
            {
                Outline o = b.gameObject.AddComponent<Outline>();
                o.effectColor = Paleta.Verde; o.effectDistance = new Vector2(5, -5);
            }
            RectTransform zd = UIFabrica.Crear("Dibujo", b.transform);
            zd.anchorMin = zd.anchorMax = new Vector2(0.5f, 0.6f);
            zd.sizeDelta = new Vector2(170, 170);
            FormasUI f = FormasUI.En(zd.gameObject);
            f.raycastTarget = false;
            Iconos.Sudadera(f, Paleta.Hex(p.colorHex), false);
            string corto = p.nombre.Replace("Sudadera ", "");
            Text t = UIFabrica.Texto(b.transform, char.ToUpper(corto[0]) + corto.Substring(1), 30, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFabrica.Anclar(t.rectTransform, 0, 0.04f, 1, 0.25f);
        }
        Boton("Continuar", Paleta.Verde, () => IrAPaso(2));
    }

    private void PasoPregunta(Pregunta q)
    {
        RectTransform filaIcono = UIFabrica.Fila(contenido, "Icono", 230);
        FormasUI ic = Iconos.Crear(filaIcono, q.area, 200f);
        ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        Text area = Texto(Areas.Nombre(q.area), 60, 38, Paleta.DeArea(q.area), FontStyle.Bold);
        Texto(q.texto, 150, 54, Paleta.Texto, FontStyle.Bold);
        Texto("Sin presión: sirve para empezar con tus barras más o menos donde estás.", 70, 28, Paleta.TextoSuave);
        for (int i = 0; i < q.opciones.Length; i++)
        {
            int indice = i;
            float v = q.valores[i];
            bool elegida = Mathf.Approximately(valores[(int)q.area], v);
            RectTransform fila = UIFabrica.Fila(contenido, "Opción", 150);
            Button b = UIFabrica.Boton(fila, q.opciones[i], elegida ? Color.Lerp(Paleta.Tarjeta, Paleta.DeArea(q.area), 0.25f) : Paleta.Tarjeta, Paleta.Texto, 40, () =>
            {
                valores[(int)q.area] = q.valores[indice];
                IrAPaso(paso + 1);
            });
            UIFabrica.Anclar((RectTransform)b.transform, 0, 0, 1, 1);
            Text t = b.GetComponentInChildren<Text>();
            t.alignment = TextAnchor.MiddleLeft;
            UIFabrica.Anclar(t.rectTransform, 0.2f, 0, 0.95f, 1);
            RectTransform zc = UIFabrica.Crear("Cara", b.transform);
            zc.anchorMin = zc.anchorMax = new Vector2(0, 0.5f);
            zc.sizeDelta = new Vector2(100, 100);
            zc.anchoredPosition = new Vector2(90, 0);
            FormasUI cara = FormasUI.En(zc.gameObject);
            cara.raycastTarget = false;
            Iconos.Carita(cara, i * 2, Color.Lerp(Paleta.Blanco, Paleta.DeArea(q.area), 0.35f + i * 0.2f));
        }
        if (paso > 2)
        {
            Button atras = UIFabrica.Boton(UIFabrica.Fila(contenido, "Atrás", 80), "Atrás", Paleta.Gris, Paleta.Texto, 30, () => IrAPaso(paso - 1));
            UIFabrica.Anclar((RectTransform)atras.transform, 0.3f, 0, 0.7f, 1);
        }
    }

    private void PasoResumen()
    {
        Avatar(560);
        Texto("¡Listo! Así empiezas", 90, 56, Paleta.Texto, FontStyle.Bold);
        RectTransform tarjeta = UIFabrica.Fila(contenido, "Resumen", 260);
        Image t = UIFabrica.Caja(tarjeta, "Fondo", Paleta.Tarjeta);
        UIFabrica.Anclar(t.rectTransform, 0, 0, 1, 1);
        UIFabrica.Sombra(t, 5f, 0.1f);
        foreach (Area a in Areas.Todas)
        {
            int i = (int)a;
            float x = (i + 0.5f) / 5f;
            FormasUI ic = Iconos.Crear(t.transform, a, 90f);
            ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(x, 0.66f);
            Text v = UIFabrica.Texto(t.transform, Mathf.RoundToInt(valores[i]).ToString(), 44, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
            v.rectTransform.anchorMin = v.rectTransform.anchorMax = new Vector2(x, 0.24f);
            v.rectTransform.sizeDelta = new Vector2(160, 60);
        }
        Texto("Ahora, cada día: un check-in rápido por la mañana y haz cosas de verdad. Yo te acompaño en la ciudad.", 140, 32, Paleta.TextoSuave);
        Boton("Empezar mi primer día", Paleta.Coral, () => Terminar(false));
    }

    public override void Actualizar()
    {
        saltar.gameObject.SetActive(paso < Pasos - 1);
    }

    private void Terminar(bool saltado)
    {
        juego.TerminarBienvenida(saltado ? null : valores, saltado ? "teal" : prenda);
    }
}
