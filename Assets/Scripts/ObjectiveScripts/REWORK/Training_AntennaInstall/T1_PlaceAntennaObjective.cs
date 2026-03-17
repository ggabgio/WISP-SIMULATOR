using TMPro;
using UnityEngine;

public class T1_PlaceAntennaObjective : BaseObjective
{
    protected override void OnObjectiveStart()
    {
        if (objectiveDisplay != null) objectiveDisplay.text = objectiveHint;
        Debug.Log("[T1_PlaceAntenna] Started");
        GameplayEvents.ObjectPlaced += HandleObjectPlaced;
    }

    protected override void OnObjectiveComplete()
    {
        Debug.Log("[T1_PlaceAntenna] Completed");
        GameplayEvents.ObjectPlaced -= HandleObjectPlaced;
    }

    private void OnDisable()
    {
        GameplayEvents.ObjectPlaced -= HandleObjectPlaced;
    }

    private void HandleObjectPlaced(GameObject placedObject)
    {
        if (!isActive) return;

        bool isAntenna = CheckIfObjectIsAntenna(placedObject);
        if (!isAntenna) return;

        bool isOnRoof = CheckPlacementLocation(placedObject);

        if (isOnRoof)
        {
            Debug.Log("Antenna placed on the roof. Objective complete.");
            SetScore(maxScore);
            CompleteObjective();
        }
        else
        {
            // This is the message you are seeing in your log
            Debug.Log("Antenna placed in the wrong location.");
            PromptManager.Instance?.RequestPrompt(this, "The antenna must be placed on the roof.", 3);
        }
    }

    private bool CheckIfObjectIsAntenna(GameObject placedObject)
    {
        if (placedObject.CompareTag("Antenna")) return true;
        
        var ii = placedObject.GetComponent<ItemInstance>();
        if (ii != null && ii.data != null && ii.data.itemName.ToLower().Contains("antenna")) return true;
        
        if (placedObject.name.ToLower().Contains("antenna")) return true;
        
        return false;
    }

    private bool CheckPlacementLocation(GameObject placedObject)
    {
        // Define the ray's starting point and direction
        Vector3 rayStartPoint = placedObject.transform.position + Vector3.up * 0.1f;
        Vector3 rayDirection = Vector3.down;
        float rayLength = 5.0f;

        // VISUAL DEBUGGING: Draw a red line in the scene view for 5 seconds to show the raycast
        Debug.DrawRay(rayStartPoint, rayDirection * rayLength, Color.red, 5.0f);

        // Perform the raycast
        if (Physics.Raycast(rayStartPoint, rayDirection, out RaycastHit hit, rayLength))
        {
            // LOGGING: Tell us what we hit
            Debug.Log($"Raycast hit: '{hit.collider.gameObject.name}' which has the tag: '{hit.collider.tag}'");

            // The actual check
            if (hit.collider.CompareTag("Roof"))
            {
                return true; // Success!
            }
        }
        else
        {
            // LOGGING: Tell us if we hit nothing
            Debug.Log("Raycast did not hit any colliders below the antenna.");
        }

        return false; // The check failed
    }
}