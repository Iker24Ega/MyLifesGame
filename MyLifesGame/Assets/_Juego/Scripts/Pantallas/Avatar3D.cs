using MyLifesGame.Motor;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Avatar chibi 3D hecho con primitivas (sudadera con capucha, mochila, pelo, mejillas) y su animación
/// procedural de andar y reposo según el estado. Lo usan la ciudad y el estudio de retrato.
/// </summary>
public class Avatar3D : MonoBehaviour
{
    public bool Caminando;
    /// <summary>Acción que está haciendo (id de la tabla: entrenar, estudiar, quedar...). Vacío: ninguna.</summary>
    public string Accion = "";
    private GameObject pesaI, pesaD, libro, mando, asiento;
    private Transform cuerpo, brazoI, brazoD, piernaI, piernaD;
    private Renderer[] sudadera;
    private TipoEstadoAvatar estadoAvatar = TipoEstadoAvatar.Normal;
    private static readonly Dictionary<Color, Material> materiales = new Dictionary<Color, Material>();

    public static Avatar3D Crear(Transform padre, int capa = 0)
    {
        var go = new GameObject("Avatar");
        go.transform.SetParent(padre, false);
        var a = go.AddComponent<Avatar3D>();
        a.Construir();
        if (capa != 0) foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = capa;
        return a;
    }

    private void Construir()
    {
        cuerpo = new GameObject("Cuerpo").transform;
        cuerpo.SetParent(transform, false);
        cuerpo.localScale = Vector3.one * 1.45f; // grande y legible (decisión de diseño)

        Color sud = Paleta.Hex("2FA3A0");
        piernaI = Articulacion("Pierna I", new Vector3(-0.2f, 0.75f, 0));
        piernaD = Articulacion("Pierna D", new Vector3(0.2f, 0.75f, 0));
        Pieza(PrimitiveType.Capsule, "Pierna", new Vector3(0, -0.38f, 0), new Vector3(0.3f, 0.38f, 0.3f), Paleta.Vaquero, piernaI);
        Pieza(PrimitiveType.Capsule, "Pierna", new Vector3(0, -0.38f, 0), new Vector3(0.3f, 0.38f, 0.3f), Paleta.Vaquero, piernaD);
        Pieza(PrimitiveType.Cube, "Zapatilla", new Vector3(0, -0.72f, 0.06f), new Vector3(0.28f, 0.14f, 0.42f), Paleta.Blanco, piernaI);
        Pieza(PrimitiveType.Cube, "Zapatilla", new Vector3(0, -0.72f, 0.06f), new Vector3(0.28f, 0.14f, 0.42f), Paleta.Blanco, piernaD);

        var torso = Pieza(PrimitiveType.Capsule, "Sudadera", new Vector3(0, 1.2f, 0), new Vector3(0.82f, 0.5f, 0.62f), sud, cuerpo);
        var capucha = Pieza(PrimitiveType.Sphere, "Capucha", new Vector3(0, 1.62f, -0.2f), new Vector3(0.62f, 0.3f, 0.42f), sud, cuerpo);
        brazoI = Articulacion("Brazo I", new Vector3(-0.47f, 1.52f, 0));
        brazoD = Articulacion("Brazo D", new Vector3(0.47f, 1.52f, 0));
        var mangaI = Pieza(PrimitiveType.Capsule, "Manga", new Vector3(0, -0.32f, 0), new Vector3(0.24f, 0.34f, 0.24f), sud, brazoI);
        var mangaD = Pieza(PrimitiveType.Capsule, "Manga", new Vector3(0, -0.32f, 0), new Vector3(0.24f, 0.34f, 0.24f), sud, brazoD);
        Pieza(PrimitiveType.Sphere, "Mano", new Vector3(0, -0.68f, 0), Vector3.one * 0.2f, Paleta.Piel, brazoI);
        Pieza(PrimitiveType.Sphere, "Mano", new Vector3(0, -0.68f, 0), Vector3.one * 0.2f, Paleta.Piel, brazoD);
        sudadera = new[] { torso.GetComponent<Renderer>(), capucha.GetComponent<Renderer>(), mangaI.GetComponent<Renderer>(), mangaD.GetComponent<Renderer>() };
        foreach (Renderer r in sudadera) r.material = new Material(Mat(sud));

        Pieza(PrimitiveType.Cube, "Mochila", new Vector3(0, 1.25f, -0.42f), new Vector3(0.66f, 0.72f, 0.3f), Paleta.Mochila, cuerpo);
        Pieza(PrimitiveType.Cube, "Bolsillo", new Vector3(0, 1.1f, -0.58f), new Vector3(0.46f, 0.3f, 0.06f), Paleta.Hex("5C616B"), cuerpo);
        Pieza(PrimitiveType.Sphere, "Cabeza", new Vector3(0, 2.1f, 0), new Vector3(0.95f, 0.92f, 0.9f), Paleta.Piel, cuerpo);
        Pieza(PrimitiveType.Sphere, "Pelo", new Vector3(0, 2.34f, -0.06f), new Vector3(1f, 0.62f, 0.98f), Paleta.Pelo, cuerpo);
        Pieza(PrimitiveType.Sphere, "Flequillo", new Vector3(0.12f, 2.42f, 0.32f), new Vector3(0.62f, 0.22f, 0.32f), Paleta.Pelo, cuerpo);
        Pieza(PrimitiveType.Sphere, "Ojo", new Vector3(-0.17f, 2.1f, 0.42f), new Vector3(0.12f, 0.16f, 0.06f), Paleta.Texto, cuerpo);
        Pieza(PrimitiveType.Sphere, "Ojo", new Vector3(0.17f, 2.1f, 0.42f), new Vector3(0.12f, 0.16f, 0.06f), Paleta.Texto, cuerpo);
        Pieza(PrimitiveType.Sphere, "Mejilla", new Vector3(-0.3f, 1.98f, 0.36f), new Vector3(0.14f, 0.08f, 0.04f), Paleta.Hex("F2A08C"), cuerpo);
        Pieza(PrimitiveType.Sphere, "Mejilla", new Vector3(0.3f, 1.98f, 0.36f), new Vector3(0.14f, 0.08f, 0.04f), Paleta.Hex("F2A08C"), cuerpo);
        CrearAccesorios();
    }

