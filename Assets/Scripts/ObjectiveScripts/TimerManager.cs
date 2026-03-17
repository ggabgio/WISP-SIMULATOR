// --- START OF REVISED FILE TimerManager.cs ---
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [Header("Settings")]
    public TMP_Text timerText;
    // No 'startTime' needed for count-up timer

    public static event Action OnTimerExpired; // Kept for compatibility, though less relevant for count-up

    private float elapsedTime = 0f; // Tracks elapsed time in seconds
    private Coroutine countUpCoroutine;
    private bool isTimerRunning = false;

    public float ElapsedTime => elapsedTime; // Public property to get current elapsed time
    // public int StartTime => 0; // Or remove if not needed, conceptually count-up starts at 0

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("TimerManager: timerText is not assigned!", this);
            enabled = false;
            return;
        }

        ResetTimer();
        UpdateTimerDisplay(); // Show initial time (00:00)
        StartTimer(); // Do not start automatically; let ObjectiveManager or game flow decide
    }

    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            Debug.Log("Count-up Timer Started.");
            isTimerRunning = true;
            // elapsedTime = 0f; // Reset on explicit start, or allow continuation if needed
            if (countUpCoroutine != null) StopCoroutine(countUpCoroutine);
            countUpCoroutine = StartCoroutine(CountUp());
        }
    }

    public void StopTimer() // Call this when the timed event ends (e.g., objectives complete)
    {
        if (isTimerRunning)
        {
            isTimerRunning = false;
            if (countUpCoroutine != null)
            {
                StopCoroutine(countUpCoroutine);
                countUpCoroutine = null;
                Debug.Log($"Timer Stopped. Final Time Taken: {elapsedTime:F2}s");
            }
        }
    }

    public void ResetTimer()
    {
        StopTimer(); // Ensure any running coroutine is stopped
        elapsedTime = 0f;
        isTimerRunning = false; // Mark as not running until StartTimer is called
        UpdateTimerDisplay();
    }


    IEnumerator CountUp()
    {
        while (isTimerRunning)
        {
            elapsedTime += Time.deltaTime; // Use Time.deltaTime for smooth count-up
            UpdateTimerDisplay();
            yield return null; // Wait for the next frame
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = "Timer: " + TimeSpan.FromSeconds(elapsedTime).ToString(@"mm\:ss"); // Added milliseconds
        }
    }

    void OnDisable()
    {
        StopTimer();
        // Static event unsubscription should be handled by listeners
    }

    private void OnApplicationQuit()
    {
        // OnTimerExpired = null; // Kept for potential cleanup if event was still used
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void OnDomainReload()
    {
        // OnTimerExpired = null; // Kept for potential cleanup
    }
#endif
}
// --- END OF REVISED FILE TimerManager.cs ---