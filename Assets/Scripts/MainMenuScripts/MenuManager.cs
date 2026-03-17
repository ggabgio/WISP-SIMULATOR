using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [System.Serializable]
    public class MenuEntry
    {
        public Button button;
        public int panelIndex;
    }

    [Header("Main Menu Setup")]
    [SerializeField] private List<MenuEntry> mainMenuEntries = new List<MenuEntry>();
    [SerializeField] private GameObject mainMenuContainer;

    [Header("Topbar Setup")]
    [SerializeField] private List<MenuEntry> topbarEntries = new List<MenuEntry>();

    [Header("References")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Button exitButton;
    [SerializeField] private PanelTransitionManager transitionManager;

    [Header("Scores")]
    [SerializeField] private Button scoresButton;       // MainMenu → Panel 2
    [SerializeField] private Button scoreDetailButton;  // Panel 2 → Panel 3
    [SerializeField] private Button scoreImageButton;   // Panel 3 → Panel 4

    private int activeMainMenuIndex = -1;
    private int activeTopbarIndex = -1;

    private void Start()
    {
        Debug.Log("MenuManager bound to UIManager: " + UIManager.Instance);

        // Main menu buttons
        for (int i = 0; i < mainMenuEntries.Count; i++)
        {
            int index = i;
            if (mainMenuEntries[i].button != null)
                mainMenuEntries[i].button.onClick.AddListener(() => ShowMainMenuPanel(index));
        }

        // Topbar buttons
        for (int i = 0; i < topbarEntries.Count; i++)
        {
            int index = i;
            if (topbarEntries[i].button != null)
                topbarEntries[i].button.onClick.AddListener(() => ToggleTopbarPanel(index));
        }

        // Username
        if (usernameText != null && UserSessionData.Instance != null)
        {
            usernameText.text = UserSessionData.Instance.profileData.username;
        }

        // Exit
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        // 🔹 Scores flow
        if (scoresButton != null) scoresButton.onClick.AddListener(() =>
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowScoresPanel();
        });

        if (scoreDetailButton != null) scoreDetailButton.onClick.AddListener(() =>
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowScoreDetailPanel();
        });

        if (scoreImageButton != null) scoreImageButton.onClick.AddListener(() =>
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowScoreImagePanel();
        });

        ResetAllPanels();
    }

    private void ResetAllPanels()
    {
        if (transitionManager != null)
            transitionManager.HideAllPanels();

        if (mainMenuContainer != null) mainMenuContainer.SetActive(true);

        if (UIManager.Instance != null && UIManager.Instance.profileMenuGroup != null)
            UIManager.Instance.profileMenuGroup.SetActive(false);

        activeMainMenuIndex = -1;
        activeTopbarIndex = -1;
    }

    public void ShowMainMenuPanel(int index)
    {
        if (index < 0 || index >= mainMenuEntries.Count) return;
        bool isSameButton = (activeMainMenuIndex == index);

        if (transitionManager != null)
            transitionManager.HideAllPanels();

        if (isSameButton)
        {
            if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
            if (UIManager.Instance != null) UIManager.Instance.profileMenuGroup.SetActive(false);
            activeMainMenuIndex = -1;
        }
        else
        {
            transitionManager.ShowPanel(mainMenuEntries[index].panelIndex);
            if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
            if (UIManager.Instance != null) UIManager.Instance.profileMenuGroup.SetActive(false);
            activeMainMenuIndex = index;
        }

        activeTopbarIndex = -1;
    }

    public void ToggleTopbarPanel(int index)
    {
        if (index < 0 || index >= topbarEntries.Count) return;
        bool isSameButton = (activeTopbarIndex == index);

        if (transitionManager != null)
            transitionManager.HideAllPanels();

        if (isSameButton)
        {
            if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
            if (UIManager.Instance != null) UIManager.Instance.profileMenuGroup.SetActive(false);
            activeTopbarIndex = -1;
        }
        else
        {
            transitionManager.ShowPanel(topbarEntries[index].panelIndex);
            if (mainMenuContainer != null) mainMenuContainer.SetActive(false);
            if (UIManager.Instance != null) UIManager.Instance.profileMenuGroup.SetActive(false);
            activeTopbarIndex = index;
        }

        activeMainMenuIndex = -1;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
