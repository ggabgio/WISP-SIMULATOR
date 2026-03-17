using UnityEngine;
using System.Collections.Generic;

public class CablePlacer : MonoBehaviour
{
    public GameObject cableSegmentPrefab;
    public GameObject anchorPrefab;
    public GameObject anchorPreviewPrefab;
    public GameObject cablePreviewPrefab;
    public Material validMaterial;
    public Material invalidMaterial;
    public LayerMask clippingLayers;
    public string poeTag = "PoEAdaptor";
    public Transform antennaTopPoint;
    public float placementThreshold = 0.3f;

    private bool isPlacing = false;
    private Vector3 lastAnchorPoint;
    private Quaternion lastAnchorRotation;
    private bool hasAnchor = false;
    private GameObject previewCable;
    private GameObject anchorPreview;

    private Stack<GameObject> placedAnchors = new Stack<GameObject>();
    private Stack<GameObject> placedCables = new Stack<GameObject>();
    private bool canUndo = false;
    
    public CableLayingManager manager;

    // --- Prompt Messages ---
    private const string PlacingCablePrompt = "[LMB] Place Anchor | [RMB] Undo";
    private const string PlacingCableInvalidPrompt = "Cannot place anchor: Invalid position or angle.";


    public bool IsPlacingActive() => isPlacing;

    void Start()
    {
        if(manager == null) manager = FindObjectOfType<CableLayingManager>();
    }

    void Update()
    {
        if (!isPlacing || !hasAnchor)
        {
            if (previewCable != null) previewCable.SetActive(false);
            if (anchorPreview != null) anchorPreview.SetActive(false);
            return;
        }

        if (previewCable != null) previewCable.SetActive(true);
        if (anchorPreview != null) anchorPreview.SetActive(true);

        GetMouseWorldPoint(out Vector3 mouseTarget, out Quaternion surfaceRotation);

        Vector3 dir = (mouseTarget - lastAnchorPoint).normalized;
        float distance = Vector3.Distance(lastAnchorPoint, mouseTarget);
        Quaternion rotation = (distance > 0.01f) ? Quaternion.LookRotation(dir) : Quaternion.identity;

        if (previewCable == null) previewCable = Instantiate(cablePreviewPrefab);
        previewCable.transform.position = lastAnchorPoint + dir * (distance / 2f);
        previewCable.transform.rotation = rotation;
        previewCable.transform.localScale = new Vector3(0.02f, 0.02f, distance);

        bool isPlacementValid = !IsClipping(lastAnchorPoint, mouseTarget) &&
                                !IsCornerSkipping(lastAnchorPoint, mouseTarget) &&
                                distance > 0.05f;
        
        previewCable.GetComponent<Renderer>().material = isPlacementValid ? validMaterial : invalidMaterial;

        if (anchorPreview == null) anchorPreview = Instantiate(anchorPreviewPrefab);
        anchorPreview.transform.position = mouseTarget;
        anchorPreview.transform.rotation = surfaceRotation;
        if(anchorPreview.GetComponent<Renderer>() != null)
            anchorPreview.GetComponent<Renderer>().material = isPlacementValid ? validMaterial : invalidMaterial;

        // --- PromptManager Integration ---
        if (isPlacementValid)
        {
            PromptManager.Instance?.RequestPrompt(this, PlacingCablePrompt, 2);
        }
        else
        {
            PromptManager.Instance?.RequestPrompt(this, PlacingCableInvalidPrompt, 2);
        }
        // --- End of Integration ---

        if (Input.GetMouseButtonDown(0) && isPlacementValid)
        {
            PlaceAnchor(mouseTarget, surfaceRotation, distance, dir, rotation);
        }

        if (Input.GetMouseButtonDown(1) && canUndo)
        {
            UndoLastPlacement();
        }
    }

