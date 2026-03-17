using UnityEngine;

public class BackToLoginButton : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject createAccountPanel;

    public void BackToLogin()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (createAccountPanel != null) createAccountPanel.SetActive(false);
    }
}
