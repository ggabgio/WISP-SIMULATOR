using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

public class UI_DiagnosisManager : MonoBehaviour
{
    [Header("UI References - Diagnosis Phase")]
    [SerializeField] private Transform _infoActionGridParent;
    [SerializeField] private GameObject _whiteButtonPrefab;
    [SerializeField] private GameObject _grayButtonPrefab;
    [SerializeField] private Button _proceedToFixingButton;
    [SerializeField] private PromptWindow _promptWindow;
    [SerializeField] private TMP_Text _pointsText;
    [SerializeField] private FeedbackFlasher _feedbackFlasher;

    [Header("Category Buttons")]
    [SerializeField] private Button _physicalChecksButton;
    [SerializeField] private Button _equipmentChecksButton;
    [SerializeField] private Button _networkChecksButton;
    [SerializeField] private Button _customerChecksButton;

    [Header("Data")]
    [SerializeField] private M_LevelData _currentLevelData;
    [SerializeField] private M_DefaultInfoData _defaultInfoData;

    [Header("Clipboard References")]
    [SerializeField] private TextMeshProUGUI _clientNameText;
    [SerializeField] private TextMeshProUGUI _planText;
    [SerializeField] private TextMeshProUGUI _customerReportText;
    [SerializeField] private Transform _clipboardContentParent;
    [SerializeField] private GameObject _clipboardEntryPrefab;

    [Header("Feedback Settings")]
    [SerializeField] private RectTransform uiContainerToShake; // Assign your Diagnosis Panel or UI container here
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 10f;
    [SerializeField] private float shakeDamping = 3f;

    private Dictionary<string, (string category, string description, int points)> _levelInfo = new();
    private List<string> _performedActions = new();
    private int _noteCount = 0;
    private int _currentPoints = 0;
    private int _incorrectFixes = 0;
    private float _elapsedTime = 0f;
    private bool _isTimerRunning = false;
    
    public M_LevelData CurrentLevelData => _currentLevelData;

    void Start()
    {
        M_LevelData dataToLoad = null;

        if (LoadingData.Instance != null && LoadingData.Instance.maintenanceLevelToLoad != null)
        {
            dataToLoad = LoadingData.Instance.maintenanceLevelToLoad;
            LoadingData.Instance.maintenanceLevelToLoad = null;
        }
        else if (_currentLevelData != null)
        {
            dataToLoad = _currentLevelData;
        }

        if (dataToLoad != null)
            LoadLevel(dataToLoad);
    }

    private void Update()
    {
        if (_isTimerRunning)
            _elapsedTime += Time.deltaTime;
    }

    public void LoadLevel(M_LevelData levelData)
    {
        _currentLevelData = levelData;
        _levelInfo.Clear();
        _performedActions.Clear();
        _noteCount = 0;
        _currentPoints = 0;
        _incorrectFixes = 0;
        _elapsedTime = 0f;
        _isTimerRunning = true;

        foreach (Transform child in _infoActionGridParent)
            Destroy(child.gameObject);
        foreach (Transform child in _clipboardContentParent)
            Destroy(child.gameObject);

        _clientNameText.text = "Client: " + _currentLevelData.clientName;
        _planText.text = "Plan: " + _currentLevelData.plan;
        _customerReportText.text = "Report: " + _currentLevelData.customerReport;

        var abnormalities = _currentLevelData.infoPoints.ToDictionary(ip => ip.infoName);
        foreach (var action in GetAllPossibleActions())
        {
            if (abnormalities.TryGetValue(action.infoName, out var abnormality))
                _levelInfo[action.infoName] = (abnormality.category, abnormality.description, abnormality.points);
            else
            {
                var defaultInfo = _defaultInfoData.GetDefaultInfo(action.infoName);
                _levelInfo[action.infoName] = (action.category, defaultInfo.description, defaultInfo.points);
            }
        }

        SetupCategoryButtons();
        UpdatePointsUIAndCheckLock();
    }

