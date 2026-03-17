using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConfigurationTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RouterInstallManager manager;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.R;

    private bool isConfigurationReady = false;
    private bool playerInRange = false;

    private const string ConfigurePrompt = "Press [R] to Configure Router";

    private void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<RouterInstallManager>();
        }
    }

    public void SetConfigurationReady(bool isReady)
    {
        isConfigurationReady = isReady;
    }

    private void Update()
    {
        if (playerInRange && isConfigurationReady && manager != null)
        {
            // Check if the configuration minigame is already active.
            if (manager.routerConfiguration != null && !manager.routerConfiguration.IsMinigameActive)
            {
                PromptManager.Instance?.RequestPrompt(this, ConfigurePrompt, 3);
                if (Input.GetKeyDown(interactionKey))
                {
                    manager.StartConfigurationMinigame();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            playerInRange = false;
        }
    }
}