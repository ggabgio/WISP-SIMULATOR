using UnityEngine;

public class T5_TightenBracketObjective : BaseObjective
{
    [Header("Dependencies")]
    public ScrewdriverController screwdriver;

    protected override void OnObjectiveStart()
    {
        if (screwdriver != null)
        {
            screwdriver.BeginTighteningObjective();
        }
    }
    
    public void NotifyBracketTightened()
    {
        if (!IsActive) return;

        SetScore(100f);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
        // Final objective is complete.
    }
}