    private void SetupCategoryButtons()
    {
        _physicalChecksButton.onClick.AddListener(() => PopulateActionsForCategory("Physical"));
        _equipmentChecksButton.onClick.AddListener(() => PopulateActionsForCategory("Equipment"));
        _networkChecksButton.onClick.AddListener(() => PopulateActionsForCategory("Network"));
        _customerChecksButton.onClick.AddListener(() => PopulateActionsForCategory("Customer"));
    }

    private void PopulateActionsForCategory(string category)
    {
        foreach (Transform child in _infoActionGridParent)
            Destroy(child.gameObject);

        var actionsToShow = GetAllPossibleActions()
            .Where(action => action.category == category && !_performedActions.Contains(action.infoName))
            .ToList();

        for (int i = 0; i < actionsToShow.Count; i++)
        {
            var action = actionsToShow[i];
            GameObject buttonPrefab = (i % 2 == 0) ? _whiteButtonPrefab : _grayButtonPrefab;
            GameObject buttonGO = Instantiate(buttonPrefab, _infoActionGridParent);
            buttonGO.GetComponent<InfoActionButton>().Initialize(action.infoName, this);
        }
    }

    public void PerformAction(string infoName, InfoActionButton clickedButton)
    {
        if (_performedActions.Contains(infoName)) return;

        _performedActions.Add(infoName);
        clickedButton.Disable();

        var info = _levelInfo[infoName];
        _promptWindow.Show(info.category, infoName, info.description, info.points, this);
    }

    public void AddNoteToClipboard(string category, string source, string information, int points)
    {
        _noteCount++;
        GameObject entryGO = Instantiate(_clipboardEntryPrefab, _clipboardContentParent);
        entryGO.GetComponent<ClipboardEntry>().Initialize(category, source, information, _noteCount);

        _currentPoints += points;
        UpdatePointsUIAndCheckLock();
    }

    private void UpdatePointsUIAndCheckLock()
    {
        if (_pointsText != null)
            _pointsText.text = $"Points: {_currentPoints} / {_currentLevelData.requiredPointsToFix}";

        if (_proceedToFixingButton != null)
            _proceedToFixingButton.interactable = (_currentPoints >= _currentLevelData.requiredPointsToFix);
    }

    public void AttemptFix(string chosenFix)
    {
        if (chosenFix == _currentLevelData.correctFix)
        {
            UIEffectsManager.Instance.PlaySound(correctSound);
            _isTimerRunning = false;

            if (LoadingData.Instance != null)
            {
                LoadingData.Instance.diagnosisTime = _elapsedTime;
                LoadingData.Instance.infoActionsUsed = _noteCount;
                LoadingData.Instance.incorrectFixes = _incorrectFixes;
                LoadingData.Instance.levelId = _currentLevelData.levelName;
            }

            SceneManager.LoadScene(_currentLevelData.fixSceneName);
        }
        else
        {
            _incorrectFixes++;
            UIEffectsManager.Instance.PlaySound(wrongSound);
            UIEffectsManager.Instance.Shake(uiContainerToShake, shakeDuration, shakeMagnitude, shakeDamping);

            if (_feedbackFlasher != null)
            {
                _feedbackFlasher.gameObject.SetActive(true);
                _feedbackFlasher.FlashMessage("Incorrect Fix Attempted");
            }
        }
    }

    private List<(string category, string infoName)> GetAllPossibleActions()
    {
        return _defaultInfoData.defaultInfoEntries.Select(entry =>
        {
            string category = "Physical";
            if (entry.infoName.Contains("Router") || entry.infoName.Contains("Access Point") || entry.infoName.Contains("Cable Connector")) category = "Equipment";
            if (entry.infoName.Contains("Signal") || entry.infoName.Contains("Link") || entry.infoName.Contains("Latency") || entry.infoName.Contains("Bandwidth")) category = "Network";
            if (entry.infoName.Contains("Customer") || entry.infoName.Contains("Weather") || entry.infoName.Contains("Power")) category = "Customer";
            return (category, entry.infoName);
        }).ToList();
    }
}
