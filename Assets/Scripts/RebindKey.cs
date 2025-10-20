using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Search;

public class RebindKey : MonoBehaviour
{
    [SerializeField] private InputActionReference action; // Action that we want to rebind
    [SerializeField] private Button rebindButton;
    [SerializeField] private TextMeshProUGUI label;

    private void Start()
    {
        // Show current label when started
        UpdateLabel();

        // Listen for the new key when user clicks button
        rebindButton.onClick.AddListener(StartRebind);
    }

    private void StartRebind()
    {
        rebindButton.interactable = false;
        label.text = "<Press a key>";

        action.action.PerformInteractiveRebinding().OnComplete(op =>
        {
            op.Dispose(); // Delete the original keybind
            rebindButton.interactable = true; // Allow user to rebind again
            UpdateLabel(); // Reflect changes
        }).Start();
    }

    private void UpdateLabel()
    {
        if (action!=null && action.action!=null)
        {
            label.text = action.action.GetBindingDisplayString();
        }
    }
}
