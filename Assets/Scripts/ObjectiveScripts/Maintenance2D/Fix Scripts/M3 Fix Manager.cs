using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class M3_FixManager : CrimpingManager
{
    [Header("Maintenance Scoring Parameters")]
    [SerializeField] private int _infoParCount = 5;
    [SerializeField] private float _infoPenaltyPercent = 5.0f;
    [SerializeField] private float _incorrectFixPenaltyPercent = 10.0f;
    
    [Header("Maintenance UI")]
    [SerializeField] private ResultsPanel _resultsPanelScript;

    private float _initialDiagnosisTime;
    private int _infoActionsUsed;
    private int _incorrectFixes;
    
    protected override void OnLevelStart()
    {
        base.OnLevelStart();
        if (LoadingData.Instance != null)
        {
            _initialDiagnosisTime = LoadingData.Instance.diagnosisTime;
            _infoActionsUsed = LoadingData.Instance.infoActionsUsed;
            _incorrectFixes = LoadingData.Instance.incorrectFixes;
            // The levelId is already set in the base CrimpingManager
        }
    }

    protected override async void OnLevelEnd(float baseTotalScore, float fixTime)
    {
        if (playerLookScript != null) playerLookScript.enabled = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float diagnosisAccuracyScore = 100f;
        int infoActionsOverPar = Mathf.Max(0, _infoActionsUsed - _infoParCount);
        diagnosisAccuracyScore -= (infoActionsOverPar * _infoPenaltyPercent);
        diagnosisAccuracyScore -= (_incorrectFixes * _incorrectFixPenaltyPercent);
        diagnosisAccuracyScore = Mathf.Max(0, diagnosisAccuracyScore);

        float fixObjectiveRawScore = allObjectives.Sum(o => o.GetScore());
        float fixObjectiveMaxScore = allObjectives.Count * 100f;
        float fixAccuracyScore = (fixObjectiveMaxScore > 0) ? (fixObjectiveRawScore / fixObjectiveMaxScore) * 100f : 0f;

        float averageAccuracy = (diagnosisAccuracyScore + fixAccuracyScore) / 2f;
        float finalAccuracyComponent = (averageAccuracy / 100f) * 70f;
        
        float finalTotalTime = _initialDiagnosisTime + fixTime;
        
        float timeScoreMultiplier = CalculateTimeScore(finalTotalTime);
        float finalTimeComponent = timeScoreMultiplier * 30f;
        
        float finalTotalScore = Mathf.Clamp(finalAccuracyComponent + finalTimeComponent, 0f, 100f);
        
        if (_resultsPanelScript != null)
        {
            _resultsPanelScript.gameObject.SetActive(true);
            _resultsPanelScript.DisplayResults(finalTotalTime, _infoActionsUsed, _incorrectFixes, finalTotalScore);
        }
        
        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            await UserSessionData.Instance.UpdateMaintenanceLevelProgress(levelId, true, finalTotalScore);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneFader fader = FindObjectOfType<SceneFader>();
        if (fader != null)
        {
            fader.FadeOutAndLoad("mainMenu");
        }
        else
        {
            SceneManager.LoadScene("mainMenu");
        }
    }
    
    protected override async void OnLevelQuit()
    {
        if (UserSessionData.Instance != null && !string.IsNullOrEmpty(levelId))
        {
            UserSessionData.Instance.UpdateMaintenanceLevelProgress(levelId, false, 0f);
        }
    }
}