using System;
using System.Collections;
using System.Collections.Generic;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Ciudad 3D inventada con los kits de Kenney (CC0): 8 solares jugables cuyo edificio cambia según el papel
/// que le da el jugador, un barrio decorativo alrededor, luz cálida de tarde, postproceso suave,
/// pines medallón y el avatar, que camina solo por las calles hasta el edificio elegido.
/// </summary>
public class Ciudad3D : MonoBehaviour
{
    public static readonly Vector3[] Solares =
    {
        new Vector3(-14, 0, -14), new Vector3(0, 0, -14), new Vector3(14, 0, -14),
        new Vector3(-14, 0, 0), new Vector3(14, 0, 0),
        new Vector3(-14, 0, 14), new Vector3(0, 0, 14), new Vector3(14, 0, 14),
    };
    public const int IndiceCasa = 1;

    private static readonly float[] Calles = { -21f, -7f, 7f, 21f };
    private const float AnchoSolar = 8.5f;

    /// <summary>Se llama al tocar un edificio (índice del solar).</summary>
    public event Action<int> AlTocarEdificio;
    /// <summary>Si es false, no responde a toques (por ejemplo, con un menú abierto).</summary>
    public bool Activa;
    public bool Caminando { get; private set; }

    private DatosCiudad datos;
    private Camera camara;
    private Transform avatar;
    private Avatar3D avatar3D;
    private Transform anilloAvatar;
    private RectTransform burbuja;
    private FormasUI dibujoEstado;
    private Text textoEstado;
    private TipoEstadoAvatar estadoDibujado = (TipoEstadoAvatar)(-1);
    private float proximaHuella;
    private bool huellaIzquierda;
    private readonly List<KeyValuePair<Transform, float>> huellas = new List<KeyValuePair<Transform, float>>();
    private float zoom = 1f;
    private Vector3 desplazamiento;          // arrastre de la cámara
    private Vector2 inicioPuntero;
    private bool arrastrando, pulsado;
    private readonly List<Solar> solares = new List<Solar>();
    private readonly List<Transform> carteles = new List<Transform>();
    private readonly Dictionary<Color, Material> materiales = new Dictionary<Color, Material>();

    private class Solar
    {
        public Vector3 centro;
        public Transform raiz;
        public GameObject modelo;
        public string rolMostrado = null;
        public GameObject medallon;
        public Image fondoMedallon;
        public Text letraMedallon;
        public Text etiqueta;
        public GameObject pastilla;
        public float alto = 6f;
        public Vector3 Puerta { get { return new Vector3(centro.x, 0, centro.z - 5.6f); } }
    }

    // ---------------------------------------------------------------- construcción

