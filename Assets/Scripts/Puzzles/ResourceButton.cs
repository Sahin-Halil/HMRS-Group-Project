using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceButton : MonoBehaviour
{
    public ResourceType resourceType;
    private LifeSupportPuzzle puzzle;
    private Button button;
    private Image image;
    private TMP_Text text;
    private Color normalColor;
    private bool isSelected = false;

    public void Initialize(ResourceType type, Color color, string label, LifeSupportPuzzle puzzleRef)
    {
        resourceType = type;
        normalColor = color;
        puzzle = puzzleRef;

        button = GetComponent<Button>();
        image = GetComponent<Image>();
        text = GetComponentInChildren<TMP_Text>();

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        if (image != null)
        {
            image.color = normalColor;
        }

        if (text != null)
        {
            text.text = label;
            text.fontSize = 32;
            text.color = Color.white;
        }
    }

    void OnClick()
    {
        puzzle.OnResourceClicked(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (image != null)
        {
            if (selected)
            {
                image.color = Color.white; // Brighten when selected
            }
            else
            {
                image.color = normalColor;
            }
        }
    }
}