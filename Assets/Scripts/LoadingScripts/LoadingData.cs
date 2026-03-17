using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingData : MonoBehaviour
{
    public static LoadingData Instance;

    [Header("Scene Management")]
    public string sceneToLoad;

    [Header("Quiz Specific Data")]
    public string quizTopicName;
    
    [Header("Maintenance Specific Data")]
    // Data to load a level
    public M_LevelData maintenanceLevelToLoad; 

    // Data to carry from diagnosis to the 3D fix scene
    public float diagnosisTime;
    public int infoActionsUsed;
    public int incorrectFixes;
    public string levelId;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}