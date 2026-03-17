// --- START OF REVISED FILE CableInteraction.cs ---
using UnityEngine;

public class CableInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.R;
    public float interactDistance = 3f;
    public CableLayingManager manager;
    public CablePlacer cablePlacer;

    private const string InteractAntennaPrompt = "Press [R] to Start Placing Cable";
    private const string NeedCablePrompt = "You need to pick up a Cable first";
    private const string CableItemName = "Cable";
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (manager == null) manager = FindObjectOfType<CableLayingManager>();
    }

    void Update()
    {
        if (cam == null || manager == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.CompareTag("Antenna"))
            {
                // Check if player has a Cable item in inventory
                bool hasCable = InventoryManager.Instance != null && InventoryManager.Instance.HasItem(CableItemName);

                if (hasCable)
                {
                    // Request the prompt from the PromptManager.
                    PromptManager.Instance?.RequestPrompt(this, InteractAntennaPrompt, 1);

                    if (Input.GetKeyDown(interactKey))
                    {
                        var currentObjective = manager.GetCurrentObjective() as T4_StartCablePlacementObjective;
                        if (currentObjective != null && currentObjective.IsActive)
                        {
                            cablePlacer.EnterCableMode(hit.point);
                            currentObjective.NotifyPlacementStarted();
                        }
                    }
                }
                else
                {
                    // Only show the error prompt when player tries to interact without having a cable
                    if (Input.GetKeyDown(interactKey))
                    {
                        // Show timed prompt that stays visible for 3 seconds
                        if (PromptManager.Instance != null)
                        {
                            PromptManager.Instance.ShowTimedPrompt(NeedCablePrompt, 3f);
                        }
                        else if (PromptManager.SafeInstance != null)
                        {
                            PromptManager.SafeInstance.ShowTimedPrompt(NeedCablePrompt, 3f);
                        }
                    }
                }
            }
        }
    }
}