using UnityEngine;
using System.Collections;

public class BundledWireClick : MonoBehaviour
{
    [Tooltip("The Animator component on this GameObject.")]
    public Animator wireAnimator;
    [SerializeField] private string animationTriggerName = "Unbundle"; 

    // Gawing non-static ang reference
    private T3_UnbundleWiresObjective objectiveManager; 
    private bool hasBeenUnbundled = false;

    void Start()
    {
        if (wireAnimator == null) wireAnimator = GetComponent<Animator>();
        
        // ** RESOLVE: Ginagamit ang bagong FindFirstObjectByType() **
        // Tatangkain lang itong hanapin isang beses sa Start()
        if (objectiveManager == null) 
        {
            objectiveManager = FindFirstObjectByType<T3_UnbundleWiresObjective>();
            if (objectiveManager == null)
            {
                Debug.LogWarning("T3_UnbundleWiresObjective not found in scene. Objective checks will be skipped.");
            }
        }
    }

    // Coroutine (Katulad ng naunang inayos)
    private IEnumerator PlayAnimationAndCompleteUnbundle()
    {
        yield return null; 

        if (wireAnimator != null)
        {
            wireAnimator.SetTrigger(animationTriggerName);
            Debug.Log("Test 5 Passed (Animation Triggered)"); 
        }

        hasBeenUnbundled = true;

        // 1. Ipadala ang notification.
        if (objectiveManager != null)
        {
             objectiveManager.NotifyWireUnbundled();
        }

        Debug.Log("Test 6 Passed (Unbundle Complete)");

        // 3. I-disable ang Collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }


    void OnMouseDown()
    {
        Debug.Log("Clicked!");

        // I-check ang basic requirements
        if (hasBeenUnbundled || wireAnimator == null) return; 
        
        Debug.Log("Test 1 Passed");

        // CHECK NG OBJECTIVE
        if (objectiveManager != null)
        {
            Debug.Log("Test 2 Passed");
            
            if (!objectiveManager.IsActive)
            {
                Debug.Log("Test 3 Passed");
                Debug.LogError("OBJECTIVE BLOCKED: T3_UnbundleWiresObjective ay HINDI ACTIVE!");
                return; // Huminto kung hindi pa active ang objective
            }
            
            Debug.Log("Test 4 Passed");
            StartCoroutine(PlayAnimationAndCompleteUnbundle());
        }
        else
        {
            // Fallback kung hindi nahanap ang objective manager (tatapusin lang ang animation)
            Debug.LogWarning("Objective Manager is NULL! Skipping objective check and proceeding with unbundle visual.");
            StartCoroutine(PlayAnimationAndCompleteUnbundle());
        }
    }
}