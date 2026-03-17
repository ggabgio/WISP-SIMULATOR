using UnityEngine;

public class RegisterPanelOpener : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject createAccountPanel;

    public void ShowRegisterPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (createAccountPanel != null) createAccountPanel.SetActive(true);
    }
}
