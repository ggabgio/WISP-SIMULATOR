using UnityEngine;

public class T3_ArrangeWiresObjective : BaseObjective
{
    [Header("Dependencies")]
    public EthernetMinigameManager ethernetManager;

    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective Started: Arrange Wires");
        if(ethernetManager != null)
        {
            ethernetManager.BeginArrangementObjective(Manager);
        }
    }

    // Called by EthernetMinigameManager when the wires are in the correct T568B order
    public void NotifyArrangementCorrect()
    {
        if (!IsActive) return;

        SetScore(100f);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        Debug.Log("Objective Complete: Arrange Wires");
    }
}