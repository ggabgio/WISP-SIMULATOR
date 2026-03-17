using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OutlinePulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{   
    public Color colorA = Color.black;
    public Color colorB = Color.white;
    public float pulseDuration = 1f;

    private Outline outline;
    private Button button;
    private bool isHovering = false;
    private float pulseTimer = 0f;

    void Start()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = colorA;
        }
    }

    void Update()
    {
        if (isHovering && outline != null && button != null && button.interactable)
        {
            pulseTimer += Time.deltaTime;
            float t = Mathf.PingPong(pulseTimer / pulseDuration, 1f);
            outline.effectColor = Color.Lerp(colorA, colorB, t);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable) 
        {
            isHovering = true;
            pulseTimer = 0f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (outline != null)
        {
            outline.effectColor = colorA;
        }
    }
}
