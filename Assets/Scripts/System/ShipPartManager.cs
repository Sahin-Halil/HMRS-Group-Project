using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPartManager : MonoBehaviour
{
    public static ShipPartManager Instance;

    // Ship part tracking and UI reference
    [SerializeField] private int collectedParts = 0;
    [SerializeField] private int totalPartsNeeded = 4;

    [SerializeField] private TMP_Text shipPartText;
    [SerializeField] private TMP_Text completionText;

    // Console tracking
    public bool enginePuzzleCompleted = false;
    public bool cockpitPuzzleCompleted = false;
    public bool lifeSupportPuzzleCompleted = false;
    public bool airlockPuzzleCompleted = false;

    // Ship part placement
    public bool enginePartPlaced = false;
    public bool cockpitPartPlaced = false;
    public bool lifeSupportPartPlaced = false;
    public bool airlockPartPlaced = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (shipPartText != null)
        {
            shipPartText.text = "Ship Parts: " + collectedParts.ToString() + "/" + totalPartsNeeded.ToString();
        }

        if (completionText != null)
        {
            int partsPlaced = GetPlacedPartsCount();
            completionText.text = "Parts Installed: " + partsPlaced.ToString() + "/" + totalPartsNeeded.ToString();
        }
    }

    // Increases collected ship part count
    public void AddPart()
    {
        collectedParts++;
        Debug.Log($"Ship part collected! Total: {collectedParts}/{totalPartsNeeded}");
    }

    // Check if player has parts available
    public bool HasParts()
    {
        return collectedParts > 0;
    }

    // Use a ship part (when placing at console)
    public bool UsePart()
    {
        if (collectedParts > 0)
        {
            collectedParts--;
            return true;
        }
        return false;
    }

    public int GetParts()
    {
        return collectedParts;
    }

    // Check if specific console puzzle is completed
    public bool IsPuzzleCompleted(PuzzleType puzzleType)
    {
        switch (puzzleType)
        {
            case PuzzleType.Engine:
                return enginePuzzleCompleted;
            case PuzzleType.Cockpit:
                return cockpitPuzzleCompleted;
            case PuzzleType.LifeSupport:
                return lifeSupportPuzzleCompleted;
            case PuzzleType.Airlock:
                return airlockPuzzleCompleted;
            default:
                return false;
        }
    }

    // Mark puzzle as completed
    public void CompletePuzzle(PuzzleType puzzleType)
    {
        switch (puzzleType)
        {
            case PuzzleType.Engine:
                enginePuzzleCompleted = true;
                break;
            case PuzzleType.Cockpit:
                cockpitPuzzleCompleted = true;
                break;
            case PuzzleType.LifeSupport:
                lifeSupportPuzzleCompleted = true;
                break;
            case PuzzleType.Airlock:
                airlockPuzzleCompleted = true;
                break;
        }
        Debug.Log($"{puzzleType} puzzle completed!");
    }

    // Mark ship part as placed
    public void PlaceShipPart(PuzzleType puzzleType)
    {
        switch (puzzleType)
        {
            case PuzzleType.Engine:
                enginePartPlaced = true;
                break;
            case PuzzleType.Cockpit:
                cockpitPartPlaced = true;
                break;
            case PuzzleType.LifeSupport:
                lifeSupportPartPlaced = true;
                break;
            case PuzzleType.Airlock:
                airlockPartPlaced = true;
                break;
        }
        Debug.Log($"{puzzleType} ship part placed!");
    }

    // Check if ship part is already placed
    public bool IsPartPlaced(PuzzleType puzzleType)
    {
        switch (puzzleType)
        {
            case PuzzleType.Engine:
                return enginePartPlaced;
            case PuzzleType.Cockpit:
                return cockpitPartPlaced;
            case PuzzleType.LifeSupport:
                return lifeSupportPartPlaced;
            case PuzzleType.Airlock:
                return airlockPartPlaced;
            default:
                return false;
        }
    }

    public int GetPlacedPartsCount()
    {
        int count = 0;
        if (enginePartPlaced) count++;
        if (cockpitPartPlaced) count++;
        if (lifeSupportPartPlaced) count++;
        if (airlockPartPlaced) count++;
        return count;
    }

    // Check if all parts are placed
    public bool IsShipRepaired()
    {
        return GetPlacedPartsCount() >= totalPartsNeeded;
    }
}

public enum PuzzleType
{
    Engine,
    Cockpit,
    LifeSupport,
    Airlock
}