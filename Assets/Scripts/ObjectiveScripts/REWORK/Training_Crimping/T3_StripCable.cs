using UnityEngine;

public class T3_StripCableObjective : BaseObjective
{
    [Header("Dependencies")]
    [Tooltip("The CableCutting trigger object in the scene.")]
    public CableCutting cableCutter;

    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective Started: Strip Cable");
        // The base class will show the hint text.
        if (cableCutter != null)
        {
            cableCutter.gameObject.SetActive(true);
        }
    }
    
    public void NotifyCableStripped()
    {
        if (!IsActive) return;
        
        SetScore(100f);
        CompleteObjective();
    }
    
    protected override void OnObjectiveComplete()
    {
        Debug.Log("Objective Complete: Strip Cable");
        // We can hide the cutter trigger if we don't want it to be re-triggered
        if (cableCutter != null)
        {
            cableCutter.gameObject.SetActive(false);
        }
    }
}