using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Player References
    [SerializeField] private PlayerInput playerInput;
    public Transform player;
    public HealthSystem playerHealth;
    public PlayerOxygen playerOxygen;
    public DieScript playerDieScript;

    // Checkpoint System
    public Vector3 currentCheckpoint;
    public int maxLives = 3;
    public int currentLives;

    private int checkpointShipParts = 0;
    private bool[] checkpointPuzzlesCompleted = new bool[4];
    private bool[] checkpointPartsPlaced = new bool[4];

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentLives = maxLives;
    }

    // Set initial checkpoint to player's starting position
    void Start()
    {
        if (player != null)
        {
            currentCheckpoint = player.position;
        }

        SaveCheckpoint();
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position;
        SaveCheckpoint();
    }

    // Save ship parts and puzzle progress at checkpoint
    void SaveCheckpoint()
    {
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
    }

    // Automatically respawn at checkpoint after short delay
    public void PlayerDied()
    {
        currentLives--;

        if (currentLives > 0)
        {
            Invoke(nameof(RespawnAtCheckpoint), 1f);
        }
    }

    void RespawnAtCheckpoint()
    {
        if (player == null) return;

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

        // Reset death status
        if (playerDieScript != null)
        {
            playerDieScript.ResetDeathStatus();
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

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.actions.Enable();
        }
    }

    void RestoreCheckpointData()
    {
        if (ShipPartManager.Instance == null) return;

        int currentParts = ShipPartManager.Instance.GetParts();
        int targetParts = checkpointShipParts;

        while (ShipPartManager.Instance.GetParts() > 0)
        {
            ShipPartManager.Instance.UsePart();
        }

        for (int i = 0; i < targetParts; i++)
        {
            ShipPartManager.Instance.AddPart();
        }

        // Restore puzzle completion states
        ShipPartManager.Instance.enginePuzzleCompleted = checkpointPuzzlesCompleted[0];
        ShipPartManager.Instance.cockpitPuzzleCompleted = checkpointPuzzlesCompleted[1];
        ShipPartManager.Instance.lifeSupportPuzzleCompleted = checkpointPuzzlesCompleted[2];
        ShipPartManager.Instance.airlockPuzzleCompleted = checkpointPuzzlesCompleted[3];

        // Restore part placement states
        ShipPartManager.Instance.enginePartPlaced = checkpointPartsPlaced[0];
        ShipPartManager.Instance.cockpitPartPlaced = checkpointPartsPlaced[1];
        ShipPartManager.Instance.lifeSupportPartPlaced = checkpointPartsPlaced[2];
        ShipPartManager.Instance.airlockPartPlaced = checkpointPartsPlaced[3];
    }

    public int GetRemainingLives()
    {
        return currentLives;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        currentLives = maxLives;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
