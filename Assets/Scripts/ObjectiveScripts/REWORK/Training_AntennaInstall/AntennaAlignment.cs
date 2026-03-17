using UnityEngine;
using System.Collections; // Needed for IEnumerator
using TMPro; // Needed for TextMeshPro

public class AntennaAlignment : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Assign the Transform of the Cell Tower target here.")]
    public Transform targetTower; // Assign the cell tower object in the Inspector

    [Header("Alignment Settings")]
    [Tooltip("Local direction vector representing the antenna's 'front' or pointing direction (e.g., Vector3.forward for blue Z-axis).")]
    public Vector3 pointingDirectionLocal = Vector3.forward; // Adjust if your model points differently

    [Header("UI Feedback")]
    [Tooltip("Assign a TextMeshProUGUI element to display the alignment score.")]
    public TMP_Text alignmentScoreText; // Assign this in the Inspector

    [Tooltip("How often to update the alignment score text (in seconds).")]
    public float scoreUpdateInterval = 0.5f;

    private Coroutine updateScoreCoroutine;
    private bool isUpdatingScore = false;

    void Start()
    {
        // Ensure text object is initially inactive or cleared if assigned
        if (alignmentScoreText != null)
        {
            alignmentScoreText.gameObject.SetActive(false);
            alignmentScoreText.text = "";
        }
        if (targetTower == null)
        {
            Debug.LogWarning("AntennaAlignment: Target Tower is not assigned!", this.gameObject);
        }

        StartUpdatingScoreDisplay();
    }

    // Call this method after the antenna has been placed to get its score
    public float CalculateAlignmentScore()
    {
        if (targetTower == null)
        {
            // No error log here as it might be called frequently by UI before setup
            return 0f;
        }

        Vector3 antennaWorldForward = transform.TransformDirection(pointingDirectionLocal.normalized);
        Vector3 directionToTower = (targetTower.position - transform.position).normalized;
        float angleDifference = Vector3.Angle(antennaWorldForward, directionToTower);
        float score = Mathf.Clamp(100f - angleDifference, 0f, 100f);
        return score;
    }

    // --- Score Display Methods ---

    // Call this from ObjectiveManager when alignment phase begins
    public void StartUpdatingScoreDisplay()
    {
        if (alignmentScoreText == null)
        {
            Debug.LogWarning("AntennaAlignment: alignmentScoreText not assigned. Cannot display score.", this);
            return;
        }
        if (!isUpdatingScore)
        {
            alignmentScoreText.gameObject.SetActive(true);
            if (updateScoreCoroutine != null) // Stop any previous instance just in case
            {
                StopCoroutine(updateScoreCoroutine);
            }
            updateScoreCoroutine = StartCoroutine(UpdateScoreDisplayCoroutine());
            isUpdatingScore = true;
        }
    }

    private IEnumerator UpdateScoreDisplayCoroutine()
    {
        while (true) // Loop indefinitely, stopped by StopUpdatingScoreDisplay()
        {
            float currentScore = CalculateAlignmentScore();
            alignmentScoreText.text = $"Alignment: {currentScore.ToString("00.00")}";

            yield return new WaitForSeconds(scoreUpdateInterval);
        }
    }

    // Optional: Visualize the pointing direction in the editor
    void OnDrawGizmosSelected()
    {
        Vector3 worldPointingDir = transform.TransformDirection(pointingDirectionLocal.normalized);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, worldPointingDir * 2.0f);

        if (targetTower != null)
        {
             Gizmos.color = Color.green;
             Gizmos.DrawLine(transform.position, targetTower.position);
        }
    }
}