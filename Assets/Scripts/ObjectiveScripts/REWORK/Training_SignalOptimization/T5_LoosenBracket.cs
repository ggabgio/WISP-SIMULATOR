using UnityEngine;

public class T5_LoosenBracketObjective : BaseObjective
{
    [Header("Dependencies")]
    public ScrewdriverController screwdriver;

    protected override void OnObjectiveStart()
    {
        if (screwdriver != null)
        {
            screwdriver.BeginLooseningObjective();
        }
    }

    public void NotifyBracketLoosened()
    {
        if (!IsActive) return;
        
        SetScore(100f);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        // Logic after loosening is complete.
    }
}