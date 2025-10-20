using UnityEngine;

public class PlayerOxygen : MonoBehaviour
{
    public float maxOxygen = 100f;
    public float currentOxygen;

    void Start()
    {
        currentOxygen = maxOxygen;
    }

    void Update()
    {
        // Simulate oxygen decreasing over time
        currentOxygen -= Time.deltaTime * 2; // Decrease oxygen over time

        if (currentOxygen <= 0)
        {
            Debug.Log("Oxygen depleted!");
        }
    }
}

