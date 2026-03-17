using UnityEngine;

public class CableCutting : MonoBehaviour
{
    public GameObject sealedCable;
    public GameObject bundledCable;
    public CrimpingManager manager;

    [Header("Wire Bundle Interaction")]
    [Tooltip("Ito ang target na i-click ng user para mag-final swap ng visual. Dapat ito ang SealedCable.")]
    public GameObject cableClickTarget; 
    
    [Header("Camera References for Cut Transition")]
    public Camera cameraBeforeCut;
    public Camera cameraAfterCut_WireView;

    private bool isCutSequenceStarted = false; // Triggered by Wire Cutter OnMouseDown
    private bool isReadyForFinalClick = false; // Triggered after cutter animation

    private void Start()
    {
        if (manager == null) manager = FindFirstObjectByType<CrimpingManager>();

        // Tiyakin na ang bundled cable ay naka-deactivate sa simula
        if (bundledCable != null) bundledCable.SetActive(false);
    }
    
    // Tinanggal ang buong OnTriggerEnter() method

    /// <summary>
    /// HAKBANG 1: Pagkatapos ng Wire Cutter animation, magpalit ng camera at ihanda para sa click.
    /// Tanging ang WireCutterAnimator lamang ang tatawag dito.
    /// </summary>
    public void AfterCutAnimationFinished()
    {
        if (isReadyForFinalClick) return;
        isReadyForFinalClick = true;
        isCutSequenceStarted = true; // Para hindi na ulitin ang Wire Cutter animation

        // 1. Camera Change: Mula sa general view > Wire View
        if (cameraBeforeCut != null && cameraAfterCut_WireView != null)
        {
            cameraBeforeCut.enabled = false;
            cameraAfterCut_WireView.enabled = true;
        }

        Debug.Log("✂️ CableCutting: Animation finished. Ready for final click to swap visuals.");
    }
    
    /// <summary>
    /// HAKBANG 2: Pagkatapos ng animation, ikiklick ng user ang cable para sa final swap.
    /// Kailangan ng Collider ang cableClickTarget/SealedCable.
    /// </summary>
    private void OnMouseDown()
    {
        // Tiyaking ang kiniklik ay ang Sealed Cable at handa na ito.
        if (!isReadyForFinalClick || sealedCable == null || bundledCable == null) return;
        
        // 1. Visual Swap: Sealed Cable -> Bundled Wires
        sealedCable.SetActive(false);
        bundledCable.SetActive(true);
        
        // 2. Objective Notification: Tapos na ang Stripping/Cutting objective
        var currentObjective = manager?.GetCurrentObjective() as T3_StripCableObjective;
        currentObjective?.NotifyCableStripped();

        isReadyForFinalClick = false; // Tapos na ang buong cutting process
        Debug.Log("✅ CableCutting Final Click: Visual swapped and objective notified.");
    }
    
    // Ginagamit ito ng WireCutterAnimator para makita kung dapat nang gumana ang cutter
    public bool GetIsCutSequenceStarted()
    {
        return isCutSequenceStarted;
    }
}