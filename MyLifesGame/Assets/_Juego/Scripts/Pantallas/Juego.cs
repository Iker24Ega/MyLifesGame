using System;
using System.Collections.Generic;
using System.Text;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Arranca la demo al pulsar Play (sin montar nada en la escena): carga o crea la partida, construye la
/// ciudad 3D y la interfaz, lleva el reloj del juego, guarda en disco y conecta las pantallas con el motor.
/// </summary>
public class Juego : MonoBehaviour
{
    /// <summary>Modo de capturas automáticas: partida en memoria, sin guardar.</summary>
    public static bool ModoCapturas;

    public TablaAcciones Tabla { get; private set; }
    public DatosPartida Datos { get; private set; }
    public Jornada Jornada { get; private set; }
    public Progreso Progreso { get; private set; }
    public Ciudad3D Ciudad { get; private set; }

    /// <summary>Minuto del juego desde el día 1 a las 00:00.</summary>
    public float MinutoJuego;
    /// <summary>Minutos de juego por segundo real (1/60 = tiempo real).</summary>
    public float Velocidad = 1f;
    public bool ConAmigos;
    public DefinicionAccion AccionEnCurso { get; private set; }
    public float InicioAccion { get; private set; }
    public bool VieneDeAusencia { get; private set; }
    public int MinutosAusencia { get; private set; }

    public int MinutoEntero { get { return Mathf.FloorToInt(MinutoJuego); } }
    public int Dia { get { return MinutoEntero / 1440 + 1; } }
    public int MinutosAccion { get { return AccionEnCurso == null ? 0 : Mathf.FloorToInt(MinutoJuego - InicioAccion); } }

    private GameObject canvasGO;
    private PantallaCiudad pCiudad;
    private PantallaBienvenida pBienvenida;
    public PantallaBienvenida Bienvenida { get { return pBienvenida; } }
    private PantallaHoy pHoy;
    private PantallaEquilibrio pEquilibrio;
    private PantallaHacer pHacer;
    private PantallaCronometro pCronometro;
    private PantallaResultado pResultado;
    private PantallaCheckIn pCheckIn;
    private PantallaRegreso pRegreso;
    private PantallaAvatar pAvatar;
    private PantallaAlbum pAlbum;
    private Pantalla actual, anterior;

