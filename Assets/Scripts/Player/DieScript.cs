using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class DieScript : MonoBehaviour
{
    // Attributes
    [SerializeField] private GameObject respawnMenuUI;
    [SerializeField] private TMP_Text gameOverText;
    private PlayerInput playerInput;
    private bool isDead = false;

    // Setup references
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Handles player death logic and UI activation
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Die() called");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
            int remainingLives = GameManager.Instance.GetRemainingLives();
            Debug.Log($"After death, remaining lives: {remainingLives}");

            if (remainingLives <= 0)
            {
                ShowGameOverScreen();
            }
            else
            {
                // Still have lives - GameManager handle auto-respawn
                Debug.Log("Auto-respawning at checkpoint...");
            }
        }
        else
        {
            // No GameManager - show game over
            ShowGameOverScreen();
        }
    }

    void ShowGameOverScreen()
    {
        respawnMenuUI.SetActive(true);

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER";
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.actions.Disable();
        }
    }

    // Toggles death state
    public void toggleDeathStatus()
    {
        isDead = true;
    }

    // Returns current death state
    public bool checkDead()
    {
        return isDead;
    }

    public void ResetDeathStatus()
    {
        if (isDead)
        {
            isDead = false;
        }
    }

    public void RestartFromGameOver()
    {
        Debug.Log("Restarting game from Game Over");

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

}