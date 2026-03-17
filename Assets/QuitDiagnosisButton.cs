using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitDiagnosisButton : MonoBehaviour
{
    public async void QuitAndReturnToMenu()
    {
        UI_DiagnosisManager manager = FindObjectOfType<UI_DiagnosisManager>();
        
        if (manager != null && manager.CurrentLevelData != null)
        {
            if (UserSessionData.Instance != null && !string.IsNullOrEmpty(manager.CurrentLevelData.levelName))
            {
                await UserSessionData.Instance.UpdateMaintenanceLevelProgress(manager.CurrentLevelData.levelName, false, 0f);
            }
        }
        
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
}