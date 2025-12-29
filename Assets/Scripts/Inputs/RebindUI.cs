using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RebindUI : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionReference actionReference;
    public int bindingIndex;

    [Header("UI")]
    public TextMeshProUGUI bindingText;

    void Start()
    {
        UpdateBindingDisplay();
    }

    // 绑定在 UI Button 的 OnClick()
    public void StartRebind()
    {
        bindingText.text = "Press a key...";

        actionReference.action.Disable();

        actionReference.action
            .PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                operation.Dispose();
                actionReference.action.Enable();
                UpdateBindingDisplay();
                SaveBinding();
            })
            .Start();
    }

    void UpdateBindingDisplay()
    {
        bindingText.text =
            InputControlPath.ToHumanReadableString(
                actionReference.action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
    }

    void SaveBinding()
    {
        PlayerPrefs.SetString(
            actionReference.action.name,
            actionReference.action.SaveBindingOverridesAsJson()
        );
    }
}
