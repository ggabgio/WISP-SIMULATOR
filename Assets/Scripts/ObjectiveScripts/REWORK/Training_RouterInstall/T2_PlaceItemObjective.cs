using UnityEngine;

public class T2_PlaceItemObjective : BaseObjective
{
    [Header("Placement Settings")]
    [Tooltip("The specific item that must be placed to complete this objective.")]
    public ItemData requiredItem;

    [Header("Objective Order")]
    [Tooltip("If set, this objective will only activate after the previous one is complete.")]
    public T2_PlaceItemObjective prerequisiteObjective;

    private RouterInstallManager L2_Manager;

    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective (Install Object) started.");
        
        if (prerequisiteObjective != null && !prerequisiteObjective.IsCompleted)
        {
            Debug.Log($"Objective '{objectiveName}' blocked. Waiting for prerequisite '{prerequisiteObjective.objectiveName}'.");
            return;
        }

        L2_Manager = Manager as RouterInstallManager;
        if (L2_Manager == null)
        {
            Debug.LogError("L2_PlaceItemObjective requires a Level2_ObjectiveManager!", this);
            return;
        }

        L2_Manager.SetCurrentItemToPlace(requiredItem);
    }

    public void OnItemPlaced()
    {
        if (!IsActive) return;

        Debug.Log($"Objective '{objectiveName}' complete: {requiredItem.itemName} was placed.");
        SetScore(100f); // Full score for correct placement.
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        // Clean up or notify the manager if needed.
        if (L2_Manager != null)
        {
            L2_Manager.SetCurrentItemToPlace(null);
        }
    }
}
