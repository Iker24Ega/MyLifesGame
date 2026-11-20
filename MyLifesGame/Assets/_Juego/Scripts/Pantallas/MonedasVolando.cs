using UnityEngine;

/// <summary>Monedas que saltan desde las recompensas y vuelan hacia arriba al ganar algo.</summary>
[RequireComponent(typeof(FormasUI))]
public class MonedasVolando : MonoBehaviour
{
    private const int N = 14;
    private readonly Vector2[] inicio = new Vector2[N], control = new Vector2[N];
    private readonly float[] retraso = new float[N];
    private Vector2 destino;
    private FormasUI formas;
    private float t = 99f;

    public static MonedasVolando Crear(Transform padre)
    {
        RectTransform rt = UIFabrica.Crear("Monedas volando", padre);
        UIFabrica.Anclar(rt, 0, 0, 1, 1);
        FormasUI f = FormasUI.En(rt.gameObject);
        f.raycastTarget = false;
        return rt.gameObject.AddComponent<MonedasVolando>();
    }

    /// <summary>Lanza las monedas desde "desde" hacia "hasta" (coordenadas de FormasUI: -1..1 en el lado corto).</summary>
    public void Lanzar(Vector2 desde, Vector2 hasta)
    {
        formas = GetComponent<FormasUI>();
        destino = hasta;
        for (int i = 0; i < N; i++)
        {
            inicio[i] = desde + new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.05f, 0.05f));
            control[i] = inicio[i] + new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0.4f, 0.9f));
            retraso[i] = i * 0.045f;
        }
        t = 0f;
        transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (formas == null || t > 50f) return;
        t += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        formas.Limpiar();
        if (t > 2f) { t = 99f; return; }
        Color oro = Paleta.Hex("E3B341"), brillo = Paleta.Hex("F6D26B");
        for (int i = 0; i < N; i++)
        {
            float k = Mathf.Clamp01((t - retraso[i]) / 1.1f);
            if (k <= 0f || k >= 1f) continue;
            float e = k * k * (3f - 2f * k);
            Vector2 a = Vector2.Lerp(inicio[i], control[i], e), b = Vector2.Lerp(control[i], destino, e);
            Vector2 p = Vector2.Lerp(a, b, e);
            float r = Mathf.Lerp(0.07f, 0.04f, e);
            float giro = Mathf.Abs(Mathf.Cos((t + i) * 9f));
            formas.Rect(p, new Vector2(r * 2f * Mathf.Max(0.25f, giro), r * 2f), 0f, oro);
            formas.Rect(p, new Vector2(r * 1.2f * Mathf.Max(0.2f, giro), r * 1.2f), 0f, brillo);
        }
    }
}
