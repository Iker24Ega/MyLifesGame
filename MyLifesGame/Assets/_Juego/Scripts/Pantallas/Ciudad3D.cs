using System;
using System.Collections;
using System.Collections.Generic;
using MyLifesGame.Motor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Ciudad 3D inventada, hecha con formas básicas: calles en cuadrícula, 8 solares con edificios,
/// una plaza con fuente, árboles y el avatar, que camina solo por las calles hasta el edificio elegido.
/// Es un prototipo: el arte definitivo llegará más adelante (estilo de las imágenes de referencia).
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
    private static readonly string[] Fachadas = { "F4D9C6", "CFE5D8", "D6E2F2", "F6E6B8", "E9D5EE", "F2CFC4", "D9EAD3", "F1E3D3" };

    /// <summary>Se llama al tocar un edificio (índice del solar).</summary>
    public event Action<int> AlTocarEdificio;

    /// <summary>Si es false, no responde a toques (por ejemplo, con un menú abierto).</summary>
    public bool Activa;
    public bool Caminando { get; private set; }

    private Juego juego;
    private DatosCiudad datos;
    private Camera camara;
    private Transform avatar;
    private Renderer sudadera;
    private float zoom = 1f;
    private readonly List<Edificio> edificios = new List<Edificio>();
    private readonly List<Transform> carteles = new List<Transform>();
    private readonly Dictionary<Color, Material> materiales = new Dictionary<Color, Material>();

    private class Edificio
    {
        public Vector3 centro;
        public float ancho, alto, fondo;
        public Renderer fachada;
        public Renderer pin;
        public Text letrero;
        public GameObject cartel;
        public GameObject interrogacion;
        public Vector3 Puerta { get { return new Vector3(centro.x, 0, centro.z + 0.5f - fondo / 2f - 1.6f); } }
    }

    public void Construir(Juego juego, DatosCiudad datos)
    {
        this.juego = juego;
        this.datos = datos;
        while (datos.roles.Count < Solares.Length) datos.roles.Add("");
        if (string.IsNullOrEmpty(datos.roles[IndiceCasa])) datos.roles[IndiceCasa] = "casa";
        if (datos.edificioActual < 0) datos.edificioActual = IndiceCasa;

        camara = Camera.main;
        if (camara == null) camara = new GameObject("Cámara", typeof(Camera)).GetComponent<Camera>();
        camara.fieldOfView = 45f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = Paleta.Hex("F3E9D8");

        // Suelo, calles y plaza
        Pieza(PrimitiveType.Plane, "Suelo", new Vector3(0, 0, 0), new Vector3(8, 1, 8), Paleta.Hex("CFE3B4"), true);
        foreach (float c in Calles)
        {
            Pieza(PrimitiveType.Cube, "Calle", new Vector3(c, 0.03f, 0), new Vector3(3.5f, 0.06f, 46f), Paleta.Hex("D2C8BA"), false);
            Pieza(PrimitiveType.Cube, "Calle", new Vector3(0, 0.03f, c), new Vector3(46f, 0.06f, 3.5f), Paleta.Hex("D2C8BA"), false);
        }
        Pieza(PrimitiveType.Cylinder, "Plaza", new Vector3(0, 0.05f, 0), new Vector3(9f, 0.05f, 9f), Paleta.Hex("E8DCCB"), false);
        Pieza(PrimitiveType.Cylinder, "Fuente", new Vector3(0, 0.35f, 0), new Vector3(3.2f, 0.3f, 3.2f), Paleta.Hex("A9CBEA"), false);
        Pieza(PrimitiveType.Cylinder, "Columna fuente", new Vector3(0, 1f, 0), new Vector3(0.6f, 0.8f, 0.6f), Paleta.Hex("EDE6DB"), false);

        // Edificios
        for (int i = 0; i < Solares.Length; i++) CrearEdificio(i);

        // Árboles en las esquinas traseras de cada solar
        foreach (Vector3 s in Solares)
        {
            Arbol(s + new Vector3(4.6f, 0, 4.4f));
            Arbol(s + new Vector3(-4.6f, 0, 4.4f));
        }

        CrearAvatar();
        avatar.position = edificios[datos.edificioActual].Puerta;
        ActualizarTodos();
        SituarCamara(true);
    }

    // ---------------------------------------------------------------- piezas

    private Material Mat(Color color)
    {
        Material m;
        if (materiales.TryGetValue(color, out m)) return m;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        m = new Material(s);
        m.SetColor("_BaseColor", color);
        m.color = color;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.1f);
        materiales[color] = m;
        return m;
    }

    private GameObject Pieza(PrimitiveType tipo, string nombre, Vector3 pos, Vector3 escala, Color color, bool conColisor, Transform padre = null)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre != null ? padre : transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = Mat(color);
        if (!conColisor) Destroy(go.GetComponent<Collider>());
        return go;
    }

    private void Arbol(Vector3 pos)
    {
        Pieza(PrimitiveType.Cylinder, "Tronco", pos + new Vector3(0, 0.8f, 0), new Vector3(0.35f, 0.8f, 0.35f), Paleta.Hex("8B6A4E"), false);
        Pieza(PrimitiveType.Sphere, "Copa", pos + new Vector3(0, 2.3f, 0), new Vector3(2f, 1.8f, 2f), Paleta.Hex("8CC084"), false);
    }

    private void CrearEdificio(int i)
    {
        var e = new Edificio();
        e.centro = Solares[i];
        e.ancho = 6f + (i % 3);
        e.fondo = 6f;
        e.alto = 4f + (i * 37 % 5);

        var raiz = new GameObject("Edificio " + i);
        raiz.transform.SetParent(transform, false);
        raiz.AddComponent<Lote>().indice = i;

        Vector3 c = e.centro + new Vector3(0, 0, 0.5f);
        GameObject fachada = Pieza(PrimitiveType.Cube, "Fachada", c + new Vector3(0, e.alto / 2f, 0), new Vector3(e.ancho, e.alto, e.fondo),
            Paleta.Hex(Fachadas[i]), true, raiz.transform);
        e.fachada = fachada.GetComponent<Renderer>();
        Pieza(PrimitiveType.Cube, "Tejado", c + new Vector3(0, e.alto + 0.3f, 0), new Vector3(e.ancho + 0.5f, 0.6f, e.fondo + 0.5f),
            Paleta.Hex("C9785B"), true, raiz.transform);
        Pieza(PrimitiveType.Cube, "Puerta", c + new Vector3(0, 1f, -e.fondo / 2f - 0.05f), new Vector3(1.3f, 2f, 0.12f),
            Paleta.Hex("8B5A3C"), false, raiz.transform);
        for (int v = -1; v <= 1; v += 2)
            Pieza(PrimitiveType.Cube, "Ventana", c + new Vector3(v * e.ancho * 0.28f, e.alto * 0.62f, -e.fondo / 2f - 0.05f),
                new Vector3(1.2f, 1.1f, 0.1f), Paleta.Hex("BFD8E6"), false, raiz.transform);

        GameObject pin = Pieza(PrimitiveType.Sphere, "Pin", e.centro + new Vector3(0, e.alto + 2.6f, 0), Vector3.one * 1.5f,
            Paleta.Gris, false, raiz.transform);
        e.pin = pin.GetComponent<Renderer>();

        e.cartel = Cartel(raiz.transform, e.centro + new Vector3(0, e.alto + 4.3f, 0), out e.letrero, 70);
        Text t;
        e.interrogacion = Cartel(raiz.transform, e.centro + new Vector3(0, e.alto + 2.6f, 0), out t, 120);
        t.text = "?";
        edificios.Add(e);
    }

    private GameObject Cartel(Transform padre, Vector3 pos, out Text texto, int tam)
    {
        var go = new GameObject("Cartel", typeof(RectTransform), typeof(Canvas));
        go.transform.SetParent(padre, false);
        go.transform.position = pos;
        go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(480, 150);
        rt.localScale = Vector3.one * 0.012f;
        Image fondo = UIFabrica.Caja(go.transform, "Fondo", Paleta.Tarjeta);
        UIFabrica.Anclar(fondo.rectTransform, 0, 0, 1, 1);
        texto = UIFabrica.Texto(go.transform, "", tam, Paleta.Texto, TextAnchor.MiddleCenter, FontStyle.Bold);
        UIFabrica.Anclar(texto.rectTransform, 0, 0, 1, 1);
        carteles.Add(go.transform);
        return go;
    }

    private void CrearAvatar()
    {
        avatar = new GameObject("Avatar").transform;
        avatar.SetParent(transform, false);
        var cuerpo = new GameObject("Cuerpo").transform;
        cuerpo.SetParent(avatar, false);
        cuerpo.localScale = Vector3.one * 1.25f; // grande y legible (decisión de diseño)
        Pieza(PrimitiveType.Cylinder, "Pierna", new Vector3(-0.22f, 0.4f, 0), new Vector3(0.3f, 0.4f, 0.3f), Paleta.Vaquero, false, cuerpo);
        Pieza(PrimitiveType.Cylinder, "Pierna", new Vector3(0.22f, 0.4f, 0), new Vector3(0.3f, 0.4f, 0.3f), Paleta.Vaquero, false, cuerpo);
        GameObject torso = Pieza(PrimitiveType.Capsule, "Sudadera", new Vector3(0, 1.3f, 0), new Vector3(0.95f, 0.6f, 0.8f), Paleta.Hex("2FA3A0"), false, cuerpo);
        sudadera = torso.GetComponent<Renderer>();
        sudadera.material = new Material(Mat(Paleta.Hex("2FA3A0")));
        Pieza(PrimitiveType.Cube, "Mochila", new Vector3(0, 1.35f, -0.48f), new Vector3(0.75f, 0.85f, 0.35f), Paleta.Mochila, false, cuerpo);
        Pieza(PrimitiveType.Sphere, "Cabeza", new Vector3(0, 2.35f, 0), Vector3.one * 0.85f, Paleta.Piel, false, cuerpo);
        Pieza(PrimitiveType.Sphere, "Pelo", new Vector3(0, 2.6f, -0.05f), new Vector3(0.9f, 0.5f, 0.9f), Paleta.Pelo, false, cuerpo);
        Pieza(PrimitiveType.Sphere, "Ojo", new Vector3(-0.17f, 2.4f, 0.38f), Vector3.one * 0.13f, Paleta.Texto, false, cuerpo);
        Pieza(PrimitiveType.Sphere, "Ojo", new Vector3(0.17f, 2.4f, 0.38f), Vector3.one * 0.13f, Paleta.Texto, false, cuerpo);
        Pieza(PrimitiveType.Cylinder, "Anillo", new Vector3(0, 0.02f, 0), new Vector3(2.4f, 0.01f, 2.4f), Paleta.Blanco, false, avatar);
        avatar.rotation = Quaternion.Euler(0, 180, 0); // mirando a la cámara
    }

    // ---------------------------------------------------------------- estado

    public void ActualizarTodos()
    {
        for (int i = 0; i < edificios.Count; i++) ActualizarEdificio(i);
    }

    public void ActualizarEdificio(int i)
    {
        Edificio e = edificios[i];
        RolEdificio rol = Catalogos.BuscarRol(datos.roles[i]);
        bool tieneRol = rol != null;
        e.pin.gameObject.SetActive(tieneRol);
        e.cartel.SetActive(tieneRol);
        e.interrogacion.SetActive(!tieneRol);
        if (tieneRol)
        {
            Color colorArea = Paleta.DeArea(rol.area);
            e.pin.sharedMaterial = Mat(colorArea);
            e.fachada.sharedMaterial = Mat(Color.Lerp(colorArea, Color.white, 0.6f));
            e.letrero.text = rol.nombre;
        }
        else
        {
            e.fachada.sharedMaterial = Mat(Paleta.Hex("E7E1D8"));
        }
    }

    public void PonerRopa(string colorHex)
    {
        if (sudadera != null) sudadera.material.SetColor("_BaseColor", Paleta.Hex(colorHex));
        if (sudadera != null) sudadera.material.color = Paleta.Hex(colorHex);
    }

    public void PonerEstado(TipoEstadoAvatar estado)
    {
        if (avatar == null) return;
        Transform cuerpo = avatar.Find("Cuerpo");
        float inclinacion = estado == TipoEstadoAvatar.Cansado ? 10f : estado == TipoEstadoAvatar.Descuidado ? 16f : 0f;
        cuerpo.localRotation = Quaternion.Euler(inclinacion, 0, 0);
    }

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
        StartCoroutine(Caminar(indice, alLlegar));
    }

    private IEnumerator Caminar(int indice, Action alLlegar)
    {
        Caminando = true;
        Vector3 inicio = avatar.position;
        Vector3 destino = edificios[indice].Puerta;
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
        Transform cuerpo = avatar.Find("Cuerpo");
        float t = 0f;
        foreach (Vector3 p in puntos)
        {
            while ((avatar.position - p).sqrMagnitude > 0.01f)
            {
                Vector3 dir = p - avatar.position;
                avatar.position = Vector3.MoveTowards(avatar.position, p, 8f * Time.deltaTime);
                if (dir.sqrMagnitude > 0.001f)
                    avatar.rotation = Quaternion.Slerp(avatar.rotation, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)), 12f * Time.deltaTime);
                t += Time.deltaTime;
                cuerpo.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(t * 12f)) * 0.15f, 0);
                yield return null;
            }
        }
        cuerpo.localPosition = Vector3.zero;
        avatar.rotation = Quaternion.Euler(0, 180, 0);
        datos.edificioActual = indice;
        Caminando = false;
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
        Vector3 objetivo = avatar.position + Vector3.up * 1.5f;
        Vector3 deseada = objetivo + new Vector3(0, 15f, -15f) * zoom;
        camara.transform.position = instantaneo ? deseada : Vector3.Lerp(camara.transform.position, deseada, 5f * Time.deltaTime);
        camara.transform.rotation = Quaternion.LookRotation(objetivo - deseada);
    }

    private void Update()
    {
        if (Mouse.current != null)
        {
            float rueda = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(rueda) > 0.01f) zoom = Mathf.Clamp(zoom - rueda * 0.0015f, 0.6f, 1.7f); // zoom limitado: el avatar siempre se ve
        }

        if (!Activa || Caminando) return;
        Pointer puntero = Pointer.current;
        if (puntero == null || !puntero.press.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        Ray rayo = camara.ScreenPointToRay(puntero.position.ReadValue());
        RaycastHit golpe;
        if (Physics.Raycast(rayo, out golpe, 400f))
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
        foreach (Edificio e in edificios)
            if (e.pin.gameObject.activeSelf)
                e.pin.transform.localPosition = e.centro + new Vector3(0, e.alto + 2.6f + Mathf.Sin(tiempo * 2f) * 0.2f, 0);
    }
}
