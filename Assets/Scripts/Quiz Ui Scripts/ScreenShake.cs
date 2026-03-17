using UnityEngine;
using System.Collections;

public class UIShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private RectTransform uiContainerToShake; // The UI element you want to shake
    [SerializeField] private float shakeDuration = 0.3f;       // How long the shake lasts
    [SerializeField] private float shakeMagnitude = 12f;       // Strength of the shake
    [SerializeField] private float shakeDamping = 1f;          // Damping speed (higher = slower ease out)

    private Vector2 _originalUIPos;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if (uiContainerToShake == null)
            uiContainerToShake = GetComponent<RectTransform>();

        if (uiContainerToShake != null)
            _originalUIPos = uiContainerToShake.anchoredPosition;
    }

    /// <summary>
    /// Triggers a UI shake with custom duration and magnitude.
    /// </summary>
    public void Shake(float duration = 0.3f, float magnitude = 12f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;

        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);

        _shakeCoroutine = StartCoroutine(ShakeUI());
    }

    private IEnumerator ShakeUI()
    {
        if (uiContainerToShake == null) yield break;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // Smooth elastic movement pattern
            float damper = 1f - Mathf.Clamp01(elapsed / shakeDuration);
            float x = Mathf.Sin(elapsed * 40f) * shakeMagnitude * damper;
            float y = Mathf.Cos(elapsed * 40f) * shakeMagnitude * damper;

            uiContainerToShake.anchoredPosition = _originalUIPos + new Vector2(x, y);

            elapsed += Time.deltaTime * shakeDamping;
            yield return null;
        }

        uiContainerToShake.anchoredPosition = _originalUIPos;
        _shakeCoroutine = null;
    }
}
