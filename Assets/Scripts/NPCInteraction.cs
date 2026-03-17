using UnityEngine;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject speechBubbleUI;
    public TextMeshProUGUI speechText;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 4f;

    [Header("Initial Guidance (Optional)")]
    [Tooltip("If true, this NPC will provide a persistent prompt until the player interacts once.")]
    public bool isInitialInstructor = true;

    private Transform player;
    private bool hasBeenTalkedTo = false;

    private const string InitialPrompt = "Talk to the Instructor";
    private const string InteractPrompt = "Press [E] to Talk";

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (speechBubbleUI != null)
            speechBubbleUI.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        bool inRange = Vector3.Distance(transform.position, player.position) <= interactionRange;

        // Logic for the initial instructor prompt
        if (isInitialInstructor && !hasBeenTalkedTo)
        {
            if (inRange)
            {
                // When in range, show the high-priority "Press E" prompt.
                PromptManager.Instance?.RequestPrompt(this, InteractPrompt, 2);
                if (Input.GetKeyDown(interactKey))
                {
                    TriggerInteraction();
                }
            }
            else
            {
                // When out of range, show the low-priority "Talk to..." prompt.
                PromptManager.Instance?.RequestPrompt(this, InitialPrompt, 100);
            }
        }
        // Standard NPC interaction logic (if not the initial instructor, or after being talked to)
        else
        {
            if (inRange && Input.GetKeyDown(interactKey))
            {
                TriggerInteraction();
            }
            else if (!inRange && speechBubbleUI.activeSelf)
            {
                speechBubbleUI.SetActive(false);
            }
        }
    }

    void TriggerInteraction()
    {
        if (speechBubbleUI != null)
        {
            speechBubbleUI.SetActive(true);
        }

        // If this was the initial instructor, mark them as talked to.
        if (isInitialInstructor)
        {
            hasBeenTalkedTo = true;
        }
    }
}