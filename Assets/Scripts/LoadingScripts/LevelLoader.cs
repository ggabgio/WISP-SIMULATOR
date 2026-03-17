using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LoadLevel: sceneName is null or empty!");
            return;
        }
        if (LoadingData.Instance == null)
        {
            Debug.LogError("LevelLoader: LoadingData.Instance is null! Make sure LoadingManager exists in the scene.");
            return;
        }

        LoadingData.Instance.sceneToLoad = sceneName;
        // SceneManager.LoadScene("LoadingScene");
        FindObjectOfType<SceneFader>().FadeOutAndLoad("LoadingScene");

    }
    
    public void SetQuizTopic(string topicName)
    {
        LoadingData.Instance.quizTopicName = topicName;
        Debug.Log($"Quiz topic set to: {topicName}");
        // SceneManager.LoadScene("QuizScene");
        FindObjectOfType<SceneFader>().FadeOutAndLoad("QuizScene");
    }
    
    public void LoadMaintenanceLevel(M_LevelData maintenanceLevel)
    {
        LoadingData.Instance.maintenanceLevelToLoad = maintenanceLevel;
        Debug.Log($"Maintenance Level Loaded: {maintenanceLevel}");
        // SceneManager.LoadScene("Maintenance");
        FindObjectOfType<SceneFader>().FadeOutAndLoad("Maintenance");
    }
}