using UnityEngine;
using System.Collections.Generic; // Used for the list

// This struct is now defined in the objective, as it's the source of truth.
[System.Serializable]
public struct RouterPart
{
    [Tooltip("The specific item required for this slot.")]
    public ItemData requiredItemData;
    [Tooltip("The GameObject to enable visually when the item is placed.")]
    public GameObject placementVisual;
}

public class T2_InstallAllRouterPartsObjective : BaseObjective
{
    [Header("Objective Dependencies")]
    [Tooltip("Reference to the RouterHolder in the scene.")]
    [SerializeField] private RouterHolder routerHolder;

    [Header("Configuration")]
    [Tooltip("Define all the parts that need to be installed for this objective.")]
    [SerializeField] private List<RouterPart> partsToInstall = new List<RouterPart>();
    
    private int itemsPlacedCount = 0;

    protected override void OnObjectiveStart()
    {
        if (routerHolder == null)
        {
            Debug.LogError($"Objective '{objectiveName}' cannot find its RouterHolder! Please assign it in the Inspector.", this);
            return;
        }

        itemsPlacedCount = 0;
        
        routerHolder.BeginInstallationObjective(this, partsToInstall);
    }

    // This public method is called by RouterHolder when an item is successfully placed.
    public void NotifyItemPlaced(ItemData placedItem)
    {
        if (!IsActive) return;

        itemsPlacedCount++;
        Debug.Log($"Objective '{objectiveName}' notified. {placedItem.itemName} placed. Progress: {itemsPlacedCount}/{partsToInstall.Count}");

        if (itemsPlacedCount >= partsToInstall.Count)
        {
            SetScore(100f);
            CompleteObjective();
        }
    }

    protected override void OnObjectiveComplete()
    {
        Debug.Log($"Objective '{objectiveName}' complete. All parts installed.");
        if (routerHolder != null)
        {
            // Tell the holder it can stop listening for item placements
            routerHolder.EndInstallationObjective();
        }
    }
}