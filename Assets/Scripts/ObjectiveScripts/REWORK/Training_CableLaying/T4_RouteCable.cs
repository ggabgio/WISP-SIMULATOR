using UnityEngine;

public class T4_RouteCable : BaseObjective
{
    [Header("Dependencies")]
    [Tooltip("Reference to the CablePlacer script in the scene.")]
    public CablePlacer cablePlacer;

    protected override void OnObjectiveStart()
    {
        if (cablePlacer == null)
        {
            Debug.LogError("Objective is missing its CablePlacer reference!", this);
            return;
        }
        // At this point, the CablePlacer should already be in placing mode.
        // The objective hint from the inspector will guide the player.
    }

    // Called by CablePlacer when the cable end connects to the PoE adapter.
    public void NotifyCableConnected()
    {
        if (!IsActive) return;

        SetScore(100f);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        // Logic after the cable is fully connected.
    }
}