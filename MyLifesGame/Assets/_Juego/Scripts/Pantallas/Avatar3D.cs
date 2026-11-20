using MyLifesGame.Motor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Avatar del jugador: personaje 3D con esqueleto y animaciones reales (Quaternius, CC0, "Resources/Personajes/Chico"),
/// recoloreado con la paleta de la hoja de personaje (sudadera turquesa, vaqueros, zapatillas blancas, mochila gris)
/// y con proporciones chibi (cabeza más grande). Las acciones sin animación propia (sentarse, meditar, pesas)
/// se posan girando huesos encima de la animación base. Lo usan la ciudad y el estudio de retrato.
/// </summary>
public class Avatar3D : MonoBehaviour
{
    public bool Caminando;
    /// <summary>Acción que está haciendo (id de la tabla: entrenar, estudiar, quedar...). Vacío: ninguna.</summary>
    public string Accion = "";

    /// <summary>Altura del personaje en unidades del mundo (la ciudad y los retratos están pensados para ~4).</summary>
    public const float Altura = 3.9f;
    private float EscalaCabeza = 1.6f;
    private float EscalaPiernas = 0.82f;
    private float EscalaBrazos = 0.9f;
    /// <summary>true si usa el personaje generado (Flow → TRELLIS → Mixamo) en lugar del de Quaternius.</summary>
    public bool Generado { get; private set; }
    /// <summary>true si el último avatar creado usa el personaje generado (para encuadrar los retratos).</summary>
    public static bool UsaGenerado { get; private set; }
    private float caida;
    private int fotogramas;

    private Transform modelo;
    private Animator animador;
    private PlayableGraph grafo;
    private AnimationMixerPlayable mezcla;
    private readonly Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
    private string clipActual;
    private float fundido = 1f, duracionFundido = 0.25f;

    private Transform cabeza, cuello, pecho, cadera, brazoI, brazoD, codoI, codoD, munecaI, munecaD, musloI, musloD, rodillaI, rodillaD, pieI, pieD;
    private float escala = 1f, alturaTobillo;
    private readonly List<Material> sudadera = new List<Material>();
    private Transform mochila, pesaI, pesaD, libro, mando, asiento;
    private TipoEstadoAvatar estadoAvatar = TipoEstadoAvatar.Normal;
    private static readonly Dictionary<Color, Material> materiales = new Dictionary<Color, Material>();

    public static readonly Color ColorSudadera = Paleta.Hex("3FA59E");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar() { materiales.Clear(); }

    public static Avatar3D Crear(Transform padre, int capa = 0)
    {
        var go = new GameObject("Avatar");
        go.transform.SetParent(padre, false);
        var a = go.AddComponent<Avatar3D>();
        a.Construir();
        if (capa != 0) foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = capa;
        return a;
    }

    // ---------------------------------------------------------------- construcción

    private void Construir()
    {
        if (ConstruirGenerado()) return;
        GameObject prefab = Resources.Load<GameObject>("Personajes/Chico");
        if (prefab == null) { Debug.LogError("[Avatar] Falta Resources/Personajes/Chico"); return; }
        GameObject go = Instantiate(prefab, transform, false);
        go.name = "Modelo";
        modelo = go.transform;
        foreach (Collider c in go.GetComponentsInChildren<Collider>()) Destroy(c);

        cabeza = Hueso("Head"); cuello = Hueso("Neck"); pecho = Hueso("Chest"); cadera = Hueso("Hips");
        brazoI = Hueso("UpperArm.L"); brazoD = Hueso("UpperArm.R"); codoI = Hueso("LowerArm.L"); codoD = Hueso("LowerArm.R");
        munecaI = Hueso("Wrist.L"); munecaD = Hueso("Wrist.R");
        musloI = Hueso("UpperLeg.L"); musloD = Hueso("UpperLeg.R"); rodillaI = Hueso("LowerLeg.L"); rodillaD = Hueso("LowerLeg.R");
        pieI = Hueso("Foot.L"); pieD = Hueso("Foot.R");

        Recolorear();

        // Escala para que mida "Altura" (contando la cabeza agrandada).
        modelo.localScale = Vector3.one;
        modelo.localPosition = Vector3.zero;
        modelo.localRotation = Quaternion.identity;
        float alto = AltoModelo();
        escala = alto > 0.01f ? Altura / alto : 1f;
        modelo.localScale = new Vector3(1.12f, 1f, 1.12f) * escala;
        if (pieI != null) alturaTobillo = transform.InverseTransformPoint(pieI.position).y;
        CrearOjos();
        if (cabeza != null) cabeza.localScale = Vector3.one * EscalaCabeza;
        GuardarPies();
        EscalarExtremidades(EscalaPiernas, EscalaBrazos);
        SeguirPies();
        if (pieI != null) caida = alturaTobillo - transform.InverseTransformPoint(pieI.position).y;

        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            var smr = r as SkinnedMeshRenderer;
            if (smr != null) smr.updateWhenOffscreen = true;
        }