    public void Construir(Juego juego, DatosCiudad datos)
    {
        this.datos = datos;
        while (datos.roles.Count < Solares.Length) datos.roles.Add("");
        if (string.IsNullOrEmpty(datos.roles[IndiceCasa])) datos.roles[IndiceCasa] = "casa";
        if (datos.edificioActual < 0) datos.edificioActual = IndiceCasa;

        PrepararCamaraYLuz();

        // Suelo, calles con acera y plaza
        Pieza(PrimitiveType.Cube, "Césped", new Vector3(0, -0.05f, 0), new Vector3(140, 0.1f, 140), Paleta.Hex("A9CF8E"));
        foreach (float c in Calles)
        {
            Calle(new Vector3(c, 0, 0), true);
            Calle(new Vector3(0, 0, c), false);
        }
        Pieza(PrimitiveType.Cylinder, "Plaza", new Vector3(0, 0.06f, 0), new Vector3(9.5f, 0.06f, 9.5f), Paleta.Hex("EADFCF"));
        Pieza(PrimitiveType.Cylinder, "Fuente", new Vector3(0, 0.4f, 0), new Vector3(3.6f, 0.35f, 3.6f), Paleta.Hex("F4EEE6"));
        Pieza(PrimitiveType.Cylinder, "Agua", new Vector3(0, 0.62f, 0), new Vector3(3.1f, 0.06f, 3.1f), Paleta.Hex("8CC7E8"));
        Pieza(PrimitiveType.Cylinder, "Columna", new Vector3(0, 1.3f, 0), new Vector3(0.55f, 0.75f, 0.55f), Paleta.Hex("F4EEE6"));
        for (int i = 0; i < 4; i++)
        {
            float a = i * Mathf.PI / 2f + Mathf.PI / 4f;
            Modelo("Ciudad/Suburban/planter", transform, new Vector3(Mathf.Cos(a) * 3.6f, 0.1f, Mathf.Sin(a) * 3.6f), 1.6f, 0f);
        }

        for (int i = 0; i < Solares.Length; i++)
        {
            var s = new Solar { centro = Solares[i] };
            s.raiz = new GameObject("Solar " + i).transform;
            s.raiz.SetParent(transform, false);
            s.raiz.gameObject.AddComponent<Lote>().indice = i;
            var col = s.raiz.gameObject.AddComponent<BoxCollider>();
            col.center = s.centro + new Vector3(0, 3f, 0);
            col.size = new Vector3(10f, 6f, 10f);
            CrearMedallon(s);
            solares.Add(s);
        }

        Barrio();
        CrearAvatar();
        avatar.position = solares[datos.edificioActual].Puerta;
        ActualizarTodos();
        SituarCamara(true);
    }

    private void PrepararCamaraYLuz()
    {
        camara = Camera.main;
        if (camara == null) camara = new GameObject("Cámara", typeof(Camera)).GetComponent<Camera>();
        camara.fieldOfView = 42f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Paleta.Hex("F6E7CF");
        camara.farClipPlane = 400f;
        camara.cullingMask &= ~(1 << EstudioAvatar.Capa);
        var datosCam = camara.GetUniversalAdditionalCameraData();
        if (datosCam != null)
        {
            datosCam.renderPostProcessing = true;
            datosCam.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }

        Light sol = null;
        foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) sol = l;
        if (sol == null) sol = new GameObject("Sol", typeof(Light)).GetComponent<Light>();
        sol.type = LightType.Directional;
        sol.transform.rotation = Quaternion.Euler(42f, -38f, 0f);
        sol.color = Paleta.Hex("FFE1B5");
        sol.intensity = 1.35f;
        sol.shadows = LightShadows.Soft;
        sol.shadowStrength = 0.55f;

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Paleta.Hex("C9DDF2");
        RenderSettings.ambientEquatorColor = Paleta.Hex("F2E0C4");
        RenderSettings.ambientGroundColor = Paleta.Hex("B4A486");
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Paleta.Hex("F6E7CF");
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 160f;

