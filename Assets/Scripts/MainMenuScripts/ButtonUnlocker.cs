using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Button))]
public class ButtonUnlocker : MonoBehaviour
{
    [Header("Unlock Requirements")]
    [Tooltip("List of Level IDs that must be ATTEMPTED before this button is unlocked. Leave empty for buttons that are always unlocked.")]
    public List<string> requiredLevelIDs;
    
    private Button thisButton;

    void Awake()
    {
        thisButton = GetComponent<Button>();
    }

    void OnEnable()
    {
        CheckUnlockStatus();
    }

    private void CheckUnlockStatus()
    {
        // For testing and guest access, unlock the button by default.
        if (UserSessionData.Instance == null || UserSessionData.Instance.profileData == null || UserSessionData.Instance.profileData.username == "Guest")
        {
            thisButton.interactable = true;
            return;
        }
        
        // If a logged-in user is detected, proceed with the prerequisite logic.
        if (requiredLevelIDs == null || requiredLevelIDs.Count == 0)
        {
            thisButton.interactable = true;
            return;
        }

        bool allPrerequisitesMet = requiredLevelIDs.All(id => LevelHasBeenAttempted(id));
        thisButton.interactable = allPrerequisitesMet;
    }

    private bool LevelHasBeenAttempted(string levelId)
    {
        var profile = UserSessionData.Instance.profileData;
        
        if (profile.trainingData.ContainsKey(levelId)) return true;
        if (profile.quizData.ContainsKey(levelId)) return true;
        if (profile.assessmentData.ContainsKey(levelId)) return true;
        
        return false;
    }
}