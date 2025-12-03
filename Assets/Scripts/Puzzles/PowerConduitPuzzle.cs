using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerConduitPuzzle : PuzzleConsole
{
    // Puzzle Settings and UI
    public int gridWidth = 4;
    public int gridHeight = 3;
    public int totalNumbers = 10;

    public Transform gridParent;
    public GameObject numberButtonPrefab;
    public TMP_Text instructionText;

    public Color buttonNormalColor = Color.white;
    public Color buttonClickedColor = new Color(1f, 1f, 1f, 0.3f);

    private List<NumberButton> allButtons;
    private int currentNumber = 1; // Start at 1, click up to 10

    protected override void OnPuzzleStart()
    {
        allButtons = new List<NumberButton>();
        currentNumber = 1;

        GenerateNumberGrid();

        if (instructionText != null)
        {
            instructionText.text = "Click the numbers in order: 1 to 10";
        }
    }

    void GenerateNumberGrid()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        allButtons.Clear();

        List<int> numbers = new List<int>();
        for (int i = 1; i <= totalNumbers; i++)
        {
            numbers.Add(i);
        }

        // Shuffle the numbers
        for (int i = 0; i < numbers.Count; i++)
        {
            int temp = numbers[i];
            int randomIndex = Random.Range(i, numbers.Count);
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }

        // Create buttons in grid
        int totalSlots = gridWidth * gridHeight;
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject obj = Instantiate(numberButtonPrefab, gridParent);
            NumberButton numButton = obj.GetComponent<NumberButton>();

            if (numButton != null && i < numbers.Count)
            {
                numButton.Initialize(numbers[i], this);
                allButtons.Add(numButton);
            }
            else if (numButton != null)
            {
                numButton.gameObject.SetActive(false);
            }
        }
    }

    // Check correct number clicked
    public void OnNumberClicked(NumberButton button)
    {
        if (button.number == currentNumber)
        {
            button.MarkAsClicked();
            currentNumber++;

            if (currentNumber > totalNumbers)
            {
                StartCoroutine(PuzzleComplete());
            }
        }
    }

    IEnumerator PuzzleComplete()
    {
        if (instructionText != null)
        {
            instructionText.text = "SUCCESS! Sequence complete!";
            instructionText.color = Color.green;
        }

        yield return new WaitForSeconds(1.5f);

        PuzzleSolved();
    }

    protected override void OnPuzzleComplete()
    {
        // Cleanup
    }
}