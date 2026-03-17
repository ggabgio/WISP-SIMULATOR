using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class SignalOptimizationManager : AbstractObjectiveManager
{
    [Header("Level Identification")]
    [Tooltip("Unique ID for this training level, e.g., 'training_signal_opt'.")]
    public string levelId = "training_signal_opt";

    [Header("Results Panel")]
    public GameObject resultsPanel;
    public TMP_Text timeTakenText;
    public TMP_Text totalScoreText;

    private bool isGameOver = false;

    protected override void OnLevelStart()
    {
        isGameOver = false;
        if (resultsPanel != null) resultsPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void OnLevelEnd(float totalScore, float totalTime)
    {
        if (isGameOver) return;
        isGameOver = true;

        float performanceScore = CalculatePerformanceScore(totalTime);

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            if (timeTakenText != null) timeTakenText.text = $"Time Taken: {System.TimeSpan.FromSeconds(totalTime):mm\\:ss\\.ff}";
            if (totalScoreText != null) totalScoreText.text = $"Performance Score: {performanceScore:F2}%";
        }

        // Only record training level progress if NOT in assessment mode
        if (!isAssessmentMode && UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            bool allCompleted = allObjectives.All(o => o.IsCompleted);
            UserSessionData.Instance.UpdateTrainingLevelProgress(levelId, allCompleted, performanceScore);
        }

        Time.timeScale = 0f;
    }
    
    private float CalculatePerformanceScore(float timeTaken)
    {
        const float objectiveBaseScoreMax = 70.0f;
        const float timeBonusMax = 30.0f;

        float totalMaxScore = allObjectives.Sum(o => o.maxScore);
        if (totalMaxScore == 0) return 0;
        
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