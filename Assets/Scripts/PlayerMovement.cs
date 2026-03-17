using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float gravity = 30f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    private float verticalVelocity = 0f;

    public Slider staminaBar;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    public bool canMove = true;

    // Sprint & Stamina System
    public float maxStamina = 2f; // Sprint duration 2 seconds
    private float currentStamina;
    public float staminaRegenDelay = 2f;
    private float staminaRegenRate;
    private bool isSprinting;
    private bool isRecoveringStamina;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentStamina = maxStamina;
        staminaRegenRate = maxStamina / 2f;
        
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        }
        
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = maxStamina;
        }
    }

    void Update()
    {
        // Don't process movement or camera rotation if game is paused (Time.timeScale = 0)
        if (Time.timeScale == 0f)
        {
            return;
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Sprinting logic
        bool sprintKeyPressed = Input.GetKey(KeyCode.LeftShift);
        bool canSprint = sprintKeyPressed && currentStamina > 0 && !isRecoveringStamina;

        if (canSprint)
        {
            isSprinting = true;
            currentStamina -= maxStamina / 2f * Time.deltaTime; // Depletes in 2 sec
        }
        else
        {
            isSprinting = false;
        }

        if (!sprintKeyPressed && currentStamina < maxStamina && !isRecoveringStamina)
        {
            StartCoroutine(RegenStamina());
        }

        // Update Stamina Bar UI
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }

        // Set movement speed based on sprinting/crouching
        float speed = isSprinting ? runSpeed : walkSpeed;

        // Apply movement
        float curSpeedX = canMove ? speed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? speed * Input.GetAxis("Horizontal") : 0;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = verticalVelocity;

        if (characterController.isGrounded)
        {
            verticalVelocity = -1f; // slight downward push to stay grounded
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Mouse Look
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    IEnumerator RegenStamina()
    {
        isRecoveringStamina = true;
        yield return new WaitForSeconds(staminaRegenDelay);

        while (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (staminaBar != null)
            {
                staminaBar.value = currentStamina;
            }
            yield return null;
        }

        currentStamina = maxStamina;
        isRecoveringStamina = false;
    }
}
