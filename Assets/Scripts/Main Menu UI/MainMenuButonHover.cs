using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private SmoothButtonExpand parent;
    private int index;

    [Header("Behavior Settings")]
    [Tooltip("If true, this button will reset its hover state after being clicked.")]
    public bool resetOnClick = false;

    void Start()
    {
        parent = GetComponentInParent<SmoothButtonExpand>();
        index = parent.buttons.FindIndex(b => b.rect == GetComponent<RectTransform>());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parent.SetHovered(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parent.ClearHovered();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (resetOnClick)   //  Only reset for selected buttons
            parent.ClearHovered();
    }

    private void OnDisable()
    {
        // Safety reset if the panel is hidden
        if (parent != null)
            parent.ClearHovered();
    }
}
