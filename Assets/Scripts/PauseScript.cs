using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panel")]
    [Tooltip("Assign the GameObject that is your Pause Menu Panel.")]
    public GameObject pauseMenuPanel;

    [Header("Player Control")]
    [Tooltip("Assign the PlayerMovement script instance from your player GameObject.")]
    public PlayerMovement playerMovementScript;

    private bool isPaused = false;
    private float previousTimeScale = 1f;

    // These variables will store the cursor's state before pausing.
    private bool wasCursorVisibleBeforePause;
    private CursorLockMode previousCursorLockMode;

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            enabled = false;
            return;
        }

        if (playerMovementScript == null)
        {
            Debug.LogWarning("PauseMenuManager: PlayerMovement script is not assigned. Player control will not be affected by pause.", this);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        // Capture the cursor's state the moment pause is triggered.
        wasCursorVisibleBeforePause = Cursor.visible;
        Debug.Log("Cursor Visibility was " + wasCursorVisibleBeforePause);
        previousCursorLockMode = Cursor.lockState;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = false;
        }

        // Force the cursor to be visible and unlocked for the pause menu UI.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = previousTimeScale;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = true;
        }

        // Restore the exact cursor state that was captured before pausing.
        Cursor.visible = wasCursorVisibleBeforePause;
        Cursor.lockState = previousCursorLockMode;
    }
}