using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Pueblo diseñado a mano siguiendo la imagen de Flow "Avatar caminando por el pueblo": plaza de adoquín con la
/// fuente en el centro, calle trasera larga con callejuelas, riachuelo con curvas y puente de piedra hacia la casa,
/// parque con estanque, carretera abajo y casas tranquilas alrededor. Nada de cuadrícula.
///
/// Las coordenadas se escriben "en pantalla" (sx a la derecha, sy hacia el fondo, como se ve con la cámara
/// isométrica) y <see cref="W"/> las pasa al mundo. La plaza (la fuente) es el origen.
/// </summary>
public partial class Ciudad3D
{
    // Ejes de la pantalla en el mundo (la cámara mira desde -U con un giro de 45°)
    private static readonly Vector3 EjeR = new Vector3(0.70710678f, 0, -0.70710678f);
    private static readonly Vector3 EjeU = new Vector3(0.70710678f, 0, 0.70710678f);

    /// <summary>De coordenadas de pantalla (unidades del mundo) a posición en el mundo.</summary>
    public static Vector3 W(float sx, float sy) { return EjeR * sx + EjeU * sy; }

    // ---------------------------------------------------------------- plano del pueblo

    /// <summary>Solares jugables (el 1 es la casa del jugador, al otro lado del río).</summary>
    private static readonly Vector2[] PlanoSolares =
    {
        new Vector2(-16f, 13f),   // 0 · junto a la plaza, al oeste (en la imagen: gimnasio)
        new Vector2(-32f, -23f),  // 1 · casa, cruzando el puente
        new Vector2(12f, 12f),    // 2 · junto a la plaza, al norte (café)
        new Vector2(27f, 9f),     // 3 · calle este (biblioteca)
        new Vector2(22f, -13f),   // 4 · junto al parque
        new Vector2(22f, 31f),    // 5 · calle trasera, derecha (recreativos)
        new Vector2(-32f, 12f),   // 6 · calle oeste
        new Vector2(-12f, 31f),   // 7 · calle trasera, izquierda
    };

    /// <summary>Puerta de cada solar (donde espera el avatar), en la acera de su calle.</summary>
    private static readonly Vector2[] PlanoPuertas =
    {
        new Vector2(-13f, 5f), new Vector2(-26f, -17f), new Vector2(9f, 5f), new Vector2(27f, 2.5f),
        new Vector2(14f, -12f), new Vector2(22f, 24f), new Vector2(-32f, 5.5f), new Vector2(-12f, 24f),
    };

    // Calles de adoquín (líneas que se suavizan); comparten puntos en los cruces para poder caminar por ellas
    private static readonly Vector2[][] PlanoCalles =
    {
        new[] { new Vector2(-2f, 8f), new Vector2(-1f, 16f), new Vector2(-2f, 24f) },                                         // callejuela al norte
        new[] { new Vector2(-48f, 23f), new Vector2(-30f, 24.5f), new Vector2(-12f, 24f), new Vector2(-2f, 24f), new Vector2(10f, 23.5f), new Vector2(22f, 24f), new Vector2(36f, 23f), new Vector2(50f, 25f) }, // calle trasera larga
        new[] { new Vector2(8f, 0f), new Vector2(18f, 2f), new Vector2(27f, 2.5f), new Vector2(38f, 1f), new Vector2(48f, -4f) },  // calle este
        new[] { new Vector2(-9f, 3f), new Vector2(-20f, 5f), new Vector2(-32f, 5.5f), new Vector2(-46f, 7f) },                 // calle oeste
        new[] { new Vector2(-5f, -7f), new Vector2(-10f, -10f), new Vector2(-15f, -12.5f), new Vector2(-21f, -15f), new Vector2(-26f, -17f) }, // al puente y a casa
        new[] { new Vector2(6f, -7f), new Vector2(11f, -10f), new Vector2(14f, -12f), new Vector2(16f, -20f), new Vector2(17f, -29f) },   // por el parque a la carretera
    };

    /// <summary>Carretera de asfalto, abajo, con su curva (como en la imagen).</summary>
    private static readonly Vector2[] PlanoCarretera =
    {
        new Vector2(-70f, -36f), new Vector2(-30f, -35f), new Vector2(0f, -32f), new Vector2(17f, -29f),
        new Vector2(32f, -20f), new Vector2(48f, -4f), new Vector2(62f, 8f), new Vector2(75f, 14f),
    };

    /// <summary>Riachuelo que serpentea entre la casa y la plaza y pasa bajo la carretera.</summary>
    private static readonly Vector2[] PlanoRio =
    {
        new Vector2(-75f, 2f), new Vector2(-55f, -1f), new Vector2(-38f, -6f), new Vector2(-24f, -9f),
        new Vector2(-15f, -12.5f), new Vector2(-6f, -17f), new Vector2(2f, -24f), new Vector2(7f, -33f), new Vector2(10f, -46f),
    };

    /// <summary>Punto del puente de piedra (la calle a casa cruza el río aquí).</summary>
    private static readonly Vector2 PlanoPuente = new Vector2(-15f, -12.5f);

    /// <summary>Contorno de la plaza (irregular, alrededor de la fuente).</summary>
    private static readonly Vector2[] PlanoPlaza =
    {
        new Vector2(10f, 1f), new Vector2(8f, 6f), new Vector2(2f, 8.5f), new Vector2(-4f, 7.5f), new Vector2(-10f, 4f),
        new Vector2(-10f, -2f), new Vector2(-6f, -7.5f), new Vector2(0f, -9f), new Vector2(7f, -8f), new Vector2(11f, -4f),
    };

