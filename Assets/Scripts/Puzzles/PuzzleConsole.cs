using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public abstract class PuzzleConsole : MonoBehaviour
{
    // Console settings
    public PuzzleType puzzleType;
    public string consoleName = "Console";

    public KeyCode interactKey = KeyCode.E;
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
                if (Keyboard.current.eKey.wasPressedThisFrame)
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
            Debug.Log($"{consoleName} already has a part installed.");
            return;
        }

        if (shipPartManager.IsPuzzleCompleted(puzzleType))
        {
            if (shipPartManager.HasParts())
            {
                PlaceShipPart();
            }
            else
            {
                Debug.Log("You need to collect a ship part first!");
            }
        }
        else
        {
            StartPuzzle();
        }
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

        // TODO Find a way to disable player movement in player controller

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

        // TODO Find a way to re-enable player movement in player controller

        shipPartManager.CompletePuzzle(puzzleType);

        UpdateConsoleVisual();
        OnPuzzleComplete();

        Debug.Log($"{consoleName} puzzle completed! You can now place a ship part.");
    }

    // Restart the puzzle or let the player try again
    protected void FailPuzzle()
    {
        OnPuzzleFail();
    }

    void PlaceShipPart()
    {
        if (shipPartManager.UsePart())
        {
            shipPartManager.PlaceShipPart(puzzleType);
            UpdateConsoleVisual();
            OnPartPlaced();
            Debug.Log($"Ship part placed at {consoleName}!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(player.position);
            }

            if (shipPartManager.IsShipRepaired())
            {
                Debug.Log("ALL SHIP PARTS PLACED! Ship is repaired!");
                OnShipRepaired();
            }
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
            return shipPartManager.HasParts()
                ? $"Press {interactKey} to Place Ship Part"
                : "Collect a Ship Part First";
        }
        else
        {
            return $"Press {interactKey} to Access {consoleName}";
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