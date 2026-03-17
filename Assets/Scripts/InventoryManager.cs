// --- START OF REVISED FILE InventoryManager.cs ---
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // MARKER: Added for TMP_Text

[System.Serializable]
public class InventoryItem
{
    public GameObject worldObject;
    public ItemData itemData;

    public InventoryItem(GameObject obj, ItemData data)
    {
        worldObject = obj;
        itemData = data;
    }
}

public class InventoryManager : MonoBehaviour
{
    private bool holdingBigItem = false;

    public static InventoryManager Instance;

    [Tooltip("Assign an empty GameObject child of the Player Camera here. MUST have scale (1,1,1).")]
    public Transform holdPos;

    // MARKER: Added itemNameText reference
    [Header("UI References")]
    [Tooltip("Assign a TextMeshPro UI element to display the current equipped item's name.")]
    public TMP_Text itemNameText; // Assign in Inspector

    private InventoryItem[] inventory = new InventoryItem[3];
    public int selectedSlot { get; private set; } = -1;
    public Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private HotbarUI hotbarUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        hotbarUI = FindAnyObjectByType<HotbarUI>();
        if (hotbarUI == null)
        {
            Debug.LogWarning("InventoryManager could not find HotbarUI in the scene!");
        }

        // MARKER: Initial check for itemNameText
        if (itemNameText == null)
        {
            Debug.LogWarning("InventoryManager: ItemNameText (TMP_Text) not assigned in Inspector! Item names will not be displayed.");
        }


