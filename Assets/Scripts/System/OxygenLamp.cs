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
        if (other.CompareTag("Enemy"))
        {
            Vector3 away = (other.transform.position - transform.position).normalized;
            other.transform.position += away * 2f * Time.deltaTime;
        }
    }

    // Handles trigger stay events
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            oxygen = other.GetComponent<PlayerOxygen>();
            oxygen.RefillOxygen(Time.deltaTime * 2);
        }
        if (other.CompareTag("Enemy"))
        {
            // Push enemy back out of the zone
            Vector3 dir = (other.transform.position - transform.position).normalized;
            other.transform.position += dir * 3f * Time.deltaTime;
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
