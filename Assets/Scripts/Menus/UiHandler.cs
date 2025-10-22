using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UiHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color pressedColor;

    //Ensures that colors are reset when buttons are enabled and disabled
    void OnEnable()
    {
        buttonText.color = normalColor;
    }

    void OnDisable()
    {
        buttonText.color = normalColor;
    }
    public void OnPointerEnter(PointerEventData eventData) => buttonText.color = hoverColor;
    public void OnPointerExit(PointerEventData eventData) => buttonText.color = normalColor;
    public void OnPointerDown(PointerEventData eventData) => buttonText.color = pressedColor;
    public void OnPointerUp(PointerEventData eventData) => buttonText.color = hoverColor;
}
