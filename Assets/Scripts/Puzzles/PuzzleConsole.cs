using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public abstract class PuzzleConsole : MonoBehaviour
{
    // Console settings
    public PuzzleType puzzleType;
    public string consoleName = "Console";

    public KeyCode interactKey = KeyCode.C;
    public float interactionRange = 3f;

    // UI References
    public GameObject puzzleUI;
    public TMP_Text promptText;
    public TMP_Text statusText;

    // Visual feedback for player
    public Renderer consoleRenderer;
    public Color originalColor = Color.gray;
    public Color puzzleCompleteColor = Color.yellow;
    public Color partPlacedColor = Color.green;

    protected Transform player;
    protected bool playerInRange = false;
    protected bool puzzleActive = false;
    protected ShipPartManager shipPartManager;

    // Set up puzzle console and set to hidden
    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        shipPartManager = ShipPartManager.Instance;

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }

        UpdateConsoleVisual();
    }

    // Handle interaction when player is in range
    protected virtual void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            playerInRange = distance <= interactionRange;

            if (promptText != null)
            {
                promptText.gameObject.SetActive(playerInRange && !puzzleActive);

                if (playerInRange && !puzzleActive)
                {
                    promptText.text = GetPromptText();
                }
            }

            if (playerInRange && !puzzleActive && Keyboard.current != null)
            {
                if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
                {
                    TryInteract();
                }

            }
        }

        if (statusText != null && playerInRange)
        {
            statusText.text = GetStatusText();
        }
    }

    void TryInteract()
    {
        if (shipPartManager.IsPartPlaced(puzzleType))
        {
            if (promptText != null)
            {
                StartCoroutine(ShowTemporaryMessage(promptText, "Part already installed!", 2f));
            }
            return;
        }

        // Check if player has the required assembled part before starting puzzle
        if (!shipPartManager.IsPuzzleCompleted(puzzleType))
        {
            bool hasPart = PlayerHasRequiredPart();

            if (!hasPart)
            {
                string partName = GetPartName(puzzleType);
                Debug.Log($"Missing required part: {partName}");

                if (promptText != null)
                {
                    StartCoroutine(ShowTemporaryMessage(promptText, $"Need {partName} to access this console!", 3f));
                }
                return;
            }

            // Player has the part, start puzzle
            Debug.Log("Starting puzzle...");
            StartPuzzle();
        }
        else
        {
            Debug.Log("Puzzle already complete, trying to place part...");
            // Puzzle already completed, try to place part
            if (PlayerHasRequiredPart())
            {
                PlaceShipPart();
            }
            else
            {
                string partName = GetPartName(puzzleType);
                Debug.Log($"Need {partName} to install");

                if (promptText != null)
                {
                    StartCoroutine(ShowTemporaryMessage(promptText, $"Need {partName} to complete installation!", 3f));
                }
            }
        }
    }

    bool PlayerHasRequiredPart()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return false;
        }

        // Convert PuzzleType to ShipPartType
        ShipPartType requiredPartType = ConvertPuzzleTypeToShipPartType(puzzleType);
        Debug.Log($"Looking for part type: {requiredPartType}");

        // Check if player has the assembled part in inventory
        List<PickupItem> inventory = InventoryManager.Instance.GetInventory();
        Debug.Log($"Inventory count: {inventory.Count}");

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                Debug.Log($"  Slot {i}: {inventory[i].itemName}, Type: {inventory[i].itemType}, ShipPartType: {inventory[i].shipPartType}");

                if (inventory[i].itemType == ItemType.AssembledShipPart &&
                    inventory[i].shipPartType == requiredPartType)
                {
                    Debug.Log($"Found required part at slot {i}!");
                    return true;
                }
            }
            else
            {
                Debug.Log($"  Slot {i}: NULL");
            }
        }

        Debug.Log("Required part not found in inventory");
        return false;
    }

    ShipPartType ConvertPuzzleTypeToShipPartType(PuzzleType puzzle)
    {
        switch (puzzle)
        {
            case PuzzleType.Engine:
                return ShipPartType.Engine;
            case PuzzleType.Cockpit:
                return ShipPartType.Navigation;
            case PuzzleType.LifeSupport:
                return ShipPartType.LifeSupport;
            case PuzzleType.Airlock:
                return ShipPartType.Airlock;
            default:
                return ShipPartType.Engine;
        }
    }

    string GetPartName(PuzzleType type)
    {
        switch (type)
        {
            case PuzzleType.Engine:
                return "Engine Part";
            case PuzzleType.Cockpit:
                return "Navigation Part";
            case PuzzleType.LifeSupport:
                return "Life Support Part";
            case PuzzleType.Airlock:
                return "Airlock Part";
            default:
                return "Ship Part";
        }
    }

    IEnumerator ShowTemporaryMessage(TMP_Text textComponent, string message, float duration)
    {
        string originalText = textComponent.text;
        Color originalColor = textComponent.color;

        textComponent.text = message;
        textComponent.color = Color.yellow;
        textComponent.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        textComponent.text = originalText;
        textComponent.color = originalColor;
    }

    protected void StartPuzzle()
    {
        puzzleActive = true;

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
        }

        // Lock cursor for puzzle
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameManager.Instance.LockGameplay();

        OnPuzzleStart();
        Debug.Log($"Started {consoleName} puzzle");
    }

    protected void CompletePuzzle()
    {
        puzzleActive = false;

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.Instance.UnlockGameplay();

        // Mark puzzle as complete
        shipPartManager.CompletePuzzle(puzzleType);

        // Remove the ship part from inventory since puzzle is complete
        RemoveShipPartFromInventory();

        UpdateConsoleVisual();
        OnPuzzleComplete();

        Debug.Log($"{consoleName} puzzle completed! Ship part consumed.");
    }

    void RemoveShipPartFromInventory()
    {
        if (InventoryManager.Instance == null) return;

        ShipPartType requiredPartType = ConvertPuzzleTypeToShipPartType(puzzleType);
        List<PickupItem> inventory = InventoryManager.Instance.GetInventory();

        // Find the assembled part
        PickupItem partToRemove = null;
        foreach (PickupItem item in inventory)
        {
            if (item != null &&
                item.itemType == ItemType.AssembledShipPart &&
                item.shipPartType == requiredPartType)
            {
                partToRemove = item;
                break;
            }
        }

        if (partToRemove != null)
        {
            InventoryManager.Instance.RemoveItem(partToRemove);
            Destroy(partToRemove.gameObject);
            Debug.Log($"Removed {requiredPartType} part from inventory after puzzle completion");
        }
    }

    // Restart the puzzle or let the player try again
    protected void FailPuzzle()
    {
        OnPuzzleFail();
    }

    void PlaceShipPart()
    {
        // Just mark as placed - part already removed from inventory after puzzle
        shipPartManager.PlaceShipPart(puzzleType);

        // Update visuals
        UpdateConsoleVisual();
        OnPartPlaced();
        Debug.Log($"Ship part placed at {consoleName}!");

        // Auto-save checkpoint
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(player.position);
        }

        // Check if ship is fully repaired
        if (shipPartManager.IsShipRepaired())
        {
            Debug.Log("ALL SHIP PARTS PLACED! Ship is repaired!");
            OnShipRepaired();
        }
    }

    void UpdateConsoleVisual()
    {
        if (consoleRenderer != null)
        {
            Material mat = consoleRenderer.material;

            if (shipPartManager.IsPartPlaced(puzzleType))
            {
                mat.color = partPlacedColor;
            }
            else if (shipPartManager.IsPuzzleCompleted(puzzleType))
            {
                mat.color = puzzleCompleteColor;
            }
            else
            {
                mat.color = originalColor;
            }
        }
    }

    string GetPromptText()
    {
        if (shipPartManager.IsPartPlaced(puzzleType))
        {
            return $"{consoleName} - Part Installed";
        }
        else if (shipPartManager.IsPuzzleCompleted(puzzleType))
        {
            if (PlayerHasRequiredPart())
            {
                return $"Press {interactKey} to Place {GetPartName(puzzleType)}";
            }
            else
            {
                return $"Need {GetPartName(puzzleType)} to Place";
            }
        }
        else
        {
            if (PlayerHasRequiredPart())
            {
                return $"Press {interactKey} to Access {consoleName}";
            }
            else
            {
                return $"Need {GetPartName(puzzleType)} First!";
            }
        }
    }

    string GetStatusText()
    {
        if (shipPartManager.IsPartPlaced(puzzleType))
        {
            return "✓ Part Installed";
        }
        else if (shipPartManager.IsPuzzleCompleted(puzzleType))
        {
            return "✓ Puzzle Complete - Need Part";
        }
        else
        {
            return "⚠ Puzzle Required";
        }
    }

    // Abstract methods for each puzzle to implement
    protected abstract void OnPuzzleStart();
    protected abstract void OnPuzzleComplete();
    protected virtual void OnPuzzleFail() { }
    protected virtual void OnPartPlaced() { }
    protected virtual void OnShipRepaired() { }

    // Helper method
    protected void PuzzleSolved()
    {
        CompletePuzzle();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}