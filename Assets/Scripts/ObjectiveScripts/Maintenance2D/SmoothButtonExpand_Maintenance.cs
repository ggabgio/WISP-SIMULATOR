using UnityEngine;
using System.Collections.Generic;

public class SmoothButtonExpand_Maintenance : MonoBehaviour
{
    [System.Serializable]
    public class ButtonLayout
    {
        public RectTransform rect;
        [HideInInspector] public float targetWidth;
        [HideInInspector] public float targetHeight;
    }

    [Header("Buttons")]
    public List<ButtonLayout> buttonList = new List<ButtonLayout>();

    [Header("Sizes")]
    public float normalWidth = 100f;
    public float expandedWidth = 200f;
    public float normalHeight = 50f;
    public float expandedHeight = 100f;

    [Header("Layout Settings")]
    public float spacingX = 10f;
    public float spacingY = 10f;
    public float startX = 0f;   // relative to panel position
    public float startY = 0f;   // relative to panel position
    public int buttonsPerRow = 3;
    public float smoothSpeed = 10f;

    private int hoveredIndex = -1;

    void Start()
    {
        foreach (var b in buttonList)
        {
            if (b.rect == null) continue;
            b.targetWidth = normalWidth;
            b.targetHeight = normalHeight;
            b.rect.sizeDelta = new Vector2(normalWidth, normalHeight);
        }
    }

    void Update()
    {
        if (buttonList.Count == 0) return;

        // Assign hover target sizes
        for (int i = 0; i < buttonList.Count; i++)
        {
            if (buttonList[i].rect == null) continue;

            if (i == hoveredIndex)
            {
                buttonList[i].targetWidth = expandedWidth;
                buttonList[i].targetHeight = expandedHeight;
            }
            else
            {
                buttonList[i].targetWidth = normalWidth;
                buttonList[i].targetHeight = normalHeight;
            }
        }

        // Smoothly interpolate sizes
        foreach (var b in buttonList)
        {
            if (b.rect == null) continue;

            float newW = Mathf.Lerp(b.rect.sizeDelta.x, b.targetWidth, Time.deltaTime * smoothSpeed);
            float newH = Mathf.Lerp(b.rect.sizeDelta.y, b.targetHeight, Time.deltaTime * smoothSpeed);
            b.rect.sizeDelta = new Vector2(newW, newH);
        }

        // Layout computation (panel stays in place)
        float y = startY;
        int index = 0;

        while (index < buttonList.Count)
        {
            int rowCount = Mathf.Min(buttonsPerRow, buttonList.Count - index);
            float totalRowWidth = 0f;
            float tallest = 0f;

            for (int i = 0; i < rowCount; i++)
            {
                var b = buttonList[index + i];
                if (b.rect == null) continue;

                totalRowWidth += b.rect.sizeDelta.x;
                if (i < rowCount - 1)
                    totalRowWidth += spacingX;
                if (b.rect.sizeDelta.y > tallest)
                    tallest = b.rect.sizeDelta.y;
            }

            // Start X so buttons expand left/right relative to startX
            float currentX = startX - (totalRowWidth / 2f);

            for (int i = 0; i < rowCount; i++)
            {
                var b = buttonList[index + i];
                if (b.rect == null) continue;

                float newW = b.rect.sizeDelta.x;
                float newH = b.rect.sizeDelta.y;
                Vector2 targetPos = new Vector2(currentX + newW / 2f, y - newH / 2f);

                b.rect.anchoredPosition = Vector2.Lerp(b.rect.anchoredPosition, targetPos, Time.deltaTime * smoothSpeed);

                currentX += newW + spacingX;
            }

            y -= tallest + spacingY;
            index += rowCount;
        }
    }

    // --- Hover methods used by ButtonHover_Maintenance ---
    public void SetHovered(int index)
    {
        hoveredIndex = index;
    }

    public void ClearHovered()
    {
        hoveredIndex = -1;
    }

    // Used by ButtonHover_Maintenance to find button index
    public int GetButtonIndex(RectTransform rect)
    {
        for (int i = 0; i < buttonList.Count; i++)
        {
            if (buttonList[i].rect == rect)
                return i;
        }
        return -1;
    }
}
