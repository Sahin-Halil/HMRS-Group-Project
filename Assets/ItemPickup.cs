using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // Set this in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory playerInventory = other.GetComponent<Inventory>();
            if (playerInventory != null)
            {
                playerInventory.AddItem(item);
                Destroy(gameObject); // Destroy the item after pickup
            }
        }
    }
}
