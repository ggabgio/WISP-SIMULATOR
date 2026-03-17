using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PromptWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _windowPanel;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _panelTransform;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _informationText;
    [SerializeField] private Button _closeButton;

    private string _categoryToLog;
    private string _sourceToLog;
    private string _infoToLog;
    private int _pointsToLog;
    private UI_DiagnosisManager _diagnosisManager;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float squeezeDuration = 0.25f;

    private bool _isClosing = false;

    private void Awake()
    {
        _closeButton.onClick.AddListener(OnCloseClicked);
        _windowPanel.SetActive(false);

        if (_canvasGroup == null)
            _canvasGroup = _windowPanel.GetComponent<CanvasGroup>();
        if (_panelTransform == null)
            _panelTransform = _windowPanel.GetComponent<RectTransform>();
    }

    public void Show(string category, string source, string information, int points, UI_DiagnosisManager manager)
    {
        _categoryToLog = category;
        _sourceToLog = source;
        _infoToLog = information;
        _pointsToLog = points;
        _diagnosisManager = manager;
        _isClosing = false;

        _titleText.text = $"Result of: {source} (+{points} pts)";
        _informationText.text = information;

        _closeButton.interactable = true;
        _windowPanel.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private void OnCloseClicked()
    {
        if (_isClosing) return;      // Prevent multiple presses
        _isClosing = true;

        _closeButton.interactable = false; // Disable button immediately

        if (_diagnosisManager != null)
            _diagnosisManager.AddNoteToClipboard(_categoryToLog, _sourceToLog, _infoToLog, _pointsToLog);

        StopAllCoroutines();
        StartCoroutine(SqueezeOut());
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        _panelTransform.localScale = Vector3.one * 0.9f;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            _panelTransform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        _panelTransform.localScale = Vector3.one;
    }

    private IEnumerator SqueezeOut()
    {
        Vector3 startScale = _panelTransform.localScale;
        Vector3 endScale = new Vector3(0.1f, 0.1f, 1f);

        float elapsed = 0f;
        while (elapsed < squeezeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / squeezeDuration;
            _panelTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        _panelTransform.localScale = endScale;
        _canvasGroup.alpha = 0f;
        _windowPanel.SetActive(false);

        _isClosing = false;
        _diagnosisManager = null;
        _categoryToLog = null;
        _sourceToLog = null;
        _infoToLog = null;
    }
}
