using UnityEngine;
using UnityEngine.EventSystems;

public class SmoothButtonGrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float normalScale = 1f;
    public float expandedScale = 1.2f;
    public float speed = 10f;

    private Vector3 targetScale;

    void Start()
    {
        targetScale = Vector3.one * normalScale;
        transform.localScale = targetScale;
    }

    void Update()
    {
        // Smoothly interpolate toward target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * expandedScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one * normalScale;
    }
}
