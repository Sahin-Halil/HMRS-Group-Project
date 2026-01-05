using UnityEngine;

public enum ItemType
{
    ShipPartPiece,
    AssembledShipPart,
    DataLog
}

public enum ShipPartType
{
    Engine,
    Navigation,
    LifeSupport,
    Airlock
}

public class PickupItem : MonoBehaviour
{
    public ItemType itemType;
    public string itemName;
    public Sprite itemIcon;

    public ShipPartType shipPartType;
    public bool isPartTypeAssigned = false;

    public bool autoPickup = true;
    public int slotSize = 1;

    private bool isBeingPickedUp = false;

    void Awake()
    {
        // Auto-assign slot size based on item type
        if (itemType == ItemType.AssembledShipPart)
        {
            slotSize = 2;
        }
        else
        {
            slotSize = 1;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isBeingPickedUp) return;

        Debug.Log($"PickupItem.OnTriggerEnter: Player touched {itemName} ({itemType})");
        TryPickup();
    }

    void TryPickup()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance is null!");
            return;
        }

        isBeingPickedUp = true;
        Debug.Log($"Attempting to pick up {itemName}...");

        bool success = InventoryManager.Instance.TryAddItem(this);
        Debug.Log($"TryAddItem result: {success}");

        if (success)
        {
            Debug.Log($"Successfully picked up {itemName}. Setting inactive.");
            gameObject.SetActive(false);

            if (itemType == ItemType.ShipPartPiece && ShipPartManager.Instance != null)
            {
                ShipPartManager.Instance.AddPart();
            }
        }
        else
        {
            Debug.Log($"Failed to pick up {itemName} - inventory full or other issue. Item stays active.");
            isBeingPickedUp = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player left {itemName} trigger area");
            isBeingPickedUp = false;
        }
    }

    public void Drop(Vector3 position)
    {
        Debug.Log($"Dropping {itemName} at {position}");
        transform.position = position;
        gameObject.SetActive(true);

        // Ensure dropped item is visible
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }
    }
}