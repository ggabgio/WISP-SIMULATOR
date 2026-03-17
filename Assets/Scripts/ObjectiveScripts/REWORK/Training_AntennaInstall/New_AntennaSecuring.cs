using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AntennaAlignment))]
public class NewAntennaSecuring : MonoBehaviour
{
    [Header("Setup")]
    public AntennaInstallManager objectiveManager;
    public Transform playerTransformReference;
    public Transform antennaBaseContactPoint;
    public float wireAttachHeightOffset = 4.0f;
    public GameObject anchorPrefab;
    public GameObject wireRendererPrefab;
    public Material[] coloredWireMaterials;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.R;

    [Header("Placement Mode")]
    public float maxPlacementDistance = 10f;
    public float minPlacementDistance = 1.5f;
    public Material anchorPreviewMaterialValid;
    public Material anchorPreviewMaterialInvalid;
    public float anchorCollisionCheckRadius = 0.3f;
    public LayerMask placementLayerMask = ~0;
    public LayerMask obstacleLayerMask = ~0;
    [Tooltip("Child transform on the antenna pole where wires connect.")]
    public Transform wireAttachPointTransform;

    [Header("Objective State")]
    [SerializeField]
    public List<Vector3> placedAnchorPositions = new List<Vector3>();
    private List<LineRenderer> placedWireRenderers = new List<LineRenderer>();
    private bool isObjective3Complete = false;

    [Header("Physics Simulation")]
    public Rigidbody antennaRigidbody;
    public float tiltThreshold = 20f;
    private bool applyWireForces = false;

    [Header("Wire Pull Settings")]
    [Tooltip("Overall strength of wire pulls (try 0.1 to 1.0 for subtle effect).")]
    public float pullForceMultiplier = 0.5f;
    [Tooltip("How much anchor distance affects pull. 0 = none, 1 = full linear scaling.")]
    [Range(0f, 1f)]
    public float distanceScalingFactor = 0.1f;
    [Tooltip("Clamp to prevent extreme pulls when anchors are very far away.")]
    public float maxPullForce = 2f;

    [Header("Objective Integration")]
    public T1_SecureAntennaObjective secureObjective;

    // --- Private Variables ---
    private Transform playerTransform;
    private Camera playerCamera;
    private bool isPlayerNearby = false;
    private bool isPlacingAnchor = false;
    private bool canPlaceCurrentAnchor = false;
    private GameObject currentPreviewAnchor;
    private LineRenderer currentPreviewWire;
    private MeshRenderer previewAnchorRenderer;
    private Vector3 antennaWireAttachPointWorld;

    // --- Prompt Messages (Constants) ---
    private const string WireInteractPromptMessage = "Press [R] to Start Placing Wire";
    private const string WirePlacePromptMessage = "[LMB] Place Wire Anchor | [RMB] Cancel";

    void Start()
    {
        playerCamera = Camera.main;
        playerTransform = playerTransformReference;

        bool setupError = (objectiveManager == null ||
                           playerTransform == null ||
                           playerCamera == null ||
                           antennaBaseContactPoint == null ||
                           anchorPrefab == null ||
                           wireRendererPrefab == null ||
                           wireRendererPrefab.GetComponent<LineRenderer>() == null);

        if (setupError)
        {
            Debug.LogError("AntennaSecuring: Critical reference (ObjectiveManager, PlayerTransform, etc.) missing! Please assign in Inspector.", this);
            enabled = false;
            return;
        }

        CalculateAntennaAttachPoint();
    }

