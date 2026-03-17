using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Linq;

public class AntennaInstallManager : AbstractObjectiveManager
{
    [Header("Level Identification")]
    [Tooltip("Unique ID for this training level, e.g., 'training_antenna_install'.")]
    public string levelId = "training_antenna_install";

    [Header("Component References")]
    public NewAntennaSecuring antennaSecuringScript;
    public MonoBehaviour playerLookScript;
    public TimerManager timerManager;

    [Header("UI References")]
    public GameObject resultsPanel;
    public TMP_Text timeLeftText;
    public TMP_Text objective1StatusText;
    public TMP_Text objective2StatusText;
    public TMP_Text objective3StatusText;
    public TMP_Text performanceScoreText;
    
    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    protected override void OnLevelStart()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (timerManager != null)
        {
            timerManager.ResetTimer();
            timerManager.StartTimer();
        }
    }

    protected override void OnLevelEnd(float totalObjectiveScore, float totalTime)
    {
        if (isGameOver) return;
        isGameOver = true;
        
        if (timerManager != null) timerManager.StopTimer();
        if (antennaSecuringScript != null) antennaSecuringScript.StopAnchorPlacement(false);
        if (playerLookScript != null) playerLookScript.enabled = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float performanceScore = _levelFailed ? 0f : CalculatePerformanceScore(totalTime);
        PopulateResultsPanel(performanceScore, totalTime);

        // Only record training level progress if NOT in assessment mode
        if (!isAssessmentMode && UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            bool allCompleted = allObjectives.All(o => o.IsCompleted);
            UserSessionData.Instance.UpdateTrainingLevelProgress(levelId, allCompleted, performanceScore);
        }
    }

    private void PopulateResultsPanel(float performanceScore, float timeTaken)
    {
        if (resultsPanel == null) return;
        
        var obj1 = FindObjective("T1_PlaceAntenna");
        var obj2 = FindObjective("T1_AlignAntenna");
        var obj3 = FindObjective("T1_SecureAntenna");

        if (timeLeftText != null) timeLeftText.text = $"Time Taken: {TimeSpan.FromSeconds(timeTaken):mm\\:ss\\.ff}";
        if (objective1StatusText != null) objective1StatusText.text = $"Place Antenna - {(obj1 != null && obj1.IsCompleted ? 100 : 0):F0}%";
        if (objective2StatusText != null) objective2StatusText.text = $"Align Antenna - {(obj2 != null ? obj2.GetScore() : 0):F0}%";
        if (objective3StatusText != null) objective3StatusText.text = $"Secure Antenna - {(obj3 != null ? obj3.GetScore() : 0):F0}%";
        if (performanceScoreText != null) performanceScoreText.text = $"Performance Score: {performanceScore:F2}%";

        resultsPanel.SetActive(true);
    }

    private float CalculatePerformanceScore(float timeTaken)
    {
        const float objectiveBaseScoreMax = 70.0f;
        const float timeBonusMax = 30.0f;

        float totalMaxScore = allObjectives.Sum(o => o.maxScore);
        if (totalMaxScore == 0) return 0; // Avoid division by zero
        
        float totalCurrentScore = allObjectives.Sum(o => o.GetScore());
        float objectiveBasedScore = (totalCurrentScore / totalMaxScore) * objectiveBaseScoreMax;
        float timeBasedScore = CalculateTimeScore(timeTaken) * timeBonusMax;
        
        return Mathf.Clamp(objectiveBasedScore + timeBasedScore, 0f, 100f);
    }
    
    protected override async void OnLevelQuit()
    {
        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            UserSessionData.Instance.UpdateTrainingLevelProgress(levelId, false, 0f);
        }
    }
}