using UnityEngine;
using System.Collections;

public class UIEffectsManager : MonoBehaviour
{
    public static UIEffectsManager Instance;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays a one-shot sound globally.
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Shakes a UI RectTransform for a short time (elastic style).
    /// </summary>
    public void Shake(RectTransform target, float duration, float magnitude, float damping)
    {
        if (target != null)
            StartCoroutine(ShakeCoroutine(target, duration, magnitude, damping));
    }

    private IEnumerator ShakeCoroutine(RectTransform target, float duration, float magnitude, float damping)
    {
        if (target == null) yield break;

        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            float angle = elapsed * 40f;

            float x = Mathf.Sin(angle) * magnitude * damper;
            float y = Mathf.Cos(angle) * magnitude * damper;

            target.anchoredPosition = originalPos + new Vector2(x, y);

            elapsed += Time.deltaTime * damping;
            yield return null;
        }

        target.anchoredPosition = originalPos;
    }
}
