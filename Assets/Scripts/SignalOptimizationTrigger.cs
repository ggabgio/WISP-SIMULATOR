using UnityEngine;
using System.Collections;
using TMPro;

public class SignalOptimizationTrigger : MonoBehaviour
{
    [Header("References")]
    public CombinedLevelManager combinedLevelManager;
    public SignalOptimizationManager signalOptimizationManager;
    public Camera optimizationCamera;
    public GameObject defaultPlayerCamera;

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Tool Requirements")]
    public InventoryManager playerInventory;
    public string requiredTool1 = "SignalChecker";
    public string requiredTool2 = "Screwdriver";

    [Header("Prompt Settings")]
    public KeyCode optimizationKey = KeyCode.Q; // changeable in inspector
    public string optimizePrompt = "Press Q to optimize antenna";

    private Transform player;
    private bool optimizationStarted = false;
    private float promptBlockUntil = 0f; // suppress optimize prompt during warnings

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (signalOptimizationManager != null)
            signalOptimizationManager.gameObject.SetActive(false);

        if (optimizationCamera != null)
            optimizationCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (optimizationStarted || player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= interactionRange)
        {
            // Only show the optimization prompt once Cable Laying is completed
            if (IsCableLayingCompleted())
            {
                if (Time.time >= promptBlockUntil)
                {
                    PromptManager.SafeInstance?.RequestPrompt(this, optimizePrompt, 5);
                }
                if (Input.GetKeyDown(optimizationKey))
                    TryStartOptimization();
            }
            // else: do not display any prompt to avoid conflicts with other interactions
        }
    }

    private void TryStartOptimization()
    {
        // Cable Laying completion is already checked before showing prompt and accepting input

        // ✅ Step 2: Check required tools
        if (!playerInventory.HasItem(requiredTool1) || !playerInventory.HasItem(requiredTool2))
        {
            Debug.Log("🚫 You need both the Signal Checker and Screwdriver to optimize the signal.");
            PromptManager.SafeInstance?.ShowTimedPrompt("You need both Signal Checker and Screwdriver", 2.5f);
            promptBlockUntil = Time.time + 2.6f; // block the Q prompt briefly so the warning is visible
            return;
        }

        // ✅ Step 3: Start optimization process
        StartOptimization();
    }

    private bool IsCableLayingCompleted()
    {
        // Check if CombinedLevelManager exists
        if (combinedLevelManager == null || combinedLevelManager.cableLayingManager == null)
            return false;

        // ✅ New logic: check if cable laying level was marked completed in CombinedLevelManager
        var cableManager = combinedLevelManager.cableLayingManager;
        var field = typeof(CombinedLevelManager).GetField("levelCompletionStatus", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            var dict = field.GetValue(combinedLevelManager) as System.Collections.IDictionary;
            if (dict != null && dict.Contains(cableManager))
            {
                return (bool)dict[cableManager]; // return true only if cable laying is done
            }
        }

        // fallback
        return false;
    }

    private void StartOptimization()
    {
        optimizationStarted = true;
        Debug.Log("✅ Starting Signal Optimization!");

        // Disable player movement and switch camera
        if (combinedLevelManager?.player != null)
            combinedLevelManager.player.GetComponent<PlayerMovement>().enabled = false;

        if (defaultPlayerCamera != null)
            defaultPlayerCamera.SetActive(false);

        if (optimizationCamera != null)
            optimizationCamera.gameObject.SetActive(true);

        if (signalOptimizationManager != null)
        {
            signalOptimizationManager.gameObject.SetActive(true);
            signalOptimizationManager.isAssessmentMode = true;
            signalOptimizationManager.StartLevel();

            // Inform CombinedLevelManager that this level started manually
            ObjectiveLevelEndWatcher.Attach(signalOptimizationManager, (score, time) =>
            {
                combinedLevelManager.OnLevelCompleted(signalOptimizationManager, score, time);
                ExitOptimization();
            });

        }
    }

    private void ExitOptimization()
    {
        Debug.Log("🏁 Exiting Signal Optimization...");

        if (optimizationCamera != null)
            optimizationCamera.gameObject.SetActive(false);

        if (defaultPlayerCamera != null)
            defaultPlayerCamera.SetActive(true);

        if (combinedLevelManager?.player != null)
            combinedLevelManager.player.GetComponent<PlayerMovement>().enabled = true;
    }
}
