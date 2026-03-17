using UnityEngine;
using System.Collections;

public class DragObject : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    public Camera minigameCamera;

    public float distanceFromCamera = 2f;
    public float rotationSpeed = 100f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool rotateOnZ = false;

    public bool canReturnToOriginalPosition = true;
    public bool isDraggable = true;
    public float movebackSpeed = 5f;

    private bool isRJ45Plug = false;
    private bool isCrimpingToolObject = false;

    public CrimpingManager manager; // Changed reference
    private EthernetMinigameManager minigameManager;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        isCrimpingToolObject = CompareTag("CrimpingTool");

        if (minigameCamera == null && isDraggable)
        {
            minigameCamera = Camera.main; // Simple fallback
        }

        if (manager == null)
        {
            manager = FindFirstObjectByType<CrimpingManager>();
        }
        minigameManager = FindFirstObjectByType<EthernetMinigameManager>();
    }

    private void Update()
    {
        if (isDragging && isDraggable && minigameCamera != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = distanceFromCamera;
            Vector3 worldPos = minigameCamera.ScreenToWorldPoint(mousePos);
            transform.position = worldPos + offset;
        }
    }

    private void OnMouseDown()
    {
        if (!isDraggable || minigameCamera == null) return;

        isDragging = true;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = distanceFromCamera;
        offset = transform.position - minigameCamera.ScreenToWorldPoint(mousePos);
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        if (canReturnToOriginalPosition)
        {
            StartCoroutine(ReturnToOriginalPosition());
        }
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        // ... (This coroutine remains unchanged)
        float t = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        while (t < 1f)
        {
            t += Time.deltaTime * movebackSpeed;
            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, t);
            yield return null;
        }
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    public void LockInPlace()
    {
        isDraggable = false;
        canReturnToOriginalPosition = false;
    }

    public void SetIsRJ45(bool value)
    {
        isRJ45Plug = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCrimpingToolObject && other.CompareTag("RJ45"))
        {
            if (minigameManager != null && minigameManager.IsRJ45Attached())
            {
                Debug.Log("Crimping Success!");
                
                // Notify the current objective
                var currentObjective = manager?.GetCurrentObjective() as T3_CrimpCableObjective;
                currentObjective?.NotifyCableCrimped();
                
                if (canReturnToOriginalPosition)
                {
                    StartCoroutine(ReturnToOriginalPosition());
                }
                else
                {
                    LockInPlace();
                }
            }
            else
            {
                // Feedback for failure
                Debug.LogWarning("Cannot Crimp: RJ45 is not properly attached.");
            }
        }
    }
}