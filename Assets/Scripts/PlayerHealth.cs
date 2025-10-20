using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        currentHealth -= Time.deltaTime * 5; // Decrease health over time
        if (currentHealth <= 0)
        {
            Debug.Log("Player is dead!");
        }
    }
}


