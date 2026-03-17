using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;          // Drag your white fullscreen image here
    public float fadeDuration = 1f;  // Time in seconds

    void Awake()
    {
        // Ensure fully transparent at start
        SetAlpha(0);

        // Force the canvas to the very top
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 9999;
        }
    }

    public void FadeOutAndLoad(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeOut(string sceneName)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration); // transparent to solid white
            fadeImage.color = c;
            yield return null;
        }

        SetAlpha(1); // ensure fully white
        SceneManager.LoadScene(sceneName);
    }

    private void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
