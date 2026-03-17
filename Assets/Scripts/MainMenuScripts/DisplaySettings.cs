using UnityEngine;
using TMPro; // Use TextMeshPro namespace

public class DisplaySettings : MonoBehaviour
{
    public TMP_Dropdown displayModeDropdown; // Assign via Inspector

    private void Start()
    {
        // Default to Borderless Windowed (index 1)
        int savedMode = PlayerPrefs.GetInt("DisplayMode", 1); 
        displayModeDropdown.value = savedMode;
        ApplyDisplayMode(savedMode);

        // Add listener
        displayModeDropdown.onValueChanged.AddListener(ApplyDisplayMode);
    }

    public void ApplyDisplayMode(int modeIndex)
    {
        switch (modeIndex)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }

        PlayerPrefs.SetInt("DisplayMode", modeIndex);

        // Debug output to confirm mode change
        Debug.Log("Display mode set to: " + Screen.fullScreenMode);
    }
}
