using UnityEngine;

public class T3_AttachRJ45Objective : BaseObjective
{
    [Header("Dependencies")]
    [Tooltip("The RJ45Placement trigger object in the scene.")]
    public RJ45Placement rj45PlacementZone;

    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective Started: Attach RJ45 Plug");
        if (rj45PlacementZone != null)
        {
            rj45PlacementZone.gameObject.SetActive(true);
        }
    }
    
    public void NotifyRJ45Placed()
    {
        if (!IsActive) return;
        
        SetScore(100f);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        Debug.Log("Objective Complete: Attach RJ45 Plug");
    }
}