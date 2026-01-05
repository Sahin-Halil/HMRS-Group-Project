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
        isDead = true;
        respawnMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.actions.Disable();
    }

    // Returns current death state
    public bool checkDead()
    {
        return isDead;
    }
}
