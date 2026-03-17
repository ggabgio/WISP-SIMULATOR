using UnityEngine;
using TMPro;
using System.Linq;

public class DisplayFirebaseScores : MonoBehaviour
{
    [Header("Training Scores")]
    public TMP_Text antennaInstallationScoreText;
    public TMP_Text routerInstallationScoreText;
    public TMP_Text cableCrimpingScoreText;
    public TMP_Text cableLayingScoreText;
    public TMP_Text signalOptimizationScoreText;

    [Header("Assessment Scores")]
    public TMP_Text assessment1Text;
    public TMP_Text assessment2Text;
    public TMP_Text assessment3Text;
    public TMP_Text assessment4Text;
    public TMP_Text assessment5Text;

    [Header("Maintenance Scores")]
    public TMP_Text maintenance1Text;
    public TMP_Text maintenance2Text;
    public TMP_Text maintenance3Text;
    public TMP_Text maintenance4Text;
    public TMP_Text maintenance5Text;

    [Header("Examination Scores")]
    public TMP_Text exam1Text;
    public TMP_Text exam2Text;
    public TMP_Text exam3Text;
    public TMP_Text exam4Text;
    public TMP_Text exam5Text;
    public TMP_Text exam6Text;

    void Start()
    {
        Invoke(nameof(UpdateAllScores), 2f);
    }

    void UpdateAllScores()
    {
        var profile = UserSessionData.Instance.profileData;
        if (profile == null)
        {
            Debug.LogWarning("Profile data not loaded yet!");
            return;
        }

        // DEBUG: Print all training keys
        Debug.Log("Examination keys: " + string.Join(", ", profile.quizData.Keys));
        Debug.Log("Training keys: " + string.Join(", ", profile.trainingData.Keys));
        Debug.Log("Assessment keys: " + string.Join(", ", profile.assessmentData.Keys));
        Debug.Log("Maintenance keys: " + string.Join(", ", profile.maintenanceData.Keys));

        // TRAINING
        SetScoreText(profile.trainingData, "training_antenna_install", antennaInstallationScoreText);
        SetScoreText(profile.trainingData, "training_router_install", routerInstallationScoreText);
        SetScoreText(profile.trainingData, "training_crimping", cableCrimpingScoreText);
        SetScoreText(profile.trainingData, "training_cable_laying", cableLayingScoreText);
        SetScoreText(profile.trainingData, "training_signal_opt", signalOptimizationScoreText);

        // ASSESSMENT
        SetScoreText(profile.assessmentData, "assessment_level", assessment1Text);
        SetScoreText(profile.assessmentData, "assessment_part2", assessment2Text);
        SetScoreText(profile.assessmentData, "assessment_part3", assessment3Text);
        SetScoreText(profile.assessmentData, "assessment_part4", assessment4Text);
        SetScoreText(profile.assessmentData, "assessment_part5", assessment5Text);

        // MAINTENANCE
        SetScoreText(profile.maintenanceData, "Signal Loss After Storm", maintenance1Text);
        SetScoreText(profile.maintenanceData, "Sudden Slow Speeds", maintenance2Text);
        SetScoreText(profile.maintenanceData, "maintenance3", maintenance3Text);
        SetScoreText(profile.maintenanceData, "maintenance_part4", maintenance4Text);
        SetScoreText(profile.maintenanceData, "maintenance_part5", maintenance5Text);

        // EXAMINATION (if stored under quizData for now)
        SetScoreText(profile.quizData, "Crimp", exam1Text);
        SetScoreText(profile.quizData, "WISP", exam2Text);
        SetScoreText(profile.quizData, "Router", exam3Text);
        SetScoreText(profile.quizData, "Tool", exam4Text);
        SetScoreText(profile.quizData, "SOP", exam5Text);
        SetScoreText(profile.quizData, "Problem", exam6Text);
    }

    void SetScoreText(System.Collections.Generic.Dictionary<string, LevelData> data, string levelKey, TMP_Text textField)
    {
        if (textField == null) return;
        if (data == null || !data.ContainsKey(levelKey))
        {
            textField.text = "N/A";
            return;
        }

        LevelData level = data[levelKey];
        if (level.attempts == null || level.attempts.Count == 0)
        {
            textField.text = "0%";
            return;
        }

        // Get the FIRST recorded score (not latest)
        float firstScore = level.attempts.First().performanceScore;

        // Round to whole number and add %
        int roundedScore = Mathf.RoundToInt(firstScore);
        textField.text = $"{roundedScore}%";
    }
}
