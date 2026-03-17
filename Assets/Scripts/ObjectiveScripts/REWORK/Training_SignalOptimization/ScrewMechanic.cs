using UnityEngine;

public class ScrewdriverController : MonoBehaviour
{
    [Header("Positions")]
    public Transform screwPosition;
    public Transform parkingPosition;

    [Header("Settings")]
    public float rotateSpeed = 50f;
    public float loosenThreshold = 90f;
    public float tightenThreshold = 90f;

    [Header("References")]
    public AntennaController antennaController;
    public AbstractObjectiveManager manager;

    private float accumulatedRotation = 0f;
    
    private enum ScrewdriverState { Idle, Loosening, WaitingForInteraction, MovingToScrew, Tightening }
    private ScrewdriverState currentState = ScrewdriverState.Idle;

    private const string ScrewPrompt = "Hold [Q] or [E] to Loosen/Tighten Bracket";
    private const string ClickPrompt = "Click the Screwdriver to Lock the Antenna";
    
    void Start()
    {
        if (manager == null) manager = FindObjectOfType<AbstractObjectiveManager>();
    }

    void Update()
    {
        switch (currentState)
        {
            case ScrewdriverState.Loosening:
                HandleRotationInput(loosenThreshold, OnBracketLoosened);
                PromptManager.Instance?.RequestPrompt(this, ScrewPrompt, 1);
                break;
                
            case ScrewdriverState.WaitingForInteraction:
                CheckScrewdriverClick();
                PromptManager.Instance?.RequestPrompt(this, ClickPrompt, 1);
                break;
                
            case ScrewdriverState.Tightening:
                HandleRotationInput(tightenThreshold, OnBracketTightened);
                PromptManager.Instance?.RequestPrompt(this, ScrewPrompt, 1);
                break;
        }
    }

    public void BeginLooseningObjective()
    {
        currentState = ScrewdriverState.Loosening;
        accumulatedRotation = 0f;
    }
    
    public void BeginTighteningObjective()
    {
        currentState = ScrewdriverState.WaitingForInteraction;
        if(antennaController != null) antennaController.DisableControl();
    }

    private void HandleRotationInput(float threshold, System.Action onComplete)
    {
        float input = Input.GetKey(KeyCode.Q) ? -1f : Input.GetKey(KeyCode.E) ? 1f : 0f;
        if (input != 0f)
        {
            float rotateAmount = input * rotateSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, rotateAmount);
            accumulatedRotation += Mathf.Abs(rotateAmount);

            if (accumulatedRotation >= threshold)
            {
                onComplete?.Invoke();
            }
        }
    }
    
    private void OnBracketLoosened()
    {
        if (parkingPosition != null)
        {
            transform.position = parkingPosition.position;
            transform.rotation = parkingPosition.rotation;
        }
        currentState = ScrewdriverState.Idle;
        
        var currentObjective = manager?.GetCurrentObjective() as T5_LoosenBracketObjective;
        currentObjective?.NotifyBracketLoosened();
    }

    private void CheckScrewdriverClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 3f) && hit.collider.gameObject == this.gameObject)
            {
                ReturnToScrewPosition();
            }
        }
    }

    private void ReturnToScrewPosition()
    {
        if (screwPosition != null)
        {
            transform.position = screwPosition.position;
            transform.rotation = screwPosition.rotation;
            accumulatedRotation = 0f;
            currentState = ScrewdriverState.Tightening;
        }
    }
    
    private void OnBracketTightened()
    {
        if (parkingPosition != null)
        {
            transform.position = parkingPosition.position;
            transform.rotation = parkingPosition.rotation;
        }
        currentState = ScrewdriverState.Idle;

        var currentObjective = manager?.GetCurrentObjective() as T5_TightenBracketObjective;
        currentObjective?.NotifyBracketTightened();
    }
}