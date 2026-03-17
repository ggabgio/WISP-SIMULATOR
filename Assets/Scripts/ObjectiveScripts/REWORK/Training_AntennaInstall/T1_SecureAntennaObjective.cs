using UnityEngine;
using TMPro;

public class T1_SecureAntennaObjective : BaseObjective
{
    [Header("Objective 3: Securing")]
    public NewAntennaSecuring antennaSecuringScript;

    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective 3 (Secure Antenna) started.");
        if (objectiveDisplay != null) objectiveDisplay.text = objectiveHint;

        if (antennaSecuringScript == null)
        {
            Debug.LogError("AntennaSecuring script reference is missing!", this);
            CompleteObjective(); // Fail gracefully
        }
    }
    
    public void CompleteWithPhysicsResult(bool passed)
    {
        if (!IsActive) return;

        if (passed)
        {
            SetScore(100f);
            CompleteObjective();
        }
        else
        {
            SetScore(0f);
            Manager.ForceEndLevelWithFailure();
        }
    }

    protected override void OnObjectiveComplete()
    {
        Debug.Log("Objective 3: Secure Antenna - COMPLETE!");
    }
}