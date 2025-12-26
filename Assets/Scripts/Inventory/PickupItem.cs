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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.TryAddItem(this);

            if (success)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void Drop(Vector3 position)
    {
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