using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    // References to pause and ship part managers
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ShipPartManager shipPartManager;

    // Handles win or insufficient parts when player enters trigger
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && shipPartManager.getParts() == 5)
        {
            uiManager.Win();
            return;
        }

        else if (other.CompareTag("Player") && shipPartManager.getParts() < 5)
        {
            uiManager.NotEnoughParts(true);
        }
    }

    // Hides 'not enough parts' message when player exits trigger
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && shipPartManager.getParts() < 5)
            uiManager.NotEnoughParts(false);
    }
}
