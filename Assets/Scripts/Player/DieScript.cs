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

        toggleDeathStatus();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
            int remainingLives = GameManager.Instance.GetRemainingLives();
            if (remainingLives <= 0)
            {
                ShowGameOverScreen();
            }
        }
        else
        {
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
        playerInput.actions.Disable();
    }

    // Toggles death state
    public void toggleDeathStatus()
    {
        isDead = !isDead;
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
}
