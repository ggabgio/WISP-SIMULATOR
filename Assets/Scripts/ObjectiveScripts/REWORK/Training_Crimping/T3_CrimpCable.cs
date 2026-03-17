using UnityEngine;

public class T3_CrimpCableObjective : BaseObjective
{

    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective Started: Crimp Cable");
    }
    
    public void NotifyCableCrimped()
    {
        if (!IsActive) return;

        SetScore(100f);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        Debug.Log("Objective Complete: Crimp Cable");
    }
}