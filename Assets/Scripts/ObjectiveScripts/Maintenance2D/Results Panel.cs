using UnityEngine;
using TMPro;
using System;

public class ResultsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _timeTakenText;
    [SerializeField] private TMP_Text _infoGatheredText;
    [SerializeField] private TMP_Text _wrongFixesText;
    [SerializeField] private TMP_Text _finalScoreText;

    public void DisplayResults(float timeTaken, int infoCount, int wrongFixes, float finalScore)
    {
        // This log will confirm the exact values this script is receiving.
        Debug.Log($"--- RESULTS PANEL: Displaying... Time: {timeTaken}, Info: {infoCount}, Fixes: {wrongFixes}, Score: {finalScore} ---");

        if (_timeTakenText != null)
        {
            _timeTakenText.text = $"Time Taken: {TimeSpan.FromSeconds(timeTaken):mm\\:ss\\.fff}";
            _timeTakenText.ForceMeshUpdate(); // Force the text to redraw immediately.
        }

        if (_infoGatheredText != null)
        {
            _infoGatheredText.text = $"Info Gathered: {infoCount}";
            _infoGatheredText.ForceMeshUpdate();
        }

        if (_wrongFixesText != null)
        {
            _wrongFixesText.text = $"Wrong Fixes: {wrongFixes}";
            _wrongFixesText.ForceMeshUpdate();
        }

        if (_finalScoreText != null)
        {
            _finalScoreText.text = $"Score: {finalScore.ToString("0.00")}%";
            _finalScoreText.ForceMeshUpdate();
        }
    }
}