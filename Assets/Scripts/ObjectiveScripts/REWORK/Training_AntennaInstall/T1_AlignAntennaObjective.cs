using UnityEngine;
using TMPro;

public class T1_AlignAntennaObjective : BaseObjective
{
    [Header("Objective 2: Alignment")]
    public AntennaAlignment antenna;
    public float alignmentCompletionThreshold = 90f;
    public float alignmentMaxScoreThreshold = 94f;

    private float finalAlignmentScore = 0f;
    private bool isReadyToComplete = false;

    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective 2 (Align Antenna) started.");
        if (objectiveDisplay != null) objectiveDisplay.text = objectiveHint;

        if (antenna != null)
        {
            antenna.StartUpdatingScoreDisplay();
            // Immediate evaluation in case the antenna was already placed and aligned
            finalAlignmentScore = antenna.CalculateAlignmentScore();
            if (finalAlignmentScore >= alignmentCompletionThreshold)
            {
                float scorePercent = CalculateObjective2ScorePercent(finalAlignmentScore);
                SetScore(scorePercent);
                CompleteObjective();
                return;
            }
        }
        else
        {
            Debug.LogError("AntennaAlignment reference is missing!", this);
            CompleteObjective(); // fail gracefully
        }

        // Listen for placements
        GameplayEvents.ObjectPlaced += HandleObjectPlaced;
    }

    private void Update()
    {
        if (!isActive || isCompleted || antenna == null) return;

        finalAlignmentScore = antenna.CalculateAlignmentScore();

        Debug.Log($"[AlignObjective] IsActive={isActive}, Score={finalAlignmentScore:F2}, Threshold={alignmentCompletionThreshold}");

        // Only flag readiness, don't complete yet
        if (finalAlignmentScore >= alignmentCompletionThreshold)
        {
            isReadyToComplete = true;
        }
        else
        {
            isReadyToComplete = false;
        }
    }

    private void HandleObjectPlaced(GameObject placedObject)
    {
        if (!isActive || isCompleted) return;

        // Check if the placed object is THIS antenna
        if (placedObject == antenna.gameObject)
        {
            // Re-evaluate alignment immediately at placement time so first-time aligned placement completes the objective
            if (antenna != null)
            {
                finalAlignmentScore = antenna.CalculateAlignmentScore();
            }

            if (finalAlignmentScore >= alignmentCompletionThreshold)
            {
                Debug.Log("<color=lime><b>Objective 2 completed: antenna placed with good alignment!</b></color>");
                float scorePercent = CalculateObjective2ScorePercent(finalAlignmentScore);
                SetScore(scorePercent);
                CompleteObjective();
            }
        }
    }

    protected override void OnObjectiveComplete()
    {
        Debug.Log($"Objective 2: Align Antenna - COMPLETE! Final Alignment Score: {finalAlignmentScore:F2}");
        GameplayEvents.ObjectPlaced -= HandleObjectPlaced;

        if (antenna != null && antenna.gameObject.CompareTag("canPickUp"))
        {
            antenna.gameObject.tag = "Untagged";
            Debug.Log("Antenna tag changed to 'Untagged'. Cannot be picked up.");
        }
    }

        private float CalculateObjective2ScorePercent(float currentAlignmentScore)
    {
        // Just return the raw alignment value clamped between 0–100
        return Mathf.Clamp(currentAlignmentScore, 0f, 100f);
    }
}
