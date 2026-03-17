using UnityEngine;

public class RJ45Animator : MonoBehaviour
{
    // I-drag and drop mo ang 3D object na may Animator component (Kung hindi ang mismong object)
    // Kung ang script na ito ay naka-attach sa object na may Animator, 
    // maaari mo itong palitan ng [SerializeField] private Animator rj45Animator; 
    // at kunin ito sa Start() gamit ang GetComponent<Animator>().
    public Animator rj45Animator; 
    
    // Pangalan ng Trigger na gagamitin natin sa Animator
    private const string ANIMATION_TRIGGER_NAME = "StartPlugIn"; 

    void Start()
    {
        // Safety check: Kunin ang Animator kung hindi pa naka-set sa Inspector
        if (rj45Animator == null)
        {
            rj45Animator = GetComponent<Animator>();
            if (rj45Animator == null)
            {
                Debug.LogError("Walang Animator Component na nakita sa object! Kailangan ng Animator para gumana ang animation.");
            }
        }
        
        // Safety check para sa Click: Kailangan ng Collider!
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("Walang Collider Component na nakita! Kailangan ng Collider (e.g., Box Collider) para maging clickable ang object.");
        }

        Debug.Log("RJ45 Animator Ready. Click the object to start animation (Trigger: " + ANIMATION_TRIGGER_NAME + ")");
    }

    // Unity function na tinatawag kapag na-click ang Collider ng object na ito
    void OnMouseDown()
    {
        TriggerAnimation();
    }

    // Ito ang function na tatawag sa Animator Trigger
    public void TriggerAnimation()
    {
        if (rj45Animator != null)
        {
            // Ito ang magsisimula ng animation sa Animator State Machine
            rj45Animator.SetTrigger(ANIMATION_TRIGGER_NAME);
            Debug.Log("Animator Trigger '" + ANIMATION_TRIGGER_NAME + "' Fired!");
        }
    }
}