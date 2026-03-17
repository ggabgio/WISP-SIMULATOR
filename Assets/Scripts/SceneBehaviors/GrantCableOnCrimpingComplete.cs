using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene-specific helper for assessment scene: when the Crimping level finishes,
/// grant the player a single item (CableLayingCable) into their inventory.
/// Attach this script to any GameObject in the newTest_Duplicate.unity scene.
/// Assign the prefab (with proper visuals/colliders) and ItemData in the Inspector.
/// </summary>
public class GrantCableOnCrimpingComplete : MonoBehaviour
{
    [Header("Required References")]
    [Tooltip("Prefab representing the CableLayingCable world object (will be instantiated and hidden in inventory).")]
    public GameObject cableLayingCablePrefab;

    [Tooltip("ItemData asset for CableLayingCable.")]
    public ItemData cableLayingCableData;

    [Header("Settings")]
    [Tooltip("Grant only once per scene run.")]
    public bool grantOnce = true;

    [Tooltip("Optional: Only grant when inside this scene name (leave empty to allow any).")]
    public string requiredSceneName = "newTest_Duplicate";

    private bool granted;
    private CrimpingManager crimpingManager;

    private void Awake()
    {
        // Ensure this only runs on the intended scene if specified
        if (!string.IsNullOrEmpty(requiredSceneName))
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene == null || activeScene.name != requiredSceneName)
            {
                enabled = false;
                return;
            }
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
        if (crimpingManager == null)
            StartCoroutine(RetrySubscribeRoutine());
    }

    private void OnDisable()
    {
        if (crimpingManager != null)
        {
            crimpingManager.OnLevelFinished -= HandleCrimpingFinished;
        }
    }

    private void TrySubscribe()
    {
        // 1) Try find active first
        crimpingManager = FindFirstObjectByType<CrimpingManager>();

        // 2) Fallback: include inactive
        if (crimpingManager == null)
        {
            crimpingManager = FindObjectOfType<CrimpingManager>(true);
        }

        // 3) Fallback: look for CombinedLevelManager reference
        if (crimpingManager == null)
        {
            var combined = FindFirstObjectByType<CombinedLevelManager>();
            if (combined == null)
            {
                combined = FindObjectOfType<CombinedLevelManager>(true);
            }
            if (combined != null && combined.crimpingManager != null)
            {
                crimpingManager = combined.crimpingManager;
            }
        }

        if (crimpingManager != null)
        {
            crimpingManager.OnLevelFinished -= HandleCrimpingFinished; // avoid dupes
            crimpingManager.OnLevelFinished += HandleCrimpingFinished;
            Debug.Log("[GrantCableOnCrimpingComplete] Subscribed to CrimpingManager.OnLevelFinished.");
        }
        else
        {
            Debug.LogWarning("[GrantCableOnCrimpingComplete] Could not find CrimpingManager in scene. Will retry shortly.");
        }
    }

    private System.Collections.IEnumerator RetrySubscribeRoutine()
    {
        const int attemptsMax = 10;
        for (int i = 0; i < attemptsMax && crimpingManager == null; i++)
        {
            yield return null; // wait a frame
            TrySubscribe();
        }
    }

    private void HandleCrimpingFinished(float totalScore, float totalTime)
    {
        if (grantOnce && granted) return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[GrantCableOnCrimpingComplete] InventoryManager.Instance not found; cannot grant item.");
            return;
        }

        if (cableLayingCablePrefab == null || cableLayingCableData == null)
        {
            Debug.LogWarning("[GrantCableOnCrimpingComplete] Missing prefab or ItemData reference; cannot grant item.");
            return;
        }

        // Avoid duplicates if already present
        if (InventoryManager.Instance.HasItem(cableLayingCableData.itemName))
        {
            Debug.Log("[GrantCableOnCrimpingComplete] Item already in inventory; skipping grant.");
            granted = true;
            return;
        }

        // Instantiate a world object instance to add into inventory
        GameObject spawned = Instantiate(cableLayingCablePrefab);
        spawned.name = cableLayingCablePrefab.name;

        // Ensure ItemInstance carries the correct ItemData
        ItemInstance instance = spawned.GetComponent<ItemInstance>();
        if (instance == null)
        {
            instance = spawned.AddComponent<ItemInstance>();
        }
        instance.data = cableLayingCableData;

        bool added = InventoryManager.Instance.AddItem(spawned, cableLayingCableData);
        if (added)
        {
            Debug.Log("[GrantCableOnCrimpingComplete] Granted CableLayingCable to player inventory.");
            granted = true;
        }
        else
        {
            Debug.LogWarning("[GrantCableOnCrimpingComplete] Failed to add item to inventory.");
            // If add failed, clean up the spawned object to avoid leak
            Destroy(spawned);
        }
    }
}

