using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInAtStart : MonoBehaviour
{
    public Image fadeImage;          // Fullscreen white or black UI Image
    public float fadeDuration = 1f;  // Seconds

    void Start()
    {
        // Make sure we're starting fully visible
        SetAlpha(1);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration); // fade from opaque to transparent
            fadeImage.color = c;
            yield return null;
        }

        SetAlpha(0); // Fully transparent at the end
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
