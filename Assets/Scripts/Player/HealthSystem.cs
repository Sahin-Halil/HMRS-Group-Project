using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    // Game Components
    [SerializeField] private GameObject respawnMenuUI;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private DieScript dieScript;

    // Health
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;

    // Initialize health values and UI
    void Start()
    {
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
            dieScript.Die();
        }
    }
}
