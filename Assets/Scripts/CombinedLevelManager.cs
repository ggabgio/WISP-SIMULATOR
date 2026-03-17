using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the assessment level with flexible dependencies.
/// Allows players to complete levels in various orders based on dependencies.
/// 
/// Level Structure:
/// - Level 1-3: Can be done in ANY order (Antenna, Router, Crimping)
/// - Level 4: Requires Level 1 (Cable Laying)
/// - Level 5: Requires Levels 1 & 4 (Signal Optimization)
/// </summary>
public class CombinedLevelManager : MonoBehaviour
{
    [Header("Individual Level Managers")]
    public AntennaInstallManager antennaManager;
    public RouterInstallManager routerManager;
    public CrimpingManager crimpingManager;
    public CableLayingManager cableLayingManager;
    public SignalOptimizationManager signalOptimizationManager;

    [Header("Assessment Results UI")]
    public GameObject combinedResultsPanel;
    public TMP_Text combinedScoreText;
    public TMP_Text totalTimeText;
    public TMP_Text completedLevelsText;
    public TMP_Text objectivesPercentText; // 0-70%
    public TMP_Text timePercentText;       // 0-30%
    public TMP_Text finalScoreText;        // numeric total score line

    [Header("Player Reference")]
    public GameObject player;

    // Level tracking
    private Dictionary<AbstractObjectiveManager, bool> levelCompletionStatus;
    private Dictionary<AbstractObjectiveManager, List<AbstractObjectiveManager>> levelDependencies;
    private Dictionary<AbstractObjectiveManager, ObjectiveEndProxy> activeProxies;

    private float totalScore = 0f;
    private int completedLevelCount = 0;

    [Header("Assessment Scoring Settings")]
    [Tooltip("Unique ID for this assessment level, e.g., 'assessment_combined'.")]
    public string levelId = "assessment_combined";

    [Tooltip("Par time (seconds) for the entire assessment.")]
    public float assessmentParTime = 600f;

    [Tooltip("Curve for time score decay beyond par time. X = seconds over par, Y = 0..1 fraction of max time score.")]
    public AnimationCurve assessmentTimeDecayCurve = AnimationCurve.EaseInOut(0, 1, 300, 0);

    // Aggregates for objective and time scoring across all sub-levels
    private float aggregateObjectiveScoreSum = 0f; // sum of each objective's 0..100 score
    private int aggregateObjectiveCount = 0;       // number of objectives across all levels
    private float aggregateTimeSeconds = 0f;       // sum of each level's elapsed time

    // Assessment-wide timer (tracks total duration regardless of sub-level timers)
    private float assessmentElapsedSeconds = 0f;
    private bool assessmentActive = false;
    
    public async void QuitAssessment()
    {
        if (!assessmentActive) return;
        assessmentActive = false;

        Time.timeScale = 1f;

        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            await UserSessionData.Instance.UpdateAssessmentLevelProgress(levelId, false, 0f);
        }

