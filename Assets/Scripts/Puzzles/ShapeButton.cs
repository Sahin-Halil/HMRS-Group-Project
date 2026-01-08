using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShapeButton : MonoBehaviour
{
    public int shapeIndex;
    public bool isLeftColumn;
    public int position;

    private NavigationPuzzle puzzle;
    private Button button;
    private Image image;
    private TMP_Text text;
    private Color originalColor;
    private bool isSelected = false;

    public void Initialize(int index, Color color, string symbol, NavigationPuzzle puzzleRef, bool leftCol, int pos)
    {
        shapeIndex = index;
        originalColor = color;
        puzzle = puzzleRef;
        isLeftColumn = leftCol;
        position = pos;

        button = GetComponent<Button>();
        image = GetComponent<Image>();
        text = GetComponentInChildren<TMP_Text>();

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        if (image != null)
        {
            image.color = originalColor;
        }

        if (text != null)
        {
            text.text = symbol;
            text.fontSize = 48;
            text.color = Color.white;
        }
    }

    void OnClick()
    {
        puzzle.OnShapeClicked(this);
    }

    // Visual feedback for selected shape
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (image != null)
        {
            if (selected)
            {
                image.color = Color.Lerp(originalColor, Color.white, 0.5f);
            }
            else
            {
                image.color = originalColor;
            }
        }
    }
}
