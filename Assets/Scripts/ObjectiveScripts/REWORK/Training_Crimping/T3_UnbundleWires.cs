using UnityEngine;

public class T3_UnbundleWiresObjective : BaseObjective
{
    [Header("Configuration")]
    [Tooltip("The total number of wire bundles that need to be clicked.")]
    public int totalBundles = 8; 
    public int unbundledCount = 0;
    
    // Variable to track if all wires have been clicked
    private bool allWiresClicked = false;

    // Asset Management
    [Header("Asset Management")]
    public GameObject bundledCableAsset; 
    public GameObject unbundledCableAsset; 

    // 1. Override OnObjectiveStart()
    protected override void OnObjectiveStart()
    {
        Debug.Log("Objective Started: Unbundle Wires");
        
        // Reset the counter and flag at the start
        unbundledCount = 0;
        allWiresClicked = false;
        
        // Ensure the bundled asset is active and the unbundled is inactive initially
        if (bundledCableAsset != null) bundledCableAsset.SetActive(true);
        if (unbundledCableAsset != null) unbundledCableAsset.SetActive(false);
    }

    // 2. Override OnObjectiveComplete()
    protected override void OnObjectiveComplete()
    {
        Debug.Log("Objective Complete: Unbundle Wires. Manager proceeding to next objective...");
    }
    
    // Update loop to detect the 'E' key press
    void Update()
    {
        // Check if all wires are clicked AND the objective is still active (waiting for 'E')
        if (IsActive && allWiresClicked)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PerformAssetSwap();
                
                // ⭐ SCORE FIX (Reference: T3_StripCableObjective): 
                // Set the maximum score before completing the objective to avoid 0%.
                SetScore(maxScore); 
                
                CompleteObjective(); // Complete the objective after the swap
            }
        }
    }

    // Custom Method called by BundledWireClick
    public void NotifyWireUnbundled()
    {
        if (!IsActive || allWiresClicked) return; // Ignore if complete or not active
        
        unbundledCount++;
        
        Debug.Log($"Unbundled Wires Progress: {unbundledCount} / {totalBundles}"); 

        if (unbundledCount >= totalBundles)
        {
            // LOGIC: Set the flag to wait for the 'E' key press in Update().
            allWiresClicked = true;
            Debug.Log("All wires clicked! Press 'E' to finalize the unbundling and continue.");
        }
    }
    
    // Separates the Asset Swap logic
    private void PerformAssetSwap()
    {
        if (bundledCableAsset != null && unbundledCableAsset != null)
        {
            bundledCableAsset.SetActive(false); 
            unbundledCableAsset.SetActive(true);
            Debug.Log("Asset Swap completed by pressing 'E'.");
        }
        else
        {
            Debug.LogError("Asset Swap failed: Bundled or Unbundled Cable Asset is missing a reference.");
        }
    }
}