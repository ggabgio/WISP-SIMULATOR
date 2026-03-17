using UnityEngine;
using UnityEngine.EventSystems;

public class HoverZoomFromParent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public HoverZoomImage imageZoomScript; // Drag your image here in the Inspector

    public void OnPointerEnter(PointerEventData eventData)
    {
        imageZoomScript.ZoomIn();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        imageZoomScript.ZoomOut();
    }
}
