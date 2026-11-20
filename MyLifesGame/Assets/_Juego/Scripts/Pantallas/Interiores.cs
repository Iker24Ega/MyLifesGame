using UnityEngine;

/// <summary>Fondos sencillos (gimnasio, biblioteca, casa, cafetería, parque) dibujados con FormasUI para la escena del cronómetro.</summary>
public static class Interiores
{
    public static void Dibujar(FormasUI f, string idAccion, float aspecto)
    {
        f.Limpiar();
        float w = Mathf.Max(1f, aspecto) * 1.02f; // medio ancho en unidades de FormasUI
        switch (idAccion)
        {
            case "entrenar": Gimnasio(f, w); break;
            case "estudiar": case "trabajar": Biblioteca(f, w, idAccion == "trabajar"); break;
            case "quedar": Cafeteria(f, w); break;
            case "pasear": Parque(f, w); break;
            default: Casa(f, w, idAccion == "meditar"); break;
        }
    }

    private static void Pared(FormasUI f, float w, Color pared, Color suelo, float alturaSuelo = -0.45f)
    {
        f.Rect(new Vector2(0, 0), new Vector2(w * 2f, 2.04f), 0f, pared);
        float alto = alturaSuelo + 1.02f;
        f.Rect(new Vector2(0, -1.02f + alto * 0.5f), new Vector2(w * 2f, alto), 0f, suelo);
        f.Rect(new Vector2(0, alturaSuelo), new Vector2(w * 2f, 0.04f), 0f, Color.Lerp(suelo, Color.black, 0.15f));
    }

    private static void Gimnasio(FormasUI f, float w)
    {
        Pared(f, w, Paleta.Hex("DCE6EE"), Paleta.Hex("5E6672"));
        // Espejo con marco
        f.Rect(new Vector2(-w * 0.55f, 0.28f), new Vector2(1.1f, 1.0f), 0f, Paleta.Hex("FFFFFF"));
        f.Rect(new Vector2(-w * 0.55f, 0.28f), new Vector2(1.0f, 0.9f), 0f, Paleta.Hex("BFD9EC"));
        f.Linea(new Vector2(-w * 0.55f - 0.3f, 0.0f), new Vector2(-w * 0.55f + 0.05f, 0.55f), 0.06f, new Color(1, 1, 1, 0.6f));
        // Estantería de pesas
        float x = w * 0.6f;
        f.Rect(new Vector2(x, -0.1f), new Vector2(1.0f, 0.06f), 0f, Paleta.Hex("4B4F57"));
        f.Rect(new Vector2(x, 0.25f), new Vector2(1.0f, 0.06f), 0f, Paleta.Hex("4B4F57"));
        for (int i = 0; i < 4; i++)
        {
            f.Circulo(new Vector2(x - 0.36f + i * 0.24f, -0.0f), 0.09f, Paleta.Hex("3A3F4A"));
            f.Circulo(new Vector2(x - 0.36f + i * 0.24f, 0.35f), 0.08f, i % 2 == 0 ? Paleta.Coral : Paleta.Hex("3A3F4A"));
        }
        f.Rect(new Vector2(x - 0.5f, -0.25f), new Vector2(0.05f, 0.5f), 0f, Paleta.Hex("4B4F57"));
        f.Rect(new Vector2(x + 0.5f, -0.25f), new Vector2(0.05f, 0.5f), 0f, Paleta.Hex("4B4F57"));
        // Esterilla
        f.Rect(new Vector2(0, -0.72f), new Vector2(1.6f, 0.18f), 0f, Paleta.Hex("2FA3A0"));
    }

    private static void Biblioteca(FormasUI f, float w, bool oficina)
    {
        Pared(f, w, oficina ? Paleta.Hex("EDE7F6") : Paleta.Hex("F4E9D8"), Paleta.Hex("C9A27A"));
        Color[] lomos = { Paleta.DeArea(MyLifesGame.Motor.Area.Mente), Paleta.Coral, Paleta.Hex("E3B341"), Paleta.Verde, Paleta.Hex("9B6FD1"), Paleta.Hex("4B4F57") };
        foreach (float lado in new[] { -1f, 1f })
        {
            float cx = lado * w * 0.62f;
            f.Rect(new Vector2(cx, 0.15f), new Vector2(1.1f, 1.15f), 0f, Paleta.Hex("A97C55"));
            for (int fila = 0; fila < 3; fila++)
            {
                float y = -0.25f + fila * 0.36f;
                f.Rect(new Vector2(cx, y - 0.14f), new Vector2(1.0f, 0.04f), 0f, Paleta.Hex("8A6244"));
                for (int k = 0; k < 7; k++)
                {
                    float alto = 0.2f + ((k * 7 + fila * 3) % 4) * 0.025f;
                    f.Rect(new Vector2(cx - 0.42f + k * 0.14f, y - 0.12f + alto * 0.5f), new Vector2(0.11f, alto), 0f, lomos[(k + fila * 2) % lomos.Length]);
                }
            }
        }
        // Lámpara
        f.Rect(new Vector2(0.55f, 0.7f), new Vector2(0.03f, 0.5f), 0f, Paleta.Hex("4B4F57"));
        f.Poligono(new[] { new Vector2(0.35f, 0.42f), new Vector2(0.75f, 0.42f), new Vector2(0.65f, 0.6f), new Vector2(0.45f, 0.6f) }, Paleta.Hex("FFD66B"));
    }

