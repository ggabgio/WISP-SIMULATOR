using UnityEngine;

public class Wire : MonoBehaviour
{
    private Renderer wireRenderer;
    private Color originalColor;

    public string initialColorName; 

    public Color highlightColor = Color.yellow;
    private string wireColor; // To store the color of the wire
    private EthernetMinigameManager minigameManager;

    public bool isSelected = false; // To track if the wire is selected

    private void Start()
    {
        wireRenderer = GetComponent<Renderer>();
        if (wireRenderer != null)
        {
            originalColor = wireRenderer.material.color;
        }
        else
        {
            Debug.LogError("Wire script requires a Renderer component!");
        }

        minigameManager = FindFirstObjectByType<EthernetMinigameManager>();
        if (minigameManager == null)
        {
            Debug.LogError("No EthernetMinigameManager found in scene!");
        }

        
    if (!string.IsNullOrEmpty(initialColorName))
    {
        SetWireColor(initialColorName);
    }
    }

    // Set the color of the wire
    public void SetWireColor(string color)
    {
        wireColor = color;
    }

    // Get the color of the wire
    public string GetWireColor()
    {
        return wireColor;
    }

    private void OnMouseEnter()
    {
        if (wireRenderer != null)
        {
            wireRenderer.material.color = highlightColor;
        }
    }

    private void OnMouseExit()
    {
        if (wireRenderer != null && !isSelected)  // Only reset the color if it's not selected
        {
            wireRenderer.material.color = originalColor;
        }
    }

    private void OnMouseDown()
    {
        if (minigameManager != null)
        {
            minigameManager.SelectWire(this);
        }
    }

    public void SwapPosition(Wire otherWire)
    {
        if (otherWire != null)
        {
            // Swap positions in the scene
            Vector3 tempPosition = transform.position;
            transform.position = otherWire.transform.position;
            otherWire.transform.position = tempPosition;

            // Log the swap to confirm
            Debug.Log("Swapped positions of " + name + " and " + otherWire.name);
        }
    }
}
