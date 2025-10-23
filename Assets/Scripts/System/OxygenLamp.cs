using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class OxygenLamp : MonoBehaviour
{
    // Oxygen reference for player
    PlayerOxygen oxygen;

    // Handles trigger entry events
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oxygen = other.GetComponent<PlayerOxygen>();
            oxygen.RefillOxygen(Time.deltaTime * 2);
        }
        else if (other.CompareTag("Enemy"))
        {
            Object.Destroy(other);
        }
    }

    // Handles trigger exit events
    public void OnTriggerExit(Collider other)
    {
        oxygen = null;
    }

    // Updates oxygen refill when player remains in range
    public void Update()
    {
        if (oxygen != null)
        {
            oxygen.RefillOxygen(Time.deltaTime * 10);
        }
    }
}
