using System;
using UnityEngine;

public class T5_AlignAntennaObjective : BaseObjective
{
    [Header("Dependencies")]
    public SignalChecker signalChecker;
    public AntennaController antennaController;

    [Header("Objective Settings")]
    [Tooltip("Signal strength (in dB) required to complete the objective.")]
    public float completionThreshold = 65f;
    [Tooltip("Signal considered 'perfect' for 100% score.")]
    public float perfectSignalThreshold = 55f;
    [Tooltip("Signal considered 'no signal' for 0% score.")]
    public float noSignalThreshold = 90f;

    protected override void OnObjectiveStart()
    {
        if (antennaController != null)
        {
            antennaController.EnableControl();
        }
    }
    
    void Update()
    {
        if (!IsActive || signalChecker == null) return;

        float currentSignal = signalChecker.GetCurrentSignal();

        // Complete the objective once the threshold is met.
        if (currentSignal <= completionThreshold)
        {
            // Lock in the score directly from the current signal.
            float scorePercent = CalculateScore(currentSignal);
            SetScore(scorePercent);
            CompleteObjective();
        }
    }

    private float CalculateScore(float signal)
    {
        if (signal <= perfectSignalThreshold) return 100f;
        if (signal >= noSignalThreshold) return 0f;

        float range = noSignalThreshold - perfectSignalThreshold;
        float valueInRange = signal - perfectSignalThreshold;
        
        // Invert the value since lower dB is better.
        float score = Mathf.Clamp01(1.0f - (valueInRange / range)) * 100f;
        return (float)Math.Round(score, 2);
    }
    
    protected override void OnObjectiveComplete()
    {
        // The antenna controller will be disabled by the next objective's start.
    }
}