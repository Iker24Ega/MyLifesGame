using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Pequeña animación al pulsar un botón (se encoge un poco y vuelve), como en los juegos de móvil.</summary>
public class BotonAnimado : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private float objetivo = 1f;

    public void OnPointerDown(PointerEventData e) { objetivo = 0.94f; }
    public void OnPointerUp(PointerEventData e) { objetivo = 1f; }
    public void OnPointerExit(PointerEventData e) { objetivo = 1f; }

    private void Update()
    {
        float s = Mathf.Lerp(transform.localScale.x, objetivo, 1f - Mathf.Exp(-25f * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }

    private void OnDisable()
    {
        objetivo = 1f;
        transform.localScale = Vector3.one;
    }
}
