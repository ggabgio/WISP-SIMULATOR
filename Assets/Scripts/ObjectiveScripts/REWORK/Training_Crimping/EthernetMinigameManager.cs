using UnityEngine;

public class EthernetMinigameManager : MonoBehaviour
{
    public GameObject rj45Plug;
    public GameObject crimpingTool;

    [Header("Camera References")]
    public Camera cameraShowingWires;
    public Camera cameraForRJ45Placement;

    private CrimpingManager manager; // Changed reference

    private bool wiresArrangedCorrectly = false;
    private bool rj45Attached = false;
    private bool firstSwapDone = false;

    private Wire[] wires;
    private Wire firstSelectedWire = null;

    void Start()
    {
        if (rj45Plug != null) rj45Plug.SetActive(true);
        if (crimpingTool != null) crimpingTool.SetActive(true);

        wires = FindObjectsOfType<Wire>();
    }

    
    // This is called by T3_ArrangeWiresObjective when it starts
    public void BeginArrangementObjective(AbstractObjectiveManager objectiveManager)
    {
        this.manager = objectiveManager as CrimpingManager;
    }

    public bool IsRJ45Attached()
    {
        return rj45Attached;
    }

    public bool AreWiresArrangedCorrectly()
    {
        return wiresArrangedCorrectly;
    }

    public void CheckWireArrangement()
    {
        bool wasCorrectBeforeCheck = wiresArrangedCorrectly;
        wiresArrangedCorrectly = IsCorrectArrangement();

        if (wiresArrangedCorrectly && !wasCorrectBeforeCheck)
        {
            Debug.Log("Wire arrangement is correct!");
            
            // Notify the objective
            var currentObjective = manager?.GetCurrentObjective() as T3_ArrangeWiresObjective;
            currentObjective?.NotifyArrangementCorrect();

            if (cameraShowingWires != null && cameraForRJ45Placement != null)
            {
                if(cameraShowingWires.enabled) cameraShowingWires.enabled = false;
                cameraForRJ45Placement.enabled = true;
            }
        }
        else if (!wiresArrangedCorrectly && wasCorrectBeforeCheck)
        {
             Debug.Log("Wire arrangement is now incorrect.");
             if (cameraShowingWires != null && cameraForRJ45Placement != null && cameraForRJ45Placement.enabled) 
             {
                cameraForRJ45Placement.enabled = false;
                cameraShowingWires.enabled = true;
             }
        }
    }

    bool IsCorrectArrangement()
    {
        wires = FindObjectsOfType<Wire>();
        if (wires == null || wires.Length != 8) return false;
        System.Array.Sort(wires, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        string[] t568bColors = { "Orange/White", "Orange", "Green/White", "Blue", "Blue/White", "Green", "Brown/White", "Brown" };
        for (int i = 0; i < wires.Length; i++)
        {
            if (wires[i].GetWireColor() != t568bColors[i]) return false;
        }
        return true;
    }

    public void OnRJ45Attached()
    {
        if (wiresArrangedCorrectly)
        {
            if (!rj45Attached)
            {
                rj45Attached = true;
                Debug.Log("RJ45 physically attached to wires.");
            }
        }
    }

    public void SelectWire(Wire selectedWireComponent)
    {
        if (manager != null && !manager.GetCurrentObjective().IsActive) return;
        if (selectedWireComponent == null) return;

        if (firstSelectedWire == null)
        {
            firstSelectedWire = selectedWireComponent;
            firstSelectedWire.isSelected = true;
        }
        else
        {
            firstSelectedWire.isSelected = false;
            if (firstSelectedWire != selectedWireComponent)
            {
                firstSelectedWire.SwapPosition(selectedWireComponent);
                CheckWireArrangement();
            }
            firstSelectedWire = null;
        }
    }
}