        SceneFader fader = FindObjectOfType<SceneFader>();
        if (fader != null)
        {
            fader.FadeOutAndLoad("mainMenu");
        }
        else
        {
            SceneManager.LoadScene("mainMenu");
        }
    }
    
    private void Start()
    {
        InitializeLevelSystem();
        StartCoroutine(RunAssessmentLevel());
    }

    /// <summary>
    /// Sets up dependencies and initializes all level managers.
    /// </summary>
    private void InitializeLevelSystem()
    {
        levelCompletionStatus = new Dictionary<AbstractObjectiveManager, bool>();
        levelDependencies = new Dictionary<AbstractObjectiveManager, List<AbstractObjectiveManager>>();
        activeProxies = new Dictionary<AbstractObjectiveManager, ObjectiveEndProxy>();

        // Initialize all managers (disabled by default)
        var allManagers = new List<AbstractObjectiveManager> 
        { 
            antennaManager, 
            routerManager, 
            crimpingManager, 
            cableLayingManager, 
            signalOptimizationManager 
        }.Where(m => m != null).ToList();

        foreach (var manager in allManagers)
        {
            levelCompletionStatus[manager] = false;
            manager.gameObject.SetActive(false);
            manager.manualStart = true; // Prevent auto-starting
        }

        // Set up dependencies:
        // Level 4 (Cable Laying) requires Level 1 (Antenna)
        // Level 4 (Cable Laying) requires Levels 1-3 (Antenna, Router, Crimping)
        if (cableLayingManager != null)
        {
            if (!levelDependencies.ContainsKey(cableLayingManager))
                levelDependencies[cableLayingManager] = new List<AbstractObjectiveManager>();

            if (antennaManager != null) levelDependencies[cableLayingManager].Add(antennaManager);
            if (routerManager != null) levelDependencies[cableLayingManager].Add(routerManager);
            if (crimpingManager != null) levelDependencies[cableLayingManager].Add(crimpingManager);
        }


        // Level 5 (Signal Optimization) requires Level 1 (Antenna) and Level 4 (Cable Laying)
        if (signalOptimizationManager != null && antennaManager != null && cableLayingManager != null)
        {
            if (!levelDependencies.ContainsKey(signalOptimizationManager))
                levelDependencies[signalOptimizationManager] = new List<AbstractObjectiveManager>();
            levelDependencies[signalOptimizationManager].Add(antennaManager);
            levelDependencies[signalOptimizationManager].Add(cableLayingManager);
        }

        Debug.Log("✅ Assessment Level initialized with dependency system");
    }

    /// <summary>
    /// Main assessment loop - enables and monitors available levels.
    /// </summary>
    private IEnumerator RunAssessmentLevel()
    {
        // Enable player controls
        EnablePlayerControls(true);

        // Start assessment-wide timer
        assessmentElapsedSeconds = 0f;
        assessmentActive = true;

        // Continue until all levels are complete
        while (completedLevelCount < levelCompletionStatus.Count)
        {
            // Check for newly available levels and enable them
            EnableAvailableLevels();

            // Small delay to prevent excessive checks
            yield return new WaitForSeconds(0.1f);
        }

        // All levels complete - stop timer and show results
        assessmentActive = false;
        OnAllLevelsCompleted();
    }

    private void Update()
    {
        if (assessmentActive)
            assessmentElapsedSeconds += Time.deltaTime;
    }

    /// <summary>
    /// Enables any levels whose dependencies are met.
    /// </summary>
    private void EnableAvailableLevels()
    {
        foreach (var kvp in levelCompletionStatus)
        {
            var manager = kvp.Key;
            var isComplete = kvp.Value;

            // Skip if already complete or already active
            if (isComplete || manager.gameObject.activeSelf)
                continue;

                // 🚫 Skip Signal Optimization auto-start
            if (manager == signalOptimizationManager)
                continue;

            // Check if dependencies are met
            if (AreDependenciesMet(manager))
            {
                EnableLevel(manager);
            }
        }
    }

    /// <summary>
    /// Checks if all dependencies for a level are complete.
    /// </summary>
    private bool AreDependenciesMet(AbstractObjectiveManager manager)
    {
        if (!levelDependencies.ContainsKey(manager))
            return true; // No dependencies, so available immediately

        // Check if all dependencies are complete
        foreach (var dependency in levelDependencies[manager])
        {
            if (!levelCompletionStatus.ContainsKey(dependency) || !levelCompletionStatus[dependency])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Enables a specific level and sets up completion monitoring.
    /// </summary>
    private void EnableLevel(AbstractObjectiveManager manager)
    {
        if (manager == null) return;

        Debug.Log($"▶ Enabling level: {manager.name}");

        // Activate the manager GameObject
        manager.gameObject.SetActive(true);

        // Give it a frame to initialize
        StartCoroutine(StartLevelDelayed(manager));
    }

    /// <summary>
    /// Delayed start to allow initialization.
    /// </summary>
    private IEnumerator StartLevelDelayed(AbstractObjectiveManager manager)
    {
        yield return null; // Wait one frame for initialization

        // Mark as assessment mode to prevent training level progress recording
        manager.isAssessmentMode = true;

        // Start the level
        manager.StartLevel();

        // Set up completion watcher
        System.Action<float, float> onComplete = (score, time) =>
        {
            OnLevelCompleted(manager, score, time);
        };

        // Attach the watcher proxy
        var proxy = manager.gameObject.AddComponent<ObjectiveEndProxy>();
        proxy.Initialize(manager, onComplete);
        activeProxies[manager] = proxy;
    }

    /// <summary>
    /// Called when a level is completed.
    /// </summary>
    public void OnLevelCompleted(AbstractObjectiveManager manager, float score, float time)
    {
        if (levelCompletionStatus.ContainsKey(manager) && levelCompletionStatus[manager])
            return; // Already processed

        levelCompletionStatus[manager] = true;
        completedLevelCount++;
        totalScore += score;

        // Determine score multiplier based on level's overall score
        // Below 76%: count as zero, exactly 76%: count as half, above 76%: count normally
        float scoreMultiplier = 1f;
        if (score < 76f)
        {
            scoreMultiplier = 0f; // Below 76% counts as zero
        }
        else if (Mathf.Approximately(score, 76f))
        {
            scoreMultiplier = 0.5f; // Exactly 76% counts as half
        }
        // else scoreMultiplier remains 1f for scores above 76%

        // Aggregate objective-only data for assessment-wide scoring
        if (manager != null && manager.allObjectives != null)
        {
            for (int i = 0; i < manager.allObjectives.Count; i++)
            {
                var obj = manager.allObjectives[i];
                if (obj == null) continue;
                float objectiveScore = Mathf.Clamp(obj.GetScore(), 0f, 100f);
                aggregateObjectiveScoreSum += objectiveScore * scoreMultiplier;
                aggregateObjectiveCount += 1;
            }
        }

        // Aggregate time across sub-levels
        aggregateTimeSeconds += Mathf.Max(0f, time);

        Debug.Log($"✅ Level completed: {manager.name} - Score: {score:F2}% | Total Progress: {completedLevelCount}/{levelCompletionStatus.Count}");

        // Clean up the proxy
        if (activeProxies.ContainsKey(manager))
        {
            Destroy(activeProxies[manager]);
            activeProxies.Remove(manager);
        }

        // Disable the manager GameObject
        manager.gameObject.SetActive(false);

        // Unlock cursor in case any level locked it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EnablePlayerControls(true);
    }

    /// <summary>
    /// Called when all levels are completed.
    /// </summary>
    private void OnAllLevelsCompleted()
    {
        Debug.Log("🏁 All assessment levels completed!");
        
        // Disable player controls (movement and camera)
        EnablePlayerControls(false);
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Compute assessment-wide score: 70% objectives + 30% time
        float objectivePercent = 0f;
        if (aggregateObjectiveCount > 0)
        {
            objectivePercent = (aggregateObjectiveScoreSum / (aggregateObjectiveCount * 100f)) * 70f;
        }

        float timeFraction;
        // Use assessment-wide elapsed time for time scoring
        float totalAssessmentTime = Mathf.Max(0f, assessmentElapsedSeconds);
        if (totalAssessmentTime <= assessmentParTime)
        {
            timeFraction = 1f;
        }
        else
        {
            float excess = totalAssessmentTime - assessmentParTime;
            timeFraction = Mathf.Clamp01(assessmentTimeDecayCurve != null ? assessmentTimeDecayCurve.Evaluate(excess) : 0f);
        }
        float timePercent = timeFraction * 30f;
        float finalAssessmentScore = Mathf.Clamp(objectivePercent + timePercent, 0f, 100f);

        // Display results
        if (combinedResultsPanel != null)
        {
            combinedResultsPanel.SetActive(true);

            // Header only
            if (combinedScoreText != null)
                combinedScoreText.text = "Assessment Scores";

            // Optional: you can still show levels completed if desired
            // if (completedLevelsText != null)
            //     completedLevelsText.text = $"Levels Completed: {completedLevelCount}/{levelCompletionStatus.Count}";

            if (totalTimeText != null)
                totalTimeText.text = $"Time Taken: {System.TimeSpan.FromSeconds(totalAssessmentTime):mm\\:ss\\.ff}";

            if (objectivesPercentText != null)
                objectivesPercentText.text = $"Objective Scoring: {objectivePercent:F2}%";

            if (timePercentText != null)
                timePercentText.text = $"Time Scoring: {timePercent:F2}%";

            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {finalAssessmentScore:F2}%";
        }

        Debug.Log($"📊 Final Assessment: Score={finalAssessmentScore:F2}% | Obj={objectivePercent:F2}% | Time={timePercent:F2}% | TotalTime={totalAssessmentTime:F2}s");

        // Record assessment progress to Firebase
        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            UserSessionData.Instance.UpdateAssessmentLevelProgress(levelId, true, finalAssessmentScore);
        }
    }

    /// <summary>
    /// Enables or disables player controls.
    /// </summary>
    private void EnablePlayerControls(bool enable)
    {
        if (player == null) return;

        var move = player.GetComponent<PlayerMovement>();
        if (move != null)
        {
            move.enabled = enable;
            move.canMove = enable; // Also set canMove to ensure camera rotation is disabled
        }

        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enable;
    }
}

