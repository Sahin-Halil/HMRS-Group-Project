using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    // Need the UI element for respawn
    [SerializeField] private GameObject respawnMenuUI;
    [SerializeField] private PlayerInput playerInput;

    
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Slider healthSlider;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;

    [Header("Debug")]
    public bool showDebugInfo = true;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null) {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (healthSlider != null) { 
            healthSlider.value = currentHealth;
        }

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        }

        // Trigger event for HUD
        onHealthChanged?.Invoke(currentHealth);

        if(currentHealth <= 0)
        {
            onDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
        }
        onHealthChanged?.Invoke(currentHealth);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
