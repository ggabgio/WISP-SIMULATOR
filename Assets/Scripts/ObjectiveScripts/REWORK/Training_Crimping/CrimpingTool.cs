using UnityEngine;
using System.Collections;
using System.Reflection; // Needed for protected method OnLevelEnd()

public class CrimpingTool : MonoBehaviour
{
    [Header("Camera Control")]
    public Camera cameraBeforeZoom; 
    public Camera crimpingZoomCamera; 

    [Header("Object Visibility Control")]
    public GameObject crimpingToolObj;
    public GameObject animatedCableRJ45; 
    public GameObject wireCutterObj;
    public GameObject sealedCableObj;
    public GameObject rj45Obj; 
    public GameObject unbundledCableObj; 
    public GameObject bundledCableObj;

    [Header("Level Manager & Objective")]
    public CrimpingManager crimpingManager; 
    public T3_CrimpCableObjective finalCrimpingObjectiveScript; 
    public GameObject preCompletionPromptUI; 

    [Header("Results Panel Reference (UI Fix)")]
    [Tooltip("I-drag dito ang Results Panel (Canvas Parent) mula sa Hierarchy.")]
    public GameObject externalResultsPanel; 

    [Header("Animation Settings")]
    public Animator cableRJ45Animator;
    public Animator crimpingToolAnimator;
    public string approachTriggerName = "Approach";
    public string insertTriggerName = "Insert";
    public float insertAnimationDuration = 1.5f;

    private bool isZoomSequenceStarted = false;
    private bool hasBeenClickedForFinalCrimp = false;
    private Collider crimpToolCollider;
    private bool shouldShowFinishPrompt = false; // Flag to show "Press Y to finish" prompt (assessment mode only)

    void Start()
    {
        crimpToolCollider = GetComponent<Collider>();
        if (cableRJ45Animator == null) cableRJ45Animator = GetComponent<Animator>();
        if (animatedCableRJ45 != null) animatedCableRJ45.SetActive(false);

        if (crimpingManager == null || finalCrimpingObjectiveScript == null || externalResultsPanel == null)
        {
            Debug.LogError("❌ Missing references! Please assign Manager, Final Objective, and Results Panel in the Inspector.", this);
        }
    }

    private void OnMouseDown()
    {
        if (!isZoomSequenceStarted)
        {
            StartInitialZoomSequence();
        }
        else if (isZoomSequenceStarted && !hasBeenClickedForFinalCrimp)
        {
            StartFinalCrimpingSequence();
        }
    }

    private void Update()
    {
        // Check if player presses Y to finish crimping (only in assessment mode)
        if (shouldShowFinishPrompt && Input.GetKeyDown(KeyCode.Y))
        {
            FinishCrimping();
        }
        
        // Continuously show the finish prompt while it should be displayed
        if (shouldShowFinishPrompt)
        {
            ShowFinishPrompt();
        }
    }

    private void StartInitialZoomSequence()
    {
        isZoomSequenceStarted = true;
        SetObjectVisibility(false);
        SetCameraForCrimpingZoom();

        if (animatedCableRJ45 != null)
            animatedCableRJ45.SetActive(true);

        cableRJ45Animator?.SetTrigger(approachTriggerName);
        crimpingToolAnimator?.SetTrigger(approachTriggerName);

        if (crimpToolCollider != null)
            crimpToolCollider.enabled = true;
    }

    private void StartFinalCrimpingSequence()
    {
        if (hasBeenClickedForFinalCrimp) return;
        hasBeenClickedForFinalCrimp = true;

        if (crimpToolCollider != null)
            crimpToolCollider.enabled = false;

        StartCoroutine(StartCableAnimationAndLevelEnd());
    }

