using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RouterInteraction : MonoBehaviour
{
    private M2_ConfigureRouterObjective _objectiveToNotify;
    private bool _isInteractionEnabled = false;
    private bool _playerInRange = false;
    private const string ConfigurePrompt = "Press [R] to Configure Router";

    public void EnableForObjective(M2_ConfigureRouterObjective objective)
    {
        _objectiveToNotify = objective;
        _isInteractionEnabled = true;
    }

    private void Update()
    {
        if (_playerInRange && _isInteractionEnabled)
        {
            PromptManager.Instance?.RequestPrompt(this, ConfigurePrompt, 1);
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_objectiveToNotify != null)
                {
                    _objectiveToNotify.StartMinigameLogic();
                }
                _isInteractionEnabled = false; 
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
        {
            _playerInRange = false;
        }
    }
}