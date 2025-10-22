using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RebindKey : MonoBehaviour
{
    [SerializeField] private InputActionReference action; // Specific movement action we want to change (movement, crouch etc.)
    [SerializeField] private string compositePart; // Specific composite action we want to change like up, down etc. Left empty if non composite (crouch, jump)
    [SerializeField] private Button rebindButton; // The button actually doing the rebinding
    [SerializeField] private TextMeshProUGUI label;

    private int bindingIndex;

    private void Start()
    {
        bindingIndex = FindBindingIndex(compositePart);
        LoadBindingOverride();
        UpdateLabel();
        rebindButton.onClick.AddListener(StartRebind);
    }

    private int FindBindingIndex(string partName)
    {
        
        var bindings = action.action.bindings;

        if (string.IsNullOrEmpty(partName)) return 0;

        for (int i = 0; i < bindings.Count; i++)
        {
            if (bindings[i].isPartOfComposite && bindings[i].name == partName)
                return i;
        }
        Debug.LogError($"Binding part '{partName}' not found on {action.action.name}");
        return -1;
    }

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

    private void SaveBindingOverride()
    {
        if (bindingIndex < 0) return;
        string saveKey = $"{action.action.actionMap.name}.{action.action.name}.{compositePart}";
        PlayerPrefs.SetString(saveKey, action.action.bindings[bindingIndex].overridePath);
    }

    private void LoadBindingOverride()
    {
        if (bindingIndex < 0) return;
        string saveKey = $"{action.action.actionMap.name}.{action.action.name}.{compositePart}";
        string overridePath = PlayerPrefs.GetString(saveKey, "");
        if (!string.IsNullOrEmpty(overridePath))
            action.action.ApplyBindingOverride(bindingIndex, overridePath);
    }
}