    private IEnumerator StartCableAnimationAndLevelEnd()
    {
        cableRJ45Animator?.SetTrigger(insertTriggerName);
        crimpingToolAnimator?.SetTrigger(insertTriggerName);

        // Wait for the animation to complete - use a longer duration to ensure animation finishes
        // The insertAnimationDuration is multiplied by 2.5 to give extra buffer time
        float waitDuration = insertAnimationDuration * 1.3f;
        Debug.Log($"⏳ Waiting {waitDuration} seconds for crimping animation to complete...");
        yield return new WaitForSeconds(waitDuration);
        Debug.Log("✅ Animation wait complete, proceeding to show results.");

        if (finalCrimpingObjectiveScript != null)
        {
            finalCrimpingObjectiveScript.NotifyCableCrimped();
            Debug.Log("✅ Crimping Objective completed.");
        }

        preCompletionPromptUI?.SetActive(false);

        // Check if we're in assessment mode - if so, show prompt and wait for Y press
        bool isAssessmentMode = crimpingManager != null && crimpingManager.isAssessmentMode;
        
        if (isAssessmentMode)
        {
            // In assessment mode: show prompt and wait for Y press
            shouldShowFinishPrompt = true;
            Debug.Log("📋 Assessment mode: Showing 'Press Y to finish crimping' prompt.");
        }
        else
        {
            // In training mode: immediately end the level
            EndLevelImmediately();
        }
    }

    private IEnumerator ActivateAndFreeze()
    {
        yield return null;
        if (externalResultsPanel != null) externalResultsPanel.SetActive(true);
        yield return null;
        if (externalResultsPanel != null) externalResultsPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("🧊 Time frozen. Results Panel visibility locked.");
    }

    private void SetObjectVisibility(bool isActive)
    {
        wireCutterObj?.SetActive(isActive);
        sealedCableObj?.SetActive(isActive);
        rj45Obj?.SetActive(isActive);
        unbundledCableObj?.SetActive(isActive);
        bundledCableObj?.SetActive(isActive);
        Debug.Log("🔹 Old objects visibility set to " + isActive);
    }

    private void SetCameraForCrimpingZoom()
    {
        if (cameraBeforeZoom != null && crimpingZoomCamera != null)
        {
            cameraBeforeZoom.enabled = false;
            crimpingZoomCamera.enabled = true;
            Debug.Log("📸 Camera switched to Crimping Zoom.");
        }
    }

    /// <summary>
    /// Shows the "Press Y to finish crimping" prompt using PromptManager.
    /// </summary>
    private void ShowFinishPrompt()
    {
        if (PromptManager.SafeInstance != null)
        {
            // Use a high priority to ensure this prompt shows
            PromptManager.SafeInstance.RequestPrompt(this, "Press Y to finish crimping", 10);
        }
        else
        {
            Debug.LogWarning("[CrimpingTool] PromptManager not found. Cannot show finish prompt.");
        }
    }

    /// <summary>
    /// Called when player presses Y to finish crimping in assessment mode.
    /// </summary>
    private void FinishCrimping()
    {
        if (!shouldShowFinishPrompt) return;

        Debug.Log("[CrimpingTool] Player pressed Y to finish crimping in assessment mode.");
        shouldShowFinishPrompt = false;
        
        // Clear the prompt
        if (PromptManager.SafeInstance != null)
        {
            PromptManager.SafeInstance.RequestPrompt(this, "", 0);
        }

        // End the level
        EndLevelImmediately();
    }

    /// <summary>
    /// Ends the level immediately (called in training mode or after Y press in assessment mode).
    /// </summary>
    private void EndLevelImmediately()
    {
        if (crimpingManager != null)
        {
            MethodInfo onLevelEndMethod = crimpingManager.GetType().GetMethod(
                "OnLevelEnd", BindingFlags.NonPublic | BindingFlags.Instance,
                null, new System.Type[] { typeof(float), typeof(float) }, null
            );

            if (onLevelEndMethod == null)
                onLevelEndMethod = crimpingManager.GetType().GetMethod("OnLevelEnd", BindingFlags.NonPublic | BindingFlags.Instance);

            if (onLevelEndMethod != null)
            {
                if (onLevelEndMethod.GetParameters().Length == 2)
                    onLevelEndMethod.Invoke(crimpingManager, new object[] { 0f, 0f });
                else
                    onLevelEndMethod.Invoke(crimpingManager, null);

                Debug.Log("✅ OnLevelEnd() called successfully via Reflection.");
            }
            else
            {
                Debug.LogError("❌ Could not find protected OnLevelEnd() in CrimpingManager.");
            }
        }

        // ✅ Only freeze in standalone mode (no CombinedLevelManager)
        bool isCombined = FindAnyObjectByType<CombinedLevelManager>() != null;
        if (!isCombined)
        {
            StartCoroutine(ActivateAndFreeze());
        }
        else
        {
            Debug.Log("🟢 Combined mode detected — skipping freeze and allowing player movement.");
        }
    }
}
