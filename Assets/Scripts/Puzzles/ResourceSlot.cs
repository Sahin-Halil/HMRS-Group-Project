using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceSlot : MonoBehaviour
{
    public ResourceType requiredType;
    private LifeSupportPuzzle puzzle;
    private Button button;
    private Image image;
    private TMP_Text labelText;
    private TMP_Text statusText;
    private Color slotColor;
    private bool isFilled = false;

    public void Initialize(ResourceType type, string label, Color color, LifeSupportPuzzle puzzleRef)
    {
        requiredType = type;
        slotColor = color;
        puzzle = puzzleRef;

        button = GetComponent<Button>();
        image = GetComponent<Image>();

        // Get child texts
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.name == "Label")
            {
                labelText = child.GetComponent<TMP_Text>();
            }
            else if (child.name == "Status")
            {
                statusText = child.GetComponent<TMP_Text>();
            }
        }

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        if (image != null)
        {
            image.color = new Color(slotColor.r, slotColor.g, slotColor.b, 0.3f);
        }

        if (labelText != null)
        {
            labelText.text = label;
            labelText.fontSize = 28;
            labelText.color = Color.white;
        }

        if (statusText != null)
        {
            statusText.text = "EMPTY";
            statusText.fontSize = 20;
            statusText.color = Color.gray;
        }
    }

    void OnClick()
    {
        puzzle.OnSlotClicked(this);
    }

    public bool IsFilled()
    {
        return isFilled;
    }

    public void FillSlot(ResourceButton resource)
    {
        isFilled = true;

        if (image != null)
        {
            image.color = slotColor;
        }

        if (statusText != null)
        {
            statusText.text = "FILLED";
            statusText.color = Color.green;
        }
    }
}