/// <summary>
/// Watches for AbstractObjectiveManager.OnLevelFinished events.
/// </summary>
public static class ObjectiveLevelEndWatcher
{
    private static readonly System.Collections.Generic.Dictionary<AbstractObjectiveManager, ObjectiveEndProxy> proxies =
        new System.Collections.Generic.Dictionary<AbstractObjectiveManager, ObjectiveEndProxy>();

    public static void Attach(AbstractObjectiveManager manager, System.Action<float, float> callback)
    {
        if (manager == null || callback == null) return;

        if (proxies.ContainsKey(manager)) return;
        var proxy = manager.gameObject.AddComponent<ObjectiveEndProxy>();
        proxy.Initialize(manager, callback);
        proxies[manager] = proxy;
    }

    public static void Detach(AbstractObjectiveManager manager)
    {
        if (manager == null) return;
        if (proxies.TryGetValue(manager, out var proxy))
        {
            Object.Destroy(proxy);
            proxies.Remove(manager);
        }
    }
}

public class ObjectiveEndProxy : MonoBehaviour
{
    private AbstractObjectiveManager manager;
    private System.Action<float, float> callback;

    public void Initialize(AbstractObjectiveManager m, System.Action<float, float> cb)
    {
        manager = m;
        callback = cb;
        manager.OnLevelFinished += HandleEnd;
    }

    private void HandleEnd(float score, float time)
    {
        callback?.Invoke(score, time);
    }

    private void OnDestroy()
    {
        if (manager != null)
            manager.OnLevelFinished -= HandleEnd;
    }
    
    
}
