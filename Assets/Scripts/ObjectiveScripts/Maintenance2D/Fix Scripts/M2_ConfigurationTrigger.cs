using UnityEngine;

[RequireComponent(typeof(Collider))]
public class M2_ConfigurationTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private M2_FixManager manager;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.R;

    private bool isConfigurationReady = false;
    private bool playerInRange = false;

    private const string ConfigurePrompt = "Press [R] to Configure Router";

    private void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<M2_FixManager>();
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