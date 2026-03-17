using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public abstract class AbstractObjectiveManager : MonoBehaviour
{
    [Header("Level Settings")]
    public string levelType;

    [Tooltip("If true, this level will NOT auto-start (used for CombinedLevelManager).")]
    public bool manualStart = false;

    /// <summary>
    /// When true, this level is part of an assessment and should NOT record training level progress.
    /// </summary>
    public bool isAssessmentMode = false;

    [Header("Timer Settings")]
    public bool useTimer = true;

    public event System.Action<float, float> OnLevelFinished;

    [Header("Time Scoring Settings")]
    [Tooltip("Time (in seconds) considered 'par' – full score if completed before this.")]
    public float parTime = 30f;
    [Tooltip("Curve that defines how score drops after going over par time. X = seconds over par, Y = % of max time score (0–1).")]
    public AnimationCurve timeDecayCurve = AnimationCurve.EaseInOut(0, 1, 30, 0);

    [Header("UI Settings")]
    public bool showTutorials = true;

    [Tooltip("Reference to the Results Panel GameObject (only used for standalone levels).")]
    [SerializeField] private GameObject resultsPanel;

    [Header("Debug Settings")]
    public bool uiDebugMode = false;
    public GameObject debugUIGroup;

    [Header("Objective Tracking")]
    public List<BaseObjective> allObjectives = new List<BaseObjective>();

    protected float elapsedTime;
    protected bool levelActive = false;
    protected BaseObjective currentObjective;
    protected Dictionary<string, BaseObjective> objectiveMap = new Dictionary<string, BaseObjective>();

    protected abstract void OnLevelStart();
    protected abstract void OnLevelEnd(float totalScore, float totalTime);
    
    protected bool _levelFailed = false;

    protected virtual void Awake()
    {
        if (debugUIGroup != null)
            debugUIGroup.SetActive(uiDebugMode);
    }

    protected virtual void Start()
    {
        BuildObjectiveMap();

        if (!manualStart)
            StartLevel();
    }

    protected virtual void Update()
    {
        if (levelActive && useTimer)
            elapsedTime += Time.deltaTime;
    }

    private void BuildObjectiveMap()
    {
        objectiveMap.Clear();

        for (int i = 0; i < allObjectives.Count; i++)
        {
            var obj = allObjectives[i];
            if (obj != null && !string.IsNullOrEmpty(obj.objectiveName))
            {
                obj.Manager = this;
                objectiveMap[obj.objectiveName] = obj;
            }
        }

        // 🔧 Auto-link fallback for missing nextObjectiveName
        for (int i = 0; i < allObjectives.Count - 1; i++)
        {
            var obj = allObjectives[i];
            if (obj != null && string.IsNullOrEmpty(obj.nextObjectiveName))
            {
                obj.nextObjectiveName = allObjectives[i + 1].objectiveName;
            }
        }
    }

    public void StartFirstObjective()
    {
        var startingObjective = allObjectives.FirstOrDefault(o => o != null && o.isStartingObjective);
        if (startingObjective != null)
            StartObjective(startingObjective);
        else if (allObjectives.Count > 0)
            StartObjective(allObjectives[0]); // fallback
    }

    public void StartObjective(BaseObjective obj)
    {
        if (obj == null) return;

        currentObjective = obj;
        levelActive = true;
        obj.BeginObjective();
        Debug.Log($"▶ Starting objective: {obj.objectiveName}");
    }

    public void StartObjectiveByName(string name)
    {
        if (objectiveMap.TryGetValue(name, out BaseObjective obj))
            StartObjective(obj);
        else
            Debug.LogWarning($"⚠ Objective name '{name}' not found in {gameObject.name}");
    }

    public void OnObjectiveComplete(BaseObjective completedObjective)
    {
        if (completedObjective == null) return;

        Debug.Log($"✅ Objective complete: {completedObjective.objectiveName}");

        if (!string.IsNullOrEmpty(completedObjective.nextObjectiveName) &&
            objectiveMap.TryGetValue(completedObjective.nextObjectiveName, out BaseObjective nextObjective))
        {
            Debug.Log($"➡ Next objective: {nextObjective.objectiveName}");
            StartObjective(nextObjective);
        }
        else
        {
            Debug.LogWarning($"🏁 No next objective found after '{completedObjective.objectiveName}'. Ending level.");
            EndLevel();
        }
    }

    public void EndLevel()
    {
        levelActive = false;
        currentObjective = null;

        float rawObjectiveScore = allObjectives.Where(o => o != null).Sum(o => o.GetScore());
        int objectiveCount = allObjectives.Count(o => o != null);

        float objectiveScorePercent = 0f;
        if (objectiveCount > 0)
            objectiveScorePercent = rawObjectiveScore / (objectiveCount * 100f);

        objectiveScorePercent *= 70f;
        float timeScorePercent = CalculateTimeScore(elapsedTime) * 30f;
        float totalScore = Mathf.Clamp(objectiveScorePercent + timeScorePercent, 0f, 100f);

        Debug.Log($"🏁 Level '{gameObject.name}' complete. Total Score: {totalScore:F2}%, Time: {elapsedTime:F2}s");

        // 🔧 Detect if part of a combined assessment level
        CombinedLevelManager combined = FindFirstObjectByType<CombinedLevelManager>();

        if (combined != null && combined.isActiveAndEnabled)
        {
            Debug.Log("ℹ Skipping results panel (part of combined level).");
            OnLevelFinished?.Invoke(totalScore, elapsedTime);
            return;
        }

        // ✅ Standalone behavior (show results panel)
        OnLevelFinished?.Invoke(totalScore, elapsedTime);

        // Show results panel if assigned
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            if (PromptManager.SafeInstance != null)
                PromptManager.SafeInstance.ShowTimedPrompt($"Level Complete! Score: {totalScore:F1}%", 3f);
        }
        else
        {
            Debug.LogWarning("⚠ No Results Panel assigned to AbstractObjectiveManager.");
        }

        OnLevelEnd(totalScore, elapsedTime);
    }

    public float GetElapsedTime() => elapsedTime;

    public float CalculateTimeScore(float timeTaken)
    {
        if (timeTaken <= parTime)
            return 1f;

        float excess = timeTaken - parTime;
        return Mathf.Clamp01(timeDecayCurve.Evaluate(excess));
    }

    protected BaseObjective FindObjective(string objectiveName)
    {
        return allObjectives.FirstOrDefault(o => o.objectiveName == objectiveName);
    }

    public BaseObjective GetCurrentObjective() => currentObjective;

    public void StartLevel()
    {
        elapsedTime = 0f;
        levelActive = false;
        currentObjective = null;

        OnLevelStart();

        BuildObjectiveMap(); // ensure latest map

        StartFirstObjective();
    }
    
    public void ForceEndLevelWithFailure()
    {
        if (!levelActive) return;

        _levelFailed = true;
        EndLevel();
    }
    
    public void QuitLevel()
    {
        levelActive = false;
        Time.timeScale = 1f;
        OnLevelQuit();
    }

    protected abstract void OnLevelQuit();
}
