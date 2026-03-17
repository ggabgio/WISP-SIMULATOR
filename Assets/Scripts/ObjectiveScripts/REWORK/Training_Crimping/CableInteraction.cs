// --- START OF REVISED FILE CableInteraction.txt ---
using UnityEngine;
using System.Collections; // Added for IEnumerator

public class EthernetCableInteraction : MonoBehaviour
{
    public GameObject interactionPrompt;
    public GameObject ethernetMiniGameObjects; // Parent of all minigame elements, including DragObjects
    public GameObject hotbarUI;

    public PlayerMovement playerMovementScript;

    [Header("Camera Objects")]
    [Tooltip("Player's main FPS camera GameObject.")]
    public GameObject mainCamObj;
    [Tooltip("The general camera GameObject for the Ethernet minigame view.")]
    public GameObject minigameCamObj; // This is the one DragObjects should reference

    public bool inMinigamePublic => inMinigame;
    private bool inMinigame = false;
    private bool isPlayerNearCable = false;

    void Start()
    {
        interactionPrompt?.SetActive(false);
        ethernetMiniGameObjects?.SetActive(false);
        hotbarUI?.SetActive(true);

        if (minigameCamObj != null) minigameCamObj.SetActive(false);
        else Debug.LogError("EthernetCableInteraction: Minigame Camera Object (minigameCamObj) not assigned!", this);

        if (mainCamObj != null) mainCamObj.SetActive(true);
        else Debug.LogError("EthernetCableInteraction: Main Camera Object (mainCamObj) not assigned!", this);
    }

    void Update()
    {
        if (isPlayerNearCable && InventoryManager.Instance.IsCrimpingToolEquipped())
        {
            interactionPrompt?.SetActive(true);

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (!inMinigame) EnterMinigame();
                else ExitMinigame();
            }
        }
        else
        {
            interactionPrompt?.SetActive(false);
        }

        if (inMinigame && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMinigame();
        }

        // Ensure cursor stays visible while minigame is active
        // This prevents other scripts from hiding the cursor when clicking objects
        if (inMinigame)
        {
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void EnterMinigame()
    {
        if (mainCamObj != null) mainCamObj.SetActive(false);
        if (minigameCamObj != null) minigameCamObj.SetActive(true);

        if (ethernetMiniGameObjects != null) ethernetMiniGameObjects.SetActive(true);
        else Debug.LogError("EthernetCableInteraction: Ethernet Minigame Objects (Parent) not assigned!", this);

        hotbarUI?.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovementScript != null) playerMovementScript.canMove = false;

        inMinigame = true;
        Debug.Log("Entered Ethernet Minigame.");

        // Assign the minigame camera to DragObjects *after* they are activated
        // This is important if DragObjects are children of ethernetMiniGameObjects
        AssignCameraToMinigameDragObjects();

        // Notify CrimpingManager to activate the prompts UI
        NotifyCrimpingManagerEnterMinigame();
    }

    private void AssignCameraToMinigameDragObjects()
    {
        if (minigameCamObj == null || ethernetMiniGameObjects == null) return;
        Camera mgCam = minigameCamObj.GetComponent<Camera>();
        if (mgCam == null)
        {
            Debug.LogError("EthernetCableInteraction: minigameCamObj does not have a Camera component!", minigameCamObj);
            return;
        }

        // Find DragObjects that are children of the activated minigame parent
        DragObject[] dragObjects = ethernetMiniGameObjects.GetComponentsInChildren<DragObject>(true);
        foreach (DragObject dragger in dragObjects)
        {
            // The DragObject script now has a public 'minigameCamera' field.
            // We assign our active minigame camera to it.
            if (dragger.minigameCamera == null) // Assign if not already set (e.g. by inspector on prefab)
            {
                 dragger.minigameCamera = mgCam;
                 Debug.Log($"Assigned {mgCam.name} to DragObject {dragger.name}");
            }
            else if (dragger.minigameCamera != mgCam)
            {
                Debug.LogWarning($"DragObject {dragger.name} already had a camera '{dragger.minigameCamera.name}' assigned. Overwriting with {mgCam.name}. Check inspector settings if this is unintended.");
                dragger.minigameCamera = mgCam;
            }
        }
    }


    public void ExitMinigame()
    {
        if (minigameCamObj != null) minigameCamObj.SetActive(false);
        if (mainCamObj != null) mainCamObj.SetActive(true);

        ethernetMiniGameObjects?.SetActive(false);
        hotbarUI?.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovementScript != null) playerMovementScript.canMove = true;

        inMinigame = false;
        Debug.Log("Exited Ethernet Minigame.");

        // Notify CrimpingManager to deactivate the prompts UI
        NotifyCrimpingManagerExitMinigame();
    }

    private void NotifyCrimpingManagerEnterMinigame()
    {
        // Find the CrimpingManager in the scene
        CrimpingManager crimpingManager = FindFirstObjectByType<CrimpingManager>();
        if (crimpingManager != null)
        {
            crimpingManager.OnEnterMinigame();
        }
    }

    private void NotifyCrimpingManagerExitMinigame()
    {
        // Find the CrimpingManager in the scene
        CrimpingManager crimpingManager = FindFirstObjectByType<CrimpingManager>();
        if (crimpingManager != null)
        {
            crimpingManager.OnExitMinigame();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearCable = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearCable = false;
            interactionPrompt?.SetActive(false);
        }
    }
}
// --- END OF REVISED FILE CableInteraction.txt ---