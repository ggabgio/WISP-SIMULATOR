using UnityEngine;
using System.Collections;

public class PanelTransitionManager : MonoBehaviour
{
    [Header("Panel Button Groups (CanvasGroups)")]
    public CanvasGroup[] panelGroups;

    [Header("Transition Settings")]
    public float fadeDuration = 0.6f;

    private int currentIndex = -1;
    private Coroutine transitionCoroutine;

    public void ShowPanel(int newIndex)
    {
        if (newIndex < 0 || newIndex >= panelGroups.Length) return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(CrossfadePanels(newIndex));
    }

    public void HidePanel(int index)
    {
        if (index < 0 || index >= panelGroups.Length) return;

        CanvasGroup panel = panelGroups[index];
        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);

        if (currentIndex == index)
            currentIndex = -1;
    }

    public void HideAllPanels()
    {
        for (int i = 0; i < panelGroups.Length; i++)
        {
            CanvasGroup panel = panelGroups[i];
            if (panel != null)
            {
                panel.alpha = 0;
                panel.interactable = false;
                panel.blocksRaycasts = false;
                panel.gameObject.SetActive(false);
            }
        }

        currentIndex = -1;
    }

    /// <summary>
    /// Show a new panel WITHOUT hiding the others (additive).
    /// </summary>
    public void ShowPanelAdditive(int newIndex)
    {
        if (newIndex < 0 || newIndex >= panelGroups.Length) return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(FadeInPanel(panelGroups[newIndex]));
    }

    private IEnumerator CrossfadePanels(int newIndex)
    {
        CanvasGroup oldPanel = null;
        if (currentIndex >= 0 && currentIndex < panelGroups.Length)
            oldPanel = panelGroups[currentIndex];

        CanvasGroup newPanel = panelGroups[newIndex];
        newPanel.gameObject.SetActive(true);
        newPanel.alpha = 0;
        newPanel.interactable = false;
        newPanel.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            if (oldPanel != null)
                oldPanel.alpha = Mathf.Lerp(1, 0, t);

            newPanel.alpha = Mathf.Lerp(0, 1, t);

            yield return null;
        }

        if (oldPanel != null)
        {
            oldPanel.alpha = 0;
            oldPanel.interactable = false;
            oldPanel.blocksRaycasts = false;
            oldPanel.gameObject.SetActive(false);
        }

        newPanel.alpha = 1;
        newPanel.interactable = true;
        newPanel.blocksRaycasts = true;

        currentIndex = newIndex;
    }

    private IEnumerator FadeInPanel(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        panel.alpha = 0;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            panel.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        panel.alpha = 1;
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }
}
