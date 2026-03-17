using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class FixActionButton : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string _fixName;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private UI_DiagnosisManager _diagnosisManager;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_diagnosisManager == null) _diagnosisManager = FindObjectOfType<UI_DiagnosisManager>();
        
        _button.onClick.AddListener(OnFixAttempted);

        if (_buttonText != null && !string.IsNullOrEmpty(_fixName))
        {
            _buttonText.text = _fixName;
        }
    }

    private void OnFixAttempted()
    {
        if (_diagnosisManager != null)
        {
            _diagnosisManager.AttemptFix(_fixName);
        }
    }
}