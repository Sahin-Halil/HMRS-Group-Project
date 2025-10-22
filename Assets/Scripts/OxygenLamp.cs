using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class OxygenLamp : MonoBehaviour
{
    PlayerOxygen oxygen;

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
    public void OnTriggerExit(Collider other)
    {
        oxygen = null;
    }

    public void Update()
    {
        if (oxygen != null)
        {
            oxygen.RefillOxygen(Time.deltaTime * 10);
        }
    }
}
