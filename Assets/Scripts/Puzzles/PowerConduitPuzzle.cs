using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerConduitPuzzle : PuzzleConsole
{
    // Puzzle Settings and UI
    public int gridWidth = 3;
    public int gridHeight = 3;
    public int totalNumbers = 10;
    public float timeLimit = 10f;

    public Transform gridParent;
    public GameObject numberButtonPrefab;
    public TMP_Text instructionText;
    public TMP_Text timerText;

    public Color normalColor = Color.white;
    public Color buttonNormalColor = Color.white;
    public Color buttonClickedColor = new Color(1f, 1f, 1f, 0.3f);

    private List<NumberButton> allButtons;
    private int currentNumber = 1; // Start at 1, click up to 10

    private float timeRemaining;
    private bool puzzleStarted = false;

    protected override void OnPuzzleStart()
    {
        allButtons = new List<NumberButton>();
        currentNumber = 1;
        timeRemaining = timeLimit;
        puzzleStarted = true;

        GenerateNumberGrid();

        if (instructionText != null)
        {
            instructionText.text = $"Click numbers 1 to {totalNumbers} in order!";
        }

        UpdateTimerDisplay();
    }

    protected override void Update()
    {
        base.Update();

        if (!puzzleActive || !puzzleStarted) return;

        // Countdown timer
        timeRemaining -= Time.deltaTime;
        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            TimeUp();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}s";

            // Change colour based on time remaining
            if (timeRemaining <= 3f)
            {
                timerText.color = Color.red;
            }
            else if (timeRemaining <= 5f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    void TimeUp()
    {
        puzzleStarted = false;

        if (instructionText != null)
        {
            instructionText.text = "TIME'S UP! Restarting...";
            instructionText.color = Color.red;
        }

        // Restart puzzle
        StartCoroutine(RestartPuzzle());
    }

    IEnumerator RestartPuzzle()
    {
        yield return new WaitForSeconds(2f);

        // Reset puzzle game
        currentNumber = 1;
        timeRemaining = timeLimit;
        puzzleStarted = true;

        // Regenerate the grid
        GenerateNumberGrid();

        if (instructionText != null)
        {
            instructionText.text = $"Click numbers 1 to {totalNumbers} in order!";
            instructionText.color = Color.white;
        }

        UpdateTimerDisplay();
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
        puzzleStarted = false;

        if (instructionText != null)
        {
            instructionText.text = "SUCCESS! Sequence complete!";
            instructionText.color = Color.green;
        }

        if (timerText != null)
        {
            timerText.color = Color.green;
        }

        yield return new WaitForSecondsRealtime(1.5f);

        PuzzleSolved();
    }

    protected override void OnPuzzleComplete()
    {
        // Cleanup
    }
}