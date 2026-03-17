using UnityEngine;
using System.Collections;

public class TitleLogin : MonoBehaviour
{
    public CanvasGroup titleGroup;
    public float fadeDuration = 0.5f;
    public GameObject loginPanel;
    
    private bool isTitleShowing = true;
    
    IEnumerator FadeOutTitleAndShowMenu()
    {
        if (titleGroup == null) yield break; // Safety check

        float timer = 0f;
        titleGroup.interactable = false;
        titleGroup.blocksRaycasts = false;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            titleGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        titleGroup.alpha = 0f;
        titleGroup.gameObject.SetActive(false);
        loginPanel.gameObject.SetActive(true);
    }

    void Start()
    {
        if (titleGroup != null)
        {
            titleGroup.alpha = 1f;
            titleGroup.interactable = true;
            titleGroup.blocksRaycasts = true;
        }
    }
    
    void Update()
    {
        if (isTitleShowing && titleGroup != null && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            isTitleShowing = false; // Prevent running multiple times
            StartCoroutine(FadeOutTitleAndShowMenu());
        }
    }
}
