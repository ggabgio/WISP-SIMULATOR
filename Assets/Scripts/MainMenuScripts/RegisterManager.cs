using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_Text statusText;
    public Button submitButton;

    private string registerUrl = "http://localhost/ecesim_api/create_account.php";


    void Start()
    {
        submitButton.onClick.AddListener(OnRegisterClicked);
    }

    void OnRegisterClicked()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            SetStatus("All fields are required.");
            return;
        }

        if (password != confirmPassword)
        {
            SetStatus("Passwords do not match.");
            return;
        }

        StartCoroutine(RegisterUser(username, password));
    }

    IEnumerator RegisterUser(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(registerUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
                SetStatus("Registration failed: " + www.error);
            }
            else
            {
                string response = www.downloadHandler.text;
                Debug.Log("Server Response: " + response);

                if (response.Contains("success"))
                {
                    SetStatus("Account created successfully!");
                }
                else if (response.Contains("exists"))
                {
                    SetStatus("Username already exists.");
                }
                else
                {
                    SetStatus("Registration failed: " + response);
                }
            }
        }
    }

    void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log("Status: " + message);
    }
}
