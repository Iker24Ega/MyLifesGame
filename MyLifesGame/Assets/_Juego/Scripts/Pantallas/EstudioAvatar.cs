using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Estudio de retrato: un avatar 3D aparte, lejos de la ciudad, con su propia cámara que pinta en una textura.
/// Las pantallas de Avatar y Regreso muestran esa textura, así el avatar de la interfaz es el mismo que el de la ciudad.
/// </summary>
public static class EstudioAvatar
{
    public const int Capa = 30;
    private static readonly Vector3 Lugar = new Vector3(900f, 0f, 900f);
    private static RenderTexture textura;
    private static Avatar3D avatar;
    private static Transform raizEstudio;
    /// <summary>Capa de las salas 3D (solo la ve la cámara de escena).</summary>
    public const int CapaEscena = 31;
    private static Camera camEscena;
    private static RenderTexture texEscena;
    private static Transform salas;
    private static string salaActual, accionSala;
    private static Camera camara;

    public static RenderTexture Textura
    {
        get { Preparar(); return textura; }
    }

    private static void Preparar()
    {
        if (textura != null && avatar != null) return;
        var raiz = new GameObject("Estudio avatar").transform;
        raiz.position = Lugar;
        Object.DontDestroyOnLoad(raiz.gameObject);
        raizEstudio = raiz;

        avatar = Avatar3D.Crear(raiz, Capa);
        avatar.transform.localRotation = Quaternion.Euler(0, 18, 0);

        textura = new RenderTexture(768, 960, 24, RenderTextureFormat.ARGB32) { name = "Retrato avatar", antiAliasing = 4 };
        camara = new GameObject("Cámara estudio").AddComponent<Camera>();
        camara.transform.SetParent(raiz, false);
        camara.transform.localPosition = new Vector3(0, 2.6f, 9.4f);
        camara.transform.LookAt(Lugar + new Vector3(0, 2.05f, 0));
        camara.fieldOfView = 30f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0, 0, 0, 0);
        camara.cullingMask = 1 << Capa;
        camara.targetTexture = textura;
        camara.nearClipPlane = 0.5f;
        camara.farClipPlane = 30f;
        var datos = camara.GetUniversalAdditionalCameraData();
        if (datos != null) datos.renderPostProcessing = false;

        // Luz propia suave para el retrato
        var luz = new GameObject("Luz estudio").AddComponent<Light>();
        luz.transform.SetParent(raiz, false);
        luz.type = LightType.Point;
        luz.range = 20f;
        luz.intensity = 6f;
        luz.color = new Color(1f, 0.95f, 0.88f);
        luz.cullingMask = (1 << Capa) | (1 << CapaEscena);
        luz.transform.localPosition = new Vector3(2.5f, 4f, 5f);
    }

    /// <summary>Textura panorámica con la sala 3D y el avatar haciendo la acción (cronómetro).</summary>
    /// <summary>Recorte de la textura del retrato centrado en la cara del avatar (para retratos redondos).</summary>
    public static Rect RecorteCara(float ancho = 0.3f)
    {
        Preparar();
        Transform cabeza = avatar != null ? avatar.Cabeza : null;
        if (cabeza == null || camara == null) return new Rect(0.22f, 0.5f, 0.56f, 0.45f);
        Vector3 v = camara.WorldToViewportPoint(cabeza.position + Vector3.up * (0.05f * Avatar3D.Altura));
        float alto = ancho * textura.width / (float)textura.height;
        return new Rect(v.x - ancho * 0.5f, v.y - alto * 0.5f, ancho, alto);
    }

    public static RenderTexture TexturaEscena
    {
        get { Preparar(); PrepararEscena(); return texEscena; }
    }

    private static void PrepararEscena()
    {
        if (texEscena != null) return;
        texEscena = new RenderTexture(810, 1440, 24, RenderTextureFormat.ARGB32) { name = "Escena avatar", antiAliasing = 4 };
        camEscena = new GameObject("Cámara escena").AddComponent<Camera>();
        camEscena.transform.SetParent(raizEstudio, false);
        camEscena.transform.localPosition = new Vector3(8.5f, 8.2f, 15.5f);
        camEscena.transform.LookAt(Lugar + new Vector3(-0.8f, 2.1f, 0.2f));
        camEscena.fieldOfView = 40f;
        camEscena.clearFlags = CameraClearFlags.SolidColor;
        camEscena.backgroundColor = Paleta.Hex("F3E9DC");
        camEscena.cullingMask = (1 << Capa) | (1 << CapaEscena);
        camEscena.targetTexture = texEscena;
        camEscena.nearClipPlane = 0.5f;
        camEscena.farClipPlane = 60f;
        var datos = camEscena.GetUniversalAdditionalCameraData();
        if (datos != null) datos.renderPostProcessing = false;
        camEscena.enabled = false;
        salas = new GameObject("Salas").transform;
        salas.SetParent(raizEstudio, false);
        // Luz de relleno cálida para las salas
        var luz = new GameObject("Luz sala").AddComponent<Light>();
        luz.transform.SetParent(salas, false);
        luz.type = LightType.Point;
        luz.range = 30f;
        luz.intensity = 3.5f;
        luz.color = new Color(1f, 0.93f, 0.84f);
        luz.cullingMask = (1 << Capa) | (1 << CapaEscena);
        luz.transform.localPosition = new Vector3(-2f, 6.5f, 4f);
    }

    /// <summary>Monta la sala de la acción (gimnasio, biblioteca...) y enciende la cámara de escena.</summary>
    public static void PonerEscena(string accion, float mirada = 2.1f, float distancia = 1f)
    {
        Preparar(); PrepararEscena();
        camEscena.transform.position = Lugar + new Vector3(8.5f, 8.2f, 15.5f) * distancia;
        camEscena.transform.LookAt(Lugar + new Vector3(-0.8f, mirada, 0.2f));
        string sala = Salas3D.SalaDe(accion);
        if (sala != salaActual || accion != accionSala)
        {
            for (int i = salas.childCount - 1; i >= 0; i--)
                if (salas.GetChild(i).GetComponent<Light>() == null) Object.Destroy(salas.GetChild(i).gameObject);
            var cont = new GameObject("Sala " + sala).transform;
            cont.SetParent(salas, false);
            Salas3D.Construir(cont, sala, accion);
            foreach (Transform t in cont.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = CapaEscena;
            salaActual = sala; accionSala = accion;
        }
        camEscena.enabled = true;
        avatar.transform.localRotation = Quaternion.Euler(0, 30, 0);
    }

    public static void QuitarEscena()
    {
        if (camEscena != null) camEscena.enabled = false;
    }

    /// <summary>Pone al avatar del estudio a hacer una acción (id de la tabla) o nada ("").</summary>
    public static void PonerAccion(string id)
    {
        Preparar();
        avatar.Accion = id ?? "";
    }


    public static void Actualizar(TipoEstadoAvatar estado, string colorSudadera)
    {
        Preparar();
        avatar.Accion = "";
        avatar.transform.localRotation = Quaternion.Euler(0, 18, 0);
        avatar.PonerRopa(colorSudadera);
        avatar.PonerEstado(estado);
    }

    /// <summary>Activa o apaga la cámara del estudio (solo hace falta cuando se ve una pantalla con retrato).</summary>
    public static void Activo(bool si)
    {
        if (camara != null) camara.enabled = si;
    }
}
