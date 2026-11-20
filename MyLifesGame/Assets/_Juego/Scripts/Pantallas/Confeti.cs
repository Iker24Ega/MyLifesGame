using UnityEngine;

/// <summary>Confeti sencillo dibujado con FormasUI: trocitos de colores que caen girando y desaparecen.</summary>
[RequireComponent(typeof(FormasUI))]
public class Confeti : MonoBehaviour
{
    private const int N = 70;
    private readonly Vector2[] pos = new Vector2[N], vel = new Vector2[N];
    private readonly float[] giro = new float[N], vgiro = new float[N];
    private readonly Color[] col = new Color[N];
    private FormasUI formas;
    private float t = 99f;

    public static Confeti Crear(Transform padre)
    {
        RectTransform rt = UIFabrica.Crear("Confeti", padre);
        UIFabrica.Anclar(rt, 0, 0, 1, 1);
        FormasUI f = FormasUI.En(rt.gameObject);
        f.raycastTarget = false;
        return rt.gameObject.AddComponent<Confeti>();
    }

    public void Lanzar()
    {
        formas = GetComponent<FormasUI>();
        Color[] paleta = { Paleta.Coral, Paleta.Verde, Paleta.Hex("E3B341"), Paleta.Hex("4C8BD6"), Paleta.Hex("9B6FD1"), Paleta.Hex("F2A36B") };
        // Coordenadas de FormasUI: -1..1 en el lado corto. En vertical, x va de -1 a 1 y y de -1,8 a 1,8 aprox.
        for (int i = 0; i < N; i++)
        {
            pos[i] = new Vector2(Random.Range(-0.2f, 0.2f), 0.9f);
            vel[i] = new Vector2(Random.Range(-1.3f, 1.3f), Random.Range(0.6f, 1.8f));
            giro[i] = Random.Range(0f, 360f);
            vgiro[i] = Random.Range(-400f, 400f);
            col[i] = paleta[i % paleta.Length];
        }
        t = 0f;
        transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (formas == null) return;
        if (t > 3f) { if (t < 50f) { formas.Limpiar(); t = 99f; } return; }
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        t += dt;
        formas.Limpiar();
        float alfa = Mathf.Clamp01(3f - t);
        for (int i = 0; i < N; i++)
        {
            vel[i].y -= 2.2f * dt;
            vel[i].x *= 1f - 0.8f * dt;
            pos[i] += vel[i] * dt;
            giro[i] += vgiro[i] * dt;
            Color c = col[i]; c.a = alfa;
            formas.Rect(pos[i], new Vector2(0.05f, 0.028f), giro[i], c);
        }
    }
}