    void Update()
    {
        if (isObjective3Complete || objectiveManager == null || objectiveManager.IsGameOver || playerTransform == null)
        {
            if (isPlacingAnchor) StopAnchorPlacement(false);
            return;
        }

        bool isSecuringObjectiveActive = objectiveManager.GetCurrentObjective() is T1_SecureAntennaObjective;
        if (!isSecuringObjectiveActive) return;

        if (isPlacingAnchor)
        {
            // --- STATE: PLACING ANCHOR ---
            // Request the high-priority placement prompt.
            PromptManager.Instance?.RequestPrompt(this, WirePlacePromptMessage, 10);

            UpdateAnchorPreview();
            if (canPlaceCurrentAnchor && Input.GetMouseButtonDown(0) && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
            {
                PlaceAnchor();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                StopAnchorPlacement(false);
            }
        }
        else
        {
            // --- STATE: NOT PLACING (CHECKING FOR INTERACTION) ---
            isPlayerNearby = Vector3.Distance(playerTransform.position, transform.position) <= interactionDistance;
            bool canInteract = isPlayerNearby && IsPlayerHoldingWire() && placedAnchorPositions.Count < 3;

            // If we can interact, request the low-priority interaction prompt.
            if (canInteract)
            {
                PromptManager.Instance?.RequestPrompt(this, WireInteractPromptMessage, 5);
            }

            // Handle actual interaction input.
            if (canInteract && Input.GetKeyDown(interactionKey))
            {
                TryStartAnchorPlacement();
            }
        }
    }

    void FixedUpdate()
    {
        if (!applyWireForces || antennaRigidbody == null) return;
        if (placedAnchorPositions.Count < 3) return;

        Vector3 attachPoint = antennaWireAttachPointWorld;

        foreach (var anchorPos in placedAnchorPositions)
        {
            Vector3 dir = (anchorPos - attachPoint).normalized;
            float distance = Vector3.Distance(anchorPos, attachPoint);
            float scaledForce = 1f + (distance * distanceScalingFactor);
            Vector3 force = dir * Mathf.Min(pullForceMultiplier * scaledForce, maxPullForce);
            antennaRigidbody.AddForceAtPosition(force, attachPoint, ForceMode.Force);
        }
    }

    bool IsPlayerHoldingWire()
    {
        if (InventoryManager.Instance == null) return false;
        GameObject equippedObject = InventoryManager.Instance.GetEquippedItem();
        if (equippedObject == null) return false;
        if (equippedObject.TryGetComponent<ItemInstance>(out ItemInstance instance))
        {
            return instance.data != null && instance.data.isWire;
        }
        return false;
    }

    public void TryStartAnchorPlacement()
    {
        if (placedAnchorPositions.Count >= 3 || !IsPlayerHoldingWire()) return;
        if (placedAnchorPositions.Count == 0 && gameObject.CompareTag("canPickUp"))
        {
            gameObject.tag = "Untagged";
        }
        StartAnchorPlacement();
    }

    void StartAnchorPlacement()
    {
        if (isPlacingAnchor) return;
        CalculateAntennaAttachPoint();
        if (antennaWireAttachPointWorld == Vector3.zero) return;

        isPlacingAnchor = true;
        Debug.Log("AntennaSecuring: Starting anchor placement.");

        currentPreviewAnchor = Instantiate(anchorPrefab);
        currentPreviewAnchor.name = "Anchor_Preview";
        previewAnchorRenderer = currentPreviewAnchor.GetComponentInChildren<MeshRenderer>();
        foreach (var col in currentPreviewAnchor.GetComponentsInChildren<Collider>(true)) col.enabled = false;

        GameObject wireObj = Instantiate(wireRendererPrefab);
        wireObj.name = "Wire_Preview";
        currentPreviewWire = wireObj.GetComponent<LineRenderer>();
        if (currentPreviewWire == null) { StopAnchorPlacement(false); return; }

        currentPreviewWire.positionCount = 2;
        currentPreviewWire.SetPosition(0, antennaWireAttachPointWorld);
        if (anchorPreviewMaterialInvalid != null)
        {
            if (previewAnchorRenderer != null) previewAnchorRenderer.material = anchorPreviewMaterialInvalid;
            currentPreviewWire.material = anchorPreviewMaterialInvalid;
        }
        UpdateAnchorPreview();
    }

    void UpdateAnchorPreview()
    {
        if (!isPlacingAnchor || currentPreviewAnchor == null || currentPreviewWire == null || playerCamera == null) return;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        bool isValidHit = false;
        Vector3 targetPosition = Vector3.zero;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayerMask))
        {
            targetPosition = hit.point;
            Vector3 antennaBaseHorizontal = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 targetHorizontal = new Vector3(targetPosition.x, 0, targetPosition.z);
            float distanceToAntennaBase = Vector3.Distance(targetHorizontal, antennaBaseHorizontal);
            isValidHit = distanceToAntennaBase >= minPlacementDistance && distanceToAntennaBase <= maxPlacementDistance;
        }
        else
        {
            isValidHit = false;
            targetPosition = ray.GetPoint(maxPlacementDistance);
        }