        if (selectedSlot == -1)
        {
            for (int i = 0; i < inventory.Length; ++i)
            {
                if (inventory[i] != null)
                {
                    selectedSlot = i;
                    break;
                }
            }
            if (selectedSlot == -1) selectedSlot = 0;
        }
    }

    void Start()
    {
        hotbarUI?.UpdateHotbar(inventory, selectedSlot);
        UpdateEquippedItemNameDisplay(); // MARKER: Initial update for item name display
    }

    private void UpdateHandsFullUI()
    {
        bool inventoryFull = true;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                inventoryFull = false;
                break;
            }
        }
        bool shouldShow = holdingBigItem || inventoryFull;
        hotbarUI?.SetHandsFullText(shouldShow);
    }

    void Update()
    {
        if (!holdingBigItem)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) RequestSlotSelection(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) RequestSlotSelection(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) RequestSlotSelection(2);
        }
        if (Input.GetKeyDown(KeyCode.G)) DropItem();
    }

    public ItemData GetSelectedItemData()
    {
        if (IsValidSlot(selectedSlot) && inventory[selectedSlot] != null)
        {
            return inventory[selectedSlot].itemData;
        }
        return null;
    }

    public bool AddItem(GameObject item, ItemData data)
    {
        if (holdingBigItem)
        {
            Debug.Log("Cannot pick up item while holding a big item.");
            return false;
        }
        int emptySlot = -1;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                if (emptySlot == -1) emptySlot = i;
            }
        }
        if (emptySlot == -1)
        {
            Debug.Log("Inventory is full.");
            return false;
        }
        if (data.isBigItem)
        {
            holdingBigItem = true;
            hotbarUI?.ShowHandsFullText();
        }
        if (!originalScales.ContainsKey(item))
        {
            originalScales[item] = item.transform.localScale;
        }
        inventory[emptySlot] = new InventoryItem(item, data);
        item.SetActive(false);
        UnequipItemVisuals(selectedSlot);
        selectedSlot = emptySlot;
        EquipItem(selectedSlot); // This will call UpdateEquippedItemNameDisplay
        Debug.Log($"Picked up: {item.name} into slot {emptySlot + 1}. Slot selected.");
        hotbarUI?.UpdateHotbar(inventory, selectedSlot);
        UpdateHandsFullUI();
        return true;
    }

    void RequestSlotSelection(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Length || slotIndex == selectedSlot || holdingBigItem) // MARKER: Added holdingBigItem check
            return;

        UnequipItemVisuals(selectedSlot);
        selectedSlot = slotIndex;
        EquipItem(selectedSlot); // This will call UpdateEquippedItemNameDisplay
        Debug.Log($"Selected slot {selectedSlot + 1}");
        hotbarUI?.UpdateHotbar(inventory, selectedSlot);
    }

    public void EquipItem(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || inventory[slotIndex] == null)
        {
            UnequipItemVisuals(selectedSlot); // Unequip current visuals if any
            UpdateEquippedItemNameDisplay(); // MARKER: Update display (will clear if new slot is empty)
            return;
        }
        if (holdPos == null)
        {
            Debug.LogError("HoldPos is not assigned in InventoryManager Inspector!");
            return;
        }
        if (holdPos.localScale != Vector3.one)
        {
            Debug.LogError($"CRITICAL: holdPos '{holdPos.name}' MUST have a scale of (1,1,1). Current: {holdPos.localScale}");
        }
        GameObject equippedItem = inventory[slotIndex].worldObject;
        GameplayEvents.ItemEquipped?.Invoke(equippedItem);
        equippedItem.transform.SetParent(holdPos, false);
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.identity;
        if (originalScales.ContainsKey(equippedItem))
        {
            Vector3 baseScale = originalScales[equippedItem];
            float scaleMultiplier = inventory[slotIndex].itemData != null
                ? inventory[slotIndex].itemData.equipScaleMultiplier
                : 1f;
            equippedItem.transform.localScale = baseScale * scaleMultiplier;
        }
        else
        {
            equippedItem.transform.localScale = Vector3.one;
            Debug.LogError($"Original scale for {equippedItem.name} not found! Defaulting to scale (1,1,1). Was it picked up correctly?");
        }

        if (equippedItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (equippedItem.TryGetComponent<Collider>(out Collider col))
        {
            col.enabled = false;
        }
        equippedItem.SetActive(true);
        UpdateEquippedItemNameDisplay();
    }

    void UnequipItemVisuals(int slotIndex)
    {
        if (IsValidSlot(slotIndex) && inventory[slotIndex] != null)
        {
            GameObject item = inventory[slotIndex].worldObject;
            if (item != null) item.SetActive(false);
        }
    }

    public void DropItem()
    {
        if (IsValidSlot(selectedSlot) && inventory[selectedSlot] != null)
        {
            GameObject itemToDrop = inventory[selectedSlot].worldObject;
            bool wasBigItem = inventory[selectedSlot].itemData.isBigItem;
            inventory[selectedSlot] = null; // Clear the slot
            itemToDrop.transform.SetParent(null);
            if (originalScales.ContainsKey(itemToDrop))
            {
                itemToDrop.transform.localScale = originalScales[itemToDrop];
            }
            if (itemToDrop.TryGetComponent<Collider>(out Collider col)) col.enabled = true;
            if (itemToDrop.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(Camera.main.transform.forward * 2f, ForceMode.Impulse);
            }
            itemToDrop.SetActive(true);
            
            // Try to play drop sound, but don't let audio errors prevent item dropping
            try
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDrop();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"InventoryManager: Failed to play drop sound: {e.Message}");
            }
            
            LayerSwitcher layerSwitcher = itemToDrop.GetComponent<LayerSwitcher>();
            if (layerSwitcher != null) layerSwitcher.SwitchToDefaultLayer();
            else Debug.LogWarning($"Dropped item {itemToDrop.name} is missing LayerSwitcher component.");

            if (wasBigItem) holdingBigItem = false;

            UpdateHandsFullUI();
            hotbarUI?.UpdateHotbar(inventory, selectedSlot);
            UpdateEquippedItemNameDisplay(); // MARKER: Update display (will clear name as slot is now empty)
            Debug.Log("Dropped item from slot " + (selectedSlot + 1));
            
            GameplayEvents.ItemEquipped?.Invoke(null); // Dropped, nothing equipped
        }
    }

    public void ClearEquippedSlot()
    {
        if (IsValidSlot(selectedSlot) && inventory[selectedSlot] != null)
        {
            bool wasBigItem = inventory[selectedSlot].itemData != null && inventory[selectedSlot].itemData.isBigItem;
            inventory[selectedSlot] = null;
            if (wasBigItem) holdingBigItem = false;
            Debug.Log("Cleared inventory slot reference for " + (selectedSlot + 1));
            hotbarUI?.UpdateHotbar(inventory, selectedSlot);
            UpdateHandsFullUI();
            UpdateEquippedItemNameDisplay();
            GameplayEvents.ItemEquipped?.Invoke(null); // No item equipped now
        }
    }

    public GameObject GetEquippedItem()
    {
        if (IsValidSlot(selectedSlot))
        {
            return inventory[selectedSlot]?.worldObject;
        }
        return null;
    }

    bool IsValidSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < inventory.Length;
    }

    public void DiscardItem()
    {
        if (IsValidSlot(selectedSlot) && inventory[selectedSlot] != null)
        {
            bool wasBigItem = inventory[selectedSlot].itemData != null && inventory[selectedSlot].itemData.isBigItem;
            GameObject itemToDiscard = inventory[selectedSlot].worldObject;
            inventory[selectedSlot] = null; // Clear the slot
            if (originalScales.ContainsKey(itemToDiscard))
            {
                originalScales.Remove(itemToDiscard);
            }
            Destroy(itemToDiscard);
            if (wasBigItem) holdingBigItem = false;
            Debug.Log("Item discarded permanently from slot " + (selectedSlot + 1));
            hotbarUI?.UpdateHotbar(inventory, selectedSlot);
            UpdateHandsFullUI();
            UpdateEquippedItemNameDisplay();
        }
    }

    public bool IsCrimpingToolEquipped()
    {
        if (!IsValidSlot(selectedSlot)) return false;
        var item = inventory[selectedSlot];
        return item != null && item.itemData != null && item.itemData.isCrimpingTool;
    }

    // MARKER: New method to update the item name display
    private void UpdateEquippedItemNameDisplay()
    {
        if (itemNameText == null) return;

        if (IsValidSlot(selectedSlot) && inventory[selectedSlot] != null && inventory[selectedSlot].itemData != null)
        {
            itemNameText.text = inventory[selectedSlot].itemData.itemName;
        }
        else
        {
            itemNameText.text = "";
        }
    }

        public bool HasItem(string itemName)
    {
        foreach (var slot in inventory)
        {
            if (slot != null && slot.itemData != null && slot.itemData.itemName == itemName)
                return true;
        }
        return false;
    }

}