using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UiHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    // UI text and color states
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color pressedColor;

    // Ensures button color is reset when enabled or disabled
    void OnEnable()
    {
        buttonText.color = normalColor;
    }

    void OnDisable()
    {
        buttonText.color = normalColor;
    }

    // Handles color changes on hover and click
    public void OnPointerEnter(PointerEventData eventData) => buttonText.color = hoverColor;
    public void OnPointerExit(PointerEventData eventData) => buttonText.color = normalColor;
    public void OnPointerDown(PointerEventData eventData) => buttonText.color = pressedColor;
    public void OnPointerUp(PointerEventData eventData) => buttonText.color = hoverColor;
}
