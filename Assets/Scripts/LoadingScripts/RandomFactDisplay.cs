using UnityEngine;
using TMPro;

public class RandomFactDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text factText; // Assign TextMeshPro text here

    [Header("Facts List")]
    [TextArea(2, 5)]
    public string[] facts = new string[]
    {
        "Fiber optic cables can transmit data at nearly the speed of light.",
        "Wireless signals weaken when passing through walls and obstacles.",
        "Network latency is the time it takes for data to travel from source to destination.",
        "5G technology can deliver latency as low as 1 millisecond.",
        "A router forwards data packets between computer networks.",
        "Bandwidth is the maximum amount of data that can be transferred in a network.",
        "Wi-Fi stands for 'Wireless Fidelity'.",
        "Most operating systems have built-in network diagnostic tools.",
        "Servers are powerful computers that manage network resources.",
        "IP addresses are unique identifiers assigned to every device connected to a network."
    };

    void Start()
    {
        if (factText == null)
        {
            Debug.LogWarning("RandomFactDisplay: No Text component assigned!");
            return;
        }

        if (facts.Length == 0)
        {
            factText.text = "No facts available.";
            return;
        }

        // Choose a random fact and display it
        int randomIndex = Random.Range(0, facts.Length);
        factText.text = facts[randomIndex];
    }
}
