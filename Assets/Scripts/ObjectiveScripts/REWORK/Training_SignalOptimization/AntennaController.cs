using UnityEngine;

public class AntennaController : MonoBehaviour
{
    public Transform horizontalPivot;
    public Transform antennaMover;
    public Transform bracketMover;
    public PlayerMovement playerMovement;

    public float rotateSpeed = 50f;
    public float slideSpeed = 1f;
    public float slideRange = 1f;

    private bool canControl = false;
    private float initialAntennaY;
    private float initialBracketY;

    private const string AdjustPrompt = "Use [WASD] or [Arrow Keys] to Align Antenna";

    public void EnableControl()
    {
        canControl = true;
        if (antennaMover != null) initialAntennaY = antennaMover.position.y;
        if (bracketMover != null) initialBracketY = bracketMover.position.y;
        if (playerMovement != null) playerMovement.canMove = false;
    }

    public void DisableControl()
    {
        canControl = false;
        if (playerMovement != null) playerMovement.canMove = true;
    }

    void Update()
    {
        if (!canControl) return;
        
        PromptManager.Instance?.RequestPrompt(this, AdjustPrompt, 1);

        float h = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? -1 :
                  Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
        if (h != 0) horizontalPivot.Rotate(Vector3.up, h * rotateSpeed * Time.deltaTime);

        float v = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 :
                  Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
        if (v != 0)
        {
            if (antennaMover != null)
            {
                Vector3 pos = antennaMover.position;
                pos.y = Mathf.Clamp(pos.y + v * slideSpeed * Time.deltaTime, initialAntennaY - slideRange, initialAntennaY + slideRange);
                antennaMover.position = pos;
            }
            if (bracketMover != null)
            {
                Vector3 pos = bracketMover.position;
                pos.y = Mathf.Clamp(pos.y + v * slideSpeed * Time.deltaTime, initialBracketY - slideRange, initialBracketY + slideRange);
                bracketMover.position = pos;
            }
        }
    }
}