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

    private void Update() { Animar(); }

    private void Animar()
    {
        float t = Time.time;
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
