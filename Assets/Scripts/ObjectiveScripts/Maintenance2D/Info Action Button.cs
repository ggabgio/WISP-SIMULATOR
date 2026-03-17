using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoActionButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _actionNameText;
    [SerializeField] private Button _button;

    private string _infoName;
    private UI_DiagnosisManager _diagnosisManager;

    public void Initialize(string infoName, UI_DiagnosisManager manager)
    {
        _infoName = infoName;
        _diagnosisManager = manager;

        _actionNameText.text = infoName;
        _button.onClick.AddListener(OnActionClicked);
    }

    private void OnActionClicked()
    {
        _diagnosisManager.PerformAction(_infoName, this);
    }

    public void Disable()
    {
        _button.interactable = false;
    }
}