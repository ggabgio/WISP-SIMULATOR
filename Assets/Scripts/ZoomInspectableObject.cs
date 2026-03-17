using UnityEngine;

public class ZoomInspectableObject : MonoBehaviour
{
    [Header("Camera References")]
    public Camera zoomCamera;             // Camera used for zoom view
    public Camera playerCamera;           // Main gameplay camera
    public Camera minigameCamera;         // Optional MinigameCamera to disable when exiting zoom

    [Header("Player References")]
    public MonoBehaviour playerMovementScript;  // Player movement component (e.g. FirstPersonController)

    [Header("Interaction")]
    public Collider interactionTrigger;

    [HideInInspector]
    public bool playerInside;

    private bool isZoomed = false;

    private void OnEnable()
    {
        if (playerCamera == null)
            playerCamera = Camera.main; // fallback

        if (zoomCamera != null)
            zoomCamera.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    private void Update()
    {
        if (!playerInside)
            return;

        // Enter zoom camera
        if (Input.GetKeyDown(KeyCode.T) && !isZoomed)
        {
            EnterZoomView();
        }

        // Exit zoom camera
        if (Input.GetKeyDown(KeyCode.Y) && isZoomed)
        {
            ExitZoomView();
        }
    }

    public void EnterZoomView()
    {
        if (zoomCamera == null) return;

        // Disable player control and camera
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Activate zoom camera
        zoomCamera.gameObject.SetActive(true);

        // Show cursor for zoom/minigame
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isZoomed = true;

        // Notify CrimpingManager to activate the prompts UI
        NotifyCrimpingManagerEnterZoom();
    }

    public void ExitZoomView()
    {
        if (zoomCamera != null)
            zoomCamera.gameObject.SetActive(false);

        if (minigameCamera != null && minigameCamera.gameObject.activeSelf)
            minigameCamera.gameObject.SetActive(false);

        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // 🔒 Reset cursor back to gameplay mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isZoomed = false;

        // Notify CrimpingManager to deactivate the prompts UI
        NotifyCrimpingManagerExitZoom();

        // Also exit inspect mode in InspectController so player doesn't need to press T again
        if (InspectController.Instance != null)
        {
            InspectController.Instance.ForceExitInspect();
            Debug.Log("[ZoomInspectableObject] Exited inspect mode in InspectController.");
        }
    }

    private void NotifyCrimpingManagerEnterZoom()
    {
        // Find the CrimpingManager in the scene
        CrimpingManager crimpingManager = FindFirstObjectByType<CrimpingManager>();
        if (crimpingManager != null)
        {
            crimpingManager.OnEnterMinigame();
        }
    }

    private void NotifyCrimpingManagerExitZoom()
    {
        // Try to find and use the manager first (including inactive ones)
        CrimpingManager crimpingManager = FindFirstObjectByType<CrimpingManager>();
        
        // If not found, try FindObjectOfType as fallback (includes inactive)
        if (crimpingManager == null)
        {
            crimpingManager = FindObjectOfType<CrimpingManager>(true); // true = include inactive
        }
        
        if (crimpingManager != null)
        {
            crimpingManager.OnExitMinigame();
            Debug.Log("[ZoomInspectableObject] Notified CrimpingManager to deactivate prompts UI.");
        }
        else
        {
            // Fallback: use static method that can find the UI even if manager is disabled or inactive
            Debug.Log("[ZoomInspectableObject] CrimpingManager not found, using static fallback method.");
            CrimpingManager.DeactivateCrimpingUI();
        }
    }
}
