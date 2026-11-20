using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Acceso a los modelos generados (Flow → TRELLIS) que viven en Resources/Modelos3D.
/// Los modelos de IA llegan con tamaños, pivotes y materiales arbitrarios: aquí se normalizan
/// (pivote abajo en el centro, tamaño pedido, material mate y sombras) para colocarlos sin retoques.
/// Si un modelo aún no existe, <see cref="Crear"/> devuelve null y el juego sigue con el arte anterior.
/// </summary>
public static class Modelos3D
{
    public enum Medida { Ancho, Alto }

    private static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();
    private static Shader litUrp;
    /// <summary>Cuánta luz propia llevan los modelos generados (0 = ninguna).</summary>
    public static float Brillo = 0.35f;

    /// <summary>¿Hay modelo con ese nombre? (p. ej. "edificio_gimnasio").</summary>
    public static bool Existe(string nombre)
    {
        return Cargar(nombre) != null;
    }

    /// <summary>
    /// Crea el modelo dentro de un contenedor cuyo origen es el centro de la base del modelo.
    /// <paramref name="tam"/> es el ancho mayor (edificios, parcelas) o el alto (farolas, árboles, objetos).
    /// </summary>
    public static GameObject Crear(string nombre, Transform padre, float tam, Medida medida = Medida.Ancho, bool mate = true)
    {
        GameObject fuente = Cargar(nombre);
        if (fuente == null) return null;
        var raiz = new GameObject(nombre);
        raiz.transform.SetParent(padre, false);
        GameObject modelo = Object.Instantiate(fuente, raiz.transform, false);
        modelo.name = "Modelo";

        Renderer[] rs = modelo.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return raiz;
        Bounds b = Limites(rs);
        float actual = medida == Medida.Alto ? b.size.y : Mathf.Max(b.size.x, b.size.z);
        float escala = actual > 0.0001f ? tam / actual : 1f;
        modelo.transform.localScale *= escala;
        b = Limites(rs);
        // Origen del contenedor = centro de la base
        Vector3 desfase = raiz.transform.position - new Vector3(b.center.x, b.min.y, b.center.z);
        modelo.transform.position += desfase;

        foreach (Renderer r in rs)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
            if (mate) r.sharedMaterials = Mates(r.sharedMaterials);
        }
        return raiz;
    }

    /// <summary>Añade un colisionador de caja que envuelve el modelo (para poder tocarlo).</summary>
    public static BoxCollider Colisionador(GameObject raiz)
    {
        Renderer[] rs = raiz.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return null;
        Bounds b = Limites(rs);
        var c = raiz.AddComponent<BoxCollider>();
        c.center = raiz.transform.InverseTransformPoint(b.center);
        Vector3 s = raiz.transform.lossyScale;
        c.size = new Vector3(b.size.x / Mathf.Max(s.x, 1e-4f), b.size.y / Mathf.Max(s.y, 1e-4f), b.size.z / Mathf.Max(s.z, 1e-4f));
        return c;
    }

    private static GameObject Cargar(string nombre)
    {
        GameObject g;
        if (cache.TryGetValue(nombre, out g)) return g;
        g = Resources.Load<GameObject>("Modelos3D/" + nombre);
        if (g == null) g = Resources.Load<GameObject>("Modelos3D/personaje/" + nombre);
        cache[nombre] = g;
        return g;
    }

    private static Bounds Limites(Renderer[] rs)
    {
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    // Los GLB de IA suelen traer brillo y metal "horneados" que en el estilo plastilina quedan raros:
    // se pasa a URP Lit mate conservando el color y la textura base.
    private static readonly Dictionary<Material, Material> mates = new Dictionary<Material, Material>();

    private static Material[] Mates(Material[] origen)
    {
        if (litUrp == null) litUrp = Shader.Find("Universal Render Pipeline/Lit");
        if (litUrp == null) return origen;
        var res = new Material[origen.Length];
        for (int i = 0; i < origen.Length; i++)
        {
            Material o = origen[i];
            if (o == null) { res[i] = null; continue; }
            Material m;
            if (!mates.TryGetValue(o, out m))
            {
                m = new Material(litUrp) { name = o.name + " (mate)" };
                Texture tex = Textura(o, "baseColorTexture", "_BaseMap", "_MainTex");
                Color color = Color(o, "baseColorFactor", "_BaseColor", "_Color");
                if (tex != null) m.SetTexture("_BaseMap", tex);
                m.SetColor("_BaseColor", color);
                // Un poco de luz propia con la misma textura: aclara las sombras horneadas y da el aspecto
                // suave y luminoso de las imágenes de Flow
                if (tex != null)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetTexture("_EmissionMap", tex);
                    m.SetColor("_EmissionColor", color * Brillo);
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                }
                m.SetFloat("_Metallic", 0f);
                m.SetFloat("_Smoothness", 0.18f);
                mates[o] = m;
            }
            res[i] = m;
        }
        return res;
    }

    private static Texture Textura(Material m, params string[] props)
    {
        foreach (string p in props) if (m.HasProperty(p) && m.GetTexture(p) != null) return m.GetTexture(p);
        return null;
    }

    private static Color Color(Material m, params string[] props)
    {
        foreach (string p in props) if (m.HasProperty(p)) return m.GetColor(p);
        return UnityEngine.Color.white;
    }
}
