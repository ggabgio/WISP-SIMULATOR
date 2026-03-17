using UnityEngine;

public class T4_StartCablePlacementObjective : BaseObjective
{
    [Header("Dependencies")]
    [Tooltip("Reference to the CableInteraction script in the scene.")]
    public CableInteraction cableInteraction;

    protected override void OnObjectiveStart()
    {
        if (cableInteraction == null)
        {
            Debug.LogError("Objective is missing its CableInteraction reference!", this);
            return;
        }
        // The objective hint will guide the player to interact.
        // Enable the interaction script to listen for input.
        cableInteraction.enabled = true;
    }

    // Called by CableInteraction when the player successfully starts placement.
    public void NotifyPlacementStarted()
    {
        if (!IsActive) return;
        
        SetScore(100f);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        // Disable the interaction script so it can't be triggered again.
        if (cableInteraction != null)
        {
            cableInteraction.enabled = false;
        }
    }
}