        PrepararAnimaciones(go);
        CrearAccesorios();
    }

    /// <summary>
    /// Personaje generado a partir de la hoja de personaje de Flow, con esqueleto humanoide de Mixamo y sus
    /// animaciones. Ya tiene las proporciones y la cara de la imagen: no se agranda la cabeza ni se pintan ojos.
    /// </summary>
    private bool ConstruirGenerado()
    {
        GameObject prefab = Resources.Load<GameObject>("Modelos3D/personaje/personaje_rig");
        if (prefab == null) return false;
        Animator anim = prefab.GetComponentInChildren<Animator>();
#if UNITY_EDITOR
        try
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "..", "Automatizacion", "diag_generado.txt"),
                "animator=" + (anim != null) + " avatar=" + (anim != null && anim.avatar != null) +
                (anim != null && anim.avatar != null ? " humano=" + anim.avatar.isHuman + " valido=" + anim.avatar.isValid : "") +
                " hijos=" + prefab.transform.childCount + "\n");
        }
        catch (System.Exception) { }
#endif
        if (anim == null || anim.avatar == null || !anim.avatar.isHuman || !anim.avatar.isValid) return false;

        GameObject go = Instantiate(prefab, transform, false);
        go.name = "Modelo";
        modelo = go.transform;
        Generado = true;
        UsaGenerado = true;
        EscalaCabeza = 1f; EscalaPiernas = 1f; EscalaBrazos = 1f;
        foreach (Collider c in go.GetComponentsInChildren<Collider>()) Destroy(c);
        animador = go.GetComponentInChildren<Animator>();

        cabeza = H(HumanBodyBones.Head); cuello = H(HumanBodyBones.Neck);
        pecho = H(HumanBodyBones.UpperChest) ?? H(HumanBodyBones.Chest); cadera = H(HumanBodyBones.Hips);
        brazoI = H(HumanBodyBones.LeftUpperArm); brazoD = H(HumanBodyBones.RightUpperArm);
        codoI = H(HumanBodyBones.LeftLowerArm); codoD = H(HumanBodyBones.RightLowerArm);
        munecaI = H(HumanBodyBones.LeftHand); munecaD = H(HumanBodyBones.RightHand);
        musloI = H(HumanBodyBones.LeftUpperLeg); musloD = H(HumanBodyBones.RightUpperLeg);
        rodillaI = H(HumanBodyBones.LeftLowerLeg); rodillaD = H(HumanBodyBones.RightLowerLeg);
        pieI = H(HumanBodyBones.LeftFoot); pieD = H(HumanBodyBones.RightFoot);

        // Textura: la del GLB original (Mixamo no la conserva); el mallado y sus UV son los mismos
        Material[] mats = Modelos3D.MaterialesDe("personaje_base");
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            if (mats != null && mats.Length > 0)
            {
                var lista = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < lista.Length; i++) lista[i] = mats[Mathf.Min(i, mats.Length - 1)];
                r.sharedMaterials = lista;
            }
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            var smr = r as SkinnedMeshRenderer;
            if (smr != null) smr.updateWhenOffscreen = true;
        }

        modelo.localScale = Vector3.one;
        modelo.localPosition = Vector3.zero;
        modelo.localRotation = Quaternion.identity;
        // Los límites de un SkinnedMeshRenderer recién creado no son fiables (Mixamo): se mide la malla en pose de reposo
        float min, max;
        MedirMalla(go, out min, out max);
        float alto = max - min;
        escala = alto > 0.01f ? Altura / alto : 1f;
        modelo.localScale = Vector3.one * escala;
        if (pieI != null) alturaTobillo = transform.InverseTransformPoint(pieI.position).y;
        MedirMalla(go, out min, out max);
        caida = -min;
        GuardarPies();

        foreach (AnimationClip c in Resources.LoadAll<AnimationClip>("Modelos3D/personaje"))
            if (!c.name.StartsWith("__preview__") && !clips.ContainsKey(c.name)) clips[c.name] = c;
        animador.applyRootMotion = false;
        animador.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        grafo = PlayableGraph.Create("Avatar");
        grafo.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mezcla = AnimationMixerPlayable.Create(grafo, 2);
        var salida = AnimationPlayableOutput.Create(grafo, "Avatar", animador);
        salida.SetSourcePlayable(mezcla);
        grafo.Play();
        Reproducir("Idle", 0f);
        CrearAccesorios();
        return true;
    }

    /// <summary>Altura mínima y máxima (en el espacio del avatar) de las mallas en pose de reposo.</summary>
    private void MedirMalla(GameObject go, out float min, out float max)
    {
        min = float.MaxValue; max = float.MinValue;
        foreach (SkinnedMeshRenderer smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (smr.sharedMesh == null) continue;
            Matrix4x4 m = transform.worldToLocalMatrix * smr.transform.localToWorldMatrix;
            foreach (Vector3 v in smr.sharedMesh.vertices)
            {
                float y = m.MultiplyPoint3x4(v).y;
                if (y < min) min = y;
                if (y > max) max = y;
            }
        }
        if (min > max)
        {
            // Malla no legible: se estima con los huesos (pies → cabeza ≈ 86 % de la altura)
            if (cabeza != null && pieI != null)
            {
                float pies = transform.InverseTransformPoint(pieI.position).y;
                float alto = (transform.InverseTransformPoint(cabeza.position).y - pies) / 0.86f;
                min = pies - alto * 0.04f; max = min + alto;
            }
            else { min = 0f; max = Altura; }
        }
    }

    private Transform H(HumanBodyBones b) { return animador != null ? animador.GetBoneTransform(b) : null; }

    // Nombres de animación de Quaternius → Mixamo (el personaje generado usa las de Mixamo)
    private static readonly Dictionary<string, string[]> Alias = new Dictionary<string, string[]>
    {
        { "Walk", new[] { "Walking" } }, { "Idle_Neutral", new[] { "Idle" } },
        { "Curl", new[] { "Bicep Curl", "BicepCurl" } }, { "Sentado", new[] { "Sitting Idle", "Sitting" } },
        { "Teclear", new[] { "Typing" } }, { "Estirar", new[] { "Neck Stretching", "Stretching" } },
        { "Celebrar", new[] { "Cheering" } }, { "Saludar", new[] { "Waving", "Wave" } },
    };

    private bool Tiene(string nombre) { return Buscar(nombre) != null; }

    private string Buscar(string nombre)
    {
        if (clips.ContainsKey(nombre)) return nombre;
        string[] a;
        if (Alias.TryGetValue(nombre, out a)) foreach (string n in a) if (clips.ContainsKey(n)) return n;
        return null;
    }

    private Transform Hueso(string nombre)
    {
        foreach (Transform t in modelo.GetComponentsInChildren<Transform>(true))
            if (t.name == nombre) return t;
        return null;
    }

    private float AltoModelo()
    {
        float min = float.MaxValue, max = float.MinValue;
        foreach (Renderer r in modelo.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            min = Mathf.Min(min, b.min.y); max = Mathf.Max(max, b.max.y);
        }
        return max > min ? (max - min) / Mathf.Max(0.0001f, transform.lossyScale.y) : 0f;
    }

    /// <summary>Colores de la hoja de personaje (bases 5.15): sudadera turquesa, vaqueros, zapatillas blancas, pelo castaño oscuro.</summary>
    private void Recolorear()
    {
        foreach (Renderer r in modelo.GetComponentsInChildren<Renderer>())
        {
            Material[] ms = r.sharedMaterials;
            for (int i = 0; i < ms.Length; i++)
            {
                string n = ms[i] != null ? ms[i].name.Replace(" (Instance)", "") : "";
                bool pies = r.name.Contains("Feet");
                Color c;
                switch (n)
                {
                    case "Skin": c = r.name.Contains("Legs") ? Paleta.Hex("5A7DB5") : Paleta.Hex("EDB48C"); break;
                    case "Hair": c = Paleta.Hex("3E2A1E"); break;
                    case "Eyebrows": c = Paleta.Hex("3E2A1E"); break;
                    case "Eye": c = Paleta.Hex("2B211C"); break;
                    case "LightBlue": c = Paleta.Hex("5A7DB5"); break;
                    case "White": c = Paleta.Hex("F4F2EE"); break;
                    case "Purple": c = pies ? Paleta.Hex("D9D4CC") : ColorSudadera; break;
                    default: c = ms[i] != null ? ms[i].color : Color.gray; break;
                }
                Material m = NuevoMat(c, n == "Eye" ? 0.6f : 0.15f);
                if (n == "Purple" && !pies) sudadera.Add(m);
                ms[i] = m;
            }
            r.sharedMaterials = ms;
        }
    }

    private static Material NuevoMat(Color color, float brillo)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s);
        m.SetColor("_BaseColor", color);
        m.color = color;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", brillo);
        return m;
    }

    private static Material Mat(Color color)
    {
        Material m;
        if (materiales.TryGetValue(color, out m) && m != null) return m;
        m = NuevoMat(color, 0.15f);
        materiales[color] = m;
        return m;
    }

    private Transform Pieza(PrimitiveType tipo, string nombre, Vector3 pos, Vector3 tam, Color color, Transform padre)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        go.transform.localScale = tam;
        go.GetComponent<Renderer>().sharedMaterial = Mat(color);
        Destroy(go.GetComponent<Collider>());
        return go.transform;
    }

    private Transform Grupo(string nombre)
    {
        var t = new GameObject(nombre).transform;
        t.SetParent(transform, false);
        return t;
    }

    /// <summary>Mochila, pesas, libro, mando y asiento. Cuelgan del avatar y siguen a los huesos en LateUpdate.</summary>
    private void CrearAccesorios()
    {
        float h = Altura;
        mochila = Grupo("Mochila");
        Color gris = Paleta.Mochila, grisOscuro = Paleta.Hex("3A3E45");
        Pieza(PrimitiveType.Cube, "Cuerpo", Vector3.zero, new Vector3(0.29f, 0.32f, 0.13f) * h, gris, mochila);
        Pieza(PrimitiveType.Sphere, "Tapa", new Vector3(0, 0.15f * h, 0), new Vector3(0.29f, 0.09f, 0.13f) * h, gris, mochila);
        Pieza(PrimitiveType.Cube, "Bolsillo", new Vector3(0, -0.06f * h, -0.075f * h), new Vector3(0.2f, 0.13f, 0.04f) * h, grisOscuro, mochila);
        Pieza(PrimitiveType.Cube, "AsaI", new Vector3(-0.1f * h, 0.03f * h, 0.075f * h), new Vector3(0.035f, 0.3f, 0.02f) * h, grisOscuro, mochila);
        Pieza(PrimitiveType.Cube, "AsaD", new Vector3(0.1f * h, 0.03f * h, 0.075f * h), new Vector3(0.035f, 0.3f, 0.02f) * h, grisOscuro, mochila);

        pesaI = Pesa(); pesaD = Pesa();

        libro = Grupo("Libro");
        Color azul = Paleta.DeArea(Area.Mente);
        Pieza(PrimitiveType.Cube, "TapaI", new Vector3(-0.055f * h, 0, 0), new Vector3(0.11f, 0.15f, 0.012f) * h, azul, libro).localRotation = Quaternion.Euler(0, 16, 0);
        Pieza(PrimitiveType.Cube, "TapaD", new Vector3(0.055f * h, 0, 0), new Vector3(0.11f, 0.15f, 0.012f) * h, azul, libro).localRotation = Quaternion.Euler(0, -16, 0);
        Pieza(PrimitiveType.Cube, "Hojas", new Vector3(0, 0, -0.008f * h), new Vector3(0.2f, 0.135f, 0.012f) * h, Paleta.Blanco, libro);

        mando = Grupo("Mando");
        Pieza(PrimitiveType.Cube, "Cuerpo", Vector3.zero, new Vector3(0.14f, 0.035f, 0.065f) * h, Paleta.Hex("3A3F4A"), mando);
        Pieza(PrimitiveType.Sphere, "Boton", new Vector3(0.04f * h, 0.02f * h, 0), Vector3.one * 0.02f * h, Paleta.Coral, mando);

        asiento = Grupo("Asiento");
        Pieza(PrimitiveType.Cube, "Asiento", new Vector3(0, -0.05f * h, -0.02f * h), new Vector3(0.42f, 0.1f, 0.36f) * h, Paleta.Hex("D98E5F"), asiento);
        Pieza(PrimitiveType.Cube, "Respaldo", new Vector3(0, 0.16f * h, -0.2f * h), new Vector3(0.42f, 0.38f, 0.07f) * h, Paleta.Hex("C77D52"), asiento);
        Pieza(PrimitiveType.Cube, "Base", new Vector3(0, -0.3f * h, -0.02f * h), new Vector3(0.38f, 0.4f, 0.32f) * h, Paleta.Hex("C77D52"), asiento);
        MostrarAccesorios("");
    }

    private Transform Pesa()
    {
        float h = Altura;
        Transform g = Grupo("Pesa");
        Pieza(PrimitiveType.Cylinder, "Barra", Vector3.zero, new Vector3(0.022f, 0.06f, 0.022f) * h, Paleta.Hex("5C616B"), g).localRotation = Quaternion.Euler(0, 0, 90);
        Pieza(PrimitiveType.Cylinder, "Disco", new Vector3(-0.06f * h, 0, 0), new Vector3(0.07f, 0.016f, 0.07f) * h, Paleta.Hex("3A3F4A"), g).localRotation = Quaternion.Euler(0, 0, 90);
        Pieza(PrimitiveType.Cylinder, "Disco", new Vector3(0.06f * h, 0, 0), new Vector3(0.07f, 0.016f, 0.07f) * h, Paleta.Hex("3A3F4A"), g).localRotation = Quaternion.Euler(0, 0, 90);
        return g;
    }

    private string accionMostrada;

    private static bool EsSentado(string a)
    {
        return a == "estudiar" || a == "trabajar" || a == "quedar" || a == "videojuegos" || a == "descansar";
    }

    private void MostrarAccesorios(string a)
    {
        accionMostrada = a;
        pesaI.gameObject.SetActive(a == "entrenar"); pesaD.gameObject.SetActive(a == "entrenar");
        libro.gameObject.SetActive(a == "estudiar" || a == "trabajar");
        mando.gameObject.SetActive(a == "videojuegos");
        asiento.gameObject.SetActive(EsSentado(a));
    }

    // ---------------------------------------------------------------- animaciones

    private void PrepararAnimaciones(GameObject go)
    {
        foreach (AnimationClip c in Resources.LoadAll<AnimationClip>("Personajes/Chico"))
        {
            string n = c.name;
            int k = n.LastIndexOf('|');
            if (k >= 0) n = n.Substring(k + 1);
            if (!clips.ContainsKey(n)) clips[n] = c;
        }
        animador = go.GetComponent<Animator>();
        if (animador == null) animador = go.AddComponent<Animator>();
        animador.applyRootMotion = false;
        animador.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        grafo = PlayableGraph.Create("Avatar");
        grafo.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mezcla = AnimationMixerPlayable.Create(grafo, 2);
        var salida = AnimationPlayableOutput.Create(grafo, "Avatar", animador);
        salida.SetSourcePlayable(mezcla);
        grafo.Play();
        Reproducir("Idle", 0f);
        if (clips.Count == 0) Debug.LogWarning("[Avatar] El modelo no trae animaciones");
    }

    private void Reproducir(string nombre, float fundir = 0.25f, float velocidad = 1f)
    {
        if (!grafo.IsValid()) return;
        AnimationClip c;
        string real = Buscar(nombre);
        if (real == null) real = "Idle";
        if (!clips.TryGetValue(real, out c)) return;
        nombre = real;
        if (nombre == clipActual)
        {
            Playable p0 = mezcla.GetInput(0);
            if (p0.IsValid()) p0.SetSpeed(velocidad);
            return;
        }
        Playable viejo = mezcla.GetInput(1);
        if (viejo.IsValid()) { grafo.Disconnect(mezcla, 1); viejo.Destroy(); }
        Playable actual = mezcla.GetInput(0);
        if (actual.IsValid()) { grafo.Disconnect(mezcla, 0); grafo.Connect(actual, 0, mezcla, 1); }
        var nuevo = AnimationClipPlayable.Create(grafo, c);
        nuevo.SetSpeed(velocidad);
        grafo.Connect(nuevo, 0, mezcla, 0);
        clipActual = nombre;
        duracionFundido = fundir;
        fundido = fundir <= 0f ? 1f : 0f;
        AplicarPesos();
    }

    private void AplicarPesos()
    {
        float w = Mathf.Clamp01(fundido);
        mezcla.SetInputWeight(0, w);
        mezcla.SetInputWeight(1, mezcla.GetInput(1).IsValid() ? 1f - w : 0f);
    }

    private void OnDestroy()
    {
        if (grafo.IsValid()) grafo.Destroy();
    }

    // ---------------------------------------------------------------- API

    public void PonerRopa(string colorHex)
    {
        Color c = Paleta.Hex(colorHex);
        foreach (Material m in sudadera) { m.SetColor("_BaseColor", c); m.color = c; }
    }

    public void PonerEstado(TipoEstadoAvatar estado) { estadoAvatar = estado; }

    public TipoEstadoAvatar Estado { get { return estadoAvatar; } }

    // ---------------------------------------------------------------- bucle

    private void Update()
    {
        if (modelo == null) return;
        string a = Caminando ? "" : (Accion ?? "");
        if (a != accionMostrada) MostrarAccesorios(a);

        if (Caminando) Reproducir("Walk", 0.2f, Generado ? 1f : 1.15f);
        else if (a == "pasear") Reproducir("Walk", 0.25f, 0.9f);
        else if (Generado && a == "entrenar" && Tiene("Curl")) Reproducir("Curl", 0.3f);
        else if (Generado && a == "celebrar" && Tiene("Celebrar")) Reproducir("Celebrar", 0.3f);
        else if (Generado && a == "estirarse" && Tiene("Estirar")) Reproducir("Estirar", 0.3f);
        else if (Generado && a == "trabajar" && Tiene("Teclear")) Reproducir("Teclear", 0.3f);
        else if (Generado && EsSentado(a) && Tiene("Sentado")) Reproducir("Sentado", 0.3f);
        else if (a == "quedar" || a == "estirarse" || a == "celebrar") Reproducir("Idle", 0.3f);
        else if (a == "entrenar" || EsSentado(a) || a == "meditar") Reproducir("Idle_Neutral", 0.3f);
        else if (estadoAvatar == TipoEstadoAvatar.Cansado || estadoAvatar == TipoEstadoAvatar.Descuidado) Reproducir("Idle_Neutral", 0.4f, 0.7f);
        else Reproducir("Idle", 0.4f);

        if (fundido < 1f)
        {
            fundido += Time.deltaTime / Mathf.Max(0.01f, duracionFundido);
            AplicarPesos();
        }
    }

    /// <summary>Gira un hueso en el espacio del avatar (eje dado en coordenadas del avatar), encima de la animación.</summary>
    private void Girar(Transform hueso, Vector3 ejeLocal, float grados)
    {
        if (hueso == null || grados == 0f) return;
        Vector3 eje = transform.TransformDirection(ejeLocal);
        hueso.rotation = Quaternion.AngleAxis(grados, eje) * hueso.rotation;
    }

    private void LateUpdate()
    {
        if (modelo == null) return;
        float t = Time.time;
        if (cabeza != null) cabeza.localScale = Vector3.one * EscalaCabeza;
        modelo.localPosition = new Vector3(0, caida, 0);
        EscalarExtremidades(1f, 1f);
        GuardarPies();
        EscalarExtremidades(EscalaPiernas, EscalaBrazos);
        if (++fotogramas == 30) Diagnostico();
        string a = Caminando ? "" : (Accion ?? "");
        Vector3 X = Vector3.right, Y = Vector3.up, Z = Vector3.forward;

        if (a == "")
        {
            // Estados del avatar (bases 5.15): mismo cuerpo, cambia la postura.
            if (estadoAvatar == TipoEstadoAvatar.Cansado) { Girar(pecho, X, 8f); Girar(cabeza, X, 10f); }
            else if (estadoAvatar == TipoEstadoAvatar.Descuidado) { Girar(pecho, X, 14f); Girar(cabeza, X, 16f); }
            else if (estadoAvatar == TipoEstadoAvatar.EnForma && !Caminando) { Girar(pecho, X, -4f); Girar(cabeza, X, -4f); }
        }
        else if ((a == "estirarse" && Generado && Tiene("Estirar")) || (a == "celebrar" && Generado && Tiene("Celebrar"))
                 || (a == "entrenar" && Generado && Tiene("Curl")) || (a == "trabajar" && Generado && Tiene("Teclear")))
        {
            // Animación real de Mixamo: nada que posar a mano
        }
        else if (a == "estirarse" || a == "celebrar")
        {
            float c = a == "celebrar" ? Mathf.Abs(Mathf.Sin(t * 5f)) : (Mathf.Sin(t * 1.4f) + 1f) * 0.5f;
            float alto = a == "celebrar" ? 150f + c * 15f : 135f + c * 30f;
            Girar(brazoI, Z, -alto); Girar(brazoD, Z, alto);
            Girar(codoI, Z, -10f); Girar(codoD, Z, 10f);
            Girar(cabeza, X, -10f);
            if (a == "celebrar") modelo.localPosition += new Vector3(0, c * 0.12f * Altura, 0);
            else Girar(pecho, X, -6f * c);
        }
        else if (a == "entrenar")
        {
            // Curl de bíceps alterno con pesas.
            float c = (Mathf.Sin(t * 4f) + 1f) * 0.5f;
            Girar(brazoI, X, -12f); Girar(brazoD, X, -12f);
            Girar(codoI, X, -25f - c * 105f);
            Girar(codoD, X, -25f - (1f - c) * 105f);
        }
        else if (a == "meditar")
        {
            float f = Mathf.Sin(t * 1.3f);
            Girar(musloI, X, -82f); Girar(musloI, Y, -48f);
            Girar(musloD, X, -82f); Girar(musloD, Y, 48f);
            Girar(rodillaI, Y, 150f); Girar(rodillaI, X, 12f);
            Girar(rodillaD, Y, -150f); Girar(rodillaD, X, 12f);
            Girar(brazoI, X, -20f); Girar(brazoI, Z, -12f); Girar(codoI, X, -50f);
            Girar(brazoD, X, -20f); Girar(brazoD, Z, 12f); Girar(codoD, X, -50f);
            Girar(pecho, X, f * 2f);
            SeguirPies();
            ApoyarEnSuelo(true);
        }
        else if (EsSentado(a))
        {
            bool animSentado = Generado && Tiene("Sentado");
            if (!animSentado)
            {
                Girar(musloI, X, -88f); Girar(musloD, X, -88f);
                Girar(rodillaI, X, 88f); Girar(rodillaD, X, 88f);
            }
            if (a == "descansar")
            {
                Girar(pecho, X, -12f + Mathf.Sin(t * 1.2f) * 1.5f);
                Girar(cabeza, X, 8f);
            }
            else if (a == "quedar")
            {
                Girar(brazoD, X, -60f); Girar(brazoD, Z, 40f + Mathf.Sin(t * 6f) * 15f); Girar(codoD, X, -40f);
                Girar(pecho, Y, Mathf.Sin(t * 1.5f) * 6f);
            }
            else
            {
                float j = a == "videojuegos" ? Mathf.Sin(t * 14f) * 4f : 0f;
                float br = Generado ? -18f : -38f, co = Generado ? -35f : -55f; // el generado ya trae los brazos más altos
                Girar(brazoI, X, br + j); Girar(brazoD, X, br - j);
                Girar(codoI, X, co); Girar(codoD, X, co);
                Girar(pecho, X, 6f + Mathf.Sin(t * 1.6f) * 1.5f);
                Girar(cabeza, X, 10f);
            }
            SeguirPies();
            if (!animSentado) ApoyarEnSuelo(false);
        }
        SeguirPies();
        ColocarAccesorios(a);
    }

    private void EscalarExtremidades(float piernas, float brazos)
    {
        if (musloI != null) musloI.localScale = Vector3.one * piernas;
        if (musloD != null) musloD.localScale = Vector3.one * piernas;
        if (brazoI != null) brazoI.localScale = Vector3.one * brazos;
        if (brazoD != null) brazoD.localScale = Vector3.one * brazos;
    }

    /// <summary>
    /// Ojos grandes y mejillas estilo chibi (como la hoja de personaje), colocados sobre los ojos del modelo
    /// y colgados del hueso de la cabeza para que sigan la animación.
    /// </summary>
    private void CrearOjos()
    {
        if (cabeza == null) return;
        foreach (SkinnedMeshRenderer smr in modelo.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!smr.name.Contains("Head") || smr.sharedMesh == null) continue;
            int hi = System.Array.IndexOf(smr.bones, cabeza);
            if (hi < 0) continue;
            Mesh mesh = smr.sharedMesh;
            Matrix4x4 bp = mesh.bindposes[hi];
            Vector3 adelante = bp.MultiplyVector(smr.transform.InverseTransformDirection(transform.forward)).normalized;
            Vector3 arriba = bp.MultiplyVector(smr.transform.InverseTransformDirection(transform.up)).normalized;
            Vector3 derecha = Vector3.Cross(arriba, adelante).normalized;
            Material[] ms = smr.sharedMaterials;
            Vector3[] v = mesh.vertices;
            for (int sub = 0; sub < mesh.subMeshCount && sub < ms.Length; sub++)
            {
                if (ms[sub] == null || ms[sub].color != Paleta.Hex("2B211C")) continue;
                int[] tri = mesh.GetTriangles(sub);
                Vector3 cI = Vector3.zero, cD = Vector3.zero; int nI = 0, nD = 0;
                float alto = 0f;
                foreach (int k in tri)
                {
                    Vector3 pb = bp.MultiplyPoint3x4(v[k]);
                    if (Vector3.Dot(pb, derecha) < 0) { cI += pb; nI++; } else { cD += pb; nD++; }
                }
                if (nI > 0) cI /= nI;
                if (nD > 0) cD /= nD;
                foreach (int k in tri)
                {
                    Vector3 pb = bp.MultiplyPoint3x4(v[k]);
                    Vector3 c = Vector3.Dot(pb, derecha) < 0 ? cI : cD;
                    alto = Mathf.Max(alto, Mathf.Abs(Vector3.Dot(pb - c, arriba)) * 2f);
                }
                float L = cabeza.lossyScale.x;
                if (nI > 0) Ojo(cI, alto * L, adelante, arriba, Vector3.Dot(cI, derecha), L);
                if (nD > 0) Ojo(cD, alto * L, adelante, arriba, Vector3.Dot(cD, derecha), L);
                diagOjos = "ojos en hueso: " + cI.ToString("F4") + " " + cD.ToString("F4") + " alto=" + (alto * L).ToString("F3") + " L=" + L;
                return;
            }
        }
    }

    private string diagOjos = "sin ojos";

    private void Ojo(Vector3 centroHueso, float altoOjo, Vector3 adelante, Vector3 arriba, float lado, float L)
    {
        float h = Altura;
        Transform t = new GameObject("Ojo").transform;
        t.SetParent(cabeza, false);
        t.localRotation = Quaternion.LookRotation(adelante, arriba);
        t.localScale = Vector3.one / L;
        t.localPosition = centroHueso + adelante * (0.006f * h / L);
        float alto = Mathf.Max(altoOjo * 2.1f, 0.03f * h);
        float ancho = alto * 0.8f;
        Pieza(PrimitiveType.Sphere, "Pupila", Vector3.zero, new Vector3(ancho, alto, 0.01f * h), Paleta.Hex("2B211C"), t);
        Pieza(PrimitiveType.Sphere, "Brillo", new Vector3(ancho * 0.2f, alto * 0.22f, 0.005f * h), new Vector3(ancho * 0.38f, alto * 0.32f, 0.004f * h), Paleta.Blanco, t);
        Pieza(PrimitiveType.Sphere, "Mejilla", new Vector3(Mathf.Sign(lado) * ancho * 0.45f, -alto * 1.0f, -0.006f * h), new Vector3(ancho * 1.1f, alto * 0.42f, 0.005f * h), Paleta.Hex("F6AE98"), t);
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private void Diagnostico()
    {
        try
        {
            string ruta = System.IO.Path.Combine(Application.dataPath, "..", "Automatizacion", "diag_avatar.txt");
            string txt = "escala=" + escala + " cabeza.local=" + (cabeza != null ? cabeza.localScale.ToString("F2") : "-")
                + " cabeza.lossy=" + (cabeza != null ? cabeza.lossyScale.ToString("F3") : "-")
                + " muslo.lossy=" + (musloI != null ? musloI.lossyScale.ToString("F3") : "-")
                + " caida=" + caida + " tobillo=" + alturaTobillo + "\n" + diagOjos
                + "\ncabezaPos=" + (cabeza != null ? transform.InverseTransformPoint(cabeza.position).ToString("F3") : "-")
                + " cuelloPos=" + (cuello != null ? transform.InverseTransformPoint(cuello.position).ToString("F3") : "-")
                + " ruta=" + RutaHueso(cabeza)
                + " ojos=" + OjosDiag()
                + " smr=" + SmrDiag()
                + "\nclips=" + string.Join(",", new List<string>(clips.Keys).ToArray()) + "\n";
            System.IO.File.AppendAllText(ruta, gameObject.name + " " + transform.parent?.name + ": " + txt);
        }
        catch { }
    }

    // Los pies de este esqueleto son controles aparte (no cuelgan de la rodilla): al posar las piernas, hay que llevarlos con la espinilla.
    private Vector3 pieOffI, pieOffD;
    private Quaternion pieRotI, pieRotD;

    private void GuardarPies()
    {
        if (pieI == null || rodillaI == null || pieD == null || rodillaD == null) return;
        pieOffI = rodillaI.InverseTransformPoint(pieI.position); pieRotI = Quaternion.Inverse(rodillaI.rotation) * pieI.rotation;
        pieOffD = rodillaD.InverseTransformPoint(pieD.position); pieRotD = Quaternion.Inverse(rodillaD.rotation) * pieD.rotation;
    }

    private void SeguirPies()
    {
        if (pieI == null || rodillaI == null || pieD == null || rodillaD == null) return;
        pieI.position = rodillaI.TransformPoint(pieOffI); pieI.rotation = rodillaI.rotation * pieRotI;
        pieD.position = rodillaD.TransformPoint(pieOffD); pieD.rotation = rodillaD.rotation * pieRotD;
    }

    private string RutaHueso(Transform t)
    {
        string r = "";
        while (t != null && t != transform) { r = t.name + "/" + r; t = t.parent; }
        return r;
    }

    private string OjosDiag()
    {
        string r = "";
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == "Pupila") r += transform.InverseTransformPoint(t.position).ToString("F2") + (t.GetComponent<Renderer>().enabled ? "" : "off") + " L" + t.gameObject.layer + " ";
        return r;
    }

    private string SmrDiag()
    {
        string r = "";
        foreach (SkinnedMeshRenderer smr in modelo.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            r += smr.name + "[root=" + (smr.rootBone != null ? smr.rootBone.name : "-") + " huesos=" + smr.bones.Length;
            if (smr.name.Contains("Head") && smr.sharedMesh != null)
            {
                var bw = smr.sharedMesh.GetAllBoneWeights();
                var cuenta = new Dictionary<string, int>();
                foreach (var w in bw) { string n = smr.bones[w.boneIndex] != null ? smr.bones[w.boneIndex].name : "?"; int c; cuenta.TryGetValue(n, out c); cuenta[n] = c + 1; }
                foreach (var kv in cuenta) r += " " + kv.Key + ":" + kv.Value;
            }
            r += "] ";
        }
        return r;
    }

    /// <summary>Baja el modelo para que los pies (o las rodillas, al meditar) toquen el suelo después de posar.</summary>
    private void ApoyarEnSuelo(bool usarRodillas)
    {
        float y = float.MaxValue;
        if (pieI != null) y = Mathf.Min(y, transform.InverseTransformPoint(pieI.position).y - alturaTobillo);
        if (pieD != null) y = Mathf.Min(y, transform.InverseTransformPoint(pieD.position).y - alturaTobillo);
        if (usarRodillas)
        {
            if (rodillaI != null) y = Mathf.Min(y, transform.InverseTransformPoint(rodillaI.position).y - 0.04f * Altura);
            if (rodillaD != null) y = Mathf.Min(y, transform.InverseTransformPoint(rodillaD.position).y - 0.04f * Altura);
        }
        if (y < float.MaxValue) modelo.localPosition = new Vector3(0, caida - y, 0);
    }

    private Vector3 Local(Transform hueso) { return transform.InverseTransformPoint(hueso.position); }

    private void ColocarAccesorios(string a)
    {
        float h = Altura;
        if (pecho != null)
        {
            Vector3 p = Local(pecho);
            Quaternion r = Quaternion.Inverse(transform.rotation) * pecho.rotation;
            // La mochila sigue la inclinación del pecho alrededor del eje lateral.
            Vector3 arriba = r * Vector3.up;
            float incl = Mathf.Atan2(arriba.z, arriba.y) * Mathf.Rad2Deg;
            mochila.localRotation = Quaternion.Euler(incl, 0, 0);
            mochila.localPosition = p + mochila.localRotation * new Vector3(0, -0.02f * h, -0.115f * h);
        }
        if (a == "entrenar" && munecaI != null && munecaD != null)
        {
            pesaI.localPosition = Local(munecaI) + new Vector3(0, -0.01f * h, 0.03f * h);
            pesaD.localPosition = Local(munecaD) + new Vector3(0, -0.01f * h, 0.03f * h);
        }
        if ((a == "estudiar" || a == "trabajar" || a == "videojuegos") && munecaI != null && munecaD != null)
        {
            Vector3 m = (Local(munecaI) + Local(munecaD)) * 0.5f;
            libro.localPosition = m + new Vector3(0, 0.05f * h, 0.04f * h);
            libro.localRotation = Quaternion.Euler(-35f, 0, 0);
            mando.localPosition = m + new Vector3(0, 0.01f * h, 0.03f * h);
        }
        if (EsSentado(a) && cadera != null)
        {
            Vector3 c = Local(cadera);
            asiento.localPosition = new Vector3(0, c.y - 0.03f * h, c.z - 0.02f * h);
        }
    }
}
