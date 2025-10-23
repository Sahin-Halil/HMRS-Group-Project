using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private ShipPartManager shipPartManager;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && shipPartManager.getParts()==5)
        {
            pauseManager.Win();
            return;
        }

        else if (other.CompareTag("Player") && shipPartManager.getParts() < 5)
        {
            pauseManager.NotEnoughParts(true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && shipPartManager.getParts() < 5)
        pauseManager.NotEnoughParts(false);

    }
}
