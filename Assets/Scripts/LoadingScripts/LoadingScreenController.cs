using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Loading UI")]
    public Slider progressBar;
    public TMP_Text progressText;

    [Header("Settings")]
    public float minimumLoadTime = 1.0f;

    [Tooltip("Manually cap the displayed progress (0 to 1).")]
    [Range(0f, 1f)]
    public float manualProgress = 1f;

    // Fake loading curve for smooth easing progression
    [Header("Progress Smoothing")]
    [Tooltip("Controls the feel of the loading movement (X=time, Y=displayed %).")]
    public AnimationCurve fakeProgressCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.25f, 0.2f),
        new Keyframe(0.5f, 0.6f),
        new Keyframe(0.8f, 0.9f),
        new Keyframe(1f, 1f)
    );

    [Header("Dramatic Pause")]
    [Tooltip("If enabled, the progress bar will pause visually at ~99% before finishing.")]
    public bool useDramaticPause = true;

    [Tooltip("The percentage (0-1) at which the progress bar visually pauses.")]
    [Range(0.90f, 1f)]
    public float dramaticPauseThreshold = 0.99f;


    private float timeElapsed = 0f;

    void Start()
    {
        if (LoadingData.Instance == null) { Debug.LogError("Missing LoadingData instance!"); return; }

        string targetScene = LoadingData.Instance.sceneToLoad;
        if (string.IsNullOrEmpty(targetScene)) { Debug.LogError("sceneToLoad is empty!"); return; }

        if (progressBar != null) progressBar.value = 0;
        if (progressText != null) progressText.text = "0%";

        StartCoroutine(LoadSceneAsyncCoroutine(targetScene));
    }

    IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        yield return null;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        float targetProgress = 0f;
        timeElapsed = 0f;

        while (!asyncOperation.isDone)
        {
            timeElapsed += Time.deltaTime;

            // Real progress (0 to 0.9) then jumps to 1 after allowSceneActivation
            targetProgress = asyncOperation.progress < 0.9f ? asyncOperation.progress : 1f;

            // Smooth fake progress using curve over time
            float smoothFakeProgress = fakeProgressCurve.Evaluate(timeElapsed / minimumLoadTime);

            // Combine fake smoothness + manual override
            float desiredDisplayedProgress = Mathf.Min(targetProgress, smoothFakeProgress, manualProgress);

            // Dramatic pause at 99% (if enabled)
            if (useDramaticPause && desiredDisplayedProgress >= dramaticPauseThreshold && asyncOperation.progress < 1f)
            {
                desiredDisplayedProgress = dramaticPauseThreshold;
            }

            // Smoothly animate bar toward desired displayed progress
            if (progressBar != null)
                progressBar.value = Mathf.MoveTowards(progressBar.value, desiredDisplayedProgress, Time.deltaTime * 1.5f);

            if (progressText != null)
                progressText.text = $"{(progressBar.value * 100f):0}%";

            // Finish only when real progress + fake smoothing + manual allow
            if (asyncOperation.progress >= 0.9f && timeElapsed >= minimumLoadTime && manualProgress >= 1f)
            {
                progressBar.value = 1f;
                if (progressText != null) progressText.text = "100%";

                LoadingData.Instance.sceneToLoad = null;
                FindObjectOfType<SceneFader>().FadeOutAndLoad(sceneName);
                yield break;
            }

            yield return null;
        }
    }
}
