using UnityEngine;

public class LadderPlacementHandler : MonoBehaviour
{
    [Tooltip("The Rigidbody belonging to the actual Ladder model (likely a child object).")]
    public Rigidbody ladderRigidbody; // Assign the child Ladder's Rigidbody here in the Inspector

    [Tooltip("The force applied to make the ladder fall forward after placement.")]
    public float fallForwardForce = 2.0f;

    // Removed forceUprightPreview setting as ObjectPlacer will now handle this check
    // public bool forceUprightPreview = true;

    void Awake()
    {
        if (ladderRigidbody == null)
        {
            ladderRigidbody = GetComponentInChildren<Rigidbody>();
            if (ladderRigidbody == null || ladderRigidbody.gameObject == this.gameObject)
            {
                Debug.LogError("LadderPlacementHandler requires a Rigidbody component on a child object (the Ladder model). Please assign it in the Inspector.", this);
                enabled = false;
            }
            else if (ladderRigidbody != null)
            {
                 Debug.Log($"LadderPlacementHandler automatically found Rigidbody on child: {ladderRigidbody.name}", this);
            }
        }
    }

    // Method to calculate just the base upright rotation based on player aim
    public Quaternion GetUprightRotation(Quaternion basePlayerAimRotation)
    {
        // For the ladder, we likely always want it upright based on player forward, ignoring scroll wheel.
        return basePlayerAimRotation;
    }

    public void ApplyPlacementPhysics()
    {
        if (ladderRigidbody == null)
        {
            Debug.LogError("Cannot apply placement physics: Ladder Rigidbody is not assigned or found.", this);
            return;
        }

        ladderRigidbody.isKinematic = false;
        ladderRigidbody.useGravity = true;
        ladderRigidbody.WakeUp();

        Vector3 forceDirection = ladderRigidbody.transform.forward;
        ladderRigidbody.AddForce(forceDirection * fallForwardForce, ForceMode.Impulse);

        Debug.Log($"Applied fall force to {ladderRigidbody.name}");
    }
}