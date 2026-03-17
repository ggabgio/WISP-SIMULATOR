using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

// Define a class to match the JSON structure from PHP
[System.Serializable] // Required for JsonUtility
class LoginResponse
{
    public string status;
    public string message; // Optional: use the message from PHP
    public int userId;     // Needs to match JSON key "userId"
    public string username; // Optional: Use username from PHP if sent
}

public class LoginManager : MonoBehaviour
{
    // --- Assign in Inspector ---
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button guestButton;
    public TMP_Text statusText;
    public GameObject inputBlocker;

    private string loginUrl = "http://localhost/ecesim_api/login.php";

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        guestButton.onClick.AddListener(GuestLogin);
        if (inputBlocker != null) inputBlocker.SetActive(false); // Ensure blocker starts hidden
    }

    public void GuestLogin()
    {
        SetStatus("Login Successful!");
        // --- Set UserSession for Guest ---
        if (UserSession.Instance != null)
        {
            UserSession.Instance.username = "Guest";
            UserSession.Instance.userId = 0; // Or -1, or some indicator for Guest
        }
        // --- Start transition ---
        if (inputBlocker != null) inputBlocker.SetActive(true);
        StartCoroutine(DelayToNextScene());
    }

    IEnumerator DelayToNextScene()
    {
        yield return new WaitForSeconds(2f); // wait 2 seconds
        SceneManager.LoadScene("mainMenu"); // Make sure "Main Menu" is in Build Settings
    }

    public void OnLoginButtonClicked()
    {
        string username = usernameInput.text.Trim(); // Trim input
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            SetStatus("Username and Password cannot be empty.");
            return;
        }

        if (loginButton != null) loginButton.interactable = false;
        if (guestButton != null) guestButton.interactable = false; // Also disable guest button
        SetStatus("Logging in...");

        StartCoroutine(AttemptLogin(username, password));
    }

    IEnumerator AttemptLogin(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(loginUrl, form))
        {
            yield return webRequest.SendWebRequest();

            // --- Re-enable buttons regardless of outcome ---
            if (loginButton != null) loginButton.interactable = true;
            if (guestButton != null) guestButton.interactable = true;

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError ||
                webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Error: " + webRequest.error);
                SetStatus("Error: Could not connect. " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Server Response: " + jsonResponse);

                // --- Parse JSON Response ---
                try
                {
                    LoginResponse responseData = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                    // Use the status from the parsed JSON
                    if (responseData.status == "success")
                    {
                        SetStatus("Login Successful!"); // Or use responseData.message

                        // --- !!! Store User Data in Session !!! ---
                        if (UserSession.Instance != null)
                        {
                            UserSession.Instance.username = responseData.username; // Use username from response
                            UserSession.Instance.userId = responseData.userId;
                            Debug.Log($"Logged in as {UserSession.Instance.username} (ID: {UserSession.Instance.userId})");
                        }
                        else {
                             Debug.LogError("UserSession.Instance is null! Cannot store session data.");
                        }

                        // --- Start transition ---
                        if (inputBlocker != null) inputBlocker.SetActive(true);
                        StartCoroutine(DelayToNextScene());

                    }
                    else if (responseData.status == "user_not_found")
                    {
                        SetStatus("Login Failed: User not found."); // Or use responseData.message
                    }
                    else if (responseData.status == "wrong_password")
                    {
                        SetStatus("Login Failed: Incorrect password."); // Or use responseData.message
                    }
                    else
                    {
                        // Use the message from PHP if available
                        string errorMsg = string.IsNullOrEmpty(responseData.message) ? "Unexpected server response." : responseData.message;
                        SetStatus($"Login Failed: {errorMsg}");
                        Debug.LogError($"Unexpected server status: {responseData.status} - Message: {responseData.message}");
                    }
                }
                catch (System.Exception ex)
                {
                    // Handle cases where the response wasn't valid JSON
                    SetStatus("Login Failed: Could not parse server response.");
                    Debug.LogError("JSON Parse Error: " + ex.Message + "\nResponse Text: " + jsonResponse);
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