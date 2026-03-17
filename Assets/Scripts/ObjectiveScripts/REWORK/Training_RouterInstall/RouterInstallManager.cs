using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class RouterInstallManager : AbstractObjectiveManager
{
    [Header("Level Identification")]
    [Tooltip("Unique ID for this training level, e.g., 'training_router_install'.")]
    public string levelId = "training_router_install";

    [Header("Level 2 References")]
    public RouterHolder routerHolder;
    public RouterConfiguration routerConfiguration;
    public MonoBehaviour playerLookScript;
    public GameObject hotbarUI;

    [Header("Results UI")]
    public GameObject resultsPanel;
    public TMP_Text timeTakenText;
    public TMP_Text totalScoreText;
    
    private ItemData currentRequiredItem;
    private bool isGameOver = false;

    protected override void OnLevelStart()
    {
        isGameOver = false;
        if (resultsPanel != null) resultsPanel.SetActive(false);
        
        if (routerConfiguration != null) routerConfiguration.Initialize(this);
        if (routerHolder != null)
        {
            routerHolder.Initialize(this);
        }
    }

    protected override void OnLevelEnd(float totalScore, float totalTime)
    {
        if (isGameOver) return;
        isGameOver = true;
        
        // Disable player controls
        if (playerLookScript != null) playerLookScript.enabled = false;
        
        // Find and disable PlayerMovement script to prevent cursor lock
        var playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.canMove = false;
        }
        
        Time.timeScale = 0f;
        
        // Use coroutine to ensure cursor is set after all other scripts have processed
        StartCoroutine(ShowCursorAndResults(totalScore, totalTime));
    }
    
    private IEnumerator ShowCursorAndResults(float totalScore, float totalTime)
    {
        // Wait a frame using realtime to ensure all other scripts have processed
        // (even though timeScale is 0, we use WaitForSecondsRealtime)
        yield return new WaitForSecondsRealtime(0.01f);
        
        // Set cursor state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        float performanceScore = CalculatePerformanceScore(totalTime);

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            if (timeTakenText != null) timeTakenText.text = $"Time Taken: {System.TimeSpan.FromSeconds(totalTime):mm\\:ss\\.ff}";
            if (totalScoreText != null) totalScoreText.text = $"Performance Score: {performanceScore:F2}%";
        }
        
        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            bool allCompleted = allObjectives.All(o => o.IsCompleted);
            UserSessionData.Instance.UpdateTrainingLevelProgress(levelId, allCompleted, performanceScore);
        }
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
        
        return Mathf.Clamp(objectiveBasedScore + timeBonusMax, 0f, 100f);
    }
    
    public void SetCurrentItemToPlace(ItemData item)
    {
        currentRequiredItem = item;
    }
    
    public void StartConfigurationMinigame()
    {
        var configObjective = currentObjective as T2_ConfigureRouterObjective;
        if (configObjective != null)
        {
            configObjective.StartConfigurationMinigame();
        }
    }
    
    protected override async void OnLevelQuit()
    {
        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            UserSessionData.Instance.UpdateTrainingLevelProgress(levelId, false, 0f);
        }
    }
}