using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore; // Needed for checking email verification in user profile
using System.Threading.Tasks;
using Firebase;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField emailLoginInput;
    public TMP_InputField passwordLoginInput;
    public Button loginButton;
    public Button guestLoginButton;
    public TMP_Text loginStatusText;

    private FirebaseAuth auth;
    private bool isProcessing = false;

    void Start()
    {
        StartCoroutine(InitializeFirebaseManager());
    }

    IEnumerator InitializeFirebaseManager()
    {
        while (UserSessionData.Instance == null || UserSessionData.Instance.firebaseAuth == null)
        {
            if (UserSessionData.Instance == null) Debug.LogWarning("FirebaseManager waiting for UserSessionData.Instance...");
            else if (UserSessionData.Instance.firebaseAuth == null) Debug.LogWarning("FirebaseManager waiting for UserSessionData.Instance.firebaseAuth to be initialized by UserSessionData's Awake...");
            yield return null;
        }

        auth = UserSessionData.Instance.firebaseAuth;
        Debug.Log("FirebaseManager Initialized with Auth instance from UserSessionData.");

        if (loginButton != null) loginButton.onClick.AddListener(HandleLogin);
        if (guestLoginButton != null) guestLoginButton.onClick.AddListener(HandleGuestLogin);

        ClearStatusTexts();
    }

    private void ClearStatusTexts()
    {
         if(loginStatusText != null) loginStatusText.text = "";
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (loginButton != null) loginButton.interactable = interactable;
        if (guestLoginButton != null) guestLoginButton.interactable = interactable;
    }

    public async void HandleLogin()
    {
        if (isProcessing || emailLoginInput == null || passwordLoginInput == null || loginButton == null || loginStatusText == null) return;

        string email = emailLoginInput.text;
        string password = passwordLoginInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginStatusText.text = "Please enter email and password.";
            return;
        }

        isProcessing = true;
        loginStatusText.text = "Logging in...";
        SetButtonsInteractable(false);

        try
        {
            if (auth.CurrentUser != null)
            {
                Debug.Log($"HandleLogin: Current user ({auth.CurrentUser.UserId}, IsAnonymous: {auth.CurrentUser.IsAnonymous}) detected. Signing out before email/password login.");
                auth.SignOut(); // MARKER: Corrected SignOut call
                // Wait a brief moment for AuthStateChanged to potentially process the sign-out
                // This is a bit of a pragmatic delay; robust event chaining is better if issues persist.
                await Task.Delay(100); // Small delay to allow AuthStateChanged to fire for sign-out
                Debug.Log("HandleLogin: Previous user sign-out initiated.");
            }

            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser loggedInUser = result.User;
            Debug.LogFormat("FirebaseManager: User signed in successfully with Email/Pass: {0} ({1})", loggedInUser.DisplayName, loggedInUser.UserId);
            
            // Reload user to get latest email verification status
            await loggedInUser.ReloadAsync();
            loggedInUser = auth.CurrentUser;
            
            // Check email verification
            if (!loggedInUser.IsEmailVerified)
            {
                Debug.LogWarning($"FirebaseManager: Email not verified for user {loggedInUser.UserId}. Checking profile...");
                
                // Check profile to see if this account was created with email verification requirement
                bool requiresVerification = false;
                if (UserSessionData.Instance != null && UserSessionData.Instance.firestoreDb != null)
                {
                    try
                    {
                        var profileDoc = await UserSessionData.Instance.firestoreDb.Collection("userProfiles").Document(loggedInUser.UserId).GetSnapshotAsync();
                        if (profileDoc.Exists)
                        {
                            // Check if profile has emailVerified field (accounts created after verification requirement was added)
                            var profileDict = profileDoc.ToDictionary();
                            bool hasEmailVerificationFlag = profileDict != null && profileDict.ContainsKey("emailVerified");
                            
                            if (hasEmailVerificationFlag)
                            {
                                // Account was created with verification requirement - must verify
                                requiresVerification = true;
                            }
                            // If account doesn't have emailVerified flag, allow login for backward compatibility
                        }
                        else
                        {
                            // No profile found - require verification for new accounts
                            requiresVerification = true;
                        }
                    }
                    catch (System.Exception profileEx)
                    {
                        Debug.LogWarning($"FirebaseManager: Could not check profile for email verification status: {profileEx.Message}");
                        // If we can't check profile, err on the side of security and require verification
                        requiresVerification = true;
                    }
                }
                else
                {
                    // Can't check profile, require verification for security
                    requiresVerification = true;
                }
                
                if (requiresVerification)
                {
                    auth.SignOut();
                    loginStatusText.text = "Please verify your email before logging in. Check your inbox for the verification link.";
                    Debug.Log("FirebaseManager: Signing out unverified user.");
                    SetButtonsInteractable(true);
                    isProcessing = false;
                    return;
                }
            }
            
            loginStatusText.text = "Login Successful! Loading profile...";

            if (UserSessionData.Instance != null)
            {
                await UserSessionData.Instance.UserLoggedIn(loggedInUser);
                Debug.Log("FirebaseManager: Profile loaded/initialized after Email/Pass login. Proceeding to main menu...");
                // SceneManager.LoadScene("mainMenu");
                FindObjectOfType<SceneFader>().FadeOutAndLoad("mainMenu");
            }
            else
            {
                 Debug.LogError("FirebaseManager: UserSessionData.Instance is null after login!");
                 loginStatusText.text = "Login OK, but session manager error.";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"FirebaseManager: SignInWithEmailAndPasswordAsync encountered an error: {ex}");
            loginStatusText.text = "Login Failed";
        }
        finally
        {
            SetButtonsInteractable(true);
            isProcessing = false;
        }
    }

    public async void HandleGuestLogin()
    {
        if (isProcessing || guestLoginButton == null || loginStatusText == null) return;

        isProcessing = true;
        loginStatusText.text = "Logging in as Guest...";
        SetButtonsInteractable(false);

        try
        {
            // MARKER: Sign out any existing user before anonymous login for a clean session
            if (auth.CurrentUser != null)
            {
                Debug.Log($"HandleGuestLogin: Current user ({auth.CurrentUser.UserId}) detected. Signing out before anonymous login.");
                auth.SignOut();
                await Task.Delay(100); // Small delay
                Debug.Log("HandleGuestLogin: Previous user signed out.");
            }

            AuthResult result = await auth.SignInAnonymouslyAsync();
            FirebaseUser guestUser = result.User;
            Debug.LogFormat("FirebaseManager: User signed in anonymously: {0}", guestUser.UserId);
            loginStatusText.text = "Guest Login Successful!";

            if (UserSessionData.Instance != null)
            {
                await UserSessionData.Instance.GuestLoggedIn(guestUser);
                Debug.Log("FirebaseManager: Guest profile ready. Proceeding to main menu...");
                // SceneManager.LoadScene("mainMenu");
                FindObjectOfType<SceneFader>().FadeOutAndLoad("mainMenu");

            }
             else
            {
                 Debug.LogError("FirebaseManager: UserSessionData.Instance is null after guest login!");
                 loginStatusText.text = "Guest Login OK, but session manager error.";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"FirebaseManager: SignInAnonymouslyAsync encountered an error: {ex}");
            loginStatusText.text = "Guest Login Failed";
        }
        finally
        {
            SetButtonsInteractable(true);
            isProcessing = false;
        }
    }
}