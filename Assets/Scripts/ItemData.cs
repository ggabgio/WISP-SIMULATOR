using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    // public GameObject prefab;
    public bool isBigItem; // Mark true for big items 
    public bool isCrimpingTool; //Added
    public bool isLadder;
    public bool isWire;

    [Header("Visual Settings")]
    [Tooltip("Scale multiplier applied when item is held in hand.")]
    public float equipScaleMultiplier = 1f; // default normal size
}