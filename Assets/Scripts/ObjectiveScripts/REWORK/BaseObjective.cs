using TMPro;
using UnityEngine;

public abstract class BaseObjective : MonoBehaviour
{
    [Header("Objective Settings")]
    public string objectiveName;
    public bool isStartingObjective = false;
    public string nextObjectiveName;
    
    [Header("UI")]
    [Tooltip("The TextMeshPro UI element to display the results on.")]
    public TMP_Text resultsDisplay;
    [Tooltip("The TextMeshPro UI element to display the hint text on.")]
    public TMP_Text objectiveDisplay;
    [TextArea(3, 5)]
    [Tooltip("The hint or instruction for this objective.")]
    public string objectiveHint;
    [Tooltip("Check this to make the hint appear even if 'Show Tutorials' is turned off in the manager.")]
    public bool forceShowHint = false; // This is the new bypass toggle

    [Header("Scoring")]
    public float maxScore = 100f;
    protected float currentScore;

    [HideInInspector] public AbstractObjectiveManager Manager;

    protected bool isActive = false;
    protected bool isCompleted = false;
    
    public bool IsActive => isActive;
    public bool IsCompleted => isCompleted;

    public void BeginObjective()
    {
        if (isCompleted) return;
        isActive = true;
        
        if (objectiveDisplay != null)
        {
            bool shouldShowHint = Manager.showTutorials || forceShowHint;
            if (shouldShowHint)
            {
                objectiveDisplay.text = objectiveHint;
                Debug.Log("Objective Hint Displayed: " + objectiveHint);
            }
        }
        OnObjectiveStart();
    }

    public void CompleteObjective()
    {
        if (!isActive || isCompleted) return;
        isCompleted = true;
        isActive = false;

        OnObjectiveComplete();
        if (resultsDisplay != null) resultsDisplay.text = objectiveName + $" - {currentScore}%";
        if (Manager != null)
            Manager.OnObjectiveComplete(this);
    }

    public float GetScore()
    {
        return Mathf.Clamp(currentScore, 0, maxScore);
    }
    
    protected abstract void OnObjectiveStart();
    protected abstract void OnObjectiveComplete();

    protected void SetScore(float score)
    {
        currentScore = Mathf.Clamp(score, 0, maxScore);
    }
}