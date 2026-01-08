using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    // Game Components
    [SerializeField] private GameObject respawnMenuUI;
    private PlayerInput playerInput;

    // Health 
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;

    // Death state
    private bool isDead = false;

    // Initialize health values and UI
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // Handles damage intake and triggers death if health reaches zero
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handles player death logic and UI activation
    public void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }

        // Show death menu and pause (GameManager will handle actual respawn)
        respawnMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.actions.Disable();
        }

        Debug.Log("Player died - death menu shown");
    }

    // Returns current death state
    public bool checkDead()
    {
        return isDead;
    }

    public void HideDeathMenu()
    {
        if (respawnMenuUI != null)
        {
            respawnMenuUI.SetActive(false);
        }
        isDead = false;
    }
}