        canPlaceCurrentAnchor = isValidHit;
        currentPreviewAnchor.transform.position = targetPosition;
        currentPreviewAnchor.transform.rotation = (isValidHit && hit.collider != null) ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity;
        currentPreviewWire.SetPosition(0, antennaWireAttachPointWorld);
        currentPreviewWire.SetPosition(1, targetPosition);
        Material previewMat = canPlaceCurrentAnchor ? anchorPreviewMaterialValid : anchorPreviewMaterialInvalid;
        if (previewAnchorRenderer != null) previewAnchorRenderer.material = previewMat;
        if (currentPreviewWire != null) currentPreviewWire.material = previewMat;
    }

    void PlaceAnchor()
    {
        if (!isPlacingAnchor || !canPlaceCurrentAnchor) return;

        Vector3 finalAnchorPosition = currentPreviewAnchor.transform.position;
        Quaternion finalAnchorRotation = currentPreviewAnchor.transform.rotation;
        Debug.Log($"AntennaSecuring: Placing anchor #{placedAnchorPositions.Count + 1}");

        GameObject anchorObj = Instantiate(anchorPrefab, finalAnchorPosition, finalAnchorRotation);
        GameObject wireObj = Instantiate(wireRendererPrefab);
        LineRenderer placedWire = wireObj.GetComponent<LineRenderer>();
        if (placedWire == null) { Destroy(wireObj); return; }

        placedWire.positionCount = 2;
        WireUpdater updater = wireObj.AddComponent<WireUpdater>();
        updater.pointA = wireAttachPointTransform;
        updater.pointB = anchorObj.transform;
        placedWire.SetPosition(0, updater.pointA.position);
        placedWire.SetPosition(1, updater.pointB.position);

        if (coloredWireMaterials != null && coloredWireMaterials.Length > 0)
        {
            placedWire.material = coloredWireMaterials[placedAnchorPositions.Count % coloredWireMaterials.Length];
        }

        placedWireRenderers.Add(placedWire);
        placedAnchorPositions.Add(finalAnchorPosition);

        StopAnchorPlacement(true);

        if (placedAnchorPositions.Count >= 3 && !isObjective3Complete)
        {
            isObjective3Complete = true;
            Debug.Log("AntennaSecuring: Objective 3 Complete! All 3 wires placed.");
            applyWireForces = true;
            if (antennaRigidbody != null) antennaRigidbody.isKinematic = false;
            if (secureObjective != null) StartCoroutine(RunPhysicsTest(secureObjective));
        }
    }

    public void StopAnchorPlacement(bool placedSuccessfully)
    {
        if (!isPlacingAnchor) return;
        isPlacingAnchor = false;
        canPlaceCurrentAnchor = false;
        Debug.Log($"AntennaSecuring: Stopping anchor placement. Success: {placedSuccessfully}");

        if (currentPreviewAnchor != null) Destroy(currentPreviewAnchor);
        if (currentPreviewWire != null) Destroy(currentPreviewWire.gameObject);
        currentPreviewAnchor = null;
        currentPreviewWire = null;
        previewAnchorRenderer = null;
    }

    void CalculateAntennaAttachPoint()
    {
        if (antennaBaseContactPoint == null)
        {
            antennaWireAttachPointWorld = transform.position + (transform.up * wireAttachHeightOffset);
            return;
        }
        antennaWireAttachPointWorld = antennaBaseContactPoint.position + (transform.up * wireAttachHeightOffset);
    }

    private IEnumerator RunPhysicsTest(T1_SecureAntennaObjective objective)
    {
        Debug.Log("AntennaSecuring: Starting stability test (10 seconds total)...");

        if (antennaRigidbody != null)
            antennaRigidbody.isKinematic = false;

        // Wait 10 seconds while physics runs normally
        yield return new WaitForSeconds(7.5f);

        bool passed = true;
        float tiltAngle = 0f;

        if (antennaRigidbody != null)
        {
            tiltAngle = Vector3.Angle(Vector3.up, antennaRigidbody.transform.up);
            Debug.Log($"Antenna tilt angle after 10s: {tiltAngle:F1}°");

            if (tiltAngle > tiltThreshold)
                passed = false;

            // ✅ Freeze antenna permanently after test
            antennaRigidbody.isKinematic = true;
            antennaRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            Debug.Log("AntennaSecuring: Antenna frozen after stability test.");
        }

        if (objective != null)
            objective.CompleteWithPhysicsResult(passed);
    }

}