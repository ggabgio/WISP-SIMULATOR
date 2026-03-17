using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System; // Required for Action

public class RouterConfiguration : MonoBehaviour
{
    [Header("Minigame Panels")]
    [SerializeField] private GameObject mainConfigurationPanel;
    [SerializeField] private GameObject pppoePanel;
    [SerializeField] private KeyCode closeMinigameKey = KeyCode.Escape;

    [Header("UI & Player References")]
    [SerializeField] private List<GameObject> hideObjects = new List<GameObject>();
    [SerializeField] private MonoBehaviour playerLookScript;
    [SerializeField] private GameObject hotbarUI;

    [Header("Main Config UI Elements")]
    [SerializeField] private TMP_InputField ssidInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_Dropdown wanTypeDropdown;
    [SerializeField] private Button applyMainSettingsButton;
    [SerializeField] private TMP_Text configInstructionsText;

    [Header("PPPoE Config UI Elements")]
    [SerializeField] private TMP_InputField pppoeUsernameInputField;
    [SerializeField] private TMP_InputField pppoePasswordInputField;
    [SerializeField] private Button applyPppoeSettingsButton;

    [Header("Configuration Targets")]
    [SerializeField] private string targetSSID = "DLCNS_WiFi";
    [SerializeField] private string targetPppoeUsername = "DLCNS_Client01";
    
    private bool isMinigameActive = false;
    private string targetWifiPassword;
    private string targetPppoePassword;

    private AbstractObjectiveManager manager;
    private Action<float> onCompleteCallback;

    private bool ssidCorrect, wifiPasswordCorrect, wanTypeCorrect, pppoeUserCorrect, pppoePasswordCorrect;

    // Public property to check if minigame is active
    public bool IsMinigameActive => isMinigameActive;

    void Start()
    {
        if (mainConfigurationPanel != null) mainConfigurationPanel.SetActive(false);
        if (pppoePanel != null) pppoePanel.SetActive(false);
        SetupWanDropdown();
        applyMainSettingsButton?.onClick.AddListener(OnApplyMainSettings);
        applyPppoeSettingsButton?.onClick.AddListener(OnApplyPppoeSettings);
    }

    public void Initialize(AbstractObjectiveManager objectiveManager)
    {
        this.manager = objectiveManager;
    }

    void Update()
    {
        if (isMinigameActive && Input.GetKeyDown(closeMinigameKey))
        {
            EndMinigame();
        }
    }

    public void StartMinigame(Action<float> onComplete)
    {
        if (isMinigameActive) return;

        foreach (GameObject obj in hideObjects)
        {
            if (obj != null) obj.SetActive(false);
        }

        this.onCompleteCallback = onComplete;
        isMinigameActive = true;

        targetWifiPassword = GenerateRandomPassword(8);
        targetPppoePassword = GenerateRandomPassword(8);
        
        ssidCorrect = wifiPasswordCorrect = wanTypeCorrect = pppoeUserCorrect = pppoePasswordCorrect = false;
        ResetMainConfigFields();
        ResetPppoeFields();
        
        mainConfigurationPanel.SetActive(true);
        pppoePanel.SetActive(false);

        if (hotbarUI != null) hotbarUI.SetActive(false);

        var crosshair = GameObject.Find("CursorDot");
        if (crosshair != null) crosshair.SetActive(false);

        if (manager != null && manager.showTutorials && configInstructionsText != null)
        {
            configInstructionsText.text = $"Configure Router:\n- Set SSID to: {targetSSID}\n- Set Password to: {targetWifiPassword}\n- Set WAN Type to: PPPoE";
        }

        if (playerLookScript != null) playerLookScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EndMinigame()
    {
        if (!isMinigameActive) return;

        isMinigameActive = false;
        onCompleteCallback = null;
        mainConfigurationPanel.SetActive(false);
        pppoePanel.SetActive(false);
        
        foreach (GameObject obj in hideObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        if (hotbarUI != null) hotbarUI.SetActive(true);
        
        if (playerLookScript != null) playerLookScript.enabled = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var crosshair = GameObject.Find("CursorDot");
        if (crosshair != null) crosshair.SetActive(true);
    }

    private void OnApplyMainSettings()
    {
        ssidCorrect = (ssidInputField.text == targetSSID);
        wifiPasswordCorrect = (passwordInputField.text == targetWifiPassword);
        wanTypeCorrect = (wanTypeDropdown.options[wanTypeDropdown.value].text == "PPPoE");

        if (wanTypeCorrect)
        {
            mainConfigurationPanel.SetActive(false);
            pppoePanel.SetActive(true);

            if (configInstructionsText != null)
            {
                configInstructionsText.text = $"PPPoE Setup:\n- Set Username to: {targetPppoeUsername}\n- Set Password to: {targetPppoePassword}";
            }
        }
        else
        {
            pppoeUserCorrect = false;
            pppoePasswordCorrect = false;
            FinalizeConfiguration();
        }
    }

    private void OnApplyPppoeSettings()
    {
        pppoeUserCorrect = (pppoeUsernameInputField.text == targetPppoeUsername);
        pppoePasswordCorrect = (pppoePasswordInputField.text == targetPppoePassword);
        FinalizeConfiguration();
    }

    private void FinalizeConfiguration()
    {
        float score = 0f;
        if (ssidCorrect) score += 20f;
        if (wifiPasswordCorrect) score += 20f;
        if (wanTypeCorrect) score += 20f;
        if (pppoeUserCorrect) score += 20f;
        if (pppoePasswordCorrect) score += 20f;

        // Hide instructions text when configuration is complete
        if (configInstructionsText != null)
        {
            configInstructionsText.text = "";
        }

        onCompleteCallback?.Invoke(score);
        EndMinigame();
    }

    private void ResetMainConfigFields()
    {
        if (ssidInputField != null) ssidInputField.text = "";
        if (passwordInputField != null) passwordInputField.text = "";
        if (wanTypeDropdown != null) wanTypeDropdown.value = 0;
    }

    private void ResetPppoeFields()
    {
        if (pppoeUsernameInputField != null) pppoeUsernameInputField.text = "";
        if (pppoePasswordInputField != null) pppoePasswordInputField.text = "";
    }

    private void SetupWanDropdown()
    {
        if (wanTypeDropdown == null) return;
        wanTypeDropdown.ClearOptions();
        List<string> options = new List<string> { "DHCP", "Static", "PPPoE" };
        wanTypeDropdown.AddOptions(options);
    }

    private string GenerateRandomPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder sb = new StringBuilder();
        System.Random rng = new System.Random();
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[rng.Next(chars.Length)]);
        }
        return sb.ToString();
    }
}