using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ClipboardEntry : MonoBehaviour
{
    private TextMeshProUGUI _entryText;

    private void Awake()
    {
        _entryText = GetComponent<TextMeshProUGUI>();
    }
    public void Initialize(string category, string source, string information, int entryIndex)
    {
        if (_entryText == null) return;
        
        _entryText.text = $"{entryIndex}.) [{category}] {source} → {information}";
    }
}