    /// <summary>Casas tranquilas de relleno (no se pueden tocar), juntas a lo largo de las calles.</summary>
    private static readonly Vector2[] PlanoCasas =
    {
        new Vector2(-46f, 32f), new Vector2(-29f, 32f), new Vector2(5f, 32f), new Vector2(38f, 32f), new Vector2(52f, 33f),
        new Vector2(-38f, 44f), new Vector2(-20f, 45f), new Vector2(-2f, 44f), new Vector2(16f, 45f), new Vector2(34f, 44f),
        new Vector2(-46f, 15f), new Vector2(41f, 11f), new Vector2(54f, 12f), new Vector2(-46f, -18f), new Vector2(-40f, -46f),
        new Vector2(-18f, -45f), new Vector2(6f, -45f), new Vector2(30f, -38f), new Vector2(46f, -22f), new Vector2(60f, -10f),
    };

    // ---------------------------------------------------------------- construcción

    private readonly List<List<Vector3>> callesMundo = new List<List<Vector3>>();
    private List<Vector3> carreteraMundo, rioMundo;
    private readonly List<Vector3> ocupado = new List<Vector3>();   // centros de parcelas (para no plantar árboles encima)

    private static Vector3[] SolaresDelPlano()
    {
        var r = new Vector3[PlanoSolares.Length];
        for (int i = 0; i < r.Length; i++) r[i] = W(PlanoSolares[i].x, PlanoSolares[i].y);
        return r;
    }

    private static Vector3 PuertaDelPlano(int i) { return W(PlanoPuertas[i].x, PlanoPuertas[i].y); }

    private void ConstruirMapa()
    {
        // Texturas: las de Flow si están; si no, las hechas por código
        Texture2D fAdoquin = Resources.Load<Texture2D>("Modelos3D/texturas/textura_adoquin");
        Texture2D fAcera = Resources.Load<Texture2D>("Modelos3D/texturas/textura_acera");
        Texture2D fCesped = Resources.Load<Texture2D>("Modelos3D/texturas/textura_cesped");
        Texture2D fAsfalto = Resources.Load<Texture2D>("Modelos3D/texturas/textura_asfalto");
        texAdoquin = fAdoquin != null ? fAdoquin : Adoquines(Paleta.Hex("DAD4CA"), Paleta.Hex("BDB4A8"), 7, 11);
        texBaldosa = fAcera != null ? fAcera : Baldosas(Paleta.Hex("EFE6D8"), Paleta.Hex("D8CDBC"));
        texCesped = fCesped != null ? fCesped : Cesped(Paleta.Hex("A6D47E"));
        float mAdoquin = fAdoquin != null ? 4.5f : 2.6f, mAcera = fAcera != null ? 6f : 2.4f, mCesped = fCesped != null ? 9f : 6f;

        Losa("Césped", new Vector3(0, -0.05f, 0), new Vector3(260, 0.1f, 260), texCesped, mCesped);

        Material matAcera = MatTextura(texBaldosa, Vector2.one);
        Material matAdoquin = MatTextura(texAdoquin, Vector2.one);
        matAdoquin.SetColor("_BaseColor", new Color(0.8f, 0.78f, 0.78f));   // el adoquín, algo más oscuro que la acera
        if (matAdoquin.HasProperty("_Color")) matAdoquin.color = new Color(0.8f, 0.78f, 0.78f);
        Material matAsfalto = fAsfalto != null ? MatTextura(fAsfalto, Vector2.one) : Mat(Paleta.Hex("B9B2A8"));

        // Carretera: acera ancha clara y asfalto encima
        carreteraMundo = Suave(PlanoCarretera, 2f);
        Cinta("Acera carretera", carreteraMundo, 15f, 0.095f, matAcera, mAcera); // por encima del agua: cruza el río
        Cinta("Carretera", carreteraMundo, 9f, 0.115f, matAsfalto, 8f);
        Cinta("Bordillo", carreteraMundo, 9.6f, 0.105f, Mat(Paleta.Hex("D9D0C2")), 4f);

        // Calles de adoquín con su acera de baldosa
        foreach (Vector2[] c in PlanoCalles)
        {
            List<Vector3> l = Suave(c, 1.5f);
            callesMundo.Add(l);
            Cinta("Acera", l, 9.2f, 0.025f, matAcera, mAcera);
            Cinta("Calle", l, 6.6f, 0.05f, matAdoquin, mAdoquin);
        }

        // Plaza de adoquín alrededor de la fuente, un escalón por encima (con un borde de acera)
        Poligono("Acera plaza", PlanoPlaza, 1.5f, 0.03f, matAcera, mAcera);
        Poligono("Plaza", PlanoPlaza, 0f, 0.2f, matAdoquin, mAdoquin);
        Escalon(PlanoPlaza, 0f, 0.2f, Mat(Paleta.Hex("D8CCBA")));
        PlazaFuente();

        RioYPuente();
        Bordillos();

        // Parcelas de los solares (césped con borde de piedra)
        for (int i = 0; i < PlanoSolares.Length; i++)
        {
            Vector3 s = W(PlanoSolares[i].x, PlanoSolares[i].y);
            ocupado.Add(s);
            Parcela(s, 10.6f, mCesped, matAcera, mAcera, true);
        }

        // Casas tranquilas de relleno
        for (int i = 0; i < PlanoCasas.Length; i++)
        {
            Vector3 p = W(PlanoCasas[i].x, PlanoCasas[i].y);
            ocupado.Add(p);
            Parcela(p, 10.2f, mCesped, matAcera, mAcera, false);
            CasaRelleno(p, i);
        }

        AmbienteMapa();
#if UNITY_EDITOR
        int sombras = 0, franjas = 0;
        string ej = "";
        foreach (Transform t in transform)
        {
            if (t.name == "Sombra") { sombras++; if (ej == "") { var rr = t.GetComponent<Renderer>(); ej = " ej: pos=" + t.position + " esc=" + t.localScale + " shader=" + (rr.sharedMaterial != null && rr.sharedMaterial.shader != null ? rr.sharedMaterial.shader.name + " soportado=" + rr.sharedMaterial.shader.isSupported : "null") + " color=" + rr.sharedMaterial.color + " capa=" + t.gameObject.layer + " activo=" + t.gameObject.activeInHierarchy + " bounds=" + rr.bounds; } }
            if (t.name.StartsWith("Muro") || t.name.StartsWith("Bordillo")) franjas++;
        }
        sombras = sombras * 1; franjas += 0;
        ej += " cam=" + (camara != null ? camara.transform.position.ToString() : "-");
        try { System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "..", "Automatizacion", "diag_mapa.txt"), "sombras=" + sombras + " franjas=" + franjas + " calidad=" + QualitySettings.GetQualityLevel() + " distSombras=" + QualitySettings.shadowDistance + ej + "\n"); } catch (System.Exception) { }
