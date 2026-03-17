using UnityEngine;

/// <summary>
/// Controls the movement and rotation of a GameObject (intended for a wire cutter)
/// from its current position and rotation to specified target position and rotation.
/// This script does not interact with wires or manage other game elements.
/// It triggers movement when the GameObject itself is clicked.
/// </summary>
public class SimpleWireCutterMover : MonoBehaviour
{
    [Header("Cutter Movement Settings")]
    [Tooltip("The target position the cutter will move to.")]
    public Vector3 targetPosition = Vector3.zero; // Default to origin (0,0,0)
    [Tooltip("The speed at which the cutter animates towards its target (units per second).")]
    public float animationSpeed = 5f;

    [Header("Cutter Rotation Settings")]
    [Tooltip("The target rotation the cutter will rotate to (Euler angles: X, Y, Z degrees).")]
    public Vector3 targetRotationEuler = Vector3.zero; // Default to no rotation
    [Tooltip("The speed at which the cutter animates its rotation (degrees per second).")]
    public float rotationSpeed = 100f;

    // Internal flag to control if the cutter should be moving/rotating
    private bool isMoving = false;
    // Internal Quaternion representation of the target rotation for smooth interpolation
    private Quaternion targetRotationQuaternion;

    // Reference to the Rigidbody component (if any)
    private Rigidbody rb;
    private Rigidbody2D rb2d;


    /// <summary>
    /// Called when the script instance is being loaded.
    /// Converts the initial target Euler angles to a Quaternion for internal use.
    /// Gets Rigidbody reference and sets it to kinematic.
    /// </summary>
    void Awake()
    {
        // Convert the user-friendly Euler angles to a Quaternion for rotation calculations.
        targetRotationQuaternion = Quaternion.Euler(targetRotationEuler);

        // Get Rigidbody (for 3D) or Rigidbody2D (for 2D) and set it to kinematic
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Essential for manual transform control with a Rigidbody
            Debug.Log("Rigidbody found and set to Kinematic (3D).");
        }
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.isKinematic = true; // Essential for manual transform control with a Rigidbody2D
            Debug.Log("Rigidbody2D found and set to Kinematic (2D).");
        }
    }

    /// <summary>
    /// Called once per frame.
    /// Updates the cutter's position and rotation if it is currently set to move.
    /// </summary>
    void Update()
    {
        if (isMoving)
        {
            // Calculate the step size for this frame, ensuring it's frame-rate independent.
            float moveStep = animationSpeed * Time.deltaTime;
            float rotateStep = rotationSpeed * Time.deltaTime;

            // --- Position Movement ---
            // Move the cutter towards the target position.
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveStep);

            // --- Rotation Movement ---
            // Rotate the cutter towards the target rotation.
            // Quaternion.RotateTowards moves the rotation by a maximum angle 'rotateStep'.
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotationQuaternion, rotateStep);

            // --- Check for Completion ---
            // Determine if the position has been reached (within a small tolerance).
            bool positionReached = Vector3.Distance(transform.position, targetPosition) < 0.01f;
            // Determine if the rotation has been reached (within a small angular tolerance).
            bool rotationReached = Quaternion.Angle(transform.rotation, targetRotationQuaternion) < 0.1f;

            // Debugging: Log current and target positions/rotations
            // Debug.Log($"[Moving] Current Pos: {transform.position} | Target Pos: {targetPosition} | Dist: {Vector3.Distance(transform.position, targetPosition):F4}");
            // Debug.Log($"[Moving] Current Rot: {transform.rotation.eulerAngles} | Target Rot: {targetRotationEuler} | Angle: {Quaternion.Angle(transform.rotation, targetRotationQuaternion):F4}");


            // If both position and rotation targets are reached, stop the movement.
            if (positionReached && rotationReached)
            {
                // Snap to exact target position and rotation to ensure precision at the end.
                transform.position = targetPosition;
                transform.rotation = targetRotationQuaternion;
                isMoving = false; // Stop movement
                Debug.Log("Wire Cutter reached target position and rotation!");
            }
        }
    }

    /// <summary>
    /// Called when the mouse button is pressed while over this GameObject's collider.
    /// This is the new entry point for triggering the cutter's movement by clicking it.
    /// </summary>
    void OnMouseDown()
    {
        Debug.Log("Wire Cutter GameObject was clicked!");
        MoveCutterToTarget(); // Call the method to start movement
    }

    /// <summary>
    /// Public method to start the cutter's movement and rotation towards the target.
    /// This method can be called from UI buttons, other scripts, or Unity Events.
    /// </summary>
    public void MoveCutterToTarget()
    {
        // Only start moving if not already moving to prevent re-triggering mid-animation.
        if (!isMoving)
        {
            // Ensure the internal Quaternion is up-to-date with any Inspector changes to targetRotationEuler.
            targetRotationQuaternion = Quaternion.Euler(targetRotationEuler);
            isMoving = true; // Set the flag to true to initiate movement in Update()
            Debug.Log($"Wire Cutter starting movement to: {targetPosition} and rotation to: {targetRotationEuler}");
        }
        else
        {
            Debug.Log("Wire Cutter is already moving.");
        }
    }

    /// <summary>
    /// Optional: Public method to immediately stop the cutter's movement and rotation.
    /// </summary>
    public void StopCutterMovement()
    {
        isMoving = false;
        Debug.Log("Wire Cutter movement and rotation stopped.");
    }

    /// <summary>
    /// Optional: Public method to set a new target position dynamically.
    /// </summary>
    /// <param name="newTargetPos">The new Vector3 target position.</param>
    public void SetNewTargetPosition(Vector3 newTargetPos)
    {
        targetPosition = newTargetPos;
        Debug.Log($"Wire Cutter target position updated to: {targetPosition}");
    }

    /// <summary>
    /// Optional: Public method to set a new target rotation dynamically (using Euler angles).
    /// </summary>
    /// <param name="newTargetRotEuler">The new Vector3 target rotation in Euler angles.</param>
    public void SetNewTargetRotation(Vector3 newTargetRotEuler)
    {
        targetRotationEuler = newTargetRotEuler;
        targetRotationQuaternion = Quaternion.Euler(targetRotationEuler); // Update the internal Quaternion
        Debug.Log($"Wire Cutter target rotation updated to: {newTargetRotEuler}");
    }
}





