using UnityEngine;

/// <summary>Aparición suave de una pantalla (fundido y un ligero desplazamiento hacia arriba).</summary>
[RequireComponent(typeof(CanvasGroup))]
public class Aparecer : MonoBehaviour
{
    private CanvasGroup grupo;
    private RectTransform rt;
    private float t = 1f;
    private const float Duracion = 0.18f;

    private void Awake()
    {
        grupo = GetComponent<CanvasGroup>();
        rt = (RectTransform)transform;
    }

    public void Lanzar()
    {
        if (grupo == null) Awake();
        t = 0f;
        Aplicar();
    }

    private void Update()
    {
        if (t >= 1f) return;
        t = Mathf.Min(1f, t + Time.unscaledDeltaTime / Duracion);
        Aplicar();
    }

    private void Aplicar()
    {
        float e = 1f - (1f - t) * (1f - t);
        grupo.alpha = e;
        rt.anchoredPosition = new Vector2(0, -30f * (1f - e));
    }
}
