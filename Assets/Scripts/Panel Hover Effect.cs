using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class PanelHoverColorSmooth : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Panel Settings")]
    public Image panelImage; // Assign in Inspector
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;
    public float transitionSpeed = 5f; // Higher = faster

    private Coroutine colorCoroutine;

    void Start()
    {
        if (panelImage != null)
            panelImage.color = normalColor;
    }

    void OnEnable()
    {
        // Reset instantly when enabled
        ResetToNormalColor();
    }

    void OnDisable()
    {
        // Reset instantly when disabled
        ResetToNormalColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartColorTransition(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartColorTransition(normalColor);
    }

    private void StartColorTransition(Color targetColor)
    {
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);

        colorCoroutine = StartCoroutine(SmoothColorTransition(targetColor));
    }

    private IEnumerator SmoothColorTransition(Color targetColor)
    {
        while (panelImage.color != targetColor)
        {
            panelImage.color = Color.Lerp(panelImage.color, targetColor, Time.deltaTime * transitionSpeed);
            yield return null;
        }
    }

    private void ResetToNormalColor()
    {
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);

        if (panelImage != null)
            panelImage.color = normalColor;
    }
}
