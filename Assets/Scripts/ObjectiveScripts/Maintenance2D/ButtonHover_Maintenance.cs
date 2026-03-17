using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover_Maintenance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private SmoothButtonExpand_Maintenance parent;
    private int index;

    [Header("Behavior Settings")]
    [Tooltip("If true, this button will reset its hover state after being clicked.")]
    public bool resetOnClick = false;

    void Start()
    {
        parent = GetComponentInParent<SmoothButtonExpand_Maintenance>();
        index = parent.GetButtonIndex(GetComponent<RectTransform>());
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
        if (resetOnClick)
            parent.ClearHovered();
    }

    private void OnDisable()
    {
        if (parent != null)
            parent.ClearHovered();
    }
}
