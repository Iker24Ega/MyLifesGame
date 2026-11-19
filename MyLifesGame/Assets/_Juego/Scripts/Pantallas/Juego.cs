using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Arranca el prototipo al pulsar Play (sin montar nada en la escena): crea la interfaz,
/// lleva el reloj del juego y conecta las pantallas con el motor del equilibrio.
/// </summary>
public class Juego : MonoBehaviour
{
    public TablaAcciones Tabla { get; private set; }
    public Jornada Jornada { get; private set; }

    /// <summary>Minuto del juego desde el día 1 a las 00:00 (empieza a las 08:00).</summary>
    public float MinutoJuego;

    /// <summary>Minutos de juego por segundo real (1/60 = tiempo real).</summary>
    public float Velocidad = 1f;

    public bool ConAmigos;
    public DefinicionAccion AccionEnCurso { get; private set; }
    public float InicioAccion { get; private set; }

    public int MinutoEntero { get { return Mathf.FloorToInt(MinutoJuego); } }
    public int Dia { get { return MinutoEntero / 1440 + 1; } }
    public int MinutosAccion { get { return AccionEnCurso == null ? 0 : Mathf.FloorToInt(MinutoJuego - InicioAccion); } }

    private PantallaPrincipal principal;
    private PantallaHacer hacer;
    private PantallaCronometro cronometro;
    private PantallaResultado resultado;
    private Pantalla actual;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Arrancar()
    {
        if (FindAnyObjectByType<Juego>() == null)
            new GameObject("Juego").AddComponent<Juego>();
    }

    private void Awake()
    {
        TextAsset json = Resources.Load<TextAsset>("acciones");
        Tabla = JsonUtility.FromJson<TablaAcciones>(json.text);

        // Valores iniciales provisionales (el onboarding se diseñará al final).
        MinutoJuego = 8 * 60;
        Jornada = new Jornada(Tabla, new EstadoAreas(50, 50, 50, 75, 50), MinutoEntero);

        Transform canvas = CrearCanvas();
        principal = new PantallaPrincipal(this, canvas);
        hacer = new PantallaHacer(this, canvas);
        cronometro = new PantallaCronometro(this, canvas);
        resultado = new PantallaResultado(this, canvas);
        hacer.Ocultar();
        cronometro.Ocultar();
        resultado.Ocultar();
        Mostrar(principal);
    }

    private Transform CrearCanvas()
    {
        var go = new GameObject("Interfaz", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var escalador = go.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1080, 1920);
        escalador.matchWidthOrHeight = 0.5f;

        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        return go.transform;
    }

    private void Update()
    {
        MinutoJuego += Time.deltaTime * Velocidad;
        if (AccionEnCurso == null) Jornada.Avanzar(MinutoEntero);
        if (actual != null) actual.Actualizar();
    }

    public void Mostrar(Pantalla pantalla)
    {
        if (actual != null) actual.Ocultar();
        actual = pantalla;
        pantalla.Mostrar();
        pantalla.Actualizar();
    }

    public void IrAPrincipal() { Mostrar(principal); }
    public void IrAHacer() { Mostrar(hacer); }

    public void EmpezarAccion(DefinicionAccion accion)
    {
        Jornada.Avanzar(MinutoEntero);
        MinutoJuego = MinutoEntero;
        InicioAccion = MinutoEntero;
        AccionEnCurso = accion;
        cronometro.Preparar(accion);
        Mostrar(cronometro);
    }

    public void TerminarAccion()
    {
        DefinicionAccion accion = AccionEnCurso;
        int minutos = Mathf.Max(1, MinutosAccion);
        AccionEnCurso = null;
        bool conAmigos = ConAmigos && accion.socialConAmigosPorHora > 0;
        ResultadoAccion r = Jornada.HacerAccion(accion.id, (int)InicioAccion, minutos, conAmigos);
        MinutoJuego = Mathf.Max(MinutoJuego, Jornada.MinutoActual);
        resultado.Preparar(r);
        Mostrar(resultado);
    }

    public void CancelarAccion()
    {
        AccionEnCurso = null;
        Mostrar(principal);
    }

    public void Dormir()
    {
        Jornada.Dormir(MinutoEntero, 8f);
        MinutoJuego = Jornada.MinutoActual;
    }

    public void HacerRapida(AccionRapida tipo)
    {
        Jornada.HacerAccionRapida(tipo);
        string texto = tipo == AccionRapida.Agua ? "Vaso de agua" : tipo == AccionRapida.ComerBien ? "Comer bien" : "Ducha";
        Jornada.Anotar(texto);
    }
}
