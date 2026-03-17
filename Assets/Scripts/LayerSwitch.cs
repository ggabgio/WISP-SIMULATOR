using UnityEngine;

public class LayerSwitcher : MonoBehaviour
{
    
    private string holdLayerName = "holdLayer";
    private string defaultLayerName = "Default";

    private int holdLayer;
    private int defaultLayer;

    private void Awake()
    {
        holdLayer = LayerMask.NameToLayer(holdLayerName);
        defaultLayer = LayerMask.NameToLayer(defaultLayerName);
    }

    public void SwitchToHoldLayer()
    {
        SetLayerRecursively(transform, holdLayer);
    }

    public void SwitchToDefaultLayer()
    {
        SetLayerRecursively(transform, defaultLayer);
    }

    private void SetLayerRecursively(Transform obj, int newLayer)
    {
        obj.gameObject.layer = newLayer;
        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, newLayer);
        }
    }
}
