using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "DefaultInfoData", menuName = "WISP/Default Info Data")]
public class M_DefaultInfoData : ScriptableObject
{
    public List<DefaultInfoEntry> defaultInfoEntries = new List<DefaultInfoEntry>();

    // This now returns a tuple containing both the description and the points.
    public (string description, int points) GetDefaultInfo(string infoName)
    {
        var entry = defaultInfoEntries.FirstOrDefault(e => e.infoName == infoName);
        if (entry != null)
        {
            return (entry.description, entry.points);
        }
        return ("No specific abnormalities found.", 0); // Fallback
    }
}

[System.Serializable]
public class DefaultInfoEntry
{
    public string infoName;
    [TextArea(2, 5)] public string description;
    public int points; // Will always be 0 for default entries.
}