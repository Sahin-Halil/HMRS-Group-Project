using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Respwan Settings")]
    public Vector3 currentCheckpoint;
    public float respawnDelay = 2f;

    [Header("References")]
    private GameObject player;
    private HealthSystem playerHealth;

    private void Awake()
    {
        // Singleton pattern
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if(player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();

            // Set initial checkpoint to player's start position
            currentCheckpoint = player.transform.position;

            if(playerHealth != null)
            {
                playerHealth.onDeath.AddListener(OnPlayerDeath);
            }
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position;
        Debug.Log($"Checkpoint set at: {position}");
    }

    void OnPlayerDeath()
    {
        Debug.Log("Player died! Respawning...");
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    void RespawnPlayer()
    {
        if(player != null)
        {
            // Reset player position
            player.transform.position = currentCheckpoint;

            // Reset player health
            if(playerHealth != null)
            {
                playerHealth.currentHealth = playerHealth.maxHealth;
                playerHealth.onHealthChanged?.Invoke(playerHealth.currentHealth);
            }

            // Reset player velocity (if it ends up having a rigidbody)
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Debug.Log("Player respawned at checkpoint!");
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
