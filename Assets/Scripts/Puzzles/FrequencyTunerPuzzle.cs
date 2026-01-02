using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FrequencyTunerPuzzle : PuzzleConsole
{
    // Puzzle Settings and UI
    public float[] targetFrequencies = { 25f, 50f, 75f };
    public float tolerance = 5f;
    public float lockInTime = 2f; // Time to hold in position

    public Slider frequencySlider;
    public TMP_Text frequencyDisplayText;
    public TMP_Text puzzleInstructionText;
    public TMP_Text lockStatusText;  
    public Image signalStrengthBar;

    public Color weakSignalColor = Color.red;
    public Color strongSignalColor = Color.green;
    public GameObject[] lockIndicators;

    private List<bool> frequenciesLocked;
    private float currentFrequency = 0f;
    private int currentTargetIndex = -1;
    private float lockTimer = 0f;
    private bool isLocking = false;

    protected override void OnPuzzleStart()
    {
        currentFrequency = 0f;
        lockTimer = 0f;
        isLocking = false;
        currentTargetIndex = -1;

        frequenciesLocked = new List<bool>();
        for (int i = 0; i < targetFrequencies.Length; i++)
        {
            frequenciesLocked.Add(false);
        }

        if (frequencySlider != null)
        {
            frequencySlider.minValue = 0f;
            frequencySlider.maxValue = 100f;
            frequencySlider.value = 0f;
            frequencySlider.onValueChanged.AddListener(OnSliderChanged);
        }

        UpdateLockIndicators();
        UpdateInstructions();
    }

    protected override void Update()
    {
        if (!puzzleActive)
        {
            base.Update();
            return;
        }

        // Check if player is on a target frequency and unlock if valid
        int targetIndex = GetCurrentTargetIndex();

        if (targetIndex >= 0 && !frequenciesLocked[targetIndex])
        {
            if (!isLocking || currentTargetIndex != targetIndex)
            {
                isLocking = true;
                currentTargetIndex = targetIndex;
                lockTimer = 0f;
            }

            lockTimer += Time.deltaTime;

            if (lockStatusText != null)
            {
                float progress = (lockTimer / lockInTime) * 100f;
                lockStatusText.text = $"LOCKING... {Mathf.RoundToInt(progress)}%";
                lockStatusText.color = Color.yellow;
            }

            if (lockTimer >= lockInTime)
            {
                LockFrequency(targetIndex);
            }
        }
        else
        {
            // Not on target or already locked
            if (isLocking)
            {
                isLocking = false;
                lockTimer = 0f;

                if (lockStatusText != null)
                {
                    lockStatusText.text = "Searching...";
                    lockStatusText.color = Color.white;
                }
            }
        }

        UpdateVisuals();
    }

    void OnSliderChanged(float value)
    {
        currentFrequency = value;

        if (frequencyDisplayText != null)
        {
            frequencyDisplayText.text = currentFrequency.ToString("F1") + " MHz";
        }
    }

    int GetCurrentTargetIndex()
    {
        for (int i = 0; i < targetFrequencies.Length; i++)
        {
            if (!frequenciesLocked[i] && Mathf.Abs(currentFrequency - targetFrequencies[i]) <= tolerance)
            {
                return i;
            }
        }
        return -1;
    }

    void UpdateVisuals()
    {
        // Calculate signal strength
        float closestDistance = float.MaxValue;

        foreach (float target in targetFrequencies)
        {
            float distance = Mathf.Abs(currentFrequency - target);
            if (distance < closestDistance)
            {
                closestDistance = distance;
            }
        }

        float strength = 1f - Mathf.Clamp01(closestDistance / 20f);

        // Update signal bar
        if (signalStrengthBar != null)
        {
            signalStrengthBar.fillAmount = strength;
            signalStrengthBar.color = Color.Lerp(weakSignalColor, strongSignalColor, strength);
        }
    }

    void LockFrequency(int index)
    {
        frequenciesLocked[index] = true;
        isLocking = false;
        lockTimer = 0f;

        if (lockStatusText != null)
        {
            lockStatusText.text = $"LOCKED: {targetFrequencies[index]} MHz";
            lockStatusText.color = Color.green;
        }

        UpdateLockIndicators();

        bool allLocked = true;
        foreach (bool locked in frequenciesLocked)
        {
            if (!locked)
            {
                allLocked = false;
                break;
            }
        }

        if (allLocked)
        {
            StartCoroutine(PuzzleComplete());
        }
        else
        {
            UpdateInstructions();
        }
    }

    void UpdateLockIndicators()
    {
        for (int i = 0; i < lockIndicators.Length && i < frequenciesLocked.Count; i++)
        {
            if (lockIndicators[i] != null)
            {
                Image img = lockIndicators[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = frequenciesLocked[i] ? Color.green : Color.gray;
                }

                TMP_Text text = lockIndicators[i].GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = targetFrequencies[i].ToString("F0") + " MHz";
                }
            }
        }
    }

    void UpdateInstructions()
    {
        if (puzzleInstructionText == null) return;

        int lockedCount = 0;
        foreach (bool locked in frequenciesLocked)
        {
            if (locked) lockedCount++;
        }

        puzzleInstructionText.text = $"Find and lock {targetFrequencies.Length} frequencies\nLocked: {lockedCount}/{targetFrequencies.Length}";
    }

    IEnumerator PuzzleComplete()
    {
        if (puzzleInstructionText != null)
        {
            puzzleInstructionText.text = "SUCCESS! All frequencies locked!";
            puzzleInstructionText.color = Color.green;
        }

        if (lockStatusText != null)
        {
            lockStatusText.text = "SIGNAL STABLE";
            lockStatusText.color = Color.green;
        }

        yield return new WaitForSeconds(2f);

        PuzzleSolved();
    }

    protected override void OnPuzzleComplete()
    {
        // Cleanup
    }
}