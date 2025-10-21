using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged; // @Haoge can use for HUD

    [Header("Debug")]
    public bool showDebugInfo = true;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if(showDebugInfo)
        {
            Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        }

        // Trigger event for HUD
        onHealthChanged?.Invoke(currentHealth);

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if(showDebugInfo)
        {
            Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
        }
        onHealthChanged?.Invoke(currentHealth);
    }

    void Die()
    {
        if(showDebugInfo)
        {
            Debug.Log($"{gameObject.name} has died!");
        }
        onDeath?.Invoke();

        // If there is an enemy left, destroy it
        if(!CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