        // Postproceso suave y cálido
        var go = new GameObject("Postproceso");
        go.transform.SetParent(transform, false);
        var vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 50;
        var perfil = ScriptableObject.CreateInstance<VolumeProfile>();
        var bloom = perfil.Add<Bloom>(true);
        bloom.intensity.Override(0.35f);
        bloom.threshold.Override(1.05f);
        var color = perfil.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.15f);
        color.saturation.Override(8f);
        color.contrast.Override(4f);
        var blanco = perfil.Add<WhiteBalance>(true);
        blanco.temperature.Override(8f);
        var vineta = perfil.Add<Vignette>(true);
        vineta.intensity.Override(0.18f);
        vineta.smoothness.Override(0.5f);
        var tono = perfil.Add<Tonemapping>(true);
        tono.mode.Override(TonemappingMode.None);
        vol.profile = perfil;
    }

    private void Calle(Vector3 centro, bool vertical)
    {
        Vector3 largo = vertical ? new Vector3(3.6f, 0.05f, 60f) : new Vector3(60f, 0.05f, 3.6f);
        Vector3 acera = vertical ? new Vector3(5.2f, 0.08f, 60f) : new Vector3(60f, 0.08f, 5.2f);
        Pieza(PrimitiveType.Cube, "Acera", centro + new Vector3(0, 0.02f, 0), acera, Paleta.Hex("EDE3D3"));
        Pieza(PrimitiveType.Cube, "Calzada", centro + new Vector3(0, 0.05f, 0), largo, Paleta.Hex("B9B2A8"));
        for (float d = -28f; d <= 28f; d += 4f)
        {
            Vector3 pos = centro + (vertical ? new Vector3(0, 0.085f, d) : new Vector3(d, 0.085f, 0));
            Vector3 tam = vertical ? new Vector3(0.18f, 0.01f, 1.6f) : new Vector3(1.6f, 0.01f, 0.18f);
            Pieza(PrimitiveType.Cube, "Raya", pos, tam, Paleta.Hex("F7F2EA"));
        }
    }

    private void Barrio()
    {
        // Edificios decorativos alrededor (no se pueden tocar) para que la ciudad parezca grande
        string[] bajos = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n" };
        string[] casas = { "a", "b", "c", "d", "e", "g", "h", "i", "j", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u" };
        int k = 0;
        for (int i = -2; i <= 2; i++)
        {
            float x = i * 14f;
            Modelo("Ciudad/Suburban/tree-small", transform, new Vector3(x + 3f, 0, -27f), 2.2f, 0f);
            Decorado(new Vector3(x, 0, 28f), k++, bajos, "Ciudad/Comercial/low-detail-building-", 9f, 180f);
            if (i > -2 && i < 2)
            {
                Decorado(new Vector3(-28f, 0, x), k++, bajos, "Ciudad/Comercial/low-detail-building-", 9f, 90f);
                Decorado(new Vector3(28f, 0, x), k++, casas, "Ciudad/Suburban/building-type-", 7.5f, -90f);
            }
        }
        // Árboles a lo largo de las aceras
        foreach (float c in Calles)
            for (float d = -24f; d <= 24f; d += 12f)
            {
                Modelo("Ciudad/Suburban/tree-small", transform, new Vector3(c + 2.9f, 0, d + 3.5f), 2.2f, 0f);
                Modelo("Ciudad/Suburban/tree-large", transform, new Vector3(d + 3.5f, 0, c - 2.9f), 2.8f, 0f);
            }
    }

    private void Decorado(Vector3 pos, int k, string[] variantes, string ruta, float ancho, float giro)
    {
        Modelo(ruta + variantes[(k * 7) % variantes.Length], transform, pos, ancho, giro);
    }

    // ---------------------------------------------------------------- piezas y modelos

    private Material Mat(Color color)
    {
        Material m;
        if (materiales.TryGetValue(color, out m)) return m;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        m = new Material(s);
        m.SetColor("_BaseColor", color);
        m.color = color;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.12f);
        materiales[color] = m;
        return m;
    }

    private GameObject Pieza(PrimitiveType tipo, string nombre, Vector3 pos, Vector3 escala, Color color, Transform padre = null)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre != null ? padre : transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = Mat(color);
        Destroy(go.GetComponent<Collider>());
        return go;
    }

    /// <summary>Instancia un modelo de Resources, lo escala para que mida "ancho" y lo apoya en el suelo.</summary>
    private GameObject Modelo(string ruta, Transform padre, Vector3 pos, float ancho, float giroY)
    {
        GameObject prefab = Resources.Load<GameObject>(ruta);
        if (prefab == null) { Debug.LogWarning("[Ciudad] No encuentro el modelo " + ruta); return null; }
        GameObject go = Instantiate(prefab, padre, false);
        go.name = prefab.name;
        foreach (Collider c in go.GetComponentsInChildren<Collider>()) Destroy(c);
        go.transform.localRotation = Quaternion.Euler(0, giroY, 0);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        Bounds b = Limites(go);
        float tam = Mathf.Max(b.size.x, b.size.z);
        float escala = tam > 0.001f ? ancho / tam : 1f;
        go.transform.localScale = Vector3.one * escala;
        b = Limites(go);
        Vector3 centroLocal = padre != null ? padre.InverseTransformPoint(b.center) : b.center;
        float baseY = padre != null ? padre.InverseTransformPoint(new Vector3(0, b.min.y, 0)).y : b.min.y;
        go.transform.localPosition = pos - new Vector3(centroLocal.x, baseY, centroLocal.z);
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.On;
        return go;
    }

    private static Bounds Limites(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = rs[0].bounds;
        foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // ---------------------------------------------------------------- solares

    public void ActualizarTodos()
    {
        for (int i = 0; i < solares.Count; i++) ActualizarEdificio(i);
    }

    public void ActualizarEdificio(int i)
    {
        Solar s = solares[i];
        string rolId = datos.roles[i];
        RolEdificio rol = Catalogos.BuscarRol(rolId);
        if (s.rolMostrado != rolId)
        {
            if (s.modelo != null) Destroy(s.modelo);
            s.modelo = new GameObject("Edificio");
            s.modelo.transform.SetParent(s.raiz, false);
            ConstruirModelo(s, rolId, i);
            s.rolMostrado = rolId;
            Bounds b = Limites(s.modelo);
            s.alto = Mathf.Max(3f, b.max.y);
        }

        s.medallon.transform.localPosition = s.centro + new Vector3(0, s.alto + 2.4f, 0);
        if (rol != null)
        {
            s.fondoMedallon.color = Paleta.DeArea(rol.area);
            s.letraMedallon.text = rol.nombre.Substring(0, 1);
            s.etiqueta.text = rol.nombre;
            s.pastilla.SetActive(true);
        }
        else
        {
            s.fondoMedallon.color = Paleta.Hex("B8AFA3");
            s.letraMedallon.text = "?";
            s.pastilla.SetActive(false);
        }
    }

    private void ConstruirModelo(Solar s, string rol, int indice)
    {
        Transform p = s.modelo.transform;
        Vector3 c = s.centro + new Vector3(0, 0, 0.8f);
        switch (rol)
        {
            case "casa": Modelo("Ciudad/Suburban/building-type-a", p, c, AnchoSolar, 180f); Valla(p, c); break;
            case "amigos": Modelo("Ciudad/Suburban/building-type-k", p, c, AnchoSolar, 180f); Valla(p, c); break;
            case "gimnasio": Modelo("Ciudad/Comercial/building-c", p, c, AnchoSolar, 180f); break;
            case "cafeteria":
                Modelo("Ciudad/Comercial/building-a", p, c, AnchoSolar, 180f);
                Modelo("Ciudad/Comercial/detail-parasol-a", p, c + new Vector3(-2.6f, 0, -4.2f), 1.8f, 0f);
                Modelo("Ciudad/Comercial/detail-parasol-b", p, c + new Vector3(2.6f, 0, -4.2f), 1.8f, 0f);
                break;
            case "biblioteca": Modelo("Ciudad/Comercial/building-e", p, c, AnchoSolar, 180f); break;
            case "trabajo": Modelo("Ciudad/Comercial/building-skyscraper-a", p, c, AnchoSolar * 0.85f, 180f); break;
            case "recreativos": Modelo("Ciudad/Comercial/building-h", p, c, AnchoSolar, 180f); break;
            case "parque":
                Pieza(PrimitiveType.Cube, "Césped parque", s.centro + new Vector3(0, 0.03f, 0), new Vector3(9.5f, 0.06f, 9.5f), Paleta.Hex("93C47D"), p);
                Modelo("Ciudad/Suburban/tree-large", p, s.centro + new Vector3(-2.5f, 0, 2f), 3.6f, 0f);
                Modelo("Ciudad/Suburban/tree-large", p, s.centro + new Vector3(2.8f, 0, 1.2f), 3.2f, 40f);
                Modelo("Ciudad/Suburban/tree-small", p, s.centro + new Vector3(0.5f, 0, 3.4f), 2.4f, 0f);
                Modelo("Ciudad/Suburban/path-stones-long", p, s.centro + new Vector3(0, 0.05f, -1.5f), 2.2f, 90f);
                Modelo("Ciudad/Suburban/planter", p, s.centro + new Vector3(-3f, 0, -3f), 1.5f, 0f);
                break;
            default:
                // Sin papel: casa sencilla y algo apagada, con el "?" encima
                Modelo("Ciudad/Suburban/building-type-" + "fgjnpqrstu"[indice % 10], p, c, AnchoSolar * 0.9f, 180f);
                break;
        }
    }

    private void Valla(Transform p, Vector3 c)
    {
        Modelo("Ciudad/Suburban/fence-low", p, c + new Vector3(-3.3f, 0, -4.6f), 2.6f, 0f);
        Modelo("Ciudad/Suburban/fence-low", p, c + new Vector3(3.3f, 0, -4.6f), 2.6f, 0f);
    }

    private void CrearMedallon(Solar s)
    {
        var go = new GameObject("Medallón", typeof(RectTransform), typeof(Canvas));
        go.transform.SetParent(s.raiz, false);
        go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(300, 320);
        rt.localScale = Vector3.one * 0.0085f;

        Image borde = UIFabrica.Caja(go.transform, "Borde", Paleta.Blanco);
        borde.sprite = UIFabrica.Circulo; borde.type = Image.Type.Simple;
        borde.rectTransform.anchorMin = borde.rectTransform.anchorMax = new Vector2(0.5f, 0.62f);
        borde.rectTransform.sizeDelta = new Vector2(200, 200);
        UIFabrica.Sombra(borde, 8f, 0.25f);
        s.fondoMedallon = UIFabrica.Caja(borde.transform, "Color", Paleta.Gris);
        s.fondoMedallon.sprite = UIFabrica.Circulo; s.fondoMedallon.type = Image.Type.Simple;
        UIFabrica.Anclar(s.fondoMedallon.rectTransform, 0.09f, 0.09f, 0.91f, 0.91f);
        s.letraMedallon = UIFabrica.Texto(s.fondoMedallon.transform, "?", 110, Paleta.Blanco, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(s.letraMedallon.rectTransform, 0, 0, 1, 1);

        Image pastilla = UIFabrica.Caja(go.transform, "Pastilla", Paleta.Tarjeta);
        pastilla.rectTransform.anchorMin = pastilla.rectTransform.anchorMax = new Vector2(0.5f, 0.08f);
        pastilla.rectTransform.sizeDelta = new Vector2(300, 70);
        UIFabrica.Sombra(pastilla, 5f, 0.2f);
        s.etiqueta = UIFabrica.Texto(pastilla.transform, "", 44, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(s.etiqueta.rectTransform, 0, 0, 1, 1);
        s.pastilla = pastilla.gameObject;
        s.medallon = go;
        carteles.Add(go.transform);
    }

    // ---------------------------------------------------------------- avatar

    private void CrearAvatar()
    {
        avatar3D = Avatar3D.Crear(transform);
        avatar = avatar3D.transform;
        anilloAvatar = Pieza(PrimitiveType.Cylinder, "Anillo", new Vector3(0, 0.02f, 0), new Vector3(2.6f, 0.008f, 2.6f), Paleta.Hex("FFFDF8"), avatar).transform;
        Pieza(PrimitiveType.Cylinder, "Centro anillo", new Vector3(0, 0.03f, 0), new Vector3(2.2f, 0.008f, 2.2f), Paleta.Hex("F7D9A8"), avatar);
        avatar.rotation = Quaternion.Euler(0, 180, 0);
        CrearBurbujaEstado();
    }

    private void CrearBurbujaEstado()
    {
        var go = new GameObject("Burbuja estado", typeof(RectTransform), typeof(Canvas));
        go.transform.SetParent(transform, false);
        go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        burbuja = (RectTransform)go.transform;
        burbuja.sizeDelta = new Vector2(220, 220);
        burbuja.localScale = Vector3.one * 0.008f;
        Image fondo = UIFabrica.Caja(go.transform, "Fondo", Paleta.Blanco);
        fondo.sprite = UIFabrica.Circulo; fondo.type = Image.Type.Simple;
        UIFabrica.Anclar(fondo.rectTransform, 0, 0, 1, 1);
        UIFabrica.Sombra(fondo, 6f, 0.2f);
        RectTransform zd = UIFabrica.Crear("Dibujo", fondo.transform);
        UIFabrica.Anclar(zd, 0.15f, 0.15f, 0.85f, 0.85f);
        dibujoEstado = FormasUI.En(zd.gameObject);
        textoEstado = UIFabrica.Texto(fondo.transform, "", 110, Paleta.TextoSuave, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(textoEstado.rectTransform, 0, 0, 1, 1);
        carteles.Add(burbuja);
    }

    public void PonerRopa(string colorHex) { if (avatar3D != null) avatar3D.PonerRopa(colorHex); }

    public void PonerEstado(TipoEstadoAvatar estado) { if (avatar3D != null) avatar3D.PonerEstado(estado); }

    public int RolesAsignados()
    {
        int n = 0;
        foreach (string r in datos.roles) if (!string.IsNullOrEmpty(r)) n++;
        return n;
    }

    // ---------------------------------------------------------------- movimiento

    /// <summary>El avatar camina solo por las calles hasta la puerta del edificio.</summary>
    public void IrA(int indice, Action alLlegar)
    {
        StopAllCoroutines();
        desplazamiento = Vector3.zero;
        StartCoroutine(Caminar(indice, alLlegar));
    }

    private IEnumerator Caminar(int indice, Action alLlegar)
    {
        Caminando = true; avatar3D.Caminando = true;
        Vector3 inicio = avatar.position;
        Vector3 destino = solares[indice].Puerta;
        float calleInicio = MasCercana(inicio.z);
        float calleDestino = MasCercana(destino.z);
        float calleVertical = MasCercana(inicio.x);
        var puntos = new List<Vector3>
        {
            new Vector3(inicio.x, 0, calleInicio),
            new Vector3(calleVertical, 0, calleInicio),
            new Vector3(calleVertical, 0, calleDestino),
            new Vector3(destino.x, 0, calleDestino),
            destino,
        };
        foreach (Vector3 p in puntos)
        {
            while ((avatar.position - p).sqrMagnitude > 0.01f)
            {
                Vector3 dir = p - avatar.position;
                avatar.position = Vector3.MoveTowards(avatar.position, p, 7.5f * Time.deltaTime);
                if (dir.sqrMagnitude > 0.001f)
                    avatar.rotation = Quaternion.Slerp(avatar.rotation, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)), 12f * Time.deltaTime);
                yield return null;
            }
        }
        float t = 0f;
        Quaternion desde = avatar.rotation, hacia = Quaternion.Euler(0, 180, 0);
        while (t < 1f) { t += Time.deltaTime * 4f; avatar.rotation = Quaternion.Slerp(desde, hacia, t); yield return null; }
        datos.edificioActual = indice;
        Caminando = false; avatar3D.Caminando = false;
        if (alLlegar != null) alLlegar();
    }

    private static float MasCercana(float v)
    {
        float mejor = Calles[0];
        foreach (float c in Calles) if (Mathf.Abs(c - v) < Mathf.Abs(mejor - v)) mejor = c;
        return mejor;
    }

    // ---------------------------------------------------------------- cámara y toques

    private void SituarCamara(bool instantaneo)
    {
        if (camara == null || avatar == null) return;
        Vector3 objetivo = avatar.position + desplazamiento + Vector3.up * 1.6f;
        Vector3 deseada = objetivo + new Vector3(0, 27f, -21f) * zoom;
        float k = instantaneo ? 1f : 1f - Mathf.Exp(-6f * Time.deltaTime);
        camara.transform.position = Vector3.Lerp(camara.transform.position, deseada, k);
        camara.transform.rotation = Quaternion.Slerp(camara.transform.rotation, Quaternion.LookRotation(objetivo - deseada), k);
    }

    private void Update()
    {
        if (Mouse.current != null)
        {
            float rueda = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(rueda) > 0.01f) zoom = Mathf.Clamp(zoom - rueda * 0.0012f, 0.55f, 1.45f); // zoom limitado: el avatar siempre se ve
        }
        Pointer puntero = Pointer.current;
        if (!Activa || puntero == null) { pulsado = false; return; }

        Vector2 pos = puntero.position.ReadValue();
        if (puntero.press.wasPressedThisFrame)
        {
            pulsado = !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
            arrastrando = false;
            inicioPuntero = pos;
        }
        if (pulsado && puntero.press.isPressed)
        {
            Vector2 delta = puntero.delta.ReadValue();
            if ((pos - inicioPuntero).sqrMagnitude > 400f) arrastrando = true;
            if (arrastrando)
            {
                float f = 0.045f * zoom;
                desplazamiento += new Vector3(-delta.x * f, 0, -delta.y * f);
                desplazamiento.x = Mathf.Clamp(desplazamiento.x, -24f, 24f);
                desplazamiento.z = Mathf.Clamp(desplazamiento.z, -24f, 24f);
            }
        }
        if (pulsado && puntero.press.wasReleasedThisFrame)
        {
            pulsado = false;
            if (!arrastrando && !Caminando) Tocar(pos);
        }
    }

    private void Tocar(Vector2 pos)
    {
        Ray rayo = camara.ScreenPointToRay(pos);
        RaycastHit golpe;
        if (Physics.Raycast(rayo, out golpe, 500f))
        {
            Lote lote = golpe.collider.GetComponentInParent<Lote>();
            if (lote != null && AlTocarEdificio != null) AlTocarEdificio(lote.indice);
        }
    }

    private void LateUpdate()
    {
        SituarCamara(false);
        if (camara == null) return;
        foreach (Transform c in carteles) c.rotation = camara.transform.rotation;
        float tiempo = Time.time;
        AnimarAvatarEnCiudad(tiempo);
        Update2Construir();
        for (int i = 0; i < solares.Count; i++)
        {
            Solar s = solares[i];
            s.medallon.transform.localPosition = s.centro + new Vector3(0, s.alto + 2.4f + Mathf.Sin(tiempo * 2f + i) * 0.18f, 0);
        }
    }

    // ---------------------------------------------------------------- vida alrededor del avatar

    private void AnimarAvatarEnCiudad(float tiempo)
    {
        if (avatar == null) return;
        // Anillo que late suavemente bajo el avatar
        float k = 1f + Mathf.Sin(tiempo * 3f) * 0.06f;
        anilloAvatar.localScale = new Vector3(2.6f * k, 0.008f, 2.6f * k);

        // Burbuja de estado sobre la cabeza
        TipoEstadoAvatar e = avatar3D.Estado;
        bool mostrar = e != TipoEstadoAvatar.Normal && !Caminando;
        burbuja.gameObject.SetActive(mostrar);
        if (mostrar)
        {
            burbuja.position = avatar.position + new Vector3(1.1f, 4.6f + Mathf.Sin(tiempo * 2.4f) * 0.12f, 0);
            if (e != estadoDibujado)
            {
                estadoDibujado = e;
                dibujoEstado.Limpiar();
                textoEstado.text = "";
                if (e == TipoEstadoAvatar.EnForma) Estrella(dibujoEstado, Paleta.Hex("F2B632"));
                else if (e == TipoEstadoAvatar.Cansado) { textoEstado.text = "Zz"; textoEstado.color = Paleta.DeArea(Area.Mente); }
                else { textoEstado.text = "..."; textoEstado.color = Paleta.TextoSuave; }
            }
        }

        // Huellas al caminar, que se encogen y desaparecen
        if (Caminando && tiempo >= proximaHuella)
        {
            proximaHuella = tiempo + 0.16f;
            huellaIzquierda = !huellaIzquierda;
            Vector3 lado = avatar.right * (huellaIzquierda ? -0.28f : 0.28f);
            GameObject h = Pieza(PrimitiveType.Cylinder, "Huella", avatar.position + lado + new Vector3(0, 0.07f, 0), new Vector3(0.32f, 0.01f, 0.46f), Paleta.Hex("D8C3A5"));
            h.transform.rotation = avatar.rotation;
            huellas.Add(new KeyValuePair<Transform, float>(h.transform, tiempo));
        }
        for (int i = huellas.Count - 1; i >= 0; i--)
        {
            float edad = tiempo - huellas[i].Value;
            Transform t = huellas[i].Key;
            if (edad > 2.2f) { Destroy(t.gameObject); huellas.RemoveAt(i); continue; }
            float f = 1f - Mathf.Clamp01((edad - 1.2f) / 1f);
            t.localScale = new Vector3(0.32f * f, 0.01f, 0.46f * f);
        }
    }

    private static void Estrella(FormasUI f, Color c)
    {
        var p = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float a = i * 36f * Mathf.Deg2Rad;
            float r = i % 2 == 0 ? 0.95f : 0.42f;
            p[i] = new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * r;
        }
        f.Poligono(p, c, Vector2.zero);
    }

    // ---------------------------------------------------------------- modo construir

    private Transform guiasConstruir, marcaSolar;

    /// <summary>Manzanas (a pie) entre casa y el solar indicado.</summary>
    public static int Manzanas(int indice)
    {
        Vector3 d = Solares[indice] - Solares[IndiceCasa];
        return Mathf.Max(0, Mathf.RoundToInt((Mathf.Abs(d.x) + Mathf.Abs(d.z)) / 14f));
    }

    /// <summary>Muestra los anillos de distancia desde casa y marca el solar elegido.</summary>
    public void ModoConstruir(int indice)
    {
        SalirConstruir();
        guiasConstruir = new GameObject("Guías construir").transform;
        guiasConstruir.SetParent(transform, false);
        Vector3 casa = Solares[IndiceCasa];
        Color[] colores = { Paleta.Hex("FFFFFF"), Paleta.Hex("FFF1D6") };
        for (int r = 1; r <= 2; r++)
        {
            float radio = 14f * r;
            int n = 28 * r;
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n;
                Vector3 p = casa + new Vector3(Mathf.Cos(a) * radio, 0.12f, Mathf.Sin(a) * radio);
                Pieza(PrimitiveType.Cylinder, "Punto", p, new Vector3(0.55f, 0.01f, 0.55f), colores[r - 1], guiasConstruir);
            }
        }
        marcaSolar = Pieza(PrimitiveType.Cylinder, "Marca solar", Solares[indice] + new Vector3(0, 0.1f, 0), new Vector3(11f, 0.01f, 11f), Paleta.Hex("F6B49F"), guiasConstruir).transform;
    }

    public void SalirConstruir()
    {
        if (guiasConstruir != null) Destroy(guiasConstruir.gameObject);
        guiasConstruir = null;
        marcaSolar = null;
    }

    private void Update2Construir()
    {
        if (marcaSolar == null) return;
        float k = 1f + Mathf.Sin(Time.time * 4f) * 0.04f;
        marcaSolar.localScale = new Vector3(11f * k, 0.01f, 11f * k);
    }
}
