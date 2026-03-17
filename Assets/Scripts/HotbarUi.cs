using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    public TextMeshProUGUI handsFullText;
    public Image[] slotBackgrounds;
    public Image[] itemIcons;
    public TextMeshProUGUI[] slotNumbers;

    public Color normalColor = Color.gray;
    public Color selectedColor = Color.white;

    // Removed unused variables:
    // public int emptyFontSize = 36;
    // public int filledFontSize = 16;
    // public Vector2 emptyAnchorPos = new Vector2(0.5f, 0.5f); // Center
    // public Vector2 filledAnchorPos = new Vector2(0, 0); // Bottom-left

    public void UpdateHotbar(InventoryItem[] inventory, int selectedSlot)
    {
        // Input validation
        if (slotBackgrounds == null || itemIcons == null || slotNumbers == null || inventory == null)
        {
            Debug.LogError("HotbarUI is missing references to its UI elements or inventory!");
            return;
        }
        if (slotBackgrounds.Length != itemIcons.Length || slotBackgrounds.Length != slotNumbers.Length || slotBackgrounds.Length != inventory.Length)
        {
             Debug.LogError("HotbarUI element arrays (backgrounds, icons, numbers) or inventory array size mismatch!");
             return;
        }


        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            // Ensure UI elements exist for this index
            if (slotBackgrounds[i] == null || itemIcons[i] == null || slotNumbers[i] == null)
            {
                Debug.LogWarning($"HotbarUI is missing assigned UI elements for slot index {i}");
                continue; // Skip this slot if elements are missing
            }

            // Highlight selected slot
            slotBackgrounds[i].color = (i == selectedSlot) ? selectedColor : normalColor;

            // Show item icon
            bool hasItem = (inventory[i] != null && inventory[i].itemData != null && inventory[i].itemData.icon != null);
            if (hasItem)
            {
                itemIcons[i].sprite = inventory[i].itemData.icon;
                itemIcons[i].enabled = true;
            }
            else
            {
                itemIcons[i].sprite = null; // Clear sprite reference
                itemIcons[i].enabled = false;
            }

            // Slot number text formatting and positioning
            TextMeshProUGUI numberText = slotNumbers[i];
            RectTransform rt = numberText.rectTransform;
            numberText.text = (i + 1).ToString();

            if (hasItem)
            {
                 // Item present: Small number in bottom-left
                 rt.anchorMin = new Vector2(0f, 0f); // Bottom-left anchor
                 rt.anchorMax = new Vector2(0f, 0f);
                 rt.pivot = new Vector2(0f, 0f);    // Pivot at bottom-left
                 rt.anchoredPosition = new Vector2(8f, 8f); // Padding from bottom-left corner (Adjust values as needed)

                 numberText.fontSize = 24; // Smaller font size (Adjust as needed)
                 numberText.alignment = TextAlignmentOptions.BottomLeft;
            }
            else
            {
                 // Slot empty: Large number in center
                 rt.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
                 rt.anchorMax = new Vector2(0.5f, 0.5f);
                 rt.pivot = new Vector2(0.5f, 0.5f);    // Pivot at center
                 rt.anchoredPosition = Vector2.zero;     // Position at center

                 numberText.fontSize = 36; // Larger font size (Adjust as needed)
                 numberText.alignment = TextAlignmentOptions.Center;
            }

            numberText.enabled = true; // Always show the number
        }
    }

    // --- Hands Full Text Methods ---
    // (These methods remain unchanged but benefit from the null check added below)

    public void ShowHandsFullText()
    {
        SetHandsFullText(true);
    }

    public void HideHandsFullText()
    {
        SetHandsFullText(false);
    }

    public void SetHandsFullText(bool visible)
    {
        if (handsFullText != null)
        {
            handsFullText.gameObject.SetActive(visible);
        }
        else
        {
            // Log warning only once if text is missing? Or every time? Logging every time for now.
            Debug.LogWarning("Attempted to set HandsFullText visibility, but the reference is not assigned in HotbarUI.");
        }
    }
}