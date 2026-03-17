using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectPlacer : MonoBehaviour
{
    [Header("Placement Settings")]
    public Material previewMaterialValid;
    public Material previewMaterialInvalid;
    public float maxPlacementGroundCheckDistance = 10f;
    public float placementForwardOffset = 1.5f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 180f;

    [Header("Required Components")]
    public Camera playerCamera;

    // --- Private Variables ---
    private GameObject previewObjectInstance;
    private MeshRenderer[] previewRenderers;
    private bool isPlacing = false;
    private bool canPlace = false;
    private float currentYRotation = 0f;
    private float placementYOffset = 0f;

    // --- Prompt Management ---
    private const string HoldPromptMessage = "Hold [F] to Start Placing";
    private const string PlacePromptMessage = "[LMB] Place | [RMB] Cancel | [Scroll] Rotate";

    public bool IsPlacing => isPlacing;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) { Debug.LogError("ObjectPlacer requires a Player Camera!", this); enabled = false; return; }
    }

    void Update()
    {
        HandlePlacementInput();
        
        // --- Centralized Prompt Logic ---
        GameObject equippedItem = InventoryManager.Instance?.GetEquippedItem();
        bool isAntenna = IsAntennaItem(equippedItem);
        if (isPlacing)
        {
            PromptManager.Instance?.RequestPrompt(this, PlacePromptMessage, 10);
        }
        else if (equippedItem != null && isAntenna)
        {
            PromptManager.Instance?.RequestPrompt(this, HoldPromptMessage, -10);
        }
    }

    void HandlePlacementInput()
    {
         GameObject equippedItem = InventoryManager.Instance?.GetEquippedItem();
         // Only allow placement if the equipped item is an antenna
         bool isAntenna = IsAntennaItem(equippedItem);
         if (Input.GetKeyDown(KeyCode.F) && equippedItem != null && !isPlacing && isAntenna) { StartPlacing(equippedItem); }
         if (isPlacing) {
             if (Input.GetKey(KeyCode.F)) { UpdatePlacementPreview(); }
             else { StopPlacing(false); }
         }
         if (isPlacing && canPlace && Input.GetMouseButtonDown(0) && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
         { PlaceObject(); }
         if (isPlacing && Input.GetMouseButtonDown(1)) { StopPlacing(false); }
    }

    bool IsAntennaItem(GameObject item)
    {
        if (item == null) return false;
        // Check if the item is tagged as "Antenna"
        if (item.CompareTag("Antenna")) return true;
        // Check if the item's name contains "Antenna" (case-insensitive)
        if (item.name.ToLower().Contains("antenna")) return true;
        return false;
    }
    
    void StartPlacing(GameObject itemToPlace)
    {
        isPlacing = true;
        currentYRotation = 0f;
        placementYOffset = 0f;

        if (previewObjectInstance != null) Destroy(previewObjectInstance);
        previewObjectInstance = Instantiate(itemToPlace);
        previewObjectInstance.SetActive(false);
        if (previewObjectInstance.TryGetComponent<Rigidbody>(out var rb)) Destroy(rb);
        foreach (Collider col in previewObjectInstance.GetComponentsInChildren<Collider>(true)) col.enabled = false;
        previewRenderers = previewObjectInstance.GetComponentsInChildren<MeshRenderer>(true);
        if (previewRenderers.Length == 0) { StopPlacing(false); return; }
        foreach (var rend in previewRenderers) rend.material = previewMaterialInvalid;
        Transform contactPoint = previewObjectInstance.transform.Find("PlacementContactPoint");
        placementYOffset = (contactPoint != null) ? contactPoint.localPosition.y : 0f;
        previewObjectInstance.SetActive(true);
        UpdatePlacementPreview();
    }

     void UpdatePlacementPreview()
    {
        if (previewObjectInstance == null || playerCamera == null) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
            SetPreviewState(false, playerCamera.transform.position + playerCamera.transform.forward * placementForwardOffset, CalculatePlacementRotation()); return;
        }
        Quaternion finalRotation = CalculatePlacementRotation();
        PlacementResult placementResult = CalculatePlacementPosition(finalRotation);
        SetPreviewState(placementResult.IsValid, placementResult.Position, finalRotation);
    }

     Quaternion CalculatePlacementRotation()
    {
        Vector3 playerForwardHorizontal = playerCamera.transform.forward; playerForwardHorizontal.y = 0; playerForwardHorizontal.Normalize();
        Quaternion baseRotation = (playerForwardHorizontal != Vector3.zero) ? Quaternion.LookRotation(playerForwardHorizontal, Vector3.up) : Quaternion.identity;
        float scrollInput = Input.GetAxis("Mouse ScrollWheel"); currentYRotation += scrollInput * rotationSpeed * Time.deltaTime;
        Quaternion playerRotation = Quaternion.Euler(0, currentYRotation, 0);
        return baseRotation * playerRotation;
    }

    private struct PlacementResult { public bool IsValid; public Vector3 Position; }
     PlacementResult CalculatePlacementPosition(Quaternion finalRotation)
    {
        Vector3 playerBasePos = transform.position; Vector3 playerForwardHorizontal = playerCamera.transform.forward; playerForwardHorizontal.y = 0; playerForwardHorizontal.Normalize();
        Vector3 horizontalOffset = playerForwardHorizontal * placementForwardOffset; Vector3 groundCheckOrigin = playerBasePos + horizontalOffset + (Vector3.up * 0.1f);
        bool groundFound = Physics.Raycast(groundCheckOrigin, Vector3.down, out RaycastHit groundHit, maxPlacementGroundCheckDistance);
        PlacementResult result = new PlacementResult { IsValid = groundFound };
        if (groundFound) { Vector3 previewWorldUp = finalRotation * Vector3.up; Vector3 offsetAdjustment = previewWorldUp * placementYOffset; result.Position = groundHit.point - offsetAdjustment; }
        else { result.Position = playerCamera.transform.position + playerCamera.transform.forward * placementForwardOffset; }
        return result;
    }

    void SetPreviewState(bool isValid, Vector3 position, Quaternion rotation)
    {
        if (previewObjectInstance == null) return;
        canPlace = isValid; previewObjectInstance.transform.position = position; previewObjectInstance.transform.rotation = rotation;
        Material materialToApply = isValid ? previewMaterialValid : previewMaterialInvalid;
        if (previewRenderers != null) { foreach (var rend in previewRenderers) { if (rend != null) { rend.material = materialToApply; } } }
    }

    void PlaceObject()
    {
        GameObject itemToPlace = InventoryManager.Instance?.GetEquippedItem();
        if (itemToPlace == null) { StopPlacing(false); return; }
        // Safety check: ensure only antennas can be placed
        if (!IsAntennaItem(itemToPlace)) { StopPlacing(false); return; }
        itemToPlace.transform.SetParent(null);
        itemToPlace.transform.position = previewObjectInstance.transform.position;
        itemToPlace.transform.rotation = previewObjectInstance.transform.rotation;
        if (InventoryManager.Instance != null && InventoryManager.Instance.originalScales.ContainsKey(itemToPlace))
             { itemToPlace.transform.localScale = InventoryManager.Instance.originalScales[itemToPlace]; }
        if (itemToPlace.TryGetComponent<Rigidbody>(out Rigidbody rb)) { rb.isKinematic = false; rb.useGravity = true; }
        foreach(Collider col in itemToPlace.GetComponentsInChildren<Collider>(true)) { if (col != null) col.enabled = true; }
        itemToPlace.SetActive(true);
        if (itemToPlace.TryGetComponent<LayerSwitcher>(out var layerSwitcher)) { layerSwitcher.SwitchToDefaultLayer(); }
        GameplayEvents.RaiseObjectPlaced(itemToPlace);
        InventoryManager.Instance?.ClearEquippedSlot();
        AudioManager.Instance?.PlayPlace();
        StopPlacing(true);
    }

     void StopPlacing(bool placedSuccessfully)
    {
        isPlacing = false;
        canPlace = false;
        if (previewObjectInstance != null) { Destroy(previewObjectInstance); }
        if (!placedSuccessfully) {
            GameObject equippedItem = InventoryManager.Instance?.GetEquippedItem();
            if (equippedItem != null && InventoryManager.Instance != null) {
                 InventoryManager.Instance.EquipItem(InventoryManager.Instance.selectedSlot);
                 if(!equippedItem.activeSelf) equippedItem.SetActive(true);
            }
        }
    }

    void OnDisable()
    {
        if (previewObjectInstance != null) Destroy(previewObjectInstance);
        isPlacing = false;
    }
}