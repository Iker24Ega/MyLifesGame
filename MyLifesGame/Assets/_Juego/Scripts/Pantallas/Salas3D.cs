using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Salas 3D detrás del avatar mientras hace una acción (gimnasio, biblioteca, oficina, cafetería, salón, parque),
/// montadas con Kenney Furniture Kit (CC0) y piezas sencillas. Se ven con la cámara de escena del estudio.
/// El avatar está en el origen mirando hacia +Z; la cámara mira desde delante y algo a la derecha.
/// </summary>
public static class Salas3D
{
    public static string SalaDe(string accion)
    {
        switch (accion)
        {
            case "entrenar": return "gimnasio";
            case "estudiar": return "biblioteca";
            case "trabajar": return "oficina";
            case "quedar": return "cafeteria";
            case "pasear": return "parque";
            case "checkin": return "dormitorio";
            case "avatar": return "vestidor";
            default: return "salon";   // descansar, meditar, videojuegos
        }
    }

    private static Transform padre;

    public static void Construir(Transform p, string sala, string accion)
    {
        padre = p;
        switch (sala)
        {
            case "gimnasio": Gimnasio(); break;
            case "biblioteca": Biblioteca(); break;
            case "oficina": Oficina(); break;
            case "cafeteria": Cafeteria(); break;
            case "parque": Parque(); break;
            case "dormitorio": Dormitorio(); break;
            case "vestidor": Vestidor(); break;
            default: Salon(accion); break;
        }
    }

    // ---------------------------------------------------------------- piezas

