using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberButton : MonoBehaviour
{
    public int number;
    private PowerConduitPuzzle puzzle;
    private Button button;
    private Image image;
    private TMP_Text text;
    private bool hasBeenClicked = false;

    public void Initialize(int num, PowerConduitPuzzle puzzleRef)
    {
        number = num;
        puzzle = puzzleRef;

        button = GetComponent<Button>();
        image = GetComponent<Image>();
        text = GetComponentInChildren<TMP_Text>();

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        if (text != null)
        {
            text.text = number.ToString();
            text.fontSize = 36;
            text.color = Color.black;
        }

        if (image != null)
        {
            image.color = puzzle.buttonNormalColor;
        }
    }

    void OnClick()
    {
        if (!hasBeenClicked)
        {
            puzzle.OnNumberClicked(this);
        }
    }

    public void MarkAsClicked()
    {
        hasBeenClicked = true;

        if (image != null)
        {
            image.color = puzzle.buttonClickedColor;
        }

        if (button != null)
        {
            button.interactable = false;
        }
    }
}