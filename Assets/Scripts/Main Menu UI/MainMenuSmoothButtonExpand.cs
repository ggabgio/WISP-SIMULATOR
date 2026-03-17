using UnityEngine;
using System.Collections.Generic;

public class SmoothButtonExpand : MonoBehaviour
{
    [System.Serializable]
    public class ButtonData
    {
        public RectTransform rect;
        [HideInInspector] public float targetWidth;
        [HideInInspector] public float targetHeight;
    }

    public List<ButtonData> buttons = new List<ButtonData>();

    [Header("Sizes")]
    public float normalWidth = 100f;
    public float expandedWidth = 200f;
    public float normalHeight = 50f;
    public float expandedHeight = 100f;

    [Header("Layout")]
    public float spacingX = 10f;
    public float spacingY = 10f;
    public float leftPadding = 20f;
    public float topPadding = -20f; // negative for downward movement
    public int buttonsPerRow = 3;   // how many buttons before wrapping
    public float speed = 10f;

    private int hoveredIndex = -1;

    void Start()
    {
        foreach (var b in buttons)
        {
            b.targetWidth = normalWidth;
            b.targetHeight = normalHeight;
        }
    }

    void Update()
    {
        // Assign target sizes
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i == hoveredIndex)
            {
                buttons[i].targetWidth = expandedWidth;
                buttons[i].targetHeight = expandedHeight;
            }
            else
            {
                buttons[i].targetWidth = normalWidth;
                buttons[i].targetHeight = normalHeight;
            }
        }

        // Row layout with dynamic row heights
        float y = topPadding;
        int countInRow = 0;
        float x = leftPadding;

        float currentRowMaxHeight = 0f; // tallest button in current row

        for (int i = 0; i < buttons.Count; i++)
        {
            var b = buttons[i];

            // Smoothly lerp width + height
            float newWidth = Mathf.Lerp(b.rect.sizeDelta.x, b.targetWidth, Time.deltaTime * speed);
            float newHeight = Mathf.Lerp(b.rect.sizeDelta.y, b.targetHeight, Time.deltaTime * speed);
            b.rect.sizeDelta = new Vector2(newWidth, newHeight);

            // Track tallest button in this row
            if (newHeight > currentRowMaxHeight)
                currentRowMaxHeight = newHeight;

            // Place button
            Vector2 targetPos = new Vector2(x + newWidth / 2f, y - newHeight / 2f);
            b.rect.anchoredPosition = Vector2.Lerp(b.rect.anchoredPosition, targetPos, Time.deltaTime * speed);

            // Advance to next column
            x += newWidth + spacingX;
            countInRow++;

            // If row is filled -> reset X, move Y down by tallest row height
            if (countInRow >= buttonsPerRow)
            {
                countInRow = 0;
                x = leftPadding;
                y -= currentRowMaxHeight + spacingY; // push down based on tallest button
                currentRowMaxHeight = 0f; // reset for next row
            }
        }
    }

    public void SetHovered(int index) => hoveredIndex = index;
    public void ClearHovered() => hoveredIndex = -1;
}
