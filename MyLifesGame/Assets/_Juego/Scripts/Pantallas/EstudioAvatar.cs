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

        avatar = Avatar3D.Crear(raiz, Capa);
        avatar.transform.localRotation = Quaternion.Euler(0, 18, 0);

        textura = new RenderTexture(768, 960, 24, RenderTextureFormat.ARGB32) { name = "Retrato avatar", antiAliasing = 4 };
        camara = new GameObject("Cámara estudio").AddComponent<Camera>();
        camara.transform.SetParent(raiz, false);
        camara.transform.localPosition = new Vector3(0, 2.3f, 8.2f);
        camara.transform.LookAt(Lugar + new Vector3(0, 1.85f, 0));
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
        luz.cullingMask = 1 << Capa;
        luz.transform.localPosition = new Vector3(2.5f, 4f, 5f);
    }

    public static void Actualizar(TipoEstadoAvatar estado, string colorSudadera)
    {
        Preparar();
        avatar.PonerRopa(colorSudadera);
        avatar.PonerEstado(estado);
    }

    /// <summary>Activa o apaga la cámara del estudio (solo hace falta cuando se ve una pantalla con retrato).</summary>
    public static void Activo(bool si)
    {
        if (camara != null) camara.enabled = si;
    }
}
