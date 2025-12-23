using System.Collections;
using System.Collections.Generic;
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

    public bool requiresKeyPress = false;

    private bool playerInRange = false;
    private Transform player;

    void Update()
    {
        if (requiresKeyPress && playerInRange && Input.GetKeyDown(KeyCode.G))
        {
            TryPickup();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerInRange = true;

            if (!requiresKeyPress)
            {
                // Auto-pickup
                TryPickup();
            }
            else
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.ShowPickupPrompt(itemName);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;

            if (requiresKeyPress && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.HidePickupPrompt();
            }
        }
    }

    void TryPickup()
    {
        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.TryAddItem(this);

            if (success)
            {
                InventoryManager.Instance.HidePickupPrompt();
                gameObject.SetActive(false);
            }
        }
    }

    public void Drop(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        playerInRange = false;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.enabled = true;
        }
    }
}