    private void PlaceAnchor(Vector3 mouseTarget, Quaternion surfaceRotation, float distance, Vector3 dir, Quaternion rotation)
    {
        GameObject cable = Instantiate(cableSegmentPrefab);
        cable.transform.position = lastAnchorPoint + dir * (distance / 2f);
        cable.transform.rotation = rotation;
        cable.transform.localScale = new Vector3(0.02f, 0.02f, distance);
        placedCables.Push(cable);

        GameObject anchor = Instantiate(anchorPrefab, mouseTarget, surfaceRotation);
        placedAnchors.Push(anchor);

        lastAnchorPoint = mouseTarget;
        lastAnchorRotation = surfaceRotation;
        canUndo = true;

        if (IsNearPoEAdaptor(lastAnchorPoint))
        {
            var currentObjective = manager?.GetCurrentObjective() as T4_RouteCable;
            currentObjective?.NotifyCableConnected();
            EndCablePlacing();
        }
    }

    private void UndoLastPlacement()
    {
        if (placedAnchors.Count > 1 && placedCables.Count > 0)
        {
            Destroy(placedAnchors.Pop());
            Destroy(placedCables.Pop());

            GameObject previousAnchor = placedAnchors.Peek();
            lastAnchorPoint = previousAnchor.transform.position;
            lastAnchorRotation = previousAnchor.transform.rotation;
            
            canUndo = (placedAnchors.Count > 1);
        }
    }

    public void EnterCableMode(Vector3 unusedStartPoint)
    {
        isPlacing = true;

        if (previewCable != null) Destroy(previewCable);
        if (anchorPreview != null) Destroy(anchorPreview);
        while (placedAnchors.Count > 0) Destroy(placedAnchors.Pop());
        while (placedCables.Count > 0) Destroy(placedCables.Pop());

        if (antennaTopPoint != null)
        {
            GameObject anchor = Instantiate(anchorPrefab, antennaTopPoint.position, antennaTopPoint.rotation);
            lastAnchorPoint = antennaTopPoint.position;
            lastAnchorRotation = antennaTopPoint.rotation;
            hasAnchor = true;
            placedAnchors.Push(anchor);
            canUndo = false;
        }
        else
        {
            Debug.LogError("CablePlacer: Antenna top point reference is not set!");
            isPlacing = false;
        }
    }

    void EndCablePlacing()
    {
        isPlacing = false;
        hasAnchor = false;

        if (previewCable != null) Destroy(previewCable);
        if (anchorPreview != null) Destroy(anchorPreview);
    }

    public void ForceEndCablePlacing()
    {
        if (isPlacing)
        {
            EndCablePlacing();
        }
    }

    bool GetMouseWorldPoint(out Vector3 point, out Quaternion surfaceRotation)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            point = hit.point;
            surfaceRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, hit.normal), hit.normal);
            return true;
        }
        point = lastAnchorPoint;
        surfaceRotation = Quaternion.identity;
        return false;
    }

    bool IsClipping(Vector3 start, Vector3 end)
    {
        if (Vector3.Distance(start, end) < 0.02f) return false;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        return Physics.Raycast(start + direction * 0.01f, direction, distance - 0.02f, clippingLayers);
    }

    bool IsNearPoEAdaptor(Vector3 position)
    {
        GameObject[] poEs = GameObject.FindGameObjectsWithTag(poeTag);
        foreach (var poe in poEs)
        {
            if (Vector3.Distance(position, poe.transform.position) < placementThreshold)
                return true;
        }
        return false;
    }

    bool IsCornerSkipping(Vector3 start, Vector3 end)
    {
        Vector3 delta = (end - start).normalized;
        if (delta == Vector3.zero) return true;
        const float strictTolerance = 10f;
        bool aligned =
            Vector3.Angle(delta, Vector3.right) < strictTolerance ||
            Vector3.Angle(delta, -Vector3.right) < strictTolerance ||
            Vector3.Angle(delta, Vector3.up) < strictTolerance ||
            Vector3.Angle(delta, -Vector3.up) < strictTolerance ||
            Vector3.Angle(delta, Vector3.forward) < strictTolerance ||
            Vector3.Angle(delta, -Vector3.forward) < strictTolerance;
        return !aligned;
    }
}