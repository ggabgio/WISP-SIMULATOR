using UnityEngine;

public class NPCPromptTrigger : MonoBehaviour
{
    [Tooltip("Prompt shown when player is near.")]
    public string promptMessage = "Press [E] to talk";
    
    [Tooltip("The priority of this NPC's prompt.")]
    public int promptPriority = 2;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PromptManager.Instance?.RequestPrompt(this, promptMessage, promptPriority);
        }
    }
}