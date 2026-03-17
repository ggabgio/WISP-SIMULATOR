using UnityEngine;

public class M2_ConfigureRouterObjective : BaseObjective
{
    protected override void OnObjectiveStart()
    {
        // Find the new, maintenance-specific trigger and enable it.
        M2_ConfigurationTrigger configTrigger = FindObjectOfType<M2_ConfigurationTrigger>();
        if (configTrigger != null)
        {
            configTrigger.SetConfigurationReady(true);
        }
    }

    public void StartMinigameLogic()
    {
        RouterConfiguration routerConfig = FindObjectOfType<RouterConfiguration>();
        if (routerConfig != null)
        {
            routerConfig.Initialize(this.Manager);
            routerConfig.StartMinigame(CompleteConfiguration);
        }
    }

    public void CompleteConfiguration(float finalScore)
    {
        if (!IsActive) return;
        SetScore(finalScore);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
    }
}