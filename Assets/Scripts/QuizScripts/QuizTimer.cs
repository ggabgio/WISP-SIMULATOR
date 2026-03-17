// --- START OF FILE QuizTimer.cs ---
using UnityEngine;
using UnityEngine.UI; // Required for Slider
using TMPro;          // Required for TextMeshPro (if displaying text)
using System;         // Required for Action

public class QuizTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Time limit for each question in seconds.")]
    public float timePerQuestion = 10f;

    [Header("UI References")]
    [Tooltip("Assign the Slider UI element that visualizes the time remaining.")]
    public Slider timerSlider; // Assign in Inspector
    [Tooltip("Optional: Assign a TextMeshPro UI element to display the countdown numerically (SS.mmm).")]
    public TMP_Text timerTextDisplay; // Assign in Inspector (optional)

    private float currentTime;
    private bool isTimerRunning = false;
    private Coroutine timerCoroutine;

    public static event Action OnQuestionTimeUp;

    void Start()
    {
        // Slider is primary, text is optional
        if (timerSlider == null)
        {
            Debug.LogWarning("QuizTimer: Timer Slider not assigned! Visual feedback will be limited.", this);
        }
        if (timerTextDisplay == null)
        {
            Debug.Log("QuizTimer: Timer Text Display not assigned. No numerical countdown will be shown by QuizTimer.");
        }
        ResetAndPrepareUI();
    }

    private void ResetAndPrepareUI()
    {
        currentTime = timePerQuestion;
        if (timerSlider != null)
        {
            timerSlider.maxValue = timePerQuestion;
            timerSlider.value = timePerQuestion;
        }
        UpdateTimerTextDisplay(); // Update text display at reset
    }

    public void StartQuestionTimer()
    {
        ResetAndPrepareUI();
        isTimerRunning = true;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(Countdown());
        Debug.Log("Question timer started.");
    }

    public void StopQuestionTimer()
    {
        isTimerRunning = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        // Do not reset currentTime here, QuizManager might need it via GetTimeRemaining()
        Debug.Log("Question timer stopped.");
    }

    private System.Collections.IEnumerator Countdown()
    {
        while (currentTime > 0)
        {
            if (!isTimerRunning) yield break;

            currentTime -= Time.deltaTime;
            currentTime = Mathf.Max(0, currentTime); // Ensure currentTime doesn't go below zero

            if (timerSlider != null)
            {
                timerSlider.value = currentTime;
            }
            UpdateTimerTextDisplay(); // Update text display each frame
            yield return null;
        }

        // Time is up
        // currentTime is already 0 (or very close) due to Mathf.Max in loop
        if (timerSlider != null)
        {
            timerSlider.value = 0;
        }
        UpdateTimerTextDisplay(); // Final update for text display
        isTimerRunning = false;
        Debug.Log("Question time is up!");
        OnQuestionTimeUp?.Invoke();
    }

    private void UpdateTimerTextDisplay()
    {
        if (timerTextDisplay != null)
        {
            // Format as SS.mmm
            // int seconds = Mathf.FloorToInt(currentTime);
            // int milliseconds = Mathf.FloorToInt((currentTime - seconds) * 1000);
            // timerTextDisplay.text = $"{seconds:D2}.{milliseconds:D3}";

            // More precise way to get parts for TimeSpan if needed, but direct formatting is fine
            timerTextDisplay.text = currentTime.ToString("F3").PadLeft(6, '0'); // PadLeft might make it 00.000 or 010.000
                                                                                // Let's use a more robust formatting for SS.mmm
            if (currentTime < 10)
            {
                 timerTextDisplay.text = "0" + currentTime.ToString("F3");
            }
            else
            {
                 timerTextDisplay.text = currentTime.ToString("F3");
            }
            // Ensure format is exactly SS.mmm, e.g. 09.123, 10.000
            // ToString("F3") gives 3 decimal places.
            // We need to handle the seconds part for leading zero if < 10.
            // Example:
            // If currentTime = 9.12345 => "9.123"
            // If currentTime = 10.0 => "10.000"

            // A more controlled way:
            int secondsPart = (int)currentTime;
            int millisecondsPart = (int)((currentTime - secondsPart) * 1000);
            timerTextDisplay.text = string.Format("{0:00}.{1:000}", secondsPart, millisecondsPart);

        }
    }

    public void ForceStopAndClear()
    {
        StopQuestionTimer();
        currentTime = 0;
        if (timerSlider != null)
        {
            timerSlider.value = 0;
        }
        UpdateTimerTextDisplay(); // Update text display
    }

    public float GetTimeRemaining()
    {
        return currentTime; // Already clamped via Mathf.Max in Countdown
    }

    void OnDisable()
    {
        StopQuestionTimer();
    }
}
// --- END OF FILE QuizTimer.cs ---