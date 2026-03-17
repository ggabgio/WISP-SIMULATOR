using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("Scoring Parameters")]
    [SerializeField] private float _parTime = 120f;
    [SerializeField]
    private AnimationCurve _timeDecayCurve = new AnimationCurve(
        new Keyframe(0, 1), new Keyframe(30, 0.95f), new Keyframe(90, 0.5f), new Keyframe(150, 0)
    );
    [SerializeField] private int _infoParCount = 5;
    [SerializeField] private float _infoPenaltyPercent = 5.0f;
    [SerializeField] private float _incorrectFixPenaltyPercent = 10.0f;

    [Header("UI & Scene References")]
    [SerializeField] private GameObject _diagnosisPanel;
    [SerializeField] private GameObject _fixPanel;
    [SerializeField] private GameObject _resultsPanelObject;
    [SerializeField] private ResultsPanel _resultsPanelScript;
    [SerializeField] private FeedbackFlasher _feedbackFlasher;

    [Header("Feedback Effects")]
    [SerializeField] private AudioSource audioSource;      // Drag AudioSource here
    [SerializeField] private AudioClip correctSound;       // Correct answer sound
    [SerializeField] private AudioClip wrongSound;         // Wrong answer sound
    [SerializeField] private UIShake uiShake;              // UI shake script

    private float _elapsedTime;
    private bool _isTimerRunning;
    private int _infoActionsUsed;
    private int _incorrectFixes;
    private string _correctFixName;
    private string _levelId; // To store the level name for saving

    // -------------------------------
    // Start Diagnosis Setup
    // -------------------------------
    public void StartDiagnosis(M_LevelData levelData)
    {
        Time.timeScale = 1f;
        _correctFixName = levelData.correctFix;
        _levelId = levelData.levelName; // Store the level name as its unique ID

        _elapsedTime = 0f;
        _infoActionsUsed = 0;
        _incorrectFixes = 0;

        if (_resultsPanelObject != null) _resultsPanelObject.SetActive(false);
        if (_feedbackFlasher != null) _feedbackFlasher.gameObject.SetActive(false);
        _diagnosisPanel.SetActive(true);
        _fixPanel.SetActive(false);

        _isTimerRunning = true;
    }

    private void Update()
    {
        if (_isTimerRunning)
        {
            _elapsedTime += Time.deltaTime;
        }
    }

    public void RecordInfoAction()
    {
        _infoActionsUsed++;
    }

    // -------------------------------
    // Fix Attempt Logic
    // -------------------------------
    public void AttemptFix(string chosenFix)
    {
        if (chosenFix == _correctFixName)
        {
            // Play correct sound
            if (audioSource != null && correctSound != null)
                audioSource.PlayOneShot(correctSound);

            _isTimerRunning = false;
            CalculateAndShowFinalScore();
        }
        else
        {
            _incorrectFixes++;

            // Play wrong sound
            if (audioSource != null && wrongSound != null)
                audioSource.PlayOneShot(wrongSound);

            // Stronger shake for wrong attempt
            if (uiShake != null)
                uiShake.Shake(0.3f, 12f);

            if (_feedbackFlasher != null)
            {
                _feedbackFlasher.gameObject.SetActive(true);
                _feedbackFlasher.FlashMessage("Incorrect Fix Attempted");
            }
        }
    }

    // -------------------------------
    // Score Calculation
    // -------------------------------
    private async void CalculateAndShowFinalScore()
    {
        Time.timeScale = 0f;

        if (_resultsPanelObject == null || _resultsPanelScript == null)
        {
            Debug.LogError("FATAL ERROR: 'Results Panel Object' or 'Results Panel Script' is not assigned in ScoreManager!", this);
            return;
        }

        float timeScorePortion = (_elapsedTime <= _parTime)
            ? 1f
            : _timeDecayCurve.Evaluate(_elapsedTime - _parTime);
        float finalTimeScore = timeScorePortion * 30f;

        float baseAccuracyScore = 70f;
        int infoActionsOverPar = Mathf.Max(0, _infoActionsUsed - _infoParCount);
        float infoPenalty = infoActionsOverPar * _infoPenaltyPercent;
        float fixPenalty = _incorrectFixes * _incorrectFixPenaltyPercent;
        float finalAccuracyScore = baseAccuracyScore - infoPenalty - fixPenalty;

        float totalScore = Mathf.Clamp(finalTimeScore + finalAccuracyScore, 0f, 100f);

        _resultsPanelObject.SetActive(true);
        _resultsPanelScript.DisplayResults(_elapsedTime, _infoActionsUsed, _incorrectFixes, totalScore);

        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(_levelId))
        {
            await UserSessionData.Instance.UpdateMaintenanceLevelProgress(_levelId, true, totalScore);
        }
    }

    // -------------------------------
    // Scene Return
    // -------------------------------
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Un-pause the game

        var fader = FindObjectOfType<SceneFader>();
        if (fader != null)
            fader.FadeOutAndLoad("mainMenu");
        else
            SceneManager.LoadScene("mainMenu");
    }
}
