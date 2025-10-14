using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // Set this in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory playerInventory = other.GetComponent<Inventory>();
            OxygenMeter oxygenMeter = other.GetComponent<OxygenMeter>();

            if (playerInventory != null)
            {
                playerInventory.AddItem(item);

                if (item.isOxygenItem && oxygenMeter != null)
                {
                    oxygenMeter.RefillOxygen(item.oxygenAmount);
                }

                Destroy(gameObject); // Destroy the item after pickup
            }
        }
    }
}
