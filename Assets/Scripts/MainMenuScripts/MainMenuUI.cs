using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Menu Groups")]
    public GameObject mainMenuGroup;
    public GameObject trainingMenuGroup;
    public GameObject profileMenuGroup; // hidden
    public GameObject quizMenuGroup;
    public GameObject optionsMenuGroup;
    public GameObject assessmentMenuGroup;
    public GameObject maintenanceMenuGroup;

    [Header("New Score Panels")]
    public GameObject scoresPanel;        // Panel 2
    public GameObject scoreDetailPanel;   // Panel 3
    public GameObject scoreImagePanel;    // Panel 4 (has the image)

    [Header("Profile UI (legacy, hidden)")]
    public Transform trainingTextsParent;
    public Transform examTextsParent;
    public Transform assessmentTextsParent;
    public TMP_Text profileUser;

    [Header("Top Bar")]
    public TMP_Text usernameProfileText;

    private List<TMP_Text> trainingTexts = new List<TMP_Text>();
    private List<TMP_Text> examTexts = new List<TMP_Text>();
    private List<TMP_Text> assessmentTexts = new List<TMP_Text>();

    void Awake()
    {
        Instance = this;

        trainingTextsParent.GetComponentsInChildren(true, trainingTexts);
        examTextsParent.GetComponentsInChildren(true, examTexts);
        assessmentTextsParent.GetComponentsInChildren(true, assessmentTexts);

        Debug.Log("UIManager created for scene: " +
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void OnEnable()
    {
        UpdateProfileTexts();
    }

    public void UpdateProfileTexts()
    {
        if (UserSessionData.Instance == null || UserSessionData.Instance.profileData == null)
            return;

        var profile = UserSessionData.Instance.profileData;

        if (profileUser != null) profileUser.text = profile.username;
        if (usernameProfileText != null) usernameProfileText.text = profile.username;

        UpdateCategoryTexts(trainingTexts, profile.trainingData, "Level");
        UpdateCategoryTexts(examTexts, profile.quizData, "Quiz");
        UpdateCategoryTexts(assessmentTexts, profile.assessmentData, "Assessment");
    }

    private void UpdateCategoryTexts(List<TMP_Text> textElements,
        Dictionary<string, LevelData> data, string prefix)
    {
        foreach (var textElement in textElements)
        {
            if (textElement == null) continue;
            string levelId = textElement.gameObject.name;

            if (data != null && data.TryGetValue(levelId, out LevelData levelData))
            {
                float displayScore = 0f;
                string completeStatus = "Incomplete";

                if (levelData.attempts != null && levelData.attempts.Count > 0)
                {
                    AttemptData firstAttempt = levelData.attempts
                        .OrderBy(a => a.timestamp)
                        .FirstOrDefault();

                    if (firstAttempt != null)
                    {
                        displayScore = firstAttempt.performanceScore;
                    }

                    completeStatus = levelData.isCompleted ? "Complete" : "Attempted";
                }

                textElement.text = $"{prefix} ({completeStatus}): {displayScore:F2}%";
            }
            else
            {
                textElement.text = $"{prefix} (Not Attempted): 0.00%";
            }
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // New panel controls
    public void ShowScoresPanel()
    {
        HideAllMenus();
        if (scoresPanel != null) scoresPanel.SetActive(true);
    }

    public void ShowScoreDetailPanel()
    {
        HideAllMenus();
        if (scoreDetailPanel != null) scoreDetailPanel.SetActive(true);
    }

    public void ShowScoreImagePanel()
    {
        HideAllMenus();
        if (scoreImagePanel != null) scoreImagePanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        HideAllMenus();
        if (mainMenuGroup != null) mainMenuGroup.SetActive(true);
    }

    public void ShowTrainingMenu()
    {
        HideAllMenus();
        if (trainingMenuGroup != null) trainingMenuGroup.SetActive(true);
    }

    public void ShowAssessmentMenu()
    {
        HideAllMenus();
        if (assessmentMenuGroup != null) assessmentMenuGroup.SetActive(true);
    }

    public void ShowOptionsMenu()
    {
        HideAllMenus();
        if (optionsMenuGroup != null) optionsMenuGroup.SetActive(true);
    }

    public void ShowQuizMenu()
    {
        HideAllMenus();
        if (quizMenuGroup != null) quizMenuGroup.SetActive(true);
    }

    // Maintenance menu support
    public void ShowMaintenanceMenu()
    {
        HideAllMenus();
        if (maintenanceMenuGroup != null) maintenanceMenuGroup.SetActive(true);
    }

    public void HideAllMenus()
    {
        if (mainMenuGroup != null) mainMenuGroup.SetActive(false);
        if (trainingMenuGroup != null) trainingMenuGroup.SetActive(false);
        if (profileMenuGroup != null) profileMenuGroup.SetActive(false);
        if (optionsMenuGroup != null) optionsMenuGroup.SetActive(false);
        if (quizMenuGroup != null) quizMenuGroup.SetActive(false);
        if (assessmentMenuGroup != null) assessmentMenuGroup.SetActive(false);
        if (maintenanceMenuGroup != null) maintenanceMenuGroup.SetActive(false);
        if (scoresPanel != null) scoresPanel.SetActive(false);
        if (scoreDetailPanel != null) scoreDetailPanel.SetActive(false);
        if (scoreImagePanel != null) scoreImagePanel.SetActive(false);
    }
}
