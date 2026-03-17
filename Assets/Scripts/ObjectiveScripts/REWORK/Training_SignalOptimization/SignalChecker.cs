using UnityEngine;
using TMPro;

public class SignalChecker : MonoBehaviour
{
    public Transform antennaTransform;
    public Transform cellTowerTransform;
    public TMP_Text signalText;

    [Header("Signal Settings")]
    public float minDb = 90f;
    public float maxDb = 50f;
    public float fullSignalAngle = 15f;
    public float noSignalAngle = 30f;

    [Header("Vertical Sweet Spot Settings")]
    public Transform sweetSpot;
    public float verticalRange = 0.5f;

    [Header("Update Settings")]
    public float updateInterval = 1f;
    public float noiseRange = 2f;

    private float currentDb;
    private float updateTimer = 0f;

    void Start()
    {
        currentDb = minDb + 1f; // Initialize with a "no signal" value
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            CalculateSignal();
        }
    }

    void CalculateSignal()
    {
        Vector3 front = -antennaTransform.right.normalized;
        Vector3 toTower = (cellTowerTransform.position - antennaTransform.position).normalized;
        float angle = Vector3.Angle(front, toTower);

        if (angle > noSignalAngle)
        {
            signalText.text = "no\nsignal";
            currentDb = minDb + 1f; // Set a value indicating no signal
            return;
        }

        float horizontalFactor = Mathf.InverseLerp(fullSignalAngle, noSignalAngle, angle);
        float verticalOffset = Mathf.Abs(antennaTransform.position.y - sweetSpot.position.y);
        float verticalFactor = Mathf.Clamp01(verticalOffset / verticalRange);
        float totalFactor = (horizontalFactor + verticalFactor) / 2f;

        currentDb = Mathf.Lerp(maxDb, minDb, totalFactor);
        float randomizedDb = currentDb + Random.Range(-noiseRange, noiseRange);
        randomizedDb = Mathf.Clamp(randomizedDb, maxDb, minDb);

        signalText.text = Mathf.RoundToInt(randomizedDb) + " dB";
    }

    public float GetCurrentSignal()
    {
        return currentDb;
    }
}