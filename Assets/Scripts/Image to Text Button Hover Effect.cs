using UnityEngine;
using UnityEngine.EventSystems;

public class ImageToTextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CanvasGroup imageGroup;
    public CanvasGroup textGroup;
    public float transitionDuration = 0.3f;

    private Coroutine transitionCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartTransition(0f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartTransition(1f, 0f);
    }

    private void StartTransition(float imageTargetAlpha, float textTargetAlpha)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(Transition(imageTargetAlpha, textTargetAlpha));
    }

    private System.Collections.IEnumerator Transition(float imageTargetAlpha, float textTargetAlpha)
    {
        float imageStartAlpha = imageGroup.alpha;
        float textStartAlpha = textGroup.alpha;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = time / transitionDuration;
            imageGroup.alpha = Mathf.Lerp(imageStartAlpha, imageTargetAlpha, t);
            textGroup.alpha = Mathf.Lerp(textStartAlpha, textTargetAlpha, t);
            yield return null;
        }

        imageGroup.alpha = imageTargetAlpha;
        textGroup.alpha = textTargetAlpha;
    }
}
