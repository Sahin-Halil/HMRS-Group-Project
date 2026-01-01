using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Button))]
public class ButtonUnscaledTime : MonoBehaviour
{
    private Button button;
    private Mouse mouse;

    void Start()
    {
        button = GetComponent<Button>();
        mouse = Mouse.current;
    }

    void Update()
    {
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                button.GetComponent<RectTransform>(),
                mouse.position.ReadValue(),
                null))
            {
                button.onClick.Invoke();
            }
        }
    }
}