using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RebindKey : MonoBehaviour
{
    // References for rebinding setup
    [SerializeField] private InputActionReference action;
    [SerializeField] private string compositePart;
    [SerializeField] private Button rebindButton;
    [SerializeField] private TextMeshProUGUI label;

    private int bindingIndex;

    // Initialize rebind setup and load saved bindings
    private void Start()
    {
        bindingIndex = FindBindingIndex(compositePart);
        LoadBindingOverride();
        UpdateLabel();
        rebindButton.onClick.AddListener(StartRebind);
    }

    // Finds index of binding part within the action
    private int FindBindingIndex(string partName)
    {
        var bindings = action.action.bindings;

        if (string.IsNullOrEmpty(partName)) return 0;

        for (int i = 0; i < bindings.Count; i++)
        {
            if (bindings[i].isPartOfComposite && bindings[i].name == partName)
                return i;
        }
        return -1;
    }

    // Starts interactive key rebinding process
    private void StartRebind()
    {
        if (bindingIndex < 0) return;

        rebindButton.interactable = false;
        label.text = "<Press a key>";

        action.action.PerformInteractiveRebinding(bindingIndex)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                op.Dispose();
                rebindButton.interactable = true;
                SaveBindingOverride();
                UpdateLabel();
            })
            .Start();
    }

    // Updates on-screen label to show current binding
    private void UpdateLabel()
    {
        if (bindingIndex >= 0)
        {
            label.text = action.action.GetBindingDisplayString(
                bindingIndex,
                InputBinding.DisplayStringOptions.DontIncludeInteractions
            );
        }
    }

    // Saves overridden key binding to player preferences
    private void SaveBindingOverride()
    {
        if (bindingIndex < 0) return;
        string saveKey = $"{action.action.actionMap.name}.{action.action.name}.{compositePart}";
        PlayerPrefs.SetString(saveKey, action.action.bindings[bindingIndex].overridePath);
    }

    // Loads saved key binding override if it exists
    private void LoadBindingOverride()
    {
        if (bindingIndex < 0) return;
        string saveKey = $"{action.action.actionMap.name}.{action.action.name}.{compositePart}";
        string overridePath = PlayerPrefs.GetString(saveKey, "");
        if (!string.IsNullOrEmpty(overridePath))
            action.action.ApplyBindingOverride(bindingIndex, overridePath);
    }
}