    private static Material Mat(Color c, float brillo = 0.1f, Color? emision = null)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Standard");
        var m = new Material(s);
        m.SetColor("_BaseColor", c); m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", brillo);
        if (emision.HasValue) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emision.Value); }
        return m;
    }

    private static GameObject Caja(PrimitiveType tipo, string nombre, Vector3 pos, Vector3 tam, Color c, float giroY = 0f, bool sombra = false)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.SetParent(padre, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0, giroY, 0);
        go.transform.localScale = tam;
        var r = go.GetComponent<Renderer>();
        r.sharedMaterial = Mat(c);
        r.shadowCastingMode = sombra ? ShadowCastingMode.On : ShadowCastingMode.Off;
        return go;
    }

    /// <summary>Modelo de Resources escalado para que mida "ancho" y apoyado en "pos" (y = altura del suelo).</summary>
    private static GameObject Mueble(string nombre, Vector3 pos, float ancho, float giroY)
    {
        GameObject prefab = Resources.Load<GameObject>("Interiores/" + nombre);
        if (prefab == null) { Debug.LogWarning("[Salas] Falta " + nombre); return null; }
        GameObject go = Object.Instantiate(prefab, padre, false);
        go.name = nombre;
        foreach (Collider c in go.GetComponentsInChildren<Collider>()) Object.Destroy(c);
        go.transform.localRotation = Quaternion.Euler(0, giroY, 0);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        Bounds b = Limites(go);
        float tam = Mathf.Max(b.size.x, b.size.z);
        float esc = tam > 0.0001f ? ancho / tam : 1f;
        go.transform.localScale = Vector3.one * esc;
        b = Limites(go);
        Vector3 centro = padre.InverseTransformPoint(b.center);
        float baseY = padre.InverseTransformPoint(new Vector3(0, b.min.y, 0)).y;
        go.transform.localPosition = pos - new Vector3(centro.x, baseY, centro.z);
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

    /// <summary>Suelo, pared del fondo y pared izquierda, con zócalo.</summary>
    private static void Habitacion(Color pared, Color suelo, Color zocalo)
    {
        Caja(PrimitiveType.Cube, "Suelo", new Vector3(0, -0.05f, 1.5f), new Vector3(26f, 0.1f, 16f), suelo);
        Caja(PrimitiveType.Cube, "Pared fondo", new Vector3(0, 4f, -3.4f), new Vector3(26f, 8f, 0.2f), pared);
        Caja(PrimitiveType.Cube, "Zócalo", new Vector3(0, 0.3f, -3.28f), new Vector3(26f, 0.6f, 0.08f), zocalo);
        Caja(PrimitiveType.Cube, "Pared izquierda", new Vector3(-8.5f, 4f, 2f), new Vector3(0.2f, 8f, 11f), Color.Lerp(pared, Color.black, 0.06f));
    }

    private static void Ventana(Vector3 centro, float ancho, float alto)
    {
        Color marco = Paleta.Blanco;
        Caja(PrimitiveType.Cube, "Cielo", centro, new Vector3(ancho, alto, 0.05f), Paleta.Hex("BFE3F7")).GetComponent<Renderer>().sharedMaterial = Mat(Paleta.Hex("BFE3F7"), 0.3f, Paleta.Hex("BFE3F7") * 0.6f);
        Caja(PrimitiveType.Cube, "Marco", centro + new Vector3(0, alto * 0.5f, 0.04f), new Vector3(ancho + 0.2f, 0.16f, 0.12f), marco);
        Caja(PrimitiveType.Cube, "Marco", centro + new Vector3(0, -alto * 0.5f, 0.04f), new Vector3(ancho + 0.3f, 0.2f, 0.2f), marco);
        Caja(PrimitiveType.Cube, "Marco", centro + new Vector3(-ancho * 0.5f, 0, 0.04f), new Vector3(0.16f, alto, 0.12f), marco);
        Caja(PrimitiveType.Cube, "Marco", centro + new Vector3(ancho * 0.5f, 0, 0.04f), new Vector3(0.16f, alto, 0.12f), marco);
        Caja(PrimitiveType.Cube, "Marco", centro + new Vector3(0, 0, 0.04f), new Vector3(0.1f, alto, 0.1f), marco);
        // árboles al otro lado del cristal
        Caja(PrimitiveType.Sphere, "Árbol fuera", centro + new Vector3(-ancho * 0.25f, -alto * 0.2f, -0.2f), new Vector3(ancho * 0.45f, alto * 0.5f, 0.1f), Paleta.Hex("A6D47E"));
    }

    private static void Planta(Vector3 pos, float tam = 1.1f) { Mueble("pottedPlant", pos, tam, 0f); }

    private static void Cuadro(Vector3 pos, float ancho, float alto, Color c)
    {
        Caja(PrimitiveType.Cube, "Marco cuadro", pos, new Vector3(ancho + 0.16f, alto + 0.16f, 0.06f), Paleta.Hex("F7F2EA"));
        Caja(PrimitiveType.Cube, "Lienzo", pos + new Vector3(0, 0, 0.035f), new Vector3(ancho, alto, 0.02f), c);
    }

    // ---------------------------------------------------------------- salas

    private static void Gimnasio()
    {
        Habitacion(Paleta.Hex("E6EDF2"), Paleta.Hex("B9C0C9"), Paleta.Hex("4FA3A5"));
        Caja(PrimitiveType.Cube, "Franja", new Vector3(0, 2.2f, -3.27f), new Vector3(26f, 0.25f, 0.05f), Paleta.Hex("4FA3A5"));
        Ventana(new Vector3(-4.6f, 4.2f, -3.25f), 4.2f, 3.2f);
        Caja(PrimitiveType.Cube, "Esterilla", new Vector3(0, 0.02f, 0.2f), new Vector3(2.6f, 0.04f, 1.8f), Paleta.Hex("3FA59E"));
        // Estante con mancuernas
        Vector3 e = new Vector3(4.6f, 0, -2.6f);
        Color hierro = Paleta.Hex("4A4F58");
        Caja(PrimitiveType.Cube, "Estante", e + new Vector3(0, 0.75f, 0), new Vector3(3.6f, 0.12f, 0.8f), hierro, 0, true);
        Caja(PrimitiveType.Cube, "Estante", e + new Vector3(0, 1.45f, 0), new Vector3(3.6f, 0.12f, 0.8f), hierro, 0, true);
        Caja(PrimitiveType.Cube, "Pata", e + new Vector3(-1.75f, 0.75f, 0), new Vector3(0.12f, 1.5f, 0.8f), hierro);
        Caja(PrimitiveType.Cube, "Pata", e + new Vector3(1.75f, 0.75f, 0), new Vector3(0.12f, 1.5f, 0.8f), hierro);
        for (int fila = 0; fila < 2; fila++)
            for (int i = 0; i < 5; i++)
            {
                Color c = i == 4 && fila == 0 ? Paleta.Coral : Paleta.Hex("2F333A");
                Caja(PrimitiveType.Sphere, "Mancuerna", e + new Vector3(-1.4f + i * 0.7f, 0.97f + fila * 0.7f, 0), Vector3.one * (0.42f - fila * 0.06f), c, 0, true);
            }
        Mueble("bench", new Vector3(-3.6f, 0, 1.6f), 2.4f, 90f);
        Planta(new Vector3(-7f, 0, -2.4f), 1.2f);
        Cuadro(new Vector3(1.6f, 4.6f, -3.25f), 2f, 1.3f, Paleta.Hex("F2B632"));
    }

    private static void Biblioteca()
    {
        Habitacion(Paleta.Hex("F4E8D6"), Paleta.Hex("C99B6B"), Paleta.Hex("A87A52"));
        Mueble("bookcaseOpen", new Vector3(-4.2f, 0, -2.7f), 3.2f, 0f);
        Mueble("bookcaseOpen", new Vector3(4.4f, 0, -2.7f), 3.2f, 0f);
        Mueble("bookcaseClosedWide", new Vector3(-7.6f, 0, 0.5f), 3.2f, 90f);
        Libros(new Vector3(-4.2f, 0, -2.6f));
        Libros(new Vector3(4.4f, 0, -2.6f));
        Mueble("rugRound", new Vector3(0, 0.01f, 0.4f), 3.4f, 0f);
        Mueble("lampSquareTable", new Vector3(2.6f, 0, 1.0f), 0.8f, 0f);
        Planta(new Vector3(6.8f, 0, -2.3f), 1.2f);
        Ventana(new Vector3(0, 5.2f, -3.25f), 3.0f, 2.0f);
    }

    /// <summary>Lomos de colores en las baldas de una estantería abierta.</summary>
    private static void Libros(Vector3 b)
    {
        string[] cols = { "E8604C", "4A7BD0", "F2B33D", "5BAA6B", "9B6BD1", "F4F0E8" };
        for (int balda = 0; balda < 3; balda++)
            for (int i = 0; i < 7; i++)
                Caja(PrimitiveType.Cube, "Libro", b + new Vector3(-1.2f + i * 0.36f, 0.62f + balda * 1.02f, 0.05f), new Vector3(0.28f, 0.62f - (i % 3) * 0.08f, 0.5f), Paleta.Hex(cols[(i + balda * 2) % cols.Length]));
    }

    private static void Oficina()
    {
        Habitacion(Paleta.Hex("EAEFF3"), Paleta.Hex("BFAE96"), Paleta.Hex("8EA0B2"));
        Ventana(new Vector3(-3.8f, 4.4f, -3.25f), 4.4f, 3f);
        Mueble("desk", new Vector3(3.8f, 0, -1.8f), 3.2f, 0f);
        Mueble("computerScreen", new Vector3(3.8f, 1.55f, -2.1f), 1.2f, 0f);
        Mueble("chairDesk", new Vector3(3.8f, 0, -0.6f), 1.0f, 180f);
        Planta(new Vector3(-7.2f, 0, -2.4f), 1.3f);
        Cuadro(new Vector3(3.8f, 4.8f, -3.25f), 2.4f, 1.5f, Paleta.Hex("9FC7E8"));
    }

    private static void Cafeteria()
    {
        Habitacion(Paleta.Hex("F6E3CF"), Paleta.Hex("B98E66"), Paleta.Hex("8E6243"));
        // Pizarra del menú
        Caja(PrimitiveType.Cube, "Pizarra", new Vector3(-3.6f, 4.6f, -3.25f), new Vector3(3.4f, 2f, 0.06f), Paleta.Hex("3B3F3A"));
        for (int i = 0; i < 4; i++)
            Caja(PrimitiveType.Cube, "Línea", new Vector3(-3.9f + (i % 2) * 0.4f, 5.2f - i * 0.4f, -3.2f), new Vector3(2.2f - (i % 2) * 0.6f, 0.08f, 0.02f), Paleta.Hex("F4F0E8"));
        Mueble("kitchenBar", new Vector3(4.2f, 0, -2.4f), 3.6f, 0f);
        Mueble("kitchenCoffeeMachine", new Vector3(3.6f, 1.75f, -2.6f), 0.7f, 0f);
        Mueble("tableRound", new Vector3(-3.4f, 0, 1.2f), 1.8f, 0f);
        Mueble("stoolBar", new Vector3(5.6f, 0, -1.2f), 0.7f, 0f);
        Planta(new Vector3(-7f, 0, -2.4f), 1.3f);
        // Lámparas colgantes
        for (int i = 0; i < 3; i++)
        {
            float x = -4f + i * 4f;
            Caja(PrimitiveType.Cylinder, "Cable", new Vector3(x, 6.6f, -1f), new Vector3(0.03f, 1.4f, 0.03f), Paleta.Hex("3A3F4A"));
            Caja(PrimitiveType.Sphere, "Bombilla", new Vector3(x, 5.1f, -1f), new Vector3(0.7f, 0.5f, 0.7f), Paleta.Hex("FFE7A8")).GetComponent<Renderer>().sharedMaterial = Mat(Paleta.Hex("FFE7A8"), 0.3f, Paleta.Hex("FFD27A") * 0.8f);
        }
    }

    private static void Salon(string accion)
    {
        Habitacion(Paleta.Hex("E3EFE3"), Paleta.Hex("D2A77A"), Paleta.Hex("B98B5E"));
        Ventana(new Vector3(-4.2f, 4.3f, -3.25f), 3.6f, 2.8f);
        Mueble("rugRound", new Vector3(0, 0.01f, 0.4f), 3.8f, 0f);
        if (accion == "meditar")
            Caja(PrimitiveType.Cube, "Esterilla", new Vector3(0, 0.03f, 0.3f), new Vector3(2.6f, 0.04f, 1.8f), Paleta.Hex("C9A6E0"));
        if (accion == "videojuegos")
        {
            Mueble("cabinetTelevision", new Vector3(4.6f, 0, -2.6f), 2.8f, 0f);
            Mueble("televisionModern", new Vector3(4.6f, 1.0f, -2.7f), 2.4f, 0f);
        }
        else
        {
            Mueble("loungeSofa", new Vector3(4.8f, 0, -2.2f), 3.4f, 0f);
            Cuadro(new Vector3(4.8f, 4.4f, -3.25f), 2.2f, 1.4f, Paleta.Hex("F6B49F"));
        }
        Mueble("lampRoundFloor", new Vector3(-6.8f, 0, -1.8f), 0.9f, 0f);
        Planta(new Vector3(7.4f, 0, -2.4f), 1.3f);
    }

    private static void Dormitorio()
    {
        Habitacion(Paleta.Hex("F3E4D2"), Paleta.Hex("D6AB7E"), Paleta.Hex("B98B5E"));
        Ventana(new Vector3(-3.6f, 4.4f, -3.25f), 3.8f, 3.0f);
        Mueble("bedSingle", new Vector3(4.6f, 0, -1.4f), 3.0f, 0f);
        Mueble("cabinetBed", new Vector3(2.4f, 0, -2.6f), 1.1f, 0f);
        Mueble("lampSquareTable", new Vector3(2.4f, 1.05f, -2.6f), 0.6f, 0f);
        Mueble("rugRound", new Vector3(0, 0.01f, 0.4f), 3.6f, 0f);
        Planta(new Vector3(-7f, 0, -2.3f), 1.2f);
        // Mochila apoyada en la pared
        Caja(PrimitiveType.Cube, "Mochila", new Vector3(-1.6f, 0.75f, -2.9f), new Vector3(1.0f, 1.4f, 0.55f), Paleta.Mochila, 0, true);
        Caja(PrimitiveType.Cube, "Bolsillo", new Vector3(-1.6f, 0.5f, -2.58f), new Vector3(0.7f, 0.5f, 0.12f), Paleta.Hex("3A3E45"));
        Cuadro(new Vector3(4.6f, 4.6f, -3.25f), 1.8f, 1.2f, Paleta.Hex("9FC7E8"));
    }

    private static void Vestidor()
    {
        Habitacion(Paleta.Hex("F4E3CF"), Paleta.Hex("D9B48A"), Paleta.Hex("C79A6C"));
        Ventana(new Vector3(-4.2f, 4.3f, -3.25f), 3.4f, 3.0f);
        Caja(PrimitiveType.Cylinder, "Pedestal", new Vector3(0, -0.02f, 0.2f), new Vector3(3.6f, 0.06f, 3.6f), Paleta.Hex("EADBC8"), 0, true);
        Caja(PrimitiveType.Cylinder, "Borde pedestal", new Vector3(0, -0.04f, 0.2f), new Vector3(3.9f, 0.05f, 3.9f), Paleta.Hex("D8C3A8"));
        Mueble("lampRoundFloor", new Vector3(4.2f, 0, -2.2f), 1.0f, 0f);
        Mueble("sideTable", new Vector3(-6.2f, 0, -2.2f), 1.4f, 0f);
        Planta(new Vector3(6.8f, 0, -2.4f), 1.4f);
        Mueble("plantSmall1", new Vector3(-6.2f, 1.0f, -2.2f), 0.7f, 0f);
        Cuadro(new Vector3(2.2f, 4.8f, -3.25f), 1.6f, 1.1f, Paleta.Hex("F6B49F"));
    }

    private static void Parque()
    {
        Caja(PrimitiveType.Cube, "Césped", new Vector3(0, -0.05f, 0), new Vector3(40f, 0.1f, 30f), Paleta.Hex("A6D47E"));
        Caja(PrimitiveType.Cube, "Camino", new Vector3(0, 0.0f, 0.5f), new Vector3(40f, 0.06f, 2.6f), Paleta.Hex("E8DCC8"));
        Caja(PrimitiveType.Cube, "Cielo", new Vector3(0, 6f, -12f), new Vector3(60f, 16f, 0.2f), Paleta.Hex("CFE8F6")).GetComponent<Renderer>().sharedMaterial = Mat(Paleta.Hex("CFE8F6"), 0f, Paleta.Hex("CFE8F6") * 0.7f);
        string[] hojas = { "8CC65A", "7DBA4E", "98CF62" };
        float[] xs = { -8f, -4.5f, 4f, 7.5f, 11f, -11f };
        for (int i = 0; i < xs.Length; i++)
        {
            float z = -3.5f - (i % 2) * 2.5f;
            float s = 1.2f + (i % 3) * 0.25f;
            Caja(PrimitiveType.Cylinder, "Tronco", new Vector3(xs[i], 0.9f * s, z), new Vector3(0.35f, 0.9f, 0.35f) * s, Paleta.Hex("9A6B4A"));
            Caja(PrimitiveType.Sphere, "Copa", new Vector3(xs[i], 2.6f * s, z), new Vector3(2.8f, 2.4f, 2.8f) * s, Paleta.Hex(hojas[i % 3]), 0, true);
        }
        Mueble("bench", new Vector3(-4.5f, 0.03f, -1.2f), 2.4f, 0f);
    }
}
