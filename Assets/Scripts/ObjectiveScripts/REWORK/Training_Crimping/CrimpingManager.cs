using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class CrimpingManager : AbstractObjectiveManager
{
    [Header("Level Identification")]
    public string levelId = "training_crimping";

    [Header("Level References")]
    public MonoBehaviour playerLookScript; 
    public EthernetCableInteraction minigameInteraction;
    
    [Header("UI Click Management")]
    // Ito ang GameObject na i-da-drag mo sa bawat DelayedAnimationTrigger script
    public GameObject clickBlockerPanel; 
    
    [Header("Prompts UI")]
    [Tooltip("The prompts UI GameObject that should be activated when entering the minigame camera and deactivated when exiting.")]
    public GameObject crimpingUI;

    [Header("Results Panel")]
    public GameObject resultsPanel;
    public TMP_Text timeTakenText;
    public TMP_Text totalScoreText;

    private bool isGameOver = false;
    
    // ---

    private void OnEnable()
    {
        // Subscribe to InspectController events
        if (InspectController.Instance != null)
        {
            InspectController.Instance.OnEnterInspect += HandleEnterInspect;
            InspectController.Instance.OnExitInspect += HandleExitInspect;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from InspectController events
        if (InspectController.Instance != null)
        {
            InspectController.Instance.OnEnterInspect -= HandleEnterInspect;
            InspectController.Instance.OnExitInspect -= HandleExitInspect;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        // Subscribe to InspectController events in Awake as well
        SubscribeToInspectController();
    }

    private void SubscribeToInspectController()
    {
        if (InspectController.Instance != null)
        {
            // Unsubscribe first to avoid duplicate subscriptions
            InspectController.Instance.OnEnterInspect -= HandleEnterInspect;
            InspectController.Instance.OnExitInspect -= HandleExitInspect;
            
            // Subscribe to events
            InspectController.Instance.OnEnterInspect += HandleEnterInspect;
            InspectController.Instance.OnExitInspect += HandleExitInspect;
            Debug.Log("[CrimpingManager] Subscribed to InspectController events.");
        }
        else
        {
            // Try again next frame if Instance isn't ready yet (max 10 attempts)
            StartCoroutine(DelayedSubscribe(0));
        }
    }

    private IEnumerator DelayedSubscribe(int attempts)
    {
        yield return null; // Wait one frame
        if (InspectController.Instance != null)
        {
            SubscribeToInspectController();
        }
        else if (attempts < 10) // Try up to 10 times
        {
            StartCoroutine(DelayedSubscribe(attempts + 1));
        }
        else
        {
            Debug.LogWarning("[CrimpingManager] Could not find InspectController.Instance after multiple attempts. Prompts UI may not activate properly.");
        }
    }
    
    protected override void OnLevelStart()
    {
        // Ang lahat ng initialization logic ay nandito.
        isGameOver = false;
        if (resultsPanel != null) resultsPanel.SetActive(false);
        
        // Tiyakin na naka-off ang blocker sa simula
        if (clickBlockerPanel != null) clickBlockerPanel.SetActive(false); 
        
        // Ensure crimpingUI is deactivated at level start (will be activated when entering minigame)
        if (crimpingUI != null) crimpingUI.SetActive(false);
        
        if (playerLookScript != null) playerLookScript.enabled = false;
        Time.timeScale = 1f;
        if (FindAnyObjectByType<CombinedLevelManager>() == null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    // ---
    
    // Ang function na ito ay HINDI na kailangan dahil sa DelayedAnimationTrigger script.
    // public void BlockClicksDuringAnimation() { } 

    // ---
    
    protected override void OnLevelEnd(float totalScore, float totalTime)
    {
        if (isGameOver) return;

        if (FindAnyObjectByType<CombinedLevelManager>() != null)
        {
            Debug.Log("[CrimpingManager] Skipped OnLevelEnd because CombinedLevelManager is active.");
            return;
        }

        isGameOver = true;
        
        if (clickBlockerPanel != null) clickBlockerPanel.SetActive(false);

        float performanceScore = CalculatePerformanceScore(totalTime);
        
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            if (timeTakenText != null) timeTakenText.text = $"Time Taken: {System.TimeSpan.FromSeconds(totalTime):mm\\:ss\\.ff}";
            if (totalScoreText != null) totalScoreText.text = $"Performance Score: {performanceScore:F2}%";
        }
        
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

    /// <summary>
    /// Handles when player enters inspect view (via InspectController).
    /// Activates the prompts UI if entering the crimping minigame.
    /// </summary>
    private void HandleEnterInspect(InspectableObject target)
    {
        // Check if the inspect camera is the minigame camera
        // We can identify this by checking if it's related to the crimping minigame
        if (target != null && target.inspectCamera != null)
        {
            // Activate the prompts UI when entering the minigame camera
            if (crimpingUI != null) crimpingUI.SetActive(true);
            Debug.Log("[CrimpingManager] Entered inspect view - prompts UI activated.");
        }
    }

    /// <summary>
    /// Handles when player exits inspect view (via InspectController).
    /// Deactivates the prompts UI.
    /// </summary>
    private void HandleExitInspect()
    {
        // Deactivate the prompts UI when exiting the minigame camera
        if (crimpingUI != null) crimpingUI.SetActive(false);
        Debug.Log("[CrimpingManager] Exited inspect view - prompts UI deactivated.");
    }

    /// <summary>
    /// Called when the player enters the minigame camera view.
    /// Activates the prompts UI.
    /// </summary>
    public void OnEnterMinigame()
    {
        if (crimpingUI != null) crimpingUI.SetActive(true);
        Debug.Log("[CrimpingManager] Entered minigame - prompts UI activated.");
    }

    /// <summary>
    /// Called when the player exits the minigame camera view.
    /// Deactivates the prompts UI. This method should always work, even if the manager is disabled.
    /// </summary>
    public void OnExitMinigame()
    {
        // Always deactivate the UI, regardless of manager state
        if (crimpingUI != null)
        {
            crimpingUI.SetActive(false);
            Debug.Log("[CrimpingManager] Exited minigame - prompts UI deactivated.");
        }
        else
        {
            Debug.LogWarning("[CrimpingManager] crimpingUI reference is null. Cannot deactivate prompts UI.");
        }
    }
    
    /// <summary>
    /// Static method to directly deactivate the prompts UI, even if the manager is disabled.
    /// This can be called from other scripts as a fallback.
    /// </summary>
    public static void DeactivateCrimpingUI()
    {
        // Try to find the manager (including inactive ones)
        CrimpingManager manager = FindFirstObjectByType<CrimpingManager>();
        
        // If not found, try FindObjectOfType as fallback (includes inactive by default in newer Unity)
        if (manager == null)
        {
            manager = FindObjectOfType<CrimpingManager>(true); // true = include inactive
        }
        
        if (manager != null && manager.crimpingUI != null)
        {
            manager.crimpingUI.SetActive(false);
            Debug.Log("[CrimpingManager] Static method: Deactivated crimpingUI prompts via manager.");
            return;
        }
        
        // Fallback: try to find the UI GameObject directly by name
        GameObject crimpingUI = GameObject.Find("crimpingUI");
        if (crimpingUI == null)
        {
            crimpingUI = GameObject.Find("CrimpingUI");
        }
        if (crimpingUI == null)
        {
            // Try to find by searching all GameObjects in the scene
            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("crimpingUI") || obj.name.Contains("CrimpingUI") || obj.name.Contains("Crimping UI"))
                {
                    crimpingUI = obj;
                    break;
                }
            }
        }
        
        if (crimpingUI != null)
        {
            crimpingUI.SetActive(false);
            Debug.Log("[CrimpingManager] Static method: Found and deactivated crimpingUI directly by name.");
        }
        else
        {
            Debug.LogWarning("[CrimpingManager] Static method: Could not find crimpingUI GameObject to deactivate. Make sure it exists in the scene.");
        }
    }

    public void ExitMinigame()
    {
        Time.timeScale = 1f;
        if (minigameInteraction != null)
        {
            minigameInteraction.ExitMinigame();
        }
        else
        {
            SceneManager.LoadScene("mainMenu");
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