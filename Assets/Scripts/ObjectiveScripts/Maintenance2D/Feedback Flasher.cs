using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FeedbackFlasher : MonoBehaviour
{
    [SerializeField] private float _visibleDuration = 0.5f;
    [SerializeField] private float _fadeDuration = 0.5f;

    private TextMeshProUGUI _feedbackText;
    private CanvasGroup _canvasGroup;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _feedbackText = GetComponent<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false; // Start by not blocking clicks
    }

    public void FlashMessage(string message)
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(FlashRoutine(message));
    }

    private IEnumerator FlashRoutine(string message)
    {
        _feedbackText.text = message;
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true; // Block clicks while visible

        yield return new WaitForSeconds(_visibleDuration);

        float elapsedTime = 0f;
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            _canvasGroup.alpha = 1f - (elapsedTime / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false; // Stop blocking clicks when invisible
    }
}