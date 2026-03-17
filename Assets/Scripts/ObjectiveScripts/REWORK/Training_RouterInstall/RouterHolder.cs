using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RouterHolder : MonoBehaviour
{
    [System.Serializable]
    public struct PlacementSlot
    {
        public ItemData requiredItemData;
        public GameObject placementVisual;
        public bool isFilled;
    }

    [Header("Setup")]
    [SerializeField] private KeyCode interactionKey = KeyCode.R;

    private RouterInstallManager manager;
    private InventoryManager playerInventoryManager = null;
    private T2_InstallAllRouterPartsObjective currentObjective;
    private List<PlacementSlot> activeSlots = new List<PlacementSlot>();
    private bool playerInRange = false;
    private bool isInstallationActive = false;
    
    public void Initialize(RouterInstallManager objectiveManager)
    {
        this.manager = objectiveManager;
    }

    public void BeginInstallationObjective(T2_InstallAllRouterPartsObjective objective, List<RouterPart> parts)
    {
        this.currentObjective = objective;
        activeSlots.Clear();

        foreach (var part in parts)
        {
            activeSlots.Add(new PlacementSlot
            {
                requiredItemData = part.requiredItemData,
                placementVisual = part.placementVisual,
                isFilled = false
            });
            if (part.placementVisual != null)
            {
                part.placementVisual.SetActive(false);
            }
        }
        isInstallationActive = true;
    }

    public void EndInstallationObjective()
    {
        isInstallationActive = false;
    }

    void Update()
    {
        if (playerInRange)
        {
            HandlePrompts();
            if (Input.GetKeyDown(interactionKey))
            {
                HandleInteractionInput();
            }
        }
    }
    
    private void HandlePrompts()
    {
        if (isInstallationActive && playerInventoryManager != null)
        {
            ItemData heldItem = playerInventoryManager.GetSelectedItemData();
            if (CanPlaceItem(heldItem))
            {
                string msg = $"Press [{interactionKey}] to Install {heldItem?.itemName ?? "Item"}";
                PromptManager.Instance?.RequestPrompt(this, msg, 3);
            }
        }
    }
    
    private bool CanPlaceItem(ItemData heldItem)
    {
        if (heldItem == null) return false;
        return activeSlots.Any(slot => !slot.isFilled && slot.requiredItemData == heldItem);
    }

    private void HandleInteractionInput()
    {
        if (isInstallationActive)
        {
            AttemptPlacement();
        }
    }

    private void AttemptPlacement()
    {
        if (playerInventoryManager == null || currentObjective == null) return;
        ItemData heldItem = playerInventoryManager.GetSelectedItemData();
        if (heldItem == null) return;

        for (int i = 0; i < activeSlots.Count; i++)
        {
            if (!activeSlots[i].isFilled && activeSlots[i].requiredItemData == heldItem)
            {
                var slot = activeSlots[i];
                slot.isFilled = true;
                activeSlots[i] = slot;

                if (slot.placementVisual != null)
                {
                    slot.placementVisual.SetActive(true);
                }
                playerInventoryManager.DiscardItem();
                
                currentObjective.NotifyItemPlaced(heldItem);
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<InventoryManager>() is InventoryManager inv)
        {
            playerInRange = true;
            playerInventoryManager = inv;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<InventoryManager>() is InventoryManager inv && inv == playerInventoryManager)
        {
            playerInRange = false;
            playerInventoryManager = null;
        }
    }
}