    private Text aviso;
    private float finAviso;
    private float proximoGuardado;
    private int diaPreguntas = -1, preguntasHoy;

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
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        DatosPartida cargada = ModoCapturas ? null : Guardado.Cargar();
        if (cargada != null) Inicializar(cargada, true);
        else Inicializar(PartidaNueva(), false);
        if (ModoCapturas) gameObject.AddComponent<CapturadorPantallas>();
    }

    private DatosPartida PartidaNueva()
    {
        var d = new DatosPartida();
        Jornada j = Jornada.Nueva(Tabla, new float[] { 50, 50, 50, 75, 50 }, 8 * 60);
        d.jornada = j.Datos;
        d.minutoJuego = 8 * 60;
        d.velocidad = 1f;
        return d;
    }

    private void Inicializar(DatosPartida d, bool cargada)
    {
        Datos = d;
        if (cargada && d.version < 2)
        {
            // Partidas anteriores al onboarding: no se les enseña la bienvenida
            d.bienvenidaHecha = true;
            d.guiaHacerHecha = true;
            d.version = 2;
        }
        Jornada = new Jornada(Tabla, d.jornada);
        Progreso = new Progreso(d.progreso);
        MinutoJuego = Mathf.Max(d.minutoJuego, d.jornada.minutoActual);
        Velocidad = d.velocidad > 0 ? d.velocidad : 1f;
        ConAmigos = d.conAmigos;
        AccionEnCurso = null;
        VieneDeAusencia = false;
        Jornada.AlRegistrar += EscribirDiario;

        // Ciudad 3D
        var goCiudad = new GameObject("Ciudad");
        Ciudad = goCiudad.AddComponent<Ciudad3D>();
        Ciudad.Construir(this, d.ciudad);
        Ciudad.PonerRopa(Catalogos.BuscarPrenda(Progreso.Datos.ropaPuesta).colorHex);

        // Interfaz
        Transform canvas = CrearCanvas();
        pCiudad = new PantallaCiudad(this, canvas);
        pHoy = new PantallaHoy(this, canvas);
        pEquilibrio = new PantallaEquilibrio(this, canvas);
        pHacer = new PantallaHacer(this, canvas);
        pCronometro = new PantallaCronometro(this, canvas);
        pResultado = new PantallaResultado(this, canvas);
        pCheckIn = new PantallaCheckIn(this, canvas);
        pRegreso = new PantallaRegreso(this, canvas);
        pAvatar = new PantallaAvatar(this, canvas);
        pAlbum = new PantallaAlbum(this, canvas);
        pBienvenida = new PantallaBienvenida(this, canvas);
        foreach (Pantalla p in Todas()) p.Ocultar();
        CrearAviso(canvas);
        Ciudad.AlTocarEdificio += pCiudad.AlTocarEdificio;
        actual = null;
        anterior = null;

        Pantalla primera = pCiudad;
        if (cargada)
        {
            double segundos = (DateTime.UtcNow.Ticks - d.guardadoUtcTicks) / (double)TimeSpan.TicksPerSecond;
            int ausencia = (int)Math.Min(24 * 60, Math.Max(0, segundos * Velocidad));
            if (!string.IsNullOrEmpty(d.accionEnCurso))
            {
                AccionEnCurso = Tabla.Buscar(d.accionEnCurso);
                InicioAccion = d.inicioAccion;
                MinutoJuego += ausencia; // el cronómetro siguió contando
                pCronometro.Preparar(AccionEnCurso);
                primera = pCronometro;
            }
            else if (ausencia >= Ajustes.MinutosAusenciaParaRegreso)
            {
                Jornada.SaltarTiempo(ausencia);
                MinutoJuego = Jornada.MinutoActual;
                MinutosAusencia = ausencia;
                VieneDeAusencia = true;
                primera = pRegreso;
            }
            else
            {
                MinutoJuego += ausencia;
            }
            Debug.Log("[Diario] Partida cargada. Ausencia: " + ausencia + " min de juego.");
        }
        else Debug.Log("[Diario] Partida nueva.");

        if (primera == pCiudad && !Jornada.Datos.checkInHecho) primera = pCheckIn;
        if (!d.bienvenidaHecha) primera = pBienvenida;
        Mostrar(primera);
        GuardarAhora();
    }

    private IEnumerable<Pantalla> Todas()
    {
        return new Pantalla[] { pCiudad, pHoy, pEquilibrio, pHacer, pCronometro, pResultado, pCheckIn, pRegreso, pAvatar, pAlbum, pBienvenida };
    }

    private Transform CrearCanvas()
    {
        canvasGO = new GameObject("Interfaz", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var escalador = canvasGO.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1080, 1920);
        escalador.matchWidthOrHeight = 0.5f;
        return canvasGO.transform;
    }

    private void CrearAviso(Transform canvas)
    {
        // Tarjeta blanca pequeña arriba, bajo la cabecera, con una medalla: no tapa al avatar ni los menús.
        Image fondo = UIFabrica.Caja(canvas, "Aviso", Paleta.Tarjeta);
        fondo.raycastTarget = false;
        UIFabrica.Sombra(fondo, 10f, 0.22f);
        RectTransform rt = fondo.rectTransform;
        rt.anchorMin = new Vector2(0.1f, 1);
        rt.anchorMax = new Vector2(0.9f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 112);
        rt.anchoredPosition = new Vector2(0, -330);
        Image franja = UIFabrica.Caja(fondo.transform, "Franja", Paleta.Hex("F2B632"));
        UIFabrica.Anclar(franja.rectTransform, 0, 0, 0.018f, 1);
        RectTransform zm = UIFabrica.Crear("Medalla", fondo.transform);
        zm.anchorMin = zm.anchorMax = new Vector2(0, 0.5f);
        zm.sizeDelta = new Vector2(78, 78);
        zm.anchoredPosition = new Vector2(70, 0);
        Iconos.Medalla(FormasUI.En(zm.gameObject), MyLifesGame.Motor.Area.Animo, true);
        aviso = UIFabrica.Texto(fondo.transform, "", 34, Paleta.Texto, TextAnchor.MiddleLeft, FontStyle.Bold);
        UIFabrica.Anclar(aviso.rectTransform, 0.17f, 0, 0.96f, 1);
        fondo.gameObject.SetActive(false);
    }

    /// <summary>Muestra un aviso arriba unos segundos (logros, misiones...).</summary>
    private float inicioAviso;

    public void Avisar(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return;
        aviso.text = texto;
        Sonidos.Aviso();
        aviso.transform.parent.gameObject.SetActive(true);
        aviso.transform.parent.SetAsLastSibling();
        int lineas = texto.Split('\n').Length;
        ((RectTransform)aviso.transform.parent).sizeDelta = new Vector2(0, 70 + 44 * lineas);
        inicioAviso = Time.unscaledTime;
        finAviso = Time.unscaledTime + 3.5f;
    }

    private void AvisarTodos(List<string> mensajes)
    {
        if (mensajes.Count > 0) Avisar(string.Join("\n", mensajes.ToArray()));
    }

    // ---------------------------------------------------------------- bucle

    private void Update()
    {
        MinutoJuego += Time.deltaTime * Velocidad;
        if (AccionEnCurso == null) Jornada.Avanzar(MinutoEntero);
        if (actual != null) actual.Actualizar();

        if (aviso != null && aviso.transform.parent.gameObject.activeSelf)
        {
            if (Time.unscaledTime > finAviso) aviso.transform.parent.gameObject.SetActive(false);
            // Entra bajando y rebotando un poco
            var rtAviso = (RectTransform)aviso.transform.parent;
            float t = Mathf.Clamp01((Time.unscaledTime - inicioAviso) / 0.35f);
            float rebote = 1f - Mathf.Pow(1f - t, 3f);
            rtAviso.anchoredPosition = new Vector2(0, -330 + (1f - rebote) * 120f);
        }

        if (Time.unscaledTime > proximoGuardado)
        {
            proximoGuardado = Time.unscaledTime + 5f;
            string causa;
            Ciudad.PonerEstado(EstadoAvatar.Calcular(Jornada.Estado, VieneDeAusencia, out causa));
            GuardarAhora();
        }
    }

    private void OnApplicationPause(bool pausa) { if (pausa) GuardarAhora(); }
    private void OnApplicationQuit() { GuardarAhora(); }

    public void GuardarAhora()
    {
        if (Datos == null || ModoCapturas) return;
        Datos.minutoJuego = MinutoJuego;
        Datos.velocidad = Velocidad;
        Datos.conAmigos = ConAmigos;
        Datos.accionEnCurso = AccionEnCurso != null ? AccionEnCurso.id : "";
        Datos.inicioAccion = InicioAccion;
        Guardado.Guardar(Datos);
    }

    private void EscribirDiario(Evento e)
    {
        var sb = new StringBuilder("[Diario] Día ");
        sb.Append(e.minutoInicio / 1440 + 1).Append(' ').Append(Jornada.Hora(e.minutoInicio)).Append(" | ").Append(e.texto);
        if (!string.IsNullOrEmpty(e.nota)) sb.Append(": ").Append(e.nota);
        sb.Append(" | F ").Append(Mathf.RoundToInt(e.valores[0]))
          .Append(" M ").Append(Mathf.RoundToInt(e.valores[1]))
          .Append(" S ").Append(Mathf.RoundToInt(e.valores[2]))
          .Append(" E ").Append(Mathf.RoundToInt(e.valores[3]))
          .Append(" Á ").Append(Mathf.RoundToInt(e.valores[4]));
        Debug.Log(sb.ToString());
    }

    // ---------------------------------------------------------------- navegación

    public void Mostrar(Pantalla pantalla)
    {
        if (actual != null && actual != pantalla) { actual.Ocultar(); anterior = actual; }
        if (pantalla != pCronometro) EstudioAvatar.QuitarEscena();
        actual = pantalla;
        pantalla.Mostrar();
        pantalla.Actualizar();
        Ciudad.Activa = pantalla == pCiudad;
    }

    public void IrAPestana(int indice)
    {
        switch (indice)
        {
            case 0: Mostrar(pCiudad); break;
            case 1: Mostrar(pHoy); break;
            case 2: IrAHacer(); break;
            case 3: Mostrar(pAvatar); break;
            default: Mostrar(pAlbum); break;
        }
    }

    public void IrAHacer() { Datos.guiaHacerHecha = true; Mostrar(pHacer); }

    /// <summary>Para las capturas automáticas: simula tocar un edificio de la ciudad.</summary>
    public void CapturaTocarEdificio(int indice) { Mostrar(pCiudad); pCiudad.AlTocarEdificio(indice); }
    public void IrAEquilibrio() { Mostrar(pEquilibrio); }

    public void Volver()
    {
        if (anterior != null && anterior != pHacer && anterior != pCronometro && anterior != pResultado) Mostrar(anterior);
        else Mostrar(pCiudad);
    }

    // ---------------------------------------------------------------- acciones

    public void EmpezarAccion(DefinicionAccion accion)
    {
        Jornada.Avanzar(MinutoEntero);
        MinutoJuego = MinutoEntero;
        InicioAccion = MinutoEntero;
        AccionEnCurso = accion;
        pCronometro.Preparar(accion);
        Mostrar(pCronometro);
        Debug.Log("[Diario] Empieza: " + accion.nombre + " a las " + Jornada.Hora(MinutoEntero));
        GuardarAhora();
    }

    public void TerminarAccion()
    {
        DefinicionAccion accion = AccionEnCurso;
        int minutos = Mathf.Max(1, MinutosAccion);
        AccionEnCurso = null;
        bool conAmigos = ConAmigos && accion.socialConAmigosPorHora > 0;
        ResultadoAccion r = Jornada.HacerAccion(accion.id, (int)InicioAccion, minutos, conAmigos);
        MinutoJuego = Mathf.Max(MinutoJuego, Jornada.MinutoActual);
        Progreso.AlTerminarAccion(r, Jornada);
        Debug.Log("[Diario] Recompensa: +" + r.xp + " XP, +" + r.monedas + " monedas. " + string.Join(" · ", r.mensajes.ToArray()));

        if (diaPreguntas != Dia) { diaPreguntas = Dia; preguntasHoy = 0; }
        bool preguntar = (r.minutos >= 60 || r.tramo == TramoDosis.Demasiado) && preguntasHoy < 2;
        if (preguntar) preguntasHoy++;
        pResultado.Preparar(r, preguntar);
        Mostrar(pResultado);
        GuardarAhora();
    }

    public void CancelarAccion()
    {
        Debug.Log("[Diario] Acción cancelada: no cuenta.");
        AccionEnCurso = null;
        Mostrar(pCiudad);
        GuardarAhora();
    }

    public void Valorar(ResultadoAccion r, int valoracion, string motivo)
    {
        Jornada.Valorar(r, valoracion, motivo);
        GuardarAhora();
    }

    public void HacerRapida(AccionRapida tipo)
    {
        Jornada.HacerAccionRapida(tipo);
        var mensajes = new List<string>();
        Progreso.RevisarLogrosDeEstado(Jornada, mensajes);
        AvisarTodos(mensajes);
    }

    public void SumarPasos(int pasos)
    {
        Jornada.SumarPasos(pasos);
        Jornada.Anotar("+" + pasos + " pasos (total " + Jornada.Datos.pasosHoy + ")");
        var mensajes = new List<string>();
        Progreso.RevisarLogrosDeEstado(Jornada, mensajes);
        AvisarTodos(mensajes);
    }

    public void Dormir()
    {
        int diaAntes = Jornada.Dia;
        Jornada.Dormir(MinutoEntero, 8f);
        // Si se ha dormido de día (por ejemplo, para probar), se salta en pausa hasta las 08:00 del día siguiente,
        // para que siempre empiece un día nuevo con su check-in.
        if (Jornada.Dia == diaAntes)
        {
            int manana = diaAntes * 1440 + 8 * 60;
            Jornada.SaltarTiempo(manana - Jornada.MinutoActual);
        }
        MinutoJuego = Jornada.MinutoActual;
        Mostrar(pCheckIn);
        GuardarAhora();
    }

    public void CompletarCheckIn(int sueno, int sentir, List<Area> focos, bool arrastro)
    {
        Jornada.CheckIn(sueno, sentir, focos, arrastro);
        Progreso.GenerarMisiones(Jornada);
        Mostrar(pHoy);
        Avisar("¡A por el día! Tienes " + Progreso.Datos.misiones.Count + " misiones en Hoy.");
        GuardarAhora();
    }

    public void TerminarRegreso(List<KeyValuePair<string, int>> acciones)
    {
        var mensajes = new List<string>();
        foreach (KeyValuePair<string, int> a in acciones)
        {
            ResultadoAccion r = Jornada.RegistrarPosterior(a.Key, a.Value);
            Progreso.AlTerminarAccion(r, Jornada);
            mensajes.AddRange(r.mensajes);
        }
        VieneDeAusencia = false;
        if (acciones.Count > 0) mensajes.Insert(0, "Registrado. ¡Bienvenido de vuelta!");
        AvisarTodos(mensajes);
        Mostrar(Jornada.Datos.checkInHecho ? (Pantalla)pCiudad : pCheckIn);
        GuardarAhora();
    }

    public void AsignarRol(int edificio, string rol)
    {
        Datos.ciudad.roles[edificio] = rol;
        Ciudad.ActualizarEdificio(edificio);
        RolEdificio r = Catalogos.BuscarRol(rol);
        Debug.Log("[Diario] Edificio " + edificio + ": " + (r != null ? r.nombre : "sin papel"));
        var mensajes = new List<string>();
        if (Ciudad.RolesAsignados() >= 4) Progreso.Desbloquear("constructor", mensajes);
        AvisarTodos(mensajes);
        GuardarAhora();
    }

    public void ElegirPrenda(Prenda p)
    {
        var mensajes = new List<string>();
        if (Progreso.Comprada(p.id)) Progreso.Poner(p);
        else
        {
            pAvatar.MostrarAviso(Progreso.Comprar(p, mensajes));
            if (Progreso.Comprada(p.id)) Sonidos.Moneda();
        }
        Ciudad.PonerRopa(Catalogos.BuscarPrenda(Progreso.Datos.ropaPuesta).colorHex);
        AvisarTodos(mensajes);
        GuardarAhora();
    }

    public void SimularAusencia(int minutos)
    {
        Jornada.SaltarTiempo(minutos);
        MinutoJuego = Jornada.MinutoActual;
        MinutosAusencia = minutos;
        VieneDeAusencia = true;
        Debug.Log("[Diario] Ausencia simulada de " + minutos + " min (sin castigo: barras en pausa).");
        Mostrar(pRegreso);
    }

    public void CambiarSensibilidadPantallas()
    {
        SensibilidadPantallas s = Jornada.Datos.pantallas;
        Jornada.Datos.pantallas = s == SensibilidadPantallas.Nada ? SensibilidadPantallas.Algo
            : s == SensibilidadPantallas.Algo ? SensibilidadPantallas.Mucho : SensibilidadPantallas.Nada;
    }

    /// <summary>Fin del onboarding: aplica los valores iniciales (o los de serie si se saltó) y la sudadera regalada.</summary>
    public void TerminarBienvenida(float[] valores, string prenda)
    {
        if (valores != null)
            foreach (Area a in Areas.Todas) Jornada.Estado[a] = valores[(int)a];
        if (!Progreso.Datos.ropaComprada.Contains(prenda)) Progreso.Datos.ropaComprada.Add(prenda);
        Progreso.Datos.ropaPuesta = prenda;
        Ciudad.PonerRopa(Catalogos.BuscarPrenda(prenda).colorHex);
        Datos.bienvenidaHecha = true;
        GuardarAhora();
        Mostrar(Jornada.Datos.checkInHecho ? (Pantalla)pCiudad : pCheckIn);
    }

    public void NuevaPartida()
    {
        Guardado.Borrar();
        if (canvasGO != null) Destroy(canvasGO);
        if (Ciudad != null) Destroy(Ciudad.gameObject);
        Inicializar(PartidaNueva(), false);
    }
}
