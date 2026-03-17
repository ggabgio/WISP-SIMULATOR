using UnityEngine;
using UnityEngine.SceneManagement;

public class returnToMenu : MonoBehaviour
{
    public void LoadMenu()
    {
        AbstractObjectiveManager manager = FindObjectOfType<AbstractObjectiveManager>();
        if (manager != null)
        {
            manager.QuitLevel();
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
