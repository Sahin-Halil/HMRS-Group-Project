using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth = 100f;
    private List<GameObject> hitEffects = new List<GameObject>(); // Store all hit effects

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, GameObject hitEffect = null)
    {
        currentHealth -= amount;

        // Add the hit effect to the list
        if (hitEffect != null)
        {
            hitEffects.Add(hitEffect);
        }

        if (currentHealth <= 0) {
            Death();
        }
    }

    void Death()
    {
        // Destroy all hit effects associated with this enemy
        foreach (GameObject effect in hitEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        hitEffects.Clear();

        Destroy(gameObject);
    }
}
