using UnityEngine;

public class RJ45ClickRelay : MonoBehaviour
{
    public CrimpingTool crimpingTool;

    private void OnMouseDown()
    {
        if (crimpingTool != null)
            crimpingTool.SendMessage("OnMouseDown");
    }
}