    private static void Casa(FormasUI f, float w, bool zen)
    {
        Pared(f, w, zen ? Paleta.Hex("E6F0E1") : Paleta.Hex("FBE8D3"), Paleta.Hex("D8B48C"));
        // Ventana
        float vx = -w * 0.58f;
        f.Rect(new Vector2(vx, 0.3f), new Vector2(1.0f, 0.9f), 0f, Paleta.Blanco);
        f.Rect(new Vector2(vx, 0.3f), new Vector2(0.88f, 0.78f), 0f, Paleta.Hex("BFE0F2"));
        f.Circulo(new Vector2(vx + 0.22f, 0.5f), 0.12f, Paleta.Hex("FFD66B"));
        f.Rect(new Vector2(vx, 0.3f), new Vector2(0.05f, 0.78f), 0f, Paleta.Blanco);
        // Planta
        float px = w * 0.62f;
        f.Rect(new Vector2(px, -0.5f), new Vector2(0.36f, 0.34f), 0f, Paleta.Hex("D97A4E"));
        f.Circulo(new Vector2(px, -0.15f), 0.26f, Paleta.Hex("5BAA6B"));
        f.Circulo(new Vector2(px - 0.18f, -0.02f), 0.18f, Paleta.Hex("6DBB7C"));
        f.Circulo(new Vector2(px + 0.17f, 0.02f), 0.17f, Paleta.Hex("4F9A5E"));
        // Cuadro y alfombra
        f.Rect(new Vector2(w * 0.25f, 0.45f), new Vector2(0.5f, 0.4f), 0f, Paleta.Hex("FFFFFF"));
        f.Rect(new Vector2(w * 0.25f, 0.45f), new Vector2(0.42f, 0.32f), 0f, zen ? Paleta.Hex("9FD3A8") : Paleta.Hex("F2A36B"));
        f.Rect(new Vector2(0, -0.78f), new Vector2(2.0f, 0.22f), 0f, zen ? Paleta.Hex("C9A6E0") : Paleta.Hex("E8604C"));
    }

    private static void Cafeteria(FormasUI f, float w)
    {
        Pared(f, w, Paleta.Hex("DDEFE6"), Paleta.Hex("B98A66"));
        // Pizarra de menú
        f.Rect(new Vector2(-w * 0.55f, 0.4f), new Vector2(1.0f, 0.7f), 0f, Paleta.Hex("A97C55"));
        f.Rect(new Vector2(-w * 0.55f, 0.4f), new Vector2(0.9f, 0.6f), 0f, Paleta.Hex("3A3F4A"));
        for (int i = 0; i < 3; i++) f.Rect(new Vector2(-w * 0.55f, 0.58f - i * 0.17f), new Vector2(0.6f, 0.04f), 0f, new Color(1, 1, 1, 0.7f));
        // Barra con tazas
        float bx = w * 0.6f;
        f.Rect(new Vector2(bx, -0.3f), new Vector2(1.3f, 0.5f), 0f, Paleta.Hex("8A6244"));
        f.Rect(new Vector2(bx, -0.04f), new Vector2(1.4f, 0.06f), 0f, Paleta.Hex("F4EEE6"));
        for (int i = 0; i < 3; i++)
        {
            f.Rect(new Vector2(bx - 0.4f + i * 0.4f, 0.07f), new Vector2(0.16f, 0.16f), 0f, Paleta.Blanco);
            f.Arco(new Vector2(bx - 0.32f + i * 0.4f, 0.08f), 0.03f, 0.06f, 0f, 180f, Paleta.Blanco);
        }
        // Lámparas colgantes
        foreach (float lx in new[] { -0.2f, 0.4f })
        {
            f.Rect(new Vector2(lx, 0.9f), new Vector2(0.02f, 0.3f), 0f, Paleta.Hex("4B4F57"));
            f.Arco(new Vector2(lx, 0.72f), 0f, 0.14f, 270f, 450f, Paleta.Hex("E3B341"));
        }
    }

    private static void Parque(FormasUI f, float w)
    {
        f.Rect(Vector2.zero, new Vector2(w * 2f, 2.04f), 0f, Paleta.Hex("BFE3F5"));
        f.Circulo(new Vector2(w * 0.7f, 0.65f), 0.2f, Paleta.Hex("FFD66B"));
        f.Circulo(new Vector2(-w * 0.4f, 0.7f), 0.14f, Paleta.Blanco);
        f.Circulo(new Vector2(-w * 0.4f + 0.18f, 0.74f), 0.17f, Paleta.Blanco);
        f.Circulo(new Vector2(-w * 0.4f + 0.38f, 0.7f), 0.13f, Paleta.Blanco);
        f.Rect(new Vector2(0, -0.62f), new Vector2(w * 2f, 0.8f), 0f, Paleta.Hex("A9CF8E"));
        f.Rect(new Vector2(0, -0.7f), new Vector2(w * 2f, 0.22f), 0f, Paleta.Hex("EADFCF"));
        foreach (float tx in new[] { -w * 0.7f, -w * 0.35f, w * 0.45f })
        {
            f.Rect(new Vector2(tx, -0.08f), new Vector2(0.08f, 0.4f), 0f, Paleta.Hex("A97C55"));
            f.Circulo(new Vector2(tx, 0.22f), 0.28f, Paleta.Hex("5BAA6B"));
            f.Circulo(new Vector2(tx + 0.15f, 0.12f), 0.18f, Paleta.Hex("6DBB7C"));
        }
    }
}
