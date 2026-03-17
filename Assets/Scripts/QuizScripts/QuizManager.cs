using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

[System.Serializable]
public class QuizQuestion
{
    public string questionText;
    public List<string> options;
    public int correctAnswerIndex;
}

[System.Serializable]
public class QuizData
{
    public string quizTitle;
    public List<QuizQuestion> questions;
}

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Identification")]
    [SerializeField] private string quizId = "default_quiz_id";
    [Header("Question & Answer UI")]
    [SerializeField] private TMP_Text questionTextUI;
    [SerializeField] private TMP_Text questionNumberTextUI;
    [SerializeField] private List<Button> choiceButtons;
    [SerializeField] private List<TMP_Text> choiceButtonTexts;

    [Header("Feedback UI")]
    [SerializeField] private GameObject quizStartPanel;
    [SerializeField] private TMP_Text quizStartPanelTitleText;
    [SerializeField] private Button startQuizActualButton;
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackTextUI;
    [SerializeField] private GameObject feedbackInputBlocker;

    [Header("Results Panel UI")]
    [SerializeField] private GameObject resultsPanelUI;
    [SerializeField] private TMP_Text scoreTextUI;
    [SerializeField] private TMP_Text timeLeftResultsUI;
    [SerializeField] private TMP_Text performanceScoreResultsUI;

    [Header("Confirmation Popup UI")]
    [SerializeField] private GameObject escapeConfirmPopupUI;
    [SerializeField] private Button escapeConfirmButton;

    [Header("System References")]
    [SerializeField] private QuizTimer quizTimer;

    [Header("Quiz Settings")]
    [SerializeField] private int maxQuestionsPerQuiz = 10;

    // Audio
    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    // UI Shake (for Screen Space - Overlay Canvas)
    [Header("UI Shake")]
    [SerializeField] private UIShake uiShake;

    private QuizData currentQuizData;
    private List<QuizQuestion> questionsToAsk;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private bool waitingForAnswer = false;
    private float totalTimeTakenForQuiz = 0f;

    private bool isQuizActive = false;
    private bool waitingForFeedbackClick = false;

    // Colors
    private Color defaultButtonColor;
    private Color32 correctColor32 = new Color32(0, 255, 0, 250);   // RGBA with alpha 250
    private Color32 wrongColor32 = new Color32(255, 0, 0, 250);   // RGBA with alpha 250

    void OnEnable()
    {
        QuizTimer.OnQuestionTimeUp += HandleQuestionTimeUp;
    }

    void OnDisable()
    {
        QuizTimer.OnQuestionTimeUp -= HandleQuestionTimeUp;
    }

    void Start()
    {
        resultsPanelUI.SetActive(false);
        escapeConfirmPopupUI.SetActive(false);
        feedbackPanel.SetActive(false);
        feedbackInputBlocker.SetActive(false);
        questionTextUI.gameObject.SetActive(false);
        foreach (Button btn in choiceButtons) { if (btn != null) btn.gameObject.SetActive(false); }
        questionNumberTextUI.gameObject.SetActive(false);
        quizStartPanel.SetActive(false);

        // Save default button color (assume all share same base color)
        if (choiceButtons != null && choiceButtons.Count > 0 && choiceButtons[0].image != null)
        {
            defaultButtonColor = choiceButtons[0].image.color;
        }

        // Disable Color Tint transitions so our manual color changes aren't overridden
        foreach (Button btn in choiceButtons)
        {
            if (btn != null)
            {
                btn.transition = Selectable.Transition.None;
            }
        }

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            int choiceIndex = i;
            if (choiceButtons[i] != null)
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }
        if (escapeConfirmButton != null) escapeConfirmButton.onClick.AddListener(ReturnToMainMenu);
        if (startQuizActualButton != null) startQuizActualButton.onClick.AddListener(StartQuizActual);

        string topicName = DetermineQuizTopic();

        // Set quizId same as topicName here
        quizId = topicName;

        LoadQuizData(topicName + "_questions.json");

        if (currentQuizData == null || currentQuizData.questions == null || currentQuizData.questions.Count == 0)
        {
            HandleQuizLoadError(topicName);
            return;
        }

        SetupStartPanel();
        isQuizActive = false;
    }

    void SetupStartPanel()
    {
        quizStartPanelTitleText.text = (currentQuizData != null && !string.IsNullOrEmpty(currentQuizData.quizTitle))
            ? currentQuizData.quizTitle
            : "Quiz";

        quizStartPanel.SetActive(true);
    }

    string DetermineQuizTopic()
    {
        string topicName = "WISP"; // Default
        if (LoadingData.Instance != null && !string.IsNullOrEmpty(LoadingData.Instance.quizTopicName))
        {
            topicName = LoadingData.Instance.quizTopicName;
        }
        return topicName;
    }

    void HandleQuizLoadError(string topicName)
    {
        quizStartPanel.SetActive(false);
        scoreTextUI.text = $"Error: No questions loaded for {topicName}.";
        performanceScoreResultsUI.text = "Performance: 0%";
        resultsPanelUI.SetActive(true);
    }

    void Update()
    {
        if (isQuizActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscapePopup();
        }

        if (waitingForFeedbackClick && Input.GetMouseButtonDown(0))
        {
            HideFeedbackAndProceed();
        }
    }

    public void StartQuizActual()
    {
        quizStartPanel.SetActive(false);
        isQuizActive = true;

        questionTextUI.gameObject.SetActive(true);
        foreach (Button btn in choiceButtons) { if (btn != null) btn.gameObject.SetActive(true); }
        questionNumberTextUI.gameObject.SetActive(true);

        StartQuiz();
    }

    void LoadQuizData(string jsonFileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            currentQuizData = JsonUtility.FromJson<QuizData>(dataAsJson);
            if (currentQuizData.questions == null) currentQuizData.questions = new List<QuizQuestion>();
        }
    }

    void StartQuiz()
    {
        score = 0;
        currentQuestionIndex = 0;
        totalTimeTakenForQuiz = 0f;

        System.Random rng = new System.Random();
        questionsToAsk = currentQuizData.questions.OrderBy(q => rng.Next()).Take(maxQuestionsPerQuiz).ToList();
        maxQuestionsPerQuiz = questionsToAsk.Count;

        if (maxQuestionsPerQuiz == 0)
        {
            EndQuiz();
            return;
        }

        DisplayNextQuestion();
    }

    void DisplayNextQuestion()
    {
        if (currentQuestionIndex < questionsToAsk.Count)
        {
            // Reset button colors each new question
            ResetButtonColors();

            questionNumberTextUI.text = $"{currentQuestionIndex + 1}/{maxQuestionsPerQuiz}";
            QuizQuestion question = questionsToAsk[currentQuestionIndex];
            questionTextUI.text = question.questionText;

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                if (choiceButtons[i] != null)
                {
                    choiceButtons[i].gameObject.SetActive(i < question.options.Count);
                    if (i < question.options.Count)
                    {
                        choiceButtonTexts[i].text = question.options[i];
                    }
                }
            }

            SetChoiceButtonsInteractable(true);
            waitingForAnswer = true;
            quizTimer.StartQuestionTimer();
        }
        else
        {
            EndQuiz();
        }
    }

    void OnChoiceSelected(int choiceIndex)
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;

        // Stop timer and record time
        quizTimer.StopQuestionTimer();
        totalTimeTakenForQuiz += (quizTimer.timePerQuestion - quizTimer.GetTimeRemaining());

        QuizQuestion currentQuestion = questionsToAsk[currentQuestionIndex];
        bool isCorrect = (choiceIndex == currentQuestion.correctAnswerIndex);

        // Apply colors first (using Color32 so alpha is exactly 250)
        if (isCorrect)
        {
            if (choiceButtons[choiceIndex] != null && choiceButtons[choiceIndex].image != null)
                choiceButtons[choiceIndex].image.color = correctColor32;

            // Play correct sound
            if (audioSource != null && correctSound != null)
                audioSource.PlayOneShot(correctSound);

            score++;
            ShowFeedbackPanel("Correct answer, click anywhere to proceed.");
        }
        else
        {
            if (choiceButtons[choiceIndex] != null && choiceButtons[choiceIndex].image != null)
                choiceButtons[choiceIndex].image.color = wrongColor32;

            if (choiceButtons[currentQuestion.correctAnswerIndex] != null && choiceButtons[currentQuestion.correctAnswerIndex].image != null)
                choiceButtons[currentQuestion.correctAnswerIndex].image.color = correctColor32;

            // Play wrong sound
            if (audioSource != null && wrongSound != null)
                audioSource.PlayOneShot(wrongSound);

            // Stronger UI shake for wrong answer
            if (uiShake != null)
                uiShake.Shake(0.28f, 14f);

            ShowFeedbackPanel("Incorrect answer, click anywhere to proceed.");
        }

        // Then disable further clicks (transition is None so Unity won't tint/override)
        SetChoiceButtonsInteractable(false);

        currentQuestionIndex++;
    }

    void HandleQuestionTimeUp()
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;

        totalTimeTakenForQuiz += quizTimer.timePerQuestion;

        // Show correct answer (alpha = 250) first
        QuizQuestion currentQuestion = questionsToAsk[currentQuestionIndex];
        if (choiceButtons[currentQuestion.correctAnswerIndex] != null && choiceButtons[currentQuestion.correctAnswerIndex].image != null)
            choiceButtons[currentQuestion.correctAnswerIndex].image.color = correctColor32;

        SetChoiceButtonsInteractable(false);
        ShowFeedbackPanel("Time's Up! Click anywhere to Proceed");
        currentQuestionIndex++;
    }

    void ShowFeedbackPanel(string message)
    {
        feedbackTextUI.text = message;
        feedbackPanel.SetActive(true);
        feedbackInputBlocker.SetActive(true);
        waitingForFeedbackClick = true;
    }

    void HideFeedbackAndProceed()
    {
        if (!waitingForFeedbackClick) return;

        waitingForFeedbackClick = false;
        feedbackPanel.SetActive(false);
        feedbackInputBlocker.SetActive(false);
        DisplayNextQuestion();
    }

    void ResetButtonColors()
    {
        foreach (Button btn in choiceButtons)
        {
            if (btn != null && btn.image != null)
            {
                btn.image.color = defaultButtonColor;
            }
        }
    }

    void SetChoiceButtonsInteractable(bool interactable)
    {
        foreach (Button button in choiceButtons)
        {
            if (button != null) button.interactable = interactable;
        }
    }

    void ToggleEscapePopup()
    {
        if (resultsPanelUI.activeSelf) return;
        escapeConfirmPopupUI.SetActive(!escapeConfirmPopupUI.activeSelf);
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        FindObjectOfType<SceneFader>().FadeOutAndLoad("mainMenu");
    }

    void EndQuiz()
    {
        if (!isQuizActive && !resultsPanelUI.activeSelf) return;

        isQuizActive = false;
        quizTimer.ForceStopAndClear();
        waitingForAnswer = false;
        waitingForFeedbackClick = false;
        feedbackPanel.SetActive(false);
        feedbackInputBlocker.SetActive(false);

        SetChoiceButtonsInteractable(false);
        questionTextUI.gameObject.SetActive(false);
        questionNumberTextUI.gameObject.SetActive(false);

        scoreTextUI.text = $"Correct: {score} / {maxQuestionsPerQuiz}";
        timeLeftResultsUI.text = $"Time Taken: {totalTimeTakenForQuiz:F1}s";

        float performance = (maxQuestionsPerQuiz > 0) ? ((float)score / maxQuestionsPerQuiz) * 100f : 0f;
        performanceScoreResultsUI.text = $"Performance: {performance:F0}%";
        resultsPanelUI.SetActive(true);

        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(quizId))
        {
            UserSessionData.Instance.UpdateQuizLevelProgress(quizId, true, performance);
        }
    }
}
