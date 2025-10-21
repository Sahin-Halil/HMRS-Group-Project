using UnityEngine;
using UnityEngine.Events;  

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public UnityEvent onHealthChanged;  
    public UnityEvent onDeath;  // GameOver is triggered upon death

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();  // Initial refresh UI
    }

    void Update()
    {
        // Test simulation: Comment out in the actual game and replace it with TakeDamage()
        if (Input.GetKey(KeyCode.H))  // Press H to test for blood deduction
            TakeDamage(10f);
        
        // 模拟衰减（可选，调慢点）
        // currentHealth -= Time.deltaTime * 1f;  // Slow speed test
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            onDeath?.Invoke();  
            Debug.Log("Player is dead! Game Over.");
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);  // 防负值
        onHealthChanged?.Invoke();  // Notify UI update
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        onHealthChanged?.Invoke();
    }

    // For UI query
    public float GetHealthPercentage() => currentHealth / maxHealth;
}