    private Transform Articulacion(string nombre, Vector3 pos)
    {
        var t = new GameObject(nombre).transform;
        t.SetParent(cuerpo, false);
        t.localPosition = pos;
        return t;
    }

    private static Material Mat(Color color)
    {
        Material m;
        if (materiales.TryGetValue(color, out m) && m != null) return m;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        m = new Material(s);
        m.SetColor("_BaseColor", color);
        m.color = color;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.12f);
        materiales[color] = m;
        return m;
    }

    private GameObject Pieza(PrimitiveType tipo, string nombre, Vector3 pos, Vector3 escala, Color color, Transform padre)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = Mat(color);
        Destroy(go.GetComponent<Collider>());
        return go;
    }

    public void PonerRopa(string colorHex)
    {
        if (sudadera == null) return;
        Color c = Paleta.Hex(colorHex);
        foreach (Renderer r in sudadera) { r.material.SetColor("_BaseColor", c); r.material.color = c; }
    }

    public void PonerEstado(TipoEstadoAvatar estado) { estadoAvatar = estado; }

    public TipoEstadoAvatar Estado { get { return estadoAvatar; } }

    private void Update() { Animar(); }

    private void CrearAccesorios()
    {
        pesaI = Pesa(brazoI); pesaD = Pesa(brazoD);
        libro = new GameObject("Libro"); libro.transform.SetParent(cuerpo, false);
        libro.transform.localPosition = new Vector3(0, 1.25f, 0.62f);
        libro.transform.localRotation = Quaternion.Euler(-35f, 0, 0);
        Pieza(PrimitiveType.Cube, "Tapa", new Vector3(-0.2f, 0, 0), new Vector3(0.38f, 0.5f, 0.05f), Paleta.DeArea(Area.Mente), libro.transform).transform.localRotation = Quaternion.Euler(0, 18, 0);
        Pieza(PrimitiveType.Cube, "Tapa", new Vector3(0.2f, 0, 0), new Vector3(0.38f, 0.5f, 0.05f), Paleta.DeArea(Area.Mente), libro.transform).transform.localRotation = Quaternion.Euler(0, -18, 0);
        Pieza(PrimitiveType.Cube, "Hojas", new Vector3(0, 0, 0.03f), new Vector3(0.7f, 0.44f, 0.04f), Paleta.Blanco, libro.transform);
        mando = new GameObject("Mando"); mando.transform.SetParent(cuerpo, false);
        mando.transform.localPosition = new Vector3(0, 1.05f, 0.62f);
        Pieza(PrimitiveType.Cube, "Cuerpo", Vector3.zero, new Vector3(0.5f, 0.14f, 0.24f), Paleta.Hex("3A3F4A"), mando.transform);
        Pieza(PrimitiveType.Sphere, "Botón", new Vector3(0.14f, 0.08f, 0), Vector3.one * 0.07f, Paleta.Coral, mando.transform);
        asiento = new GameObject("Asiento"); asiento.transform.SetParent(transform, false);
        Pieza(PrimitiveType.Cube, "Asiento", new Vector3(0, 0.36f, -0.3f), new Vector3(1.9f, 0.72f, 1.6f), Paleta.Hex("D98E5F"), asiento.transform);
        Pieza(PrimitiveType.Cube, "Respaldo", new Vector3(0, 1.15f, -1.05f), new Vector3(1.9f, 1.6f, 0.3f), Paleta.Hex("C77D52"), asiento.transform);
        MostrarAccesorios("");
    }

    private GameObject Pesa(Transform brazo)
    {
        var g = new GameObject("Pesa"); g.transform.SetParent(brazo, false);
        g.transform.localPosition = new Vector3(0, -0.7f, 0.05f);
        Pieza(PrimitiveType.Cylinder, "Barra", Vector3.zero, new Vector3(0.07f, 0.22f, 0.07f), Paleta.Hex("5C616B"), g.transform).transform.localRotation = Quaternion.Euler(0, 0, 90);
        Pieza(PrimitiveType.Cylinder, "Disco", new Vector3(-0.22f, 0, 0), new Vector3(0.24f, 0.05f, 0.24f), Paleta.Hex("3A3F4A"), g.transform).transform.localRotation = Quaternion.Euler(0, 0, 90);
        Pieza(PrimitiveType.Cylinder, "Disco", new Vector3(0.22f, 0, 0), new Vector3(0.24f, 0.05f, 0.24f), Paleta.Hex("3A3F4A"), g.transform).transform.localRotation = Quaternion.Euler(0, 0, 90);
        return g;
    }

    private string accionMostrada = null;

    private void MostrarAccesorios(string a)
    {
        accionMostrada = a;
        bool sentado = a == "estudiar" || a == "trabajar" || a == "quedar" || a == "videojuegos" || a == "descansar";
        pesaI.SetActive(a == "entrenar"); pesaD.SetActive(a == "entrenar");
        libro.SetActive(a == "estudiar" || a == "trabajar");
        mando.SetActive(a == "videojuegos");
        asiento.SetActive(sentado);
    }

    /// <summary>Animación de la acción en curso. Devuelve false si no hay acción.</summary>
    private bool AnimarAccion(float t)
    {
        string a = Accion ?? "";
        if (a != accionMostrada) MostrarAccesorios(a);
        if (a == "" ) return false;
        Quaternion piernasSentado = Quaternion.Euler(-88f, 0, 0);
        switch (a)
        {
            case "entrenar":
            {
                float c = (Mathf.Sin(t * 4.5f) + 1f) * 0.5f;
                piernaI.localRotation = piernaD.localRotation = Quaternion.identity;
                brazoI.localRotation = Quaternion.Euler(-20f - c * 110f, 0, 8f);
                brazoD.localRotation = Quaternion.Euler(-20f - (1f - c) * 110f, 0, -8f);
                cuerpo.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(t * 4.5f)) * 0.04f, 0);
                cuerpo.localRotation = Quaternion.identity;
                break;
            }
            case "pasear":
            {
                float s = Mathf.Sin(t * 7f);
                piernaI.localRotation = Quaternion.Euler(s * 26f, 0, 0);
                piernaD.localRotation = Quaternion.Euler(-s * 26f, 0, 0);
                brazoI.localRotation = Quaternion.Euler(-s * 22f, 0, 6f);
                brazoD.localRotation = Quaternion.Euler(s * 22f, 0, -6f);
                cuerpo.localPosition = new Vector3(0, Mathf.Abs(Mathf.Cos(t * 7f)) * 0.08f, 0);
                cuerpo.localRotation = Quaternion.identity;
                break;
            }
            case "meditar":
            {
                float f = Mathf.Sin(t * 1.3f);
                piernaI.localRotation = Quaternion.Euler(-85f, 0, -38f);
                piernaD.localRotation = Quaternion.Euler(-85f, 0, 38f);
                brazoI.localRotation = Quaternion.Euler(-35f, 0, 28f);
                brazoD.localRotation = Quaternion.Euler(-35f, 0, -28f);
                cuerpo.localPosition = new Vector3(0, -0.85f + 0.12f + f * 0.06f, 0);
                cuerpo.localRotation = Quaternion.identity;
                break;
            }
            default:
            {
                // Sentado: estudiar, trabajar, quedar, videojuegos, descansar
                piernaI.localRotation = piernaD.localRotation = piernasSentado;
                cuerpo.localPosition = new Vector3(0, -0.36f, 0);
                if (a == "descansar")
                {
                    brazoI.localRotation = Quaternion.Euler(0, 0, 12f);
                    brazoD.localRotation = Quaternion.Euler(0, 0, -12f);
                    cuerpo.localRotation = Quaternion.Euler(-14f + Mathf.Sin(t * 1.2f) * 1.5f, 0, 0);
                }
                else if (a == "quedar")
                {
                    brazoI.localRotation = Quaternion.Euler(-10f, 0, 6f);
                    brazoD.localRotation = Quaternion.Euler(-160f, 0, -25f + Mathf.Sin(t * 6f) * 20f);
                    cuerpo.localRotation = Quaternion.Euler(0, Mathf.Sin(t * 1.5f) * 6f, 0);
                }
                else
                {
                    float j = a == "videojuegos" ? Mathf.Sin(t * 14f) * 3f : 0f;
                    brazoI.localRotation = Quaternion.Euler(-62f + j, 0, -14f);
                    brazoD.localRotation = Quaternion.Euler(-62f - j, 0, 14f);
                    cuerpo.localRotation = Quaternion.Euler(6f + Mathf.Sin(t * 1.6f) * 2f, 0, 0);
                }
                break;
            }
        }
        return true;
    }

    private void Animar()
    {
        float t = Time.time;
        if (!Caminando && AnimarAccion(t)) return;
        if (Caminando)
        {
            float s = Mathf.Sin(t * 11f);
            piernaI.localRotation = Quaternion.Euler(s * 32f, 0, 0);
            piernaD.localRotation = Quaternion.Euler(-s * 32f, 0, 0);
            brazoI.localRotation = Quaternion.Euler(-s * 28f, 0, 6f);
            brazoD.localRotation = Quaternion.Euler(s * 28f, 0, -6f);
            cuerpo.localPosition = new Vector3(0, Mathf.Abs(Mathf.Cos(t * 11f)) * 0.12f, 0);
            cuerpo.localRotation = Quaternion.identity;
        }
        else
        {
            float respira = Mathf.Sin(t * 2.2f);
            float inclinacion = estadoAvatar == TipoEstadoAvatar.Cansado ? 9f : estadoAvatar == TipoEstadoAvatar.Descuidado ? 14f : 0f;
            float brazos = estadoAvatar == TipoEstadoAvatar.EnForma ? 10f : 4f;
            piernaI.localRotation = piernaD.localRotation = Quaternion.identity;
            brazoI.localRotation = Quaternion.Euler(0, 0, brazos + respira * 2f);
            brazoD.localRotation = Quaternion.Euler(0, 0, -brazos - respira * 2f);
            cuerpo.localPosition = new Vector3(0, respira * 0.02f, 0);
            cuerpo.localRotation = Quaternion.Euler(inclinacion, 0, 0);
        }
    }

}
