using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "WISP/Level Data")]
public class M_LevelData : ScriptableObject
{
    [Header("Basic Info")]
    public string levelName;
    public string clientName;
    public string plan;
    [TextArea(2, 5)] public string customerReport;

    [Header("Diagnosis & Fix")]
    public string correctFix;
    public string fixSceneName; // The name of the 3D scene to load for the fix.
    public int requiredPointsToFix; // Points needed to unlock the "Proceed to Fixing" button.
    
    [Header("Diagnosis Info")]
    public List<InformationPoint> infoPoints = new List<InformationPoint>();
}

[System.Serializable]
public class InformationPoint
{
    public string category;
    public string infoName;
    [TextArea(2, 5)] public string description;
    public int points; // How many points this piece of information is worth.
}