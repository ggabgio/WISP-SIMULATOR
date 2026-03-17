using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CanvasGroup textCanvasGroup;
    public CanvasGroup imageCanvasGroup;
    public float fadeDuration = 0.3f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(textCanvasGroup, textCanvasGroup.alpha, 0));
        StartCoroutine(FadeCanvasGroup(imageCanvasGroup, imageCanvasGroup.alpha, 1));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(textCanvasGroup, textCanvasGroup.alpha, 1));
        StartCoroutine(FadeCanvasGroup(imageCanvasGroup, imageCanvasGroup.alpha, 0));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end;
    }
}
