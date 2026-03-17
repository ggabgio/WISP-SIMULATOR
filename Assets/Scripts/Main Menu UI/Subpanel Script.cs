using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SubPanelManager : MonoBehaviour
{
    [Header("Subpanels of this category (must have CanvasGroup)")]
    public List<CanvasGroup> subPanels;

    [Header("Transition Settings")]
    public float fadeDuration = 0.4f;

    private int currentIndex = -1;
    private Coroutine transitionCoroutine;

    private void Start()
    {
        HideAll();
    }

    private void OnDisable()
    {
        // Automatically reset when parent (Element 5) is hidden
        HideAll();
    }

    public void ShowSubPanel(int index)
    {
        if (index < 0 || index >= subPanels.Count) return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        if (currentIndex == index) return;

        if (currentIndex != -1)
            transitionCoroutine = StartCoroutine(CrossfadePanels(currentIndex, index));
        else
            transitionCoroutine = StartCoroutine(FadeInOnly(index));

        currentIndex = index;
    }

    public void HideAll()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        foreach (var panel in subPanels)
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
            panel.gameObject.SetActive(false);
        }

        currentIndex = -1;
    }

    private IEnumerator CrossfadePanels(int fromIndex, int toIndex)
    {
        CanvasGroup oldPanel = subPanels[fromIndex];
        CanvasGroup newPanel = subPanels[toIndex];

        newPanel.gameObject.SetActive(true);
        newPanel.alpha = 0;
        newPanel.interactable = false;
        newPanel.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            oldPanel.alpha = Mathf.Lerp(1f, 0f, t);
            newPanel.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        oldPanel.alpha = 0f;
        oldPanel.interactable = false;
        oldPanel.blocksRaycasts = false;
        oldPanel.gameObject.SetActive(false);

        newPanel.alpha = 1f;
        newPanel.interactable = true;
        newPanel.blocksRaycasts = true;
    }

    private IEnumerator FadeInOnly(int index)
    {
        CanvasGroup newPanel = subPanels[index];

        newPanel.gameObject.SetActive(true);
        newPanel.alpha = 0;
        newPanel.interactable = false;
        newPanel.blocksRaycasts = false;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            newPanel.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        newPanel.alpha = 1f;
        newPanel.interactable = true;
        newPanel.blocksRaycasts = true;
    }
}
