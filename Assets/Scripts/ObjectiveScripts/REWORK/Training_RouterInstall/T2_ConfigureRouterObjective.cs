using UnityEngine;

public class T2_ConfigureRouterObjective : BaseObjective
{
    private RouterInstallManager L2_Manager;

    protected override void OnObjectiveStart()
    {
        L2_Manager = Manager as RouterInstallManager;
        if (L2_Manager == null)
        {
            return;
        }
        
        // Find the new trigger and enable it.
        ConfigurationTrigger configTrigger = FindObjectOfType<ConfigurationTrigger>();
        if (configTrigger != null)
        {
            configTrigger.SetConfigurationReady(true);
        }
    }
    
    public void StartConfigurationMinigame()
    {
        if (L2_Manager != null && L2_Manager.routerConfiguration != null)
        {
            L2_Manager.routerConfiguration.StartMinigame(CompleteConfiguration);
        }
    }
    
    private void CompleteConfiguration(float finalScore)
    {
        if (!IsActive) return;
        
        SetScore(finalScore);
        CompleteObjective();
    }

    protected override void OnObjectiveComplete()
    {
    }
}