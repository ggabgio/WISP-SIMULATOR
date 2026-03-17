using UnityEngine;

public class HoverZoomImage : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float zoomScale = 1.2f;
    public float zoomSpeed = 5f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * zoomSpeed);
    }

    public void ZoomIn()
    {
        targetScale = originalScale * zoomScale;
    }

    public void ZoomOut()
    {
        targetScale = originalScale;
    }

    void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
}
