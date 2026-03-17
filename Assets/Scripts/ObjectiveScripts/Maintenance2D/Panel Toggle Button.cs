using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelToggleButton : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The panel that will be activated when this button is clicked.")]
    [SerializeField] private GameObject _panelToShow;

    [Tooltip("The panel that will be deactivated when this button is clicked.")]
    [SerializeField] private GameObject _panelToHide;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(TogglePanels);
    }

    private void TogglePanels()
    {
        if (_panelToShow != null)
        {
            _panelToShow.SetActive(true);
        }

        if (_panelToHide != null)
        {
            _panelToHide.SetActive(false);
        }
    }
}