using UnityEngine;

/// <summary>Aparta el contenido de la muesca (notch) y de la barra de inicio del móvil usando Screen.safeArea.</summary>
[RequireComponent(typeof(RectTransform))]
public class ZonaSegura : MonoBehaviour
{
    private RectTransform rt;
    private Canvas canvas;
    private float baseAbajo;
    private Rect ultima;

    private void Awake()
    {
        rt = (RectTransform)transform;
        baseAbajo = rt.offsetMin.y;
    }

    private void Update()
    {
        if (canvas == null) { canvas = GetComponentInParent<Canvas>(); if (canvas == null) return; canvas = canvas.rootCanvas; }
        Rect zona = Screen.safeArea;
        if (zona == ultima) return;
        ultima = zona;
        float k = 1f / Mathf.Max(0.01f, canvas.scaleFactor);
        float arriba = (Screen.height - zona.yMax) * k;
        float abajo = zona.yMin * k;
        rt.offsetMax = new Vector2(rt.offsetMax.x, -arriba);
        rt.offsetMin = new Vector2(rt.offsetMin.x, Mathf.Max(baseAbajo, abajo));
    }
}
