using UnityEngine;
using System.Collections; // Importante para sa Coroutine functionality

public class WireCutterAnimator : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator targetPosAnimator;
    public string cutStartTriggerName = "targetPos";

    private CableCutting cableCuttingTarget; 

    private void Start()
    {
        // Auto-assign Animator kung wala pang nakatalaga
        if (targetPosAnimator == null)
        {
            targetPosAnimator = GetComponent<Animator>();
        }

        // Hanapin ang CableCutting script sa simula (Gamit ang FindFirstObjectByType)
        if (cableCuttingTarget == null)
        {
            // Gumamit ng FindFirstObjectByType para sa modernong Unity API
            cableCuttingTarget = FindFirstObjectByType<CableCutting>(); 
            if (cableCuttingTarget == null)
            {
                Debug.LogError("WireCutterAnimator: CableCutting script not found in scene!");
            }
        }
    }
    
    // ** COROUTINE: Nagti-trigger ng animation pagkatapos ng isang frame delay para sa stability **
    private IEnumerator TriggerCutAnimationSafely()
    {
        // Maghintay ng isang frame (yield return null) para matiyak na updated ang Animator system.
        yield return null; 

        // 3. 🔸 Trigger the cutting animation
        if (targetPosAnimator != null)
        {
             targetPosAnimator.SetTrigger(cutStartTriggerName);
             Debug.Log("CUTTING: Animation triggered (via Coroutine).");
        }
        else
        {
             // Fallback error check
             Debug.LogError("WireCutterAnimator: targetPosAnimator is NULL during trigger attempt.");
        }
    }

    private void OnMouseDown()
    {
        // 1. Tiyakin na ang animator ay available
        if (targetPosAnimator == null) return;

        // 2. Tiyakin na hindi pa nagsisimula ang buong cut sequence
        if (cableCuttingTarget != null && cableCuttingTarget.GetIsCutSequenceStarted())
        {
            Debug.Log("CUTTING: Sequence already started/finished.");
            return;
        }

        // Simulan ang Coroutine para i-trigger ang animation. Ito ang mag-aayos ng timing issue.
        StartCoroutine(TriggerCutAnimationSafely());
    }

    /// <summary>
    /// TATAWAGIN ITO NG UNITY ANIMATION EVENT sa dulo ng cutting animation.
    /// </summary>
    public void OnCutAnimationEnd()
    {
        if (cableCuttingTarget != null)
        {
            Debug.Log("CUTTING: Animation finished. Calling AfterCutAnimationFinished on target.");
            // Tawagin ang Step 1 sa CableCutting script (Camera Swap at Ready for Click)
            cableCuttingTarget.AfterCutAnimationFinished();
        }
        else
        {
            Debug.LogError("WireCutterAnimator: CableCutting target is NULL, cannot complete cut sequence.");
        }
    }

    public void ResetCut()
    {
        // Maaaring tawagin ng ibang script upang i-reset ang state (hal. kung mayroon man)
    }
}