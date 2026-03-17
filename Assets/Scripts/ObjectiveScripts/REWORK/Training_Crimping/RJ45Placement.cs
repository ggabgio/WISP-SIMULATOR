using UnityEngine;

public class RJ45Placement : MonoBehaviour
{
    private EthernetMinigameManager minigameManager;
    public CrimpingManager manager; // Changed reference to the new manager

    private void Start()
    {
        minigameManager = FindFirstObjectByType<EthernetMinigameManager>();
        if (minigameManager == null)
        {
            Debug.LogError("RJ45Placement: EthernetMinigameManager not found in scene!", this);
        }
        if (manager == null)
        {
            // Attempt to find it if not assigned
            manager = FindFirstObjectByType<CrimpingManager>();
            if (manager == null)
            {
                Debug.LogError("RJ45Placement: CrimpingManager not assigned in Inspector and could not be found!", this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // The new objective system checks for game over status.
        if (other.CompareTag("RJ45"))
        {
            if (minigameManager != null && minigameManager.AreWiresArrangedCorrectly())
            {
                DragObject rj45DragObject = other.GetComponent<DragObject>();
                if (rj45DragObject != null)
                {
                    rj45DragObject.SetIsRJ45(true);
                    other.transform.position = transform.position;
                    other.transform.rotation = transform.rotation;
                    rj45DragObject.LockInPlace();
                    Debug.Log("RJ45 snapped and locked into place by RJ45Placement.");

                    minigameManager.OnRJ45Attached();

                    // Notify the current objective
                    var currentObjective = manager?.GetCurrentObjective() as T3_AttachRJ45Objective;
                    currentObjective?.NotifyRJ45Placed();
                }
                else
                {
                     Debug.LogError("RJ45 object that entered trigger is missing DragObject script!", other.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("Cannot place RJ45 — wires are not arranged correctly!");
                // The objective hint itself will guide the player. Direct prompts can be added if needed.
            }
        }
    }
}