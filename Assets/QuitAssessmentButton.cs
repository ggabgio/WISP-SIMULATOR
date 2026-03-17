using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitAssessmentButton : MonoBehaviour
{
    public void QuitAndReturnToMenu()
    {
        CombinedLevelManager manager = FindObjectOfType<CombinedLevelManager>();
        if (manager != null)
        {
            manager.QuitAssessment();
        }
        else
        {
            // Fallback in case the manager is not found.
            Time.timeScale = 1f;
            SceneManager.LoadScene("mainMenu");
        }
    }
}