#endif
    }

    /// <summary>Parcela cuadrada alineada con la vista (los edificios quedan ordenados, como en la imagen).</summary>
    private void Parcela(Vector3 centro, float tam, float mCesped, Material matAcera, float mAcera, bool pavimentada)
    {
        Quaternion g = Quaternion.Euler(0, GiroCamara, 0);
        if (pavimentada)
        {
            // Los edificios del juego se apoyan en una base de baldosa, como en la imagen de Flow
            GameObject b = Losa("Base", centro + new Vector3(0, 0.07f, 0), new Vector3(tam, 0.12f, tam), texBaldosa, mAcera);
            b.transform.localRotation = g;
        }
        else
        {
            // Las casas tienen jardín (un verde algo más vivo que el campo), sin bordes blancos
            GameObject j = Losa("Jardín", centro + new Vector3(0, 0.03f, 0), new Vector3(tam, 0.06f, tam), texCesped, mCesped * 0.6f);
            j.transform.localRotation = g;
            j.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", new Color(1.05f, 1.08f, 0.95f));
        }
    }

    private static readonly string[] ModelosRelleno =
    {
        "casa_relleno_1", "relleno_casa_menta", "casa_relleno_2", "relleno_esquina_melocoton", "casa_relleno_3",
        "relleno_pisos_crema", "casa_relleno_4", "casa_relleno_5", "casa_relleno_6", "casa_relleno_7", "casa_relleno_8",
    };
    private static readonly string[] KenneyRelleno = { "a", "b", "c", "d", "e", "g", "h", "i", "j", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u" };

    /// <summary>Casa de relleno: la generada (Flow → 3D) si existe; si no, una de Kenney.</summary>
    private void CasaRelleno(Vector3 p, int k)
    {
        SombraContacto(p, 7.2f, 0.3f);
        for (int i = 0; i < ModelosRelleno.Length; i++)
        {
            string n = ModelosRelleno[(k * 3 + i) % ModelosRelleno.Length];
            if (!Modelos3D.Existe(n)) continue;
            GameObject g = Modelos3D.Crear(n, transform, AnchoSolar * 1.1f);
            g.transform.localPosition = p;
            return;
        }
        Modelo("Ciudad/Suburban/building-type-" + KenneyRelleno[(k * 7) % KenneyRelleno.Length], transform, p, AnchoSolar, 180f + GiroCamara - 45f);
    }

    // ---------------------------------------------------------------- río y puente

    private void RioYPuente()
    {
        rioMundo = Suave(PlanoRio, 2f);
        Cinta("Orilla", rioMundo, 10f, 0.04f, Mat(Paleta.Hex("D9CDBA")), 4f);
        Material matAgua = new Material(Mat(Paleta.Hex("7CC4E8")));
        matAgua.SetFloat("_Smoothness", 0.85f);
        Cinta("Agua", rioMundo, 7.6f, 0.06f, matAgua, 4f);
        // Muros de piedra a ambos lados (el agua queda "metida" entre ellos), abiertos solo en los puentes
        Vector3 puente = W(PlanoPuente.x, PlanoPuente.y);
        List<Vector3> carretera = carreteraMundo;
        System.Func<int, bool> abrir = i => (rioMundo[i] - puente).sqrMagnitude < 30f || DistanciaA(carretera, rioMundo[i]) < 7f;
        Material piedra = Mat(Paleta.Hex("E1D6C4")), cara = Mat(Paleta.Hex("C4B6A0"));
        Franja("Muro río", rioMundo, 4.25f, 0.9f, 0.02f, 0.75f, piedra, cara, abrir);
        Franja("Muro río", rioMundo, -4.25f, 0.9f, 0.02f, 0.75f, piedra, cara, abrir);
        // Nenúfares
        for (int i = 2; i < rioMundo.Count - 2; i += 5)
            Modelo("Ciudad/Naturaleza/" + (i % 2 == 0 ? "lily_large" : "lily_small"), transform, rioMundo[i] + Lateral(rioMundo, i) * ((i * 13 % 5 - 2) * 0.6f) + Vector3.up * 0.1f, 0.9f, i * 40f);

        // Puente de piedra (el generado si existe) donde la calle a casa cruza el río
        Vector3 c = W(PlanoPuente.x, PlanoPuente.y);
        Vector3 dir = Tangente(rioMundo, c);
        Quaternion giro = Quaternion.LookRotation(Vector3.Cross(Vector3.up, dir)); // a lo ancho del río
        if (Modelos3D.Existe("puente"))
        {
            GameObject g = Modelos3D.Crear("puente", transform, 11f);
            g.transform.localPosition = c;
            g.transform.localRotation = giro * Quaternion.Euler(0, 90, 0);
        }
        else PuentePiedra(c, giro, 6.5f);
        // Puente de la carretera (plano, con pretiles)
        Vector3 cruce = Cruce(rioMundo, carreteraMundo);
        PuentePiedra(cruce, Quaternion.LookRotation(Tangente(carreteraMundo, cruce)), 4f, true);
    }

    private void PuentePiedra(Vector3 c, Quaternion giro, float alto, bool plano = false)
    {
        Transform p = new GameObject("Puente").transform;
        p.SetParent(transform, false);
        p.localPosition = c;
        p.localRotation = giro;
        Color piedra = Paleta.Hex("D3C8B8"), clara = Paleta.Hex("E6DDCF");
        float largo = 11f;
        float ancho = plano ? 10f : 5.2f;
        if (plano)
        {
            Pieza(PrimitiveType.Cube, "Pretil", new Vector3(-ancho / 2, 0.55f, 0), new Vector3(0.45f, 0.9f, largo), clara, p);
            Pieza(PrimitiveType.Cube, "Pretil", new Vector3(ancho / 2, 0.55f, 0), new Vector3(0.45f, 0.9f, largo), clara, p);
            return;
        }
        // Arco: tramos inclinados que suben y bajan
        int n = 7;
        for (int i = 0; i < n; i++)
        {
            float t0 = i / (float)n, t1 = (i + 1) / (float)n;
            float z0 = (t0 - 0.5f) * largo, z1 = (t1 - 0.5f) * largo;
            float y0 = Mathf.Sin(t0 * Mathf.PI) * 1.1f, y1 = Mathf.Sin(t1 * Mathf.PI) * 1.1f;
            Vector3 m = new Vector3(0, (y0 + y1) * 0.5f + 0.25f, (z0 + z1) * 0.5f);
            float ang = Mathf.Atan2(y1 - y0, z1 - z0) * Mathf.Rad2Deg;
            float l = Mathf.Sqrt((z1 - z0) * (z1 - z0) + (y1 - y0) * (y1 - y0)) + 0.1f;
            Pieza(PrimitiveType.Cube, "Tablero", m, new Vector3(ancho, 0.4f, l), piedra, p).transform.localRotation = Quaternion.Euler(-ang, 0, 0);
            Pieza(PrimitiveType.Cube, "Pretil", m + new Vector3(-ancho / 2, 0.5f, 0), new Vector3(0.45f, 0.75f, l), clara, p).transform.localRotation = Quaternion.Euler(-ang, 0, 0);
            Pieza(PrimitiveType.Cube, "Pretil", m + new Vector3(ancho / 2, 0.5f, 0), new Vector3(0.45f, 0.75f, l), clara, p).transform.localRotation = Quaternion.Euler(-ang, 0, 0);
        }
        Pieza(PrimitiveType.Cylinder, "Arco", new Vector3(0, -0.9f, 0), new Vector3(3.2f, ancho * 0.5f, 3.2f), Paleta.Hex("BFB3A2"), p).transform.localRotation = Quaternion.Euler(0, 0, 90);
    }

    // ---------------------------------------------------------------- relieve

    /// <summary>
    /// Bordillos redondeados a ambos lados de cada calle y de la carretera (como en la imagen de Flow), salvo en los
    /// cruces, en la plaza y en los puentes, para que nada corte el paso.
    /// </summary>
    private void Bordillos()
    {
        Material piedra = Mat(Paleta.Hex("E3D8C7"));
        Material sombra = Mat(Paleta.Hex("B9AC98"));
        for (int c = 0; c < callesMundo.Count; c++)
        {
            List<Vector3> l = callesMundo[c];
            int cc = c;
            System.Func<int, bool> cortar = i =>
            {
                Vector3 p = l[i];
                if (DentroPlaza(p, 2.5f)) return true;
                if (DistanciaA(rioMundo, p) < 6.5f) return true;
                if (DistanciaA(carreteraMundo, p) < 9f) return true;
                for (int o = 0; o < callesMundo.Count; o++) if (o != cc && DistanciaA(callesMundo[o], p) < 6.5f) return true;
                return false;
            };
            Franja("Bordillo", l, 3.45f, 0.55f, 0.05f, 0.2f, piedra, sombra, cortar);
            Franja("Bordillo", l, -3.45f, 0.55f, 0.05f, 0.2f, piedra, sombra, cortar);
        }
        System.Func<int, bool> cortarCarretera = i =>
        {
            Vector3 p = carreteraMundo[i];
            if (DistanciaA(rioMundo, p) < 8f) return true;
            foreach (List<Vector3> l in callesMundo) if (DistanciaA(l, p) < 6f) return true;
            return false;
        };
        Franja("Bordillo", carreteraMundo, 4.75f, 0.6f, 0.11f, 0.22f, piedra, sombra, cortarCarretera);
        Franja("Bordillo", carreteraMundo, -4.75f, 0.6f, 0.11f, 0.22f, piedra, sombra, cortarCarretera);
    }

    /// <summary>
    /// Franja con volumen a un lado de una línea (bordillo, muro): cara de arriba y caras laterales,
    /// cortada donde <paramref name="cortar"/> lo pida.
    /// </summary>
    private void Franja(string nombre, List<Vector3> linea, float lado, float ancho, float yBase, float alto,
                        Material arriba, Material costado, System.Func<int, bool> cortar)
    {
        var tramo = new List<int>();
        for (int i = 0; i <= linea.Count; i++)
        {
            bool fin = i == linea.Count || (cortar != null && cortar(i));
            if (!fin) { tramo.Add(i); continue; }
            if (tramo.Count >= 2) MallaFranja(nombre, linea, tramo, lado, ancho, yBase, alto, arriba, costado);
            tramo.Clear();
        }
    }

    private void MallaFranja(string nombre, List<Vector3> linea, List<int> idx, float lado, float ancho, float yBase, float alto,
                             Material arriba, Material costado)
    {
        int n = idx.Count;
        // 4 aristas por sección: interior abajo, interior arriba, exterior arriba, exterior abajo (arriba ligeramente biselado)
        var v = new List<Vector3>();
        var triArriba = new List<int>();
        var triLado = new List<int>();
        float bisel = Mathf.Min(0.12f, ancho * 0.25f);
        for (int k = 0; k < n; k++)
        {
            int i = idx[k];
            Vector3 lat = Lateral(linea, i);
            Vector3 c = linea[i] + lat * lado;
            Vector3 hacia = lat * Mathf.Sign(lado == 0 ? 1 : lado);
            Vector3 interior = c - hacia * (ancho * 0.5f), exterior = c + hacia * (ancho * 0.5f);
            v.Add(interior + Vector3.up * yBase);                                    // 0 interior abajo
            v.Add(interior + hacia * bisel + Vector3.up * (yBase + alto));           // 1 interior arriba (biselado)
            v.Add(exterior - hacia * bisel + Vector3.up * (yBase + alto));           // 2 exterior arriba
            v.Add(exterior + Vector3.up * yBase);                                    // 3 exterior abajo
        }
        for (int k = 0; k < n - 1; k++)
        {
            int a = k * 4, b = (k + 1) * 4;
            Quad(triArriba, a + 1, b + 1, b + 2, a + 2);
            Quad(triLado, a + 0, b + 0, b + 1, a + 1);
            Quad(triLado, a + 2, b + 2, b + 3, a + 3);
        }
        var m = new Mesh { name = nombre };
        m.SetVertices(v);
        m.subMeshCount = 2;
        m.SetTriangles(triArriba, 0);
        m.SetTriangles(triLado, 1);
        m.RecalculateNormals();
        // Si las normales salen hacia abajo, se da la vuelta a las caras
        if (m.normals[1].y < 0)
        {
            triArriba.Reverse(); triLado.Reverse();
            m.SetTriangles(triArriba, 0); m.SetTriangles(triLado, 1);
            m.RecalculateNormals();
        }
        var go = new GameObject(nombre, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.GetComponent<MeshFilter>().sharedMesh = m;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterials = new[] { arriba, costado };
        r.shadowCastingMode = ShadowCastingMode.On;
    }

    private static void Quad(List<int> t, int a, int b, int c, int d)
    {
        t.Add(a); t.Add(b); t.Add(c);
        t.Add(a); t.Add(c); t.Add(d);
    }

    /// <summary>Paredes del escalón de un suelo con forma de polígono (la plaza).</summary>
    private void Escalon(Vector2[] plano, float crecer, float alto, Material mat)
    {
        Vector2 c = Vector2.zero;
        foreach (Vector2 q in plano) c += q;
        c /= plano.Length;
        var v = new List<Vector3>();
        var t = new List<int>();
        for (int i = 0; i < plano.Length; i++)
        {
            Vector2 a2 = plano[i] + (plano[i] - c).normalized * crecer;
            Vector2 b2 = plano[(i + 1) % plano.Length] + (plano[(i + 1) % plano.Length] - c).normalized * crecer;
            Vector3 a = W(a2.x, a2.y), b = W(b2.x, b2.y);
            int k = v.Count;
            v.Add(a); v.Add(b); v.Add(b + Vector3.up * alto); v.Add(a + Vector3.up * alto);
            // hacia fuera: el orden depende del sentido del polígono; se ponen las dos caras
            Quad(t, k, k + 1, k + 2, k + 3);
            Quad(t, k + 3, k + 2, k + 1, k);
        }
        var m = new Mesh { name = "Escalón" };
        m.SetVertices(v); m.SetTriangles(t, 0); m.RecalculateNormals();
        var go = new GameObject("Escalón plaza", typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.GetComponent<MeshFilter>().sharedMesh = m;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // ---------------------------------------------------------------- sombras de contacto

    private const bool SombrasContactoActivas = false;
    private static Texture2D texSombra;
    private readonly Dictionary<int, Material> matsSombra = new Dictionary<int, Material>();
    private static Mesh mallaSombra;

    /// <summary>
    /// Sombra suave y redonda en el suelo bajo un objeto (imita la oclusión ambiental de las imágenes de Flow:
    /// todo "pisa" el suelo). Barata: un cuadro con degradado.
    /// </summary>
    private void SombraContacto(Vector3 p, float radio, float fuerza = 0.42f, float alargar = 1f)
    {
        // Desactivado: en la ciudad no llegan a verse (pendiente de averiguar por qué); las sombras reales del sol sí
        if (!SombrasContactoActivas) return;
        if (texSombra == null)
        {
            const int N = 64;
            texSombra = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float dx = (x + 0.5f) / N * 2f - 1f, dy = (y + 0.5f) / N * 2f - 1f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d);
                    px[y * N + x] = new Color(0.25f, 0.17f, 0.1f, a * a * (3f - 2f * a));
                }
            texSombra.SetPixels(px); texSombra.Apply();
        }
        int clave = Mathf.RoundToInt(fuerza * 20f);
        Material m;
        if (!matsSombra.TryGetValue(clave, out m))
        {
            m = new Material(Shader.Find("Sprites/Default")) { mainTexture = texSombra, color = new Color(1, 1, 1, clave / 20f), renderQueue = 2460 }; // después del suelo opaco, antes de lo transparente
            matsSombra[clave] = m;
        }
        // Malla propia, plana y con colores de vértice (blancos): la sombra mira hacia arriba
        var go = new GameObject("Sombra", typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(p.x, 0.3f, p.z);
        go.transform.localRotation = Quaternion.Euler(0, GiroCamara, 0);
        if (mallaSombra == null)
        {
            mallaSombra = new Mesh { name = "Sombra" };
            mallaSombra.vertices = new[] { new Vector3(-1, 0, -1), new Vector3(1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1) };
            mallaSombra.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            mallaSombra.colors = new[] { Color.white, Color.white, Color.white, Color.white };
            mallaSombra.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            mallaSombra.RecalculateNormals(); mallaSombra.RecalculateBounds();
        }
        go.GetComponent<MeshFilter>().sharedMesh = mallaSombra;
        go.transform.localScale = new Vector3(radio * alargar, 1f, radio);
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = m;
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;
    }

    // ---------------------------------------------------------------- ambiente

    private void AmbienteMapa()
    {
        int k = 0;
        // Farolas a lo largo de calles y carretera, alternando lados
        foreach (List<Vector3> l in callesMundo) Farolas(l, 11f, 3.9f, ref k);
        Farolas(carreteraMundo, 14f, 6.6f, ref k);
        Farolas(rioMundo, 16f, 4.9f, ref k);

        // Bancos y macetas alrededor de la fuente ya los pone PlazaFuente; bancos junto al río y en el parque
        Banco(W(-6f, -12f), 0f);
        Banco(W(26f, -20f), 0f);
        Bici(W(-10f, 6.5f), 20f);
        Bici(W(-8f, 6.8f), 25f);
        Bici(W(3f, -9.5f), -60f);
        Bici(W(-29f, -15f), 10f);

        // Árboles, arbustos y flores en los huecos libres (lejos de calles, río, carretera y parcelas)
        var rnd = new System.Random(7);
        for (float sx = -70f; sx <= 72f; sx += 5f)
            for (float sy = -56f; sy <= 60f; sy += 5f)
            {
                Vector3 p = W(sx + (float)rnd.NextDouble() * 3f - 1.5f, sy + (float)rnd.NextDouble() * 3f - 1.5f);
                if (!Libre(p, 1.6f)) continue;
                double r = rnd.NextDouble();
                if (r < 0.55) Arbol(p, k++, 3.2f + (float)rnd.NextDouble() * 1.2f);
                else if (r < 0.8) Arbusto(p, k++);
                else if (r < 0.88) Maceta(p, k++);
            }
        // Arbustos y flores junto a cada solar
        for (int i = 0; i < PlanoSolares.Length; i++)
        {
            Vector3 s = W(PlanoSolares[i].x, PlanoSolares[i].y);
            Arbusto(s + EjeR * 5.6f - EjeU * 5.6f, k++);
            Maceta(PuertaDelPlano(i) + EjeR * 2.2f, k++);
        }
    }

    private void Farolas(List<Vector3> linea, float cada, float lado, ref int k)
    {
        float recorrido = 0f, siguiente = cada * 0.5f;
        for (int i = 1; i < linea.Count; i++)
        {
            recorrido += Vector3.Distance(linea[i - 1], linea[i]);
            if (recorrido < siguiente) continue;
            siguiente += cada;
            Vector3 p = linea[i] + Lateral(linea, i) * (k++ % 2 == 0 ? lado : -lado);
            if (DistanciaA(rioMundo, p) < 5f && linea != rioMundo) continue;
            if (CercaDeParcela(p, 6.2f) || DentroPlaza(p, 1f)) continue;
            Farola(p);
        }
    }

    private void Bici(Vector3 p, float giro)
    {
        if (!Modelos3D.Existe("bici")) return;
        GameObject g = Modelos3D.Crear("bici", transform, 2.1f);
        g.transform.localPosition = p;
        SombraContacto(p, 1.1f, 0.28f, 1.5f);
        g.transform.localRotation = Quaternion.Euler(0, giro, 0);
    }

    /// <summary>¿Hay sitio para plantar algo aquí?</summary>
    private bool Libre(Vector3 p, float margen)
    {
        if (DentroPlaza(p, margen)) return false;
        if (CercaDeParcela(p, 5.6f + margen * 0.3f)) return false;
        if (DistanciaA(carreteraMundo, p) < 7.5f + margen) return false;
        if (DistanciaA(rioMundo, p) < 4.7f + margen) return false;
        foreach (List<Vector3> l in callesMundo) if (DistanciaA(l, p) < 4.6f + margen * 0.5f) return false;
        foreach (Vector3 c in CentrosBarrio) if (Mathf.Abs(c.x - p.x) < 15f && Mathf.Abs(c.z - p.z) < 15f) return false;
        return true;
    }

    private bool CercaDeParcela(Vector3 p, float r)
    {
        foreach (Vector3 c in ocupado)
        {
            Vector3 d = p - c;
            if (Mathf.Abs(Vector3.Dot(d, EjeR)) < r && Mathf.Abs(Vector3.Dot(d, EjeU)) < r) return true;
        }
        return false;
    }

    private static bool DentroPlaza(Vector3 p, float margen)
    {
        float sx = Vector3.Dot(p, EjeR), sy = Vector3.Dot(p, EjeU);
        return sx > -11f - margen && sx < 12f + margen && sy > -10f - margen && sy < 9.5f + margen
               && DentroPoligono(new Vector2(sx, sy), PlanoPlaza, margen);
    }

    private static bool DentroPoligono(Vector2 q, Vector2[] poli, float margen)
    {
        // Se agranda el polígono desde su centro para el margen
        Vector2 c = Vector2.zero;
        foreach (Vector2 v in poli) c += v;
        c /= poli.Length;
        bool dentro = false;
        for (int i = 0, j = poli.Length - 1; i < poli.Length; j = i++)
        {
            Vector2 a = poli[i] + (poli[i] - c).normalized * margen, b = poli[j] + (poli[j] - c).normalized * margen;
            if (((a.y > q.y) != (b.y > q.y)) && (q.x < (b.x - a.x) * (q.y - a.y) / (b.y - a.y) + a.x)) dentro = !dentro;
        }
        return dentro;
    }

    // ---------------------------------------------------------------- caminar por las calles

    private readonly List<Vector3> nodos = new List<Vector3>();
    private readonly List<List<int>> vecinos = new List<List<int>>();

    /// <summary>Red de caminos: los puntos de cada calle, unidos en los cruces y a través de la plaza.</summary>
    private void PrepararCaminos()
    {
        if (nodos.Count > 0) return;
        foreach (Vector2[] c in PlanoCalles)
        {
            int previo = -1;
            foreach (Vector2 v in c)
            {
                int n = Nodo(W(v.x, v.y));
                if (previo >= 0) Unir(previo, n);
                previo = n;
            }
        }
        // La plaza: todas sus salidas se unen con la fuente (se cruza andando por la plaza)
        int fuente = Nodo(W(0f, -3.5f));
        foreach (Vector2[] c in PlanoCalles)
        {
            Vector3 inicio = W(c[0].x, c[0].y);
            if (DentroPlaza(inicio, 3f)) Unir(fuente, Nodo(inicio));
        }
        // La callejuela a la carretera conecta con la carretera
        int carretera = Nodo(W(17f, -29f));
        Unir(carretera, Nodo(W(0f, -32f)));
    }

    private int Nodo(Vector3 p)
    {
        for (int i = 0; i < nodos.Count; i++) if ((nodos[i] - p).sqrMagnitude < 2.5f) return i;
        nodos.Add(p);
        vecinos.Add(new List<int>());
        return nodos.Count - 1;
    }

    private void Unir(int a, int b)
    {
        if (a == b) return;
        if (!vecinos[a].Contains(b)) vecinos[a].Add(b);
        if (!vecinos[b].Contains(a)) vecinos[b].Add(a);
    }

    private int MasCercano(Vector3 p)
    {
        int mejor = 0;
        for (int i = 1; i < nodos.Count; i++) if ((nodos[i] - p).sqrMagnitude < (nodos[mejor] - p).sqrMagnitude) mejor = i;
        return mejor;
    }

    /// <summary>Ruta por las calles (Dijkstra) de una puerta a otra, en pasos cortos y suavizada.</summary>
    private List<Vector3> Ruta(Vector3 desde, Vector3 hasta)
    {
        PrepararCaminos();
        int a = MasCercano(desde), b = MasCercano(hasta);
        var dist = new float[nodos.Count];
        var prev = new int[nodos.Count];
        var hecho = new bool[nodos.Count];
        for (int i = 0; i < dist.Length; i++) { dist[i] = float.MaxValue; prev[i] = -1; }
        dist[a] = 0f;
        for (int it = 0; it < nodos.Count; it++)
        {
            int u = -1;
            for (int i = 0; i < dist.Length; i++) if (!hecho[i] && (u < 0 || dist[i] < dist[u])) u = i;
            if (u < 0 || dist[u] == float.MaxValue) break;
            hecho[u] = true;
            if (u == b) break;
            foreach (int v in vecinos[u])
            {
                float d = dist[u] + Vector3.Distance(nodos[u], nodos[v]);
                if (d < dist[v]) { dist[v] = d; prev[v] = u; }
            }
        }
        var claves = new List<Vector3> { desde };
        var tramo = new List<Vector3>();
        for (int v = b; v >= 0; v = prev[v]) tramo.Add(nodos[v]);
        tramo.Reverse();
        claves.AddRange(tramo);
        claves.Add(hasta);
        // Suavizado (curvas en lugar de esquinas) y pasos cortos
        var puntos = new List<Vector3>();
        for (int i = 1; i < claves.Count; i++)
        {
            Vector3 p0 = claves[Mathf.Max(i - 2, 0)], p1 = claves[i - 1], p2 = claves[i], p3 = claves[Mathf.Min(i + 1, claves.Count - 1)];
            int n = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(p1, p2) / 1.2f));
            for (int j = 1; j <= n; j++) puntos.Add(CatmullRom(p0, p1, p2, p3, j / (float)n));
        }
        return puntos;
    }

    // ---------------------------------------------------------------- utilidades de líneas

    /// <summary>Línea suavizada (Catmull-Rom) a partir de puntos del plano, con muestras cada "paso" unidades.</summary>
    private static List<Vector3> Suave(Vector2[] plano, float paso)
    {
        var p = new List<Vector3>();
        foreach (Vector2 v in plano) p.Add(W(v.x, v.y));
        var r = new List<Vector3> { p[0] };
        for (int i = 0; i < p.Count - 1; i++)
        {
            Vector3 p0 = p[Mathf.Max(i - 1, 0)], p1 = p[i], p2 = p[i + 1], p3 = p[Mathf.Min(i + 2, p.Count - 1)];
            int n = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(p1, p2) / paso));
            for (int j = 1; j <= n; j++) r.Add(CatmullRom(p0, p1, p2, p3, j / (float)n));
        }
        return r;
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    private static Vector3 Lateral(List<Vector3> l, int i)
    {
        Vector3 d = l[Mathf.Min(i + 1, l.Count - 1)] - l[Mathf.Max(i - 1, 0)];
        d.y = 0;
        d.Normalize();
        return new Vector3(d.z, 0, -d.x);
    }

    private static Vector3 Tangente(List<Vector3> l, Vector3 p)
    {
        int mejor = 0;
        for (int i = 1; i < l.Count; i++) if ((l[i] - p).sqrMagnitude < (l[mejor] - p).sqrMagnitude) mejor = i;
        Vector3 d = l[Mathf.Min(mejor + 1, l.Count - 1)] - l[Mathf.Max(mejor - 1, 0)];
        d.y = 0;
        return d.normalized;
    }

    private static float DistanciaA(List<Vector3> l, Vector3 p)
    {
        if (l == null) return float.MaxValue;
        float mejor = float.MaxValue;
        for (int i = 1; i < l.Count; i++)
        {
            Vector3 a = l[i - 1], b = l[i];
            Vector3 ab = b - a; ab.y = 0;
            Vector3 ap = p - a; ap.y = 0;
            float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Mathf.Max(ab.sqrMagnitude, 1e-4f));
            Vector3 q = a + ab * t;
            float d = new Vector2(p.x - q.x, p.z - q.z).magnitude;
            if (d < mejor) mejor = d;
        }
        return mejor;
    }

    /// <summary>Punto aproximado donde se cruzan dos líneas.</summary>
    private static Vector3 Cruce(List<Vector3> a, List<Vector3> b)
    {
        Vector3 mejor = a[0];
        float dmin = float.MaxValue;
        foreach (Vector3 p in a)
        {
            float d = DistanciaA(b, p);
            if (d < dmin) { dmin = d; mejor = p; }
        }
        return mejor;
    }

    /// <summary>Suelo con forma de polígono (plano), triangulado desde su centro.</summary>
    private GameObject Poligono(string nombre, Vector2[] plano, float crecer, float y, Material mat, float metrosPorTesela)
    {
        Vector2 c = Vector2.zero;
        foreach (Vector2 v in plano) c += v;
        c /= plano.Length;
        var v3 = new Vector3[plano.Length + 1];
        var uv = new Vector2[plano.Length + 1];
        Vector3 cw = W(c.x, c.y) + Vector3.up * y;
        v3[0] = cw; uv[0] = new Vector2(cw.x, cw.z) / metrosPorTesela;
        for (int i = 0; i < plano.Length; i++)
        {
            Vector2 q = plano[i] + (plano[i] - c).normalized * crecer;
            Vector3 w = W(q.x, q.y) + Vector3.up * y;
            v3[i + 1] = w;
            uv[i + 1] = new Vector2(w.x, w.z) / metrosPorTesela;
        }
        var tri = new int[plano.Length * 3];
        for (int i = 0; i < plano.Length; i++)
        {
            tri[i * 3] = 0;
            tri[i * 3 + 1] = 1 + (i + 1) % plano.Length;
            tri[i * 3 + 2] = 1 + i;
        }
        var m = new Mesh { name = nombre, vertices = v3, uv = uv, triangles = tri };
        m.RecalculateNormals();
        if (m.normals.Length > 0 && m.normals[0].y < 0) { System.Array.Reverse(tri); m.triangles = tri; m.RecalculateNormals(); }
        var go = new GameObject(nombre, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.GetComponent<MeshFilter>().sharedMesh = m;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = ShadowCastingMode.Off;
        return go;
    }
}
