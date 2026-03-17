// --- START OF REVISED FILE PickUpScript.cs ---
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    public float pickUpRange = 3f;
    public Camera playerCamera;

    // State variable to track if the pickup prompt is currently displayed
    private bool isPickupPromptShowing = false;
    // Store the pickup prompt message for easy comparison
    private const string PickupPromptMessage = "Press E to Pick Up";

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        if (playerCamera == null)
        {
            Debug.LogError("PickUpScript needs a Camera assigned or a Camera tagged 'MainCamera'!", this);
            this.enabled = false;
        }
    }

    void Update()
    {
        CheckForPickupableObject(); // Check every frame if we should show the prompt

        if (Input.GetKeyDown(KeyCode.E)) // Only attempt pickup when E is pressed
        {
            AttemptPickup();
        }
    }

    // Checks if the player is looking at a pickupable object and updates the prompt
    void CheckForPickupableObject()
    {
        bool canPickupThisFrame = false; // Assume no pickupable object is in view initially

        RaycastHit hit;
        // Ensure playerCamera is not null before using it
        if (playerCamera != null && Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickUpRange))
        {
            // Check if the hit object is potentially pickupable
            if (hit.collider.CompareTag("canPickUp"))
            {
                // Verify it has a valid ItemInstance up the hierarchy
                if (FindItemInstanceObject(hit.transform) != null)
                {
                    canPickupThisFrame = true; // Conditions met to show the prompt
                }
            }
        }

        // --- Manage Prompt Display via PromptManager ---
        // Feed the prompt system instead of writing directly to the UI
        if (canPickupThisFrame && PromptManager.Instance != null)
        {
            // Priority 1 keeps it below any critical/timed prompts the manager may show
            PromptManager.Instance.RequestPrompt(this, PickupPromptMessage, 1);
            isPickupPromptShowing = true;
        }
        else
        {
            // We simply don't request a prompt; PromptManager will clear/hold as designed
            if (isPickupPromptShowing)
            {
                isPickupPromptShowing = false;
            }
        }
    }


    void AttemptPickup()
    {
        RaycastHit hit;

        // Re-do the raycast specifically for the pickup action
        if (playerCamera != null && Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickUpRange))
        {
            if (hit.collider.CompareTag("canPickUp"))
            {
                GameObject objectToPickup = FindItemInstanceObject(hit.transform);

                if (objectToPickup != null)
                {
                    Debug.Log($"Attempting to pick up item: {objectToPickup.name}");
                    ItemInstance instance = objectToPickup.GetComponent<ItemInstance>(); // Already verified by FindItemInstanceObject

                    if (instance.data != null)
                    {
                        bool wasPickedUp = InventoryManager.Instance.AddItem(objectToPickup, instance.data);

                        if (wasPickedUp)
                        {
                            // --- Let PromptManager resolve prompt state on next LateUpdate ---
                            isPickupPromptShowing = false;


                            LayerSwitcher layerSwitcher = objectToPickup.GetComponent<LayerSwitcher>();
                            if (layerSwitcher != null)
                            {
                                layerSwitcher.SwitchToHoldLayer();
                            }
                            else
                            {
                                Debug.LogWarning($"Picked up item {objectToPickup.name} is missing LayerSwitcher component.");
                            }


                            if (AudioManager.Instance != null)
                            {
                                AudioManager.Instance.PlayPickUp();
                            }
                        }
                        else
                        {
                            // AddItem failed (Inventory full, etc.) - Show specific failure prompt?
                             if(PromptManager.Instance != null) // Check if prompt manager exists
                             {
                                 // Use ShowPrompt for temporary failure messages
                                 PromptManager.Instance.RequestPrompt(this, "Cannot pick up item (Inventory full or holding large item?)", 2);
                             }
                             Debug.Log("Pickup failed — inventory might be full or you're holding a big item.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"ItemInstance found on {objectToPickup.name}, but its ItemData is not assigned.", objectToPickup);
                        if(PromptManager.Instance != null) PromptManager.Instance.RequestPrompt(this, "Cannot pick up item (Item data missing)", 2);
                    }
                }
                // No need for an else here, CheckForPickupableObject handles the case where no ItemInstance is found
            }
            // No need for an else here, CheckForPickupableObject handles hiding the prompt if the raycast misses or hits wrong tag
        }
        // No need for an else here, CheckForPickupableObject handles hiding the prompt if the raycast misses
    }

    // Finds the nearest parent (or self) with an ItemInstance component
    GameObject FindItemInstanceObject(Transform startTransform)
    {
        Transform currentTransform = startTransform;
        while (currentTransform != null)
        {
            if (currentTransform.TryGetComponent<ItemInstance>(out ItemInstance itemInstance))
            {
                // Added check: ensure ItemInstance has data assigned
                if (itemInstance.data != null) {
                     return currentTransform.gameObject;
                } else {
                    // Found ItemInstance but its data is null, treat as invalid for pickup prompt/action
                    return null;
                }
            }
            currentTransform = currentTransform.parent;
        }
        return null; // No ItemInstance found in hierarchy
    }

    // --- Optional: Ensure prompt is cleared if the script is disabled ---
    void OnDisable()
    {
         if (isPickupPromptShowing && PromptManager.Instance != null && PromptManager.Instance.promptText != null && PromptManager.Instance.promptText.text == PickupPromptMessage)
         {
             PromptManager.Instance.promptText.text = "";
         }
         isPickupPromptShowing = false; // Reset state on disable
    }
}
// --- END OF REVISED FILE PickUpScript.cs ---