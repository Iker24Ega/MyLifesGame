using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Decorado de la ciudad al estilo de la imagen de referencia "Avatar caminando por el pueblo":
/// adoquines, aceras de baldosa y césped con texturas generadas por código, farolas, fuente de dos pisos,
/// bancos, macetas con flores, árboles redondos (Kenney Nature, CC0) y un río con puentes de piedra.
/// </summary>
public partial class Ciudad3D
{
    private Texture2D texAdoquin, texBaldosa, texCesped;
    private Material matFarolaLuz;

    // ---------------------------------------------------------------- texturas

    private static Texture2D NuevaTextura(int n)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, true);
        t.wrapMode = TextureWrapMode.Repeat;
        t.filterMode = FilterMode.Trilinear;
        t.anisoLevel = 4;
        return t;
    }

    /// <summary>Adoquines redondeados en filas alternas, con juntas oscuras y variación de color.</summary>
    private static Texture2D Adoquines(Color piedra, Color junta, int filas, int semilla)
    {
        const int N = 256;
        var t = NuevaTextura(N);
        var px = new Color[N * N];
        for (int i = 0; i < px.Length; i++) px[i] = junta;
        var rnd = new System.Random(semilla);
        int alto = N / filas;
        for (int f = 0; f < filas; f++)
        {
            int x = (f % 2) * (alto / 2) - alto;
            while (x < N)
            {
                int ancho = (int)(alto * (1.1f + (float)rnd.NextDouble() * 0.7f));
                float v = 0.92f + (float)rnd.NextDouble() * 0.14f;
                Color c = new Color(piedra.r * v, piedra.g * v, piedra.b * v, 1f);
                PintarPiedra(px, N, x, f * alto, ancho, alto, c);
                x += ancho;
            }
        }
        t.SetPixels(px); t.Apply(true);
        return t;
    }

    private static void PintarPiedra(Color[] px, int N, int x0, int y0, int w, int h, Color c)
    {
        float r = Mathf.Min(w, h) * 0.32f;
        const float junta = 2.2f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                // distancia al borde de un rectángulo redondeado
                float dx = Mathf.Max(Mathf.Abs(x - (w - 1) * 0.5f) - ((w - 1) * 0.5f - r - junta), 0f);
                float dy = Mathf.Max(Mathf.Abs(y - (h - 1) * 0.5f) - ((h - 1) * 0.5f - r - junta), 0f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d > r) continue;
                float borde = Mathf.Clamp01((r - d) / 2.5f);
                float luz = 1f + 0.06f * (1f - (float)y / h) - 0.05f * (1f - borde);
                int X = ((x0 + x) % N + N) % N, Y = ((y0 + y) % N + N) % N;
                Color fondo = px[Y * N + X];
                px[Y * N + X] = Color.Lerp(fondo, new Color(c.r * luz, c.g * luz, c.b * luz, 1f), borde);
            }
    }

    /// <summary>Baldosas cuadradas para aceras y plaza.</summary>
    private static Texture2D Baldosas(Color baldosa, Color junta)
    {
        const int N = 128;
        var t = NuevaTextura(N);
        var px = new Color[N * N];
        var rnd = new System.Random(7);
        int lado = N / 4;
        for (int by = 0; by < 4; by++)
            for (int bx = 0; bx < 4; bx++)
            {
                float v = 0.96f + (float)rnd.NextDouble() * 0.07f;
                for (int y = 0; y < lado; y++)
                    for (int x = 0; x < lado; x++)
                    {
                        bool j = x < 2 || y < 2;
                        px[(by * lado + y) * N + bx * lado + x] = j ? junta : new Color(baldosa.r * v, baldosa.g * v, baldosa.b * v, 1f);
                    }
            }
        t.SetPixels(px); t.Apply(true);
        return t;
    }

    /// <summary>Césped con manchas suaves (ruido de Perlin) para que no se vea plano.</summary>
    private static Texture2D Cesped(Color c)
    {
        const int N = 128;
        var t = NuevaTextura(N);
        var px = new Color[N * N];
        var rnd = new System.Random(3);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float u = (float)x / N, v = (float)y / N;
                // ruido periódico: suma de senos para que la textura repita sin costuras
                float m = 0.5f + 0.25f * Mathf.Sin(u * Mathf.PI * 4f + Mathf.Sin(v * Mathf.PI * 2f) * 2f) * Mathf.Cos(v * Mathf.PI * 6f);
                float g = 0.95f + 0.1f * m + ((float)rnd.NextDouble() - 0.5f) * 0.06f;
                px[y * N + x] = new Color(c.r * g, c.g * g, c.b * g, 1f);
            }
        t.SetPixels(px); t.Apply(true);
        return t;
    }

    private Material MatTextura(Texture2D tex, Vector2 repeticion, float brillo = 0.08f)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s);
        m.SetColor("_BaseColor", Color.white);
        m.mainTexture = tex;
        m.mainTextureScale = repeticion;
        if (m.HasProperty("_BaseMap")) { m.SetTexture("_BaseMap", tex); m.SetTextureScale("_BaseMap", repeticion); }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", brillo);
        return m;
    }

    private GameObject Losa(string nombre, Vector3 centro, Vector3 tam, Texture2D tex, float metrosPorTesela, Transform padre = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(padre != null ? padre : transform, false);
        go.transform.localPosition = centro;
        go.transform.localScale = tam;
        go.GetComponent<Renderer>().sharedMaterial = MatTextura(tex, new Vector2(tam.x / metrosPorTesela, tam.z / metrosPorTesela));
        Destroy(go.GetComponent<Collider>());
        return go;
    }

    // ---------------------------------------------------------------- suelo y calles

    private void SueloYCalles()
    {
        texAdoquin = Adoquines(Paleta.Hex("DAD4CA"), Paleta.Hex("BDB4A8"), 7, 11);
        texBaldosa = Baldosas(Paleta.Hex("EFE6D8"), Paleta.Hex("D8CDBC"));
        texCesped = Cesped(Paleta.Hex("A6D47E"));

        float borde = Paso * 2f;                    // fin de las calles exteriores
        Losa("Césped", new Vector3(0, -0.05f, 0), new Vector3(170, 0.1f, 170), texCesped, 6f);
        Losa("Pavimento", new Vector3(0, 0.0f, 0), new Vector3(borde * 2f + 6f, 0.06f, borde * 2f + 6f), texBaldosa, 2.4f);
        foreach (float c in Calles)
        {
            bool interior = Mathf.Abs(c) < Paso;   // las calles interiores cruzan el río por los puentes
            if (interior) Losa("Calle", new Vector3(c, 0.045f, -6f), new Vector3(4.2f, 0.06f, borde * 2f + 18f), texAdoquin, 2.6f);
            else Losa("Calle", new Vector3(c, 0.045f, 0f), new Vector3(4.2f, 0.06f, borde * 2f + 6f), texAdoquin, 2.6f);
            Losa("Calle", new Vector3(0, 0.05f, c), new Vector3(borde * 2f + 6f, 0.06f, 4.2f), texAdoquin, 2.6f);
        }
        // Césped bajo cada solar, con borde de piedra
        foreach (Vector3 s in Solares)
        {
            Pieza(PrimitiveType.Cube, "Borde jardín", s + new Vector3(0, 0.06f, 0), new Vector3(9.8f, 0.09f, 9.8f), Paleta.Hex("E4D9C8"));
            Losa("Jardín", s + new Vector3(0, 0.08f, 0), new Vector3(9.4f, 0.09f, 9.4f), texCesped, 4f);
        }
    }

    private void PlazaFuente()
    {
        Losa("Plaza", new Vector3(0, 0.07f, 0), new Vector3(10f, 0.06f, 10f), texAdoquin, 2.2f);
        Color piedra = Paleta.Hex("F1EBE2"), agua = Paleta.Hex("7CC4E8");
        Pieza(PrimitiveType.Cylinder, "Pila", new Vector3(0, 0.45f, 0), new Vector3(4.6f, 0.38f, 4.6f), piedra);
        Pieza(PrimitiveType.Cylinder, "Agua", new Vector3(0, 0.84f, 0), new Vector3(4.1f, 0.02f, 4.1f), agua);
        Pieza(PrimitiveType.Cylinder, "Columna", new Vector3(0, 1.5f, 0), new Vector3(0.6f, 0.7f, 0.6f), piedra);
        Pieza(PrimitiveType.Cylinder, "Taza", new Vector3(0, 2.2f, 0), new Vector3(2.0f, 0.14f, 2.0f), piedra);
        Pieza(PrimitiveType.Cylinder, "Agua taza", new Vector3(0, 2.35f, 0), new Vector3(1.75f, 0.02f, 1.75f), agua);
        Pieza(PrimitiveType.Cylinder, "Remate", new Vector3(0, 2.7f, 0), new Vector3(0.3f, 0.35f, 0.3f), piedra);
        Pieza(PrimitiveType.Sphere, "Chorro", new Vector3(0, 3.15f, 0), new Vector3(0.45f, 0.35f, 0.45f), Paleta.Hex("BFE6F7"));
        for (int i = 0; i < 4; i++)
        {
            float a = i * Mathf.PI / 2f + Mathf.PI / 4f;
            Vector3 p = new Vector3(Mathf.Cos(a) * 4.0f, 0.1f, Mathf.Sin(a) * 4.0f);
            Maceta(p, i);
            Banco(new Vector3(Mathf.Cos(a + Mathf.PI / 4f) * 3.9f, 0.1f, Mathf.Sin(a + Mathf.PI / 4f) * 3.9f), -(a + Mathf.PI / 4f) * Mathf.Rad2Deg - 90f);
        }
    }

    // ---------------------------------------------------------------- piezas de ambiente

    private void Farola(Vector3 p)
    {
        Color hierro = Paleta.Hex("3E4B4A");
        Transform f = new GameObject("Farola").transform;
        f.SetParent(transform, false);
        f.localPosition = p;
        Pieza(PrimitiveType.Cylinder, "Base", new Vector3(0, 0.2f, 0), new Vector3(0.42f, 0.2f, 0.42f), hierro, f);
        Pieza(PrimitiveType.Cylinder, "Poste", new Vector3(0, 1.7f, 0), new Vector3(0.16f, 1.5f, 0.16f), hierro, f);
        Pieza(PrimitiveType.Cube, "Soporte", new Vector3(0, 3.25f, 0), new Vector3(0.5f, 0.1f, 0.5f), hierro, f);
        GameObject luz = Pieza(PrimitiveType.Cube, "Farol", new Vector3(0, 3.6f, 0), new Vector3(0.42f, 0.55f, 0.42f), Paleta.Hex("FFE7A8"), f);
        if (matFarolaLuz == null)
        {
            matFarolaLuz = new Material(Mat(Paleta.Hex("FFE7A8")));
            matFarolaLuz.EnableKeyword("_EMISSION");
            matFarolaLuz.SetColor("_EmissionColor", Paleta.Hex("FFD27A") * 0.9f);
        }
        luz.GetComponent<Renderer>().sharedMaterial = matFarolaLuz;
        Pieza(PrimitiveType.Cylinder, "Sombrero", new Vector3(0, 3.98f, 0), new Vector3(0.62f, 0.08f, 0.62f), hierro, f);
        Pieza(PrimitiveType.Sphere, "Punta", new Vector3(0, 4.1f, 0), new Vector3(0.16f, 0.16f, 0.16f), hierro, f);
    }

    private void Banco(Vector3 p, float giro)
    {
        Transform b = new GameObject("Banco").transform;
        b.SetParent(transform, false);
        b.localPosition = p;
        b.localRotation = Quaternion.Euler(0, giro, 0);
        Color madera = Paleta.Hex("C98B57"), hierro = Paleta.Hex("3E4B4A");
        Pieza(PrimitiveType.Cube, "Asiento", new Vector3(0, 0.5f, 0), new Vector3(1.8f, 0.1f, 0.55f), madera, b);
        Pieza(PrimitiveType.Cube, "Respaldo", new Vector3(0, 0.85f, -0.25f), new Vector3(1.8f, 0.42f, 0.08f), madera, b);
        Pieza(PrimitiveType.Cube, "Pata", new Vector3(-0.75f, 0.25f, 0), new Vector3(0.1f, 0.5f, 0.5f), hierro, b);
        Pieza(PrimitiveType.Cube, "Pata", new Vector3(0.75f, 0.25f, 0), new Vector3(0.1f, 0.5f, 0.5f), hierro, b);
    }

    private static readonly string[] Flores = { "flower_redA", "flower_yellowA", "flower_purpleA", "flower_redB", "flower_yellowB", "flower_purpleB" };

    private void Maceta(Vector3 p, int k)
    {
        Transform m = new GameObject("Maceta").transform;
        m.SetParent(transform, false);
        m.localPosition = p;
        Pieza(PrimitiveType.Cylinder, "Tiesto", new Vector3(0, 0.3f, 0), new Vector3(0.75f, 0.3f, 0.75f), Paleta.Hex("D7845A"), m);
        Pieza(PrimitiveType.Cylinder, "Tierra", new Vector3(0, 0.61f, 0), new Vector3(0.66f, 0.02f, 0.66f), Paleta.Hex("7A5A40"), m);
        Pieza(PrimitiveType.Sphere, "Mata", new Vector3(-0.05f, 0.8f, 0), new Vector3(0.75f, 0.55f, 0.75f), Paleta.Hex("7DBA4E"), m);
        Modelo("Ciudad/Naturaleza/" + Flores[k % Flores.Length], m, new Vector3(0.12f, 0.62f, 0.1f), 0.5f, k * 30f);
    }

    private static readonly string[] Hojas = { "8CC65A", "7DBA4E", "98CF62", "84C257" };

    /// <summary>Árbol redondo y suave (copa de varias esferas), como los de la imagen de referencia.</summary>
    private void Arbol(Vector3 p, int k, float tam = 3.4f) { ArbolEn(transform, p, k, tam); }

    private void ArbolEn(Transform padre, Vector3 p, int k, float tam)
    {
        float s = tam / 2.6f * (0.88f + ((k * 37) % 10) * 0.03f);
        Transform t = new GameObject("Árbol").transform;
        t.SetParent(padre, false);
        t.localPosition = p;
        t.localRotation = Quaternion.Euler(0, k * 47f, 0);
        Color c = Paleta.Hex(Hojas[k % Hojas.Length]);
        Color claro = Color.Lerp(c, Color.white, 0.12f), oscuro = Color.Lerp(c, Color.black, 0.1f);
        Pieza(PrimitiveType.Cylinder, "Tronco", new Vector3(0, 0.75f * s, 0), new Vector3(0.3f * s, 0.75f * s, 0.3f * s), Paleta.Hex("9A6B4A"), t);
        Pieza(PrimitiveType.Sphere, "Copa", new Vector3(0, 2.3f * s, 0), new Vector3(2.4f, 2.1f, 2.4f) * s, c, t);
        Pieza(PrimitiveType.Sphere, "Copa", new Vector3(0.75f * s, 1.95f * s, 0.35f * s), new Vector3(1.5f, 1.3f, 1.5f) * s, oscuro, t);
        Pieza(PrimitiveType.Sphere, "Copa", new Vector3(-0.65f * s, 1.9f * s, -0.45f * s), new Vector3(1.45f, 1.25f, 1.45f) * s, oscuro, t);
        Pieza(PrimitiveType.Sphere, "Copa", new Vector3(0.15f * s, 3.05f * s, -0.15f * s), new Vector3(1.5f, 1.25f, 1.5f) * s, claro, t);
    }

    private void Arbusto(Vector3 p, int k)
    {
        Transform t = new GameObject("Arbusto").transform;
        t.SetParent(transform, false);
        t.localPosition = p;
        Color c = Color.Lerp(Paleta.Hex(Hojas[(k + 1) % Hojas.Length]), Color.black, 0.06f);
        Pieza(PrimitiveType.Sphere, "Mata", new Vector3(0, 0.45f, 0), new Vector3(1.3f, 0.95f, 1.2f), c, t);
        Pieza(PrimitiveType.Sphere, "Mata", new Vector3(0.6f, 0.35f, 0.2f), new Vector3(0.9f, 0.7f, 0.9f), Color.Lerp(c, Color.white, 0.1f), t);
        Pieza(PrimitiveType.Sphere, "Mata", new Vector3(-0.55f, 0.32f, -0.15f), new Vector3(0.85f, 0.65f, 0.85f), c, t);
        if (k % 2 == 0)
            for (int i = 0; i < 3; i++)
                Pieza(PrimitiveType.Sphere, "Flor", new Vector3(-0.4f + i * 0.4f, 0.85f - (i % 2) * 0.15f, 0.45f), Vector3.one * 0.16f, Paleta.Hex(i % 2 == 0 ? "F4F0F7" : "F6A6C1"), t);
    }

    /// <summary>Farolas en las aceras, árboles y arbustos entre las casas, flores junto a las puertas.</summary>
    private void Ambiente()
    {
        int k = 0;
        // Farolas a lo largo de las calles (en la acera, alternando lados)
        foreach (float c in Calles)
            for (int i = -2; i <= 2; i++)
            {
                float d = i * Paso;
                if (Mathf.Abs(d) < 6f && Mathf.Abs(c) < 10f) continue; // no en la plaza
                Farola(new Vector3(c + ((i + 2) % 2 == 0 ? 2.75f : -2.75f), 0.03f, d + 3.2f));
                Farola(new Vector3(d - 3.2f, 0.03f, c + ((i + 2) % 2 == 0 ? -2.75f : 2.75f)));
            }
        // Árboles en las esquinas de las manzanas (en la acera ancha entre calles)
        for (int ix = -1; ix <= 1; ix++)
            for (int iz = -1; iz <= 1; iz++)
            {
                Vector3 s = new Vector3(ix * Paso, 0, iz * Paso);
                if (ix == 0 && iz == 0) continue;
                Arbol(s + new Vector3(-5.55f, 0.04f, 5.55f), k++, 2.6f);
                Arbol(s + new Vector3(5.55f, 0.04f, 5.55f), k++, 2.4f);
                Arbusto(s + new Vector3(-5.6f, 0.06f, -1.5f), k++);
                Arbusto(s + new Vector3(5.6f, 0.06f, 1.5f), k++);
                Maceta(s + new Vector3(-2.2f, 0.06f, -5.4f), k++);
                Maceta(s + new Vector3(2.2f, 0.06f, -5.4f), k++);
            }
        // Arbustos con flores delante de cada solar y bancos en las aceras
        for (int i = 0; i < Solares.Length; i++)
        {
            Vector3 s = Solares[i];
            Arbusto(s + new Vector3(-4.0f, 0.12f, -4.1f), k++);
            Arbusto(s + new Vector3(4.0f, 0.12f, -4.1f), k++);
            Banco(s + new Vector3(-2.2f, 0.03f, -5.3f), 180f);
        }

        // Cinturón verde exterior
        for (float x = -44f; x <= 44f; x += 7.5f)
        {
            Arbol(new Vector3(x + 2f, 0, 46f + (k % 3)), k++, 4.2f);
            if (Mathf.Abs(x) > 38f) continue;
        }
        for (float z = -40f; z <= 40f; z += 8f)
        {
            Arbol(new Vector3(-44f - (k % 2) * 2f, 0, z), k++, 4.2f);
            Arbol(new Vector3(44f + (k % 2) * 2f, 0, z + 3f), k++, 4.2f);
        }
        Rio();
    }

    /// <summary>Río al sur del barrio, con orillas de piedra, nenúfares y dos puentes de piedra donde cruzan las calles.</summary>
    private void Rio()
    {
        float z = -Paso * 2f - 7.5f;
        float ancho = 6f;
        Color orilla = Paleta.Hex("D9CDBA");
        Pieza(PrimitiveType.Cube, "Lecho", new Vector3(0, -0.12f, z), new Vector3(150f, 0.1f, ancho + 1.2f), Paleta.Hex("6FB6DB"));
        GameObject agua = Pieza(PrimitiveType.Cube, "Agua", new Vector3(0, -0.02f, z), new Vector3(150f, 0.06f, ancho), Paleta.Hex("8FD0EE"));
        agua.GetComponent<Renderer>().sharedMaterial.SetFloat("_Smoothness", 0.75f);
        Pieza(PrimitiveType.Cube, "Orilla", new Vector3(0, 0.08f, z + ancho * 0.5f + 0.35f), new Vector3(150f, 0.3f, 0.7f), orilla);
        Pieza(PrimitiveType.Cube, "Orilla", new Vector3(0, 0.08f, z - ancho * 0.5f - 0.35f), new Vector3(150f, 0.3f, 0.7f), orilla);
        for (int i = 0; i < 9; i++)
            Modelo("Ciudad/Naturaleza/" + (i % 2 == 0 ? "lily_large" : "lily_small"), transform, new Vector3(-40f + i * 9.7f, 0.03f, z + ((i * 13) % 5 - 2f) * 0.8f), 0.9f, i * 40f);
        foreach (float c in new[] { -Paso * 0.5f, Paso * 0.5f })
        {
            Transform p = new GameObject("Puente").transform;
            p.SetParent(transform, false);
            p.localPosition = new Vector3(c, 0, z);
            Color piedra = Paleta.Hex("CFC4B4");
            Pieza(PrimitiveType.Cube, "Tablero", new Vector3(0, 0.35f, 0), new Vector3(4.6f, 0.3f, ancho + 2.2f), piedra, p);
            Pieza(PrimitiveType.Cube, "Pretil", new Vector3(-2.2f, 0.85f, 0), new Vector3(0.35f, 0.8f, ancho + 2.2f), Paleta.Hex("E3D9CA"), p);
            Pieza(PrimitiveType.Cube, "Pretil", new Vector3(2.2f, 0.85f, 0), new Vector3(0.35f, 0.8f, ancho + 2.2f), Paleta.Hex("E3D9CA"), p);
            Pieza(PrimitiveType.Cylinder, "Arco", new Vector3(0, -0.6f, 0), new Vector3(2.0f, 2.2f, ancho + 1f), Paleta.Hex("B9AE9E"), p).transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        // Arbolado y casas al otro lado del río
        int k = 3;
        for (float x = -42f; x <= 42f; x += 6.5f)
        {
            if (Mathf.Abs(Mathf.Abs(x) - Paso * 0.5f) < 3f) continue;
            Arbol(new Vector3(x, 0, z - ancho * 0.5f - 3.5f - (k % 2) * 1.5f), k++, 3.6f);
        }
    }
}
