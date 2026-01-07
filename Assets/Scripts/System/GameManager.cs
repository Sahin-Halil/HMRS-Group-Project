using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Player References - find at runtime
    [HideInInspector] public Transform player;
    [HideInInspector] public HealthSystem playerHealth;
    [HideInInspector] public PlayerOxygen playerOxygen;
    [HideInInspector] public PlayerInput playerInput;

    // Checkpoint System
    public Vector3 currentCheckpoint;
    public string currentScene;

    private int gameplayLockCount = 0;
    private int checkpointShipParts = 0;
    private bool[] checkpointPuzzlesCompleted = new bool[4];
    private bool[] checkpointPartsPlaced = new bool[4];
    private bool isRespawning = false;

    private List<string> checkpointCollectedPieceNames = new List<string>();
    private int checkpointKeyCards = 0;
    private List<int> checkpointCollectedCardNumbers = new List<int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayerReferences();

        // Only respawn if in respawn state and it's the same scene
        if (isRespawning && currentScene == scene.name)
        {
            Invoke(nameof(RespawnAtCheckpoint), 0.5f);
        }
    }

    void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<HealthSystem>();
            playerOxygen = playerObj.GetComponent<PlayerOxygen>();
            playerInput = playerObj.GetComponent<PlayerInput>();
        }
    }

    void Start()
    {
        FindPlayerReferences();

        if (player != null)
        {
            currentCheckpoint = player.position;
            currentScene = SceneManager.GetActiveScene().name;
        }

        SaveCheckpoint();
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position + Vector3.up * 1.5f; // Player height offset
        currentScene = SceneManager.GetActiveScene().name;
        SaveCheckpoint();
        Debug.Log($"Checkpoint saved at {currentCheckpoint} in {currentScene}");
    }

    public void SaveCheckpoint()
    {
        // Save key cards (outdoor)
        if (KeyCardManager.Instance != null)
        {
            checkpointKeyCards = KeyCardManager.Instance.GetCollectedCards();
        }

        // Save key card objects that were collected
        checkpointCollectedCardNumbers.Clear();
        KeyCard[] allCards = FindObjectsOfType<KeyCard>(true);
        foreach (KeyCard card in allCards)
        {
            if (!card.gameObject.activeInHierarchy)
            {
                checkpointCollectedCardNumbers.Add(card.cardNumber);
            }
        }

        // Save ship parts (indoor)
        if (ShipPartManager.Instance != null)
        {
            checkpointShipParts = ShipPartManager.Instance.GetParts();

            checkpointPuzzlesCompleted[0] = ShipPartManager.Instance.enginePuzzleCompleted;
            checkpointPuzzlesCompleted[1] = ShipPartManager.Instance.cockpitPuzzleCompleted;
            checkpointPuzzlesCompleted[2] = ShipPartManager.Instance.lifeSupportPuzzleCompleted;
            checkpointPuzzlesCompleted[3] = ShipPartManager.Instance.airlockPuzzleCompleted;

            checkpointPartsPlaced[0] = ShipPartManager.Instance.enginePartPlaced;
            checkpointPartsPlaced[1] = ShipPartManager.Instance.cockpitPartPlaced;
            checkpointPartsPlaced[2] = ShipPartManager.Instance.lifeSupportPartPlaced;
            checkpointPartsPlaced[3] = ShipPartManager.Instance.airlockPartPlaced;
        }

        // Save collected ship pieces (indoor)
        checkpointCollectedPieceNames.Clear();
        PickupItem[] allItems = FindObjectsOfType<PickupItem>(true);

        foreach (PickupItem item in allItems)
        {
            if (item.itemType == ItemType.ShipPartPiece && !item.gameObject.activeInHierarchy)
            {
                string itemPath = GetGameObjectPath(item.gameObject);
                checkpointCollectedPieceNames.Add(itemPath);
            }
        }
    }

    string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    public void PlayerDied()
    {
        isRespawning = true;
        SaveCheckpoint();
    }

    public void RespawnFromDeathMenu()
    {
        isRespawning = true;
        SceneManager.LoadScene(currentScene);
    }

    void RespawnAtCheckpoint()
    {
        isRespawning = false;

        if (player == null)
        {
            FindPlayerReferences();
        }

        if (player == null)
        {
            Debug.LogError("Cannot respawn - player not found!");
            return;
        }

        Debug.Log($"Respawning at checkpoint: {currentCheckpoint}");

        // Restore position
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.position = currentCheckpoint;
            cc.enabled = true;
        }
        else
        {
            player.position = currentCheckpoint;
        }

        // Restore health
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
            if (playerHealth.healthSlider != null)
            {
                playerHealth.healthSlider.value = playerHealth.maxHealth;
            }
        }

        // Restore oxygen
        if (playerOxygen != null)
        {
            playerOxygen.RefillOxygen(999f);
        }

        // Restore ship parts and puzzle states
        RestoreCheckpointData();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null)
        {
            playerInput.actions.Enable();
        }
    }

    public bool IsRespawning()
    {
        return isRespawning;
    }

    void RestoreCheckpointData()
    {
        if (KeyCardManager.Instance != null)
        {
            KeyCardManager.Instance.SetCollectedCards(checkpointKeyCards);

            Debug.Log($"Restoring {checkpointCollectedCardNumbers.Count} collected cards");

            KeyCard[] allCards = FindObjectsOfType<KeyCard>(true);
            foreach (KeyCard card in allCards)
            {
                if (checkpointCollectedCardNumbers.Contains(card.cardNumber))
                {
                    card.gameObject.SetActive(false);
                    Debug.Log($"Hiding collected card #{card.cardNumber}");
                }
                else
                {
                    card.gameObject.SetActive(true);
                }
            }
        }

        ShipPartManager shipPartManager = FindObjectOfType<ShipPartManager>();

        if (shipPartManager == null)
        {
            // Create a new one if it doesn't exist
            GameObject shipPartManagerObj = new GameObject("ShipPartManager");
            shipPartManager = shipPartManagerObj.AddComponent<ShipPartManager>();
        }

        if (shipPartManager != null)
        {
            Debug.Log($"Target ship parts: {checkpointShipParts}");

            // Reset to zero first
            while (shipPartManager.GetParts() > 0)
            {
                shipPartManager.UsePart();
            }

            // Add back saved parts
            for (int i = 0; i < checkpointShipParts; i++)
            {
                shipPartManager.AddPart();
            }

            // Restore puzzle completion states
            shipPartManager.enginePuzzleCompleted = checkpointPuzzlesCompleted[0];
            shipPartManager.cockpitPuzzleCompleted = checkpointPuzzlesCompleted[1];
            shipPartManager.lifeSupportPuzzleCompleted = checkpointPuzzlesCompleted[2];
            shipPartManager.airlockPuzzleCompleted = checkpointPuzzlesCompleted[3];

            // Restore part placement states
            shipPartManager.enginePartPlaced = checkpointPartsPlaced[0];
            shipPartManager.cockpitPartPlaced = checkpointPartsPlaced[1];
            shipPartManager.lifeSupportPartPlaced = checkpointPartsPlaced[2];
            shipPartManager.airlockPartPlaced = checkpointPartsPlaced[3];
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        currentScene = "";
        currentCheckpoint = Vector3.zero;
        isRespawning = false;
        checkpointCollectedPieceNames.Clear();
        checkpointCollectedCardNumbers.Clear();

        if (KeyCardManager.Instance != null)
        {
            KeyCardManager.Instance.ResetCards();
        }

        SceneManager.LoadScene("Main");
    }

    public void LockGameplay()
    {
        gameplayLockCount++;

        if (gameplayLockCount == 1 && playerInput != null)
        {
            playerInput.actions.Disable();
        }
    }

    public void UnlockGameplay()
    {
        gameplayLockCount = Mathf.Max(0, gameplayLockCount - 1);

        if (gameplayLockCount == 0 && playerInput != null)
        {
            playerInput.actions.Enable();
        }
    }

    public bool IsGameplayLocked()
    {
        return gameplayLockCount > 0;
    }

    public void ResetRespawnState()
    {
        isRespawning = false;
        Debug.Log("Respawn state reset");
    }
}