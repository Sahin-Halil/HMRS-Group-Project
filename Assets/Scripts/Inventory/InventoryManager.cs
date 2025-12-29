using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // Inventory Settings
    public int maxSlots = 4;
    private List<PickupItem> inventory = new List<PickupItem>();

    // UI References
    public GameObject inventoryPanel;
    public Image slot1Image;
    public Image slot2Image;
    public Image slot3Image;
    public Image slot4Image;
    public TMP_Text pickupPromptText;
    public GameObject craftingPanel;
    public TMP_Text craftingText;
    public Button craftButton;

    // Player References
    public Transform player;
    public Transform dropPosition;

    // Ship Part Pieces Tracking
    private Dictionary<ShipPartType, int> partPieces = new Dictionary<ShipPartType, int>();
    private Dictionary<ShipPartType, int> piecesRequired = new Dictionary<ShipPartType, int>()
    {
        { ShipPartType.Engine, 3 },
        { ShipPartType.Navigation, 4 },
        { ShipPartType.LifeSupport, 2 },
        { ShipPartType.Airlock, 3 }
    };

    private ShipPartType currentCraftingPart;

    public GameObject enginePartPrefab;
    public GameObject navigationPartPrefab;
    public GameObject lifeSupportPartPrefab;
    public GameObject airlockPartPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialise piece tracking
        partPieces[ShipPartType.Engine] = 0;
        partPieces[ShipPartType.Navigation] = 0;
        partPieces[ShipPartType.LifeSupport] = 0;
        partPieces[ShipPartType.Airlock] = 0;
    }

    void Start()
    {
        UpdateInventoryUI();

        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
            Debug.Log("Crafting panel initialized as inactive");
        }
        else
        {
            Debug.LogError("Crafting panel is not assigned!");
        }

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
        else
        {
            Debug.LogError("Craft button is not assigned!");
        }
    }

    void Update()
    {
        // Drop item with Q key
        if (Time.timeScale > 0 && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            DropItem();
        }

        // Alternative craft input
        if (craftingPanel != null && craftingPanel.activeSelf && Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.cKey.wasPressedThisFrame)
            {
                OnCraftButtonClicked();
            }
        }
    }

    int GetUsedSlots()
    {
        int used = 0;
        foreach (var item in inventory)
        {
            if (item != null)
            {
                used += item.slotSize;
            }
        }
        return used;
    }

    public bool TryAddItem(PickupItem item)
    {
        if (item.itemType == ItemType.DataLog)
        {
            ShowDataLogInfo(item);
            return true;
        }

        if (item.itemType == ItemType.ShipPartPiece)
        {
            Debug.Log($"Attempting to add ship part piece. Current inventory: {inventory.Count}/{maxSlots}");

            if (GetUsedSlots() + item.slotSize > maxSlots)
            {
                Debug.Log("Inventory full!");
                ShowInventoryFullMessage();
                return false;
            }

            // If piece does not have assigned type, give it a random one
            if (!item.isPartTypeAssigned)
            {
                ShipPartType assignedPart = GetRandomIncompletePartType();
                item.shipPartType = assignedPart;
            }

            inventory.Add(item);
            partPieces[item.shipPartType]++;

            Debug.Log($"Added piece for {item.shipPartType}. Total: {partPieces[item.shipPartType]}/{piecesRequired[item.shipPartType]}");

            UpdateInventoryUI();
            CheckCraftingAvailability();

            return true;
        }

        if (item.itemType == ItemType.AssembledShipPart)
        {
            // Assembled parts take 2 slots
            if (GetUsedSlots() + item.slotSize > maxSlots)
            {
                ShowInventoryFullMessage();
                return false;
            }

            inventory.Add(item);

            UpdateInventoryUI();
            return true;
        }

        return false;
    }

    ShipPartType GetRandomIncompletePartType()
    {
        // Find parts that still need pieces
        List<ShipPartType> incomplete = new List<ShipPartType>();

        foreach (var part in partPieces.Keys)
        {
            if (partPieces[part] < piecesRequired[part])
            {
                incomplete.Add(part);
            }
        }

        if (incomplete.Count > 0)
        {
            return incomplete[Random.Range(0, incomplete.Count)];
        }

        // If all complete return random
        return (ShipPartType)Random.Range(0, 4);
    }

    // Check if any part can be crafted
    void CheckCraftingAvailability()
    {
        Debug.Log("Checking crafting availability...");

        foreach (var part in piecesRequired.Keys)
        {
            Debug.Log($"Checking {part}: {partPieces[part]}/{piecesRequired[part]}");

            if (partPieces[part] >= piecesRequired[part])
            {
                Debug.Log($"✓ CAN CRAFT {part}! Showing panel...");
                ShowCraftingPanel(part);
                return;
            }
        }

        Debug.Log("No parts ready to craft yet");
        HideCraftingPanel();
    }

    void ShowCraftingPanel(ShipPartType partType)
    {
        if (craftingPanel != null)
        {
            currentCraftingPart = partType;
            craftingPanel.SetActive(true);

            if (craftingText != null)
            {
                craftingText.text = $"Ready to craft: {partType} Part\n({partPieces[partType]}/{piecesRequired[partType]} pieces collected)\n\nClick to craft!";
            }

            // Pause the game and show cursor
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable player input
            if (player != null)
            {
                PlayerInput playerInput = player.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.actions.Disable();
                }
            }

            Debug.Log($"Crafting panel shown for {partType}");
        }
    }

    void HideCraftingPanel()
    {

        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
        }

        // Resume game
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player input
        if (player != null)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actions.Enable();
            }
        }
    }

    void OnCraftButtonClicked()
    {
        CraftShipPart(currentCraftingPart);
    }

    void CraftShipPart(ShipPartType partType)
    {
        Debug.Log($"Attempting to craft {partType}...");

        if (partPieces[partType] < piecesRequired[partType])
        {
            Debug.Log($"Not enough pieces! Have {partPieces[partType]}, need {piecesRequired[partType]}");
            return;
        }

        // Remove pieces from inventory of this part type
        int piecesToRemove = piecesRequired[partType];
        Debug.Log($"Need to remove {piecesToRemove} pieces of type {partType}");

        for (int i = inventory.Count - 1; i >= 0 && piecesToRemove > 0; i--)
        {
            if (inventory[i] != null &&
                inventory[i].itemType == ItemType.ShipPartPiece &&
                inventory[i].shipPartType == partType)
            {
                Debug.Log($"Removing piece {i}");
                Destroy(inventory[i].gameObject);
                inventory.RemoveAt(i);
                piecesToRemove--;
            }
        }

        // Reset piece count for this part
        partPieces[partType] = 0;

        CreateAssembledPart(partType);
        HideCraftingPanel();
        UpdateInventoryUI();
        CheckCraftingAvailability();

        Debug.Log($"Successfully crafted {partType} Part!");
    }

    void CreateAssembledPart(ShipPartType partType)
    {
        GameObject prefab = null;

        switch (partType)
        {
            case ShipPartType.Engine:
                prefab = enginePartPrefab;
                break;
            case ShipPartType.Navigation:
                prefab = navigationPartPrefab;
                break;
            case ShipPartType.LifeSupport:
                prefab = lifeSupportPartPrefab;
                break;
            case ShipPartType.Airlock:
                prefab = airlockPartPrefab;
                break;
        }

        if (prefab != null)
        {
            GameObject assembledPartObj = Instantiate(prefab, player.position, Quaternion.identity);
            PickupItem item = assembledPartObj.GetComponent<PickupItem>();

            if (item != null)
            {
                item.shipPartType = partType;
                item.gameObject.SetActive(false);

                // Check there is space in inventory
                if (GetUsedSlots() + item.slotSize > maxSlots)
                {
                    Destroy(assembledPartObj);
                    return;
                }

                inventory.Add(item);
                UpdateInventoryUI();
            }
            else
            {
                Destroy(assembledPartObj);
            }
        }
        else
        {
            Debug.LogError($"No prefab assigned for {partType}!");
        }
    }

    void DropItem()
    {
        if (inventory.Count == 0)
        {
            Debug.Log("No items to drop - inventory is empty!");
            return;
        }

        PickupItem itemToDrop = null;
        int dropIndex = -1;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null)
            {
                itemToDrop = inventory[i];
                dropIndex = i;
                break;
            }
        }

        if (itemToDrop == null)
        {
            Debug.Log("No valid items to drop!");
            return;
        }

        Debug.Log($"Dropping item: {itemToDrop.itemName} from slot {dropIndex}");

        inventory.RemoveAt(dropIndex);

        if (itemToDrop.itemType == ItemType.ShipPartPiece)
        {
            partPieces[itemToDrop.shipPartType]--;
            Debug.Log($"Decreased {itemToDrop.shipPartType} pieces to {partPieces[itemToDrop.shipPartType]}");
        }

        // Calculate drop position in front of player
        Vector3 dropPos;
        if (dropPosition != null)
        {
            dropPos = dropPosition.position;
            Debug.Log($"Dropping at dropPosition: {dropPos}");
        }
        else if (player != null)
        {
            dropPos = player.position + player.forward * 2f + Vector3.up * 1f;
            Debug.Log($"Dropping at player forward position: {dropPos}");
        }
        else
        {
            Debug.LogError("No drop position or player reference!");
            return;
        }

        itemToDrop.gameObject.SetActive(true);

        // Disable physics temporarily to set position
        Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Set position
        itemToDrop.transform.position = dropPos;
        itemToDrop.transform.rotation = Quaternion.identity;

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Renderer[] renderers = itemToDrop.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }

        UpdateInventoryUI();
        CheckCraftingAvailability();
    }

    void UpdateInventoryUI()
    {
        // Create array of slot images
        Image[] slots = new Image[] { slot1Image, slot2Image, slot3Image, slot4Image };

        // Disable all slots
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].enabled = false;
                slots[i].color = Color.white;
            }
        }

        // Enable slots and set sprites
        int slotIndex = 0;
        for (int i = 0; i < inventory.Count && slotIndex < slots.Length; i++)
        {
            if (inventory[i] != null)
            {
                if (slots[slotIndex] != null)
                {
                    slots[slotIndex].sprite = inventory[i].itemIcon;
                    slots[slotIndex].enabled = true;
                    slots[slotIndex].color = Color.white;
                }
                slotIndex++;

                // If assembled part, visually take up a second slot
                if (inventory[i].itemType == ItemType.AssembledShipPart && slotIndex < slots.Length)
                {
                    if (slots[slotIndex] != null)
                    {
                        slots[slotIndex].sprite = inventory[i].itemIcon;
                        slots[slotIndex].enabled = true;
                        slots[slotIndex].color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    slotIndex++;
                }
            }
        }
    }

    void ShowInventoryFullMessage()
    {
        Debug.Log("Inventory Full! Press Q to drop an item.");

        if (pickupPromptText != null)
        {
            StartCoroutine(ShowTemporaryInventoryFullMessage());
        }
    }

    IEnumerator ShowTemporaryInventoryFullMessage()
    {
        if (pickupPromptText != null)
        {
            pickupPromptText.text = "Inventory Full! Press Q to drop an item.";
            pickupPromptText.color = Color.red;
            pickupPromptText.gameObject.SetActive(true);

            yield return new WaitForSeconds(2f);

            pickupPromptText.gameObject.SetActive(false);
            pickupPromptText.color = Color.white;
        }
    }

    void ShowDataLogInfo(PickupItem dataLog)
    {
        DataLogInfo info = dataLog.GetComponent<DataLogInfo>();

        if (info != null && DataLogUI.Instance != null)
        {
            DataLogUI.Instance.ShowDataLog(info.logTitle, info.logContent);
        }
    }

    public List<PickupItem> GetInventory()
    {
        return new List<PickupItem>(inventory);
    }

    public Dictionary<ShipPartType, int> GetPartPieces()
    {
        return new Dictionary<ShipPartType, int>(partPieces);
    }

    public void RestoreInventory(List<PickupItem> items, Dictionary<ShipPartType, int> pieces)
    {
        foreach (var item in inventory)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        inventory.Clear();

        inventory = new List<PickupItem>(items);

        partPieces = new Dictionary<ShipPartType, int>(pieces);

        UpdateInventoryUI();
        CheckCraftingAvailability();
    }

    public void RemoveItem(PickupItem item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
            UpdateInventoryUI();
            CheckCraftingAvailability();
        }
    }
}