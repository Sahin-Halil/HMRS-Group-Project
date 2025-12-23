using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameActive = true;
    public bool isGamePaused = false;

    [Header("Player Stats")]
    public int shipPartsCollected = 0;
    public int totalShipParts = 5;
    public float currentOxygen = 100f;
    public float maxOxygen = 100f;
    public float oxygenDepletionRate = 1f; // per second

    [Header("Enemy Stats")]
    private int enemiesAvoided = 0;
    private float gameStartTime;

    [Header("References")]
    public GameObject player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        gameStartTime = Time.time;
        isGameActive = true;
        
        // Create EndingManager if it doesn't exist
        if (EndingManager.Instance == null)
        {
            GameObject endingManagerObj = new GameObject("EndingManager");
            endingManagerObj.AddComponent<EndingManager>();
        }
    }

    private void Update()
    {
        if (!isGameActive || isGamePaused)
            return;

        // Deplete oxygen over time
        DepleteOxygen();

        // Check for oxygen death
        if (currentOxygen <= 0f)
        {
            TriggerLoseCondition(true);
        }
    }

    private void DepleteOxygen()
    {
        currentOxygen -= oxygenDepletionRate * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);
    }

    public void CollectShipPart()
    {
        shipPartsCollected++;
        Debug.Log($"Ship Part Collected! Total: {shipPartsCollected}/{totalShipParts}");

        // Check win condition
        if (shipPartsCollected >= totalShipParts)
        {
            TriggerWinCondition();
        }
    }

    public void RestoreOxygen(float amount)
    {
        currentOxygen += amount;
        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);
        Debug.Log($"Oxygen Restored! Current: {currentOxygen}%");
    }

    public void IncrementEnemiesAvoided()
    {
        enemiesAvoided++;
        Debug.Log($"Enemies Avoided: {enemiesAvoided}");
    }

    public void TriggerWinCondition()
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        float survivalTime = Time.time - gameStartTime;
        float oxygenPercentage = (currentOxygen / maxOxygen) * 100f;

        Debug.Log("WIN! Loading Win Ending Scene...");

        if (EndingManager.Instance != null)
        {
            EndingManager.Instance.SetEndingData(
                shipPartsCollected,
                survivalTime,
                enemiesAvoided,
                oxygenPercentage,
                false
            );
            EndingManager.Instance.LoadWinEnding();
        }
        else
        {
            SceneManager.LoadScene("WinEndingScene");
        }
    }

    public void TriggerLoseCondition(bool diedFromOxygen = false)
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        float survivalTime = Time.time - gameStartTime;
        float oxygenPercentage = (currentOxygen / maxOxygen) * 100f;

        Debug.Log("LOSE! Loading Lose Ending Scene...");

        if (EndingManager.Instance != null)
        {
            EndingManager.Instance.SetEndingData(
                shipPartsCollected,
                survivalTime,
                enemiesAvoided,
                oxygenPercentage,
                diedFromOxygen
            );
            EndingManager.Instance.LoadLoseEnding();
        }
        else
        {
            SceneManager.LoadScene("LoseEndingScene");
        }
    }

    public void PlayerDiedFromEnemy()
    {
        TriggerLoseCondition(false);
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
