using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NavigationPuzzle : PuzzleConsole
{
    // Puzzle Settings and UI
    public int numberOfPairs = 5;

    public Transform leftColumn;
    public Transform rightColumn;
    public GameObject shapeButtonPrefab;
    public TMP_Text instructionText;

    public Color[] shapeColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };
    public string[] shapeSymbols = { "●", "■", "▲", "★", "◆" };

    private List<ShapeButton> leftButtons;
    private List<ShapeButton> rightButtons;
    private ShapeButton firstSelected = null;

    protected override void OnPuzzleStart()
    {
        leftButtons = new List<ShapeButton>();
        rightButtons = new List<ShapeButton>();
        firstSelected = null;

        GenerateMatchingPuzzle();

        if (instructionText != null)
        {
            instructionText.text = "Match the shapes! Click two to swap them.";
        }
    }

    // Create matching pairs and shuffle one column
    void GenerateMatchingPuzzle()
    {
        foreach (Transform child in leftColumn)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rightColumn)
        {
            Destroy(child.gameObject);
        }
        leftButtons.Clear();
        rightButtons.Clear();

        List<int> leftOrder = new List<int>();
        List<int> rightOrder = new List<int>();

        for (int i = 0; i < numberOfPairs; i++)
        {
            leftOrder.Add(i);
            rightOrder.Add(i);
        }

        // Shuffle right column
        for (int i = 0; i < rightOrder.Count; i++)
        {
            int temp = rightOrder[i];
            int randomIndex = Random.Range(i, rightOrder.Count);
            rightOrder[i] = rightOrder[randomIndex];
            rightOrder[randomIndex] = temp;
        }

        // Create left column buttons in order
        for (int i = 0; i < numberOfPairs; i++)
        {
            CreateShapeButton(leftOrder[i], leftColumn, true, i);
        }

        // Create right column buttons shuffled
        for (int i = 0; i < numberOfPairs; i++)
        {
            CreateShapeButton(rightOrder[i], rightColumn, false, i);
        }
    }

    void CreateShapeButton(int shapeIndex, Transform parent, bool isLeftColumn, int position)
    {
        GameObject obj = Instantiate(shapeButtonPrefab, parent);
        ShapeButton shapeBtn = obj.GetComponent<ShapeButton>();

        if (shapeBtn != null)
        {
            shapeBtn.Initialize(
                shapeIndex,
                shapeColors[shapeIndex],
                shapeSymbols[shapeIndex],
                this,
                isLeftColumn,
                position
            );

            if (isLeftColumn)
            {
                leftButtons.Add(shapeBtn);
            }
            else
            {
                rightButtons.Add(shapeBtn);
            }
        }
    }

    public void OnShapeClicked(ShapeButton button)
    {
        // Only allow swap within the same column
        if (firstSelected == null)
        {
            firstSelected = button;
            button.SetSelected(true);
        }
        else
        {
            // Deselect
            if (firstSelected == button)
            {
                firstSelected.SetSelected(false);
                firstSelected = null;
            }
            else if (firstSelected.isLeftColumn == button.isLeftColumn)
            {
                SwapButtons(firstSelected, button);
                firstSelected.SetSelected(false);
                firstSelected = null;

                CheckSolution();
            }
            else
            {
                firstSelected.SetSelected(false);
                firstSelected = button;
                button.SetSelected(true);
            }
        }
    }

    void SwapButtons(ShapeButton button1, ShapeButton button2)
    {
        int index1 = button1.transform.GetSiblingIndex();
        int index2 = button2.transform.GetSiblingIndex();

        button1.transform.SetSiblingIndex(index2);
        button2.transform.SetSiblingIndex(index1);

        int tempPos = button1.position;
        button1.position = button2.position;
        button2.position = tempPos;
    }

    void CheckSolution()
    {
        bool allMatched = true;

        List<ShapeButton> currentLeftOrder = new List<ShapeButton>();
        List<ShapeButton> currentRightOrder = new List<ShapeButton>();

        foreach (Transform child in leftColumn)
        {
            ShapeButton btn = child.GetComponent<ShapeButton>();
            if (btn != null)
            {
                currentLeftOrder.Add(btn);
            }
        }

        foreach (Transform child in rightColumn)
        {
            ShapeButton btn = child.GetComponent<ShapeButton>();
            if (btn != null)
            {
                currentRightOrder.Add(btn);
            }
        }

        // Check if LHS matches RHS
        for (int i = 0; i < currentLeftOrder.Count; i++)
        {
            if (i >= currentRightOrder.Count || currentLeftOrder[i].shapeIndex != currentRightOrder[i].shapeIndex)
            {
                allMatched = false;
                break;
            }
        }

        Debug.Log($"Checking solution... All matched: {allMatched}");

        if (allMatched)
        {
            StartCoroutine(PuzzleComplete());
        }
    }

    IEnumerator PuzzleComplete()
    {
        if (instructionText != null)
        {
            instructionText.text = "SUCCESS! All shapes matched!";
            instructionText.color = Color.green;
        }

        // Flash all buttons green
        foreach (var btn in leftButtons)
        {
            btn.GetComponent<Image>().color = Color.green;
        }
        foreach (var btn in rightButtons)
        {
            btn.GetComponent<Image>().color = Color.green;
        }

        yield return new WaitForSeconds(2f);

        PuzzleSolved();
    }

    protected override void OnPuzzleComplete()
    {
        // Cleanup
    }
}