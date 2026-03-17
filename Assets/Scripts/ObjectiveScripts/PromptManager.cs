using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PromptManager : MonoBehaviour
{
    public static PromptManager Instance { get; private set; }

    public static PromptManager SafeInstance {
        get {
            if (Instance == null || Instance.gameObject == null) {
                // Try to find one in the current scene
                Instance = GameObject.FindObjectOfType<PromptManager>();
                if (Instance == null) return null;
            }
            return Instance;
        }
    }

    [Tooltip("The UI Text component to display the prompts.")]
    public TMP_Text promptText;

    private struct PromptRequest
    {
        public object Source;
        public string Message;
        public int Priority;
    }

    private readonly Dictionary<object, PromptRequest> _incomingRequests = new Dictionary<object, PromptRequest>();
    private readonly HashSet<object> _activeSourcesLastFrame = new HashSet<object>();

    private PromptRequest? _currentWinningPrompt = null;
    
    private Coroutine _timedPromptCoroutine = null;
    private string _timedPromptMessage = null;
    private int _emptyFrameBuffer = 0;
    
    private int _displayHoldFrames = 0;
    private string _lastDisplayed = "";
    
    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else if (Instance != this) { Destroy(gameObject); }
    }

    private void Start()
    {
        if (promptText != null) { promptText.text = ""; }
        else { Debug.LogError("PromptManager: The 'promptText' reference is not assigned!", this); }
    }

    public void RequestPrompt(object source, string message, int priority)
    {
        if (source == null) return;
        _incomingRequests[source] = new PromptRequest { Source = source, Message = message, Priority = priority };
    }

    // --- NEW PUBLIC METHOD FOR TIMED PROMPTS ---
    public void ShowTimedPrompt(string message, float duration = 2.5f)
    {
        if (_timedPromptCoroutine != null)
        {
            StopCoroutine(_timedPromptCoroutine);
        }
        _timedPromptCoroutine = StartCoroutine(TimedPromptCoroutine(message, duration));
    }

    private IEnumerator TimedPromptCoroutine(string message, float duration)
    {
        _timedPromptMessage = message;
        yield return new WaitForSeconds(duration);
        _timedPromptMessage = null;
        _timedPromptCoroutine = null;
    }
    // --- END OF ADDITION ---
    
    private void LateUpdate()
    {
        if (promptText == null) return;

        DetermineWinningPrompt();

        string toDisplay = null;

        if (!string.IsNullOrEmpty(_timedPromptMessage))
        {
            toDisplay = _timedPromptMessage;
        }
        else if (_currentWinningPrompt.HasValue)
        {
            toDisplay = _currentWinningPrompt.Value.Message;
        }

        if (!string.IsNullOrEmpty(toDisplay))
        {
            promptText.text = toDisplay;
            _lastDisplayed = toDisplay;
            _displayHoldFrames = 2; // tweakable
        }
        else if (_displayHoldFrames > 0)
        {
            promptText.text = _lastDisplayed;
            _displayHoldFrames--;
        }
        else
        {
            promptText.text = "";
        }

        _activeSourcesLastFrame.Clear();
        foreach (var source in _incomingRequests.Keys) 
        { 
            _activeSourcesLastFrame.Add(source); 
        }
        _incomingRequests.Clear();
    }

    private void DetermineWinningPrompt()
    {
        if (_incomingRequests.Count == 0)
        {
            if (_emptyFrameBuffer > 0)
            {
                _emptyFrameBuffer--; // count down hold
                return; // keep last prompt
            }

            _currentWinningPrompt = null;
            return;
        }

        // if we have prompts, reset buffer
        _emptyFrameBuffer = 2; // <- hold for 1 frame (tweakable)

        PromptRequest? bestPrompt = null;
        foreach (var request in _incomingRequests.Values)
        {
            if (!bestPrompt.HasValue) { bestPrompt = request; continue; }

            if (request.Priority > bestPrompt.Value.Priority)
            {
                bestPrompt = request;
            }
            else if (request.Priority == bestPrompt.Value.Priority)
            {
                bool isNewSource = !_activeSourcesLastFrame.Contains(request.Source);
                if (isNewSource) { bestPrompt = request; }
            }
        }
        _currentWinningPrompt = bestPrompt;
    }
}