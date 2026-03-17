using UnityEngine;

public class InspectController : MonoBehaviour
{
    public static InspectController Instance { get; private set; }
    public Camera playerCamera; // Player camera to disable during inspect
    public MonoBehaviour[] movementControllers; // Scripts to disable when inspecting (e.g., FPS controller)

    private InspectableObject current;
    private bool inspecting;

    public bool IsInspecting => inspecting;
    public InspectableObject CurrentTarget => current;

    public System.Action<InspectableObject> OnEnterInspect;
    public System.Action OnExitInspect;

    void Awake()
    {
        if (Instance == null) Instance = this; else if (Instance != this) Destroy(gameObject);
    }

    void Update()
    {
        if (inspecting)
        {
            if (Input.GetKeyDown(KeyCode.T)) ExitInspect();
            return;
        }

        // Find closest inspectable the player is inside
        InspectableObject candidate = null;
        float best = float.MaxValue;
        var all = InspectableObject.Instances;
        for (int i = 0; i < all.Count; i++)
        {
            var io = all[i];
            if (!io.playerInside) continue;
            float d = (io.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                candidate = io;
            }
        }

        // UI prompt while able to inspect
        if (candidate != null && PromptManager.Instance != null)
        {
            PromptManager.Instance.RequestPrompt(candidate, "Press T to start CRIMPING a cable", 5);
        }

        if (candidate != null && Input.GetKeyDown(KeyCode.T))
        {
            EnterInspect(candidate);
        }
    }

    private void EnterInspect(InspectableObject target)
    {
        if (target == null || target.inspectCamera == null) return;
        current = target;
        inspecting = true;

        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        current.inspectCamera.gameObject.SetActive(true);

        for (int i = 0; i < movementControllers.Length; i++)
        {
            if (movementControllers[i] != null) movementControllers[i].enabled = false; // disable player movement
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnEnterInspect?.Invoke(current);
    }

    private void ExitInspect()
    {
        if (current != null && current.inspectCamera != null)
        {
            current.inspectCamera.gameObject.SetActive(false);
        }

        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        for (int i = 0; i < movementControllers.Length; i++)
        {
            if (movementControllers[i] != null) movementControllers[i].enabled = true; // re-enable movement
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        current = null;
        inspecting = false;

        OnExitInspect?.Invoke();
    }

    /// <summary>
    /// Public method to exit inspect mode. Can be called from other scripts (e.g., ZoomInspectableObject)
    /// to reset the inspect state when exiting zoom camera.
    /// </summary>
    public void ForceExitInspect()
    {
        if (inspecting)
        {
            ExitInspect();
        }
    }
}


