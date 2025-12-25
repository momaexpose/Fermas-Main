using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player inventory. Persists between scenes.
/// Player inventory is 5 columns x 9 rows = 45 slots.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Size")]
    public int inventoryWidth = 5;
    public int inventoryHeight = 9;

    [Header("Player Money")]
    public int money = 0;

    [Header("Item Database")]
    public ItemData[] allItems;

    [Header("Events")]
    public System.Action onInventoryChanged;
    public System.Action onMoneyChanged;

    public Inventory playerInventory { get; private set; }

    private Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create player inventory (5x9 = 45 slots)
        playerInventory = new Inventory(inventoryWidth, inventoryHeight);

        // Build item lookup
        BuildItemLookup();
    }

    void BuildItemLookup()
    {
        itemLookup.Clear();

        if (allItems != null)
        {
            foreach (var item in allItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemID))
                {
                    itemLookup[item.itemID] = item;
                }
            }
        }
    }

    /// <summary>
    /// Get item data by ID
    /// </summary>
    public ItemData GetItemByID(string itemID)
    {
        if (itemLookup.TryGetValue(itemID, out ItemData item))
        {
            return item;
        }
        return null;
    }

    /// <summary>
    /// Add item to player inventory
    /// </summary>
    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int remaining = playerInventory.AddItem(item, amount);

        if (remaining < amount)
        {
            onInventoryChanged?.Invoke();
        }

        return remaining == 0;
    }

    /// <summary>
    /// Add item by ID
    /// </summary>
    public bool AddItemByID(string itemID, int amount = 1)
    {
        ItemData item = GetItemByID(itemID);
        if (item == null) return false;
        return AddItem(item, amount);
    }

    /// <summary>
    /// Remove item from player inventory
    /// </summary>
    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        if (!playerInventory.HasItem(item, amount)) return false;

        playerInventory.RemoveItem(item, amount);
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Remove item by ID
    /// </summary>
    public bool RemoveItemByID(string itemID, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemID) || amount <= 0) return false;

        if (!playerInventory.HasItemByID(itemID, amount)) return false;

        playerInventory.RemoveItemByID(itemID, amount);
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Check if player has item
    /// </summary>
    public bool HasItem(ItemData item, int amount = 1)
    {
        return playerInventory.HasItem(item, amount);
    }

    /// <summary>
    /// Check by ID
    /// </summary>
    public bool HasItemByID(string itemID, int amount = 1)
    {
        return playerInventory.HasItemByID(itemID, amount);
    }

    /// <summary>
    /// Count item in inventory
    /// </summary>
    public int CountItem(ItemData item)
    {
        return playerInventory.CountItem(item);
    }

    /// <summary>
    /// Count by ID
    /// </summary>
    public int CountItemByID(string itemID)
    {
        return playerInventory.CountItemByID(itemID);
    }

    /// <summary>
    /// Add money
    /// </summary>
    public void AddMoney(int amount)
    {
        money += amount;
        onMoneyChanged?.Invoke();
    }

    /// <summary>
    /// Remove money (returns false if not enough)
    /// </summary>
    public bool RemoveMoney(int amount)
    {
        if (money < amount) return false;
        money -= amount;
        onMoneyChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Notify UI that inventory changed
    /// </summary>
    public void NotifyChange()
    {
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Give starting items for testing
    /// </summary>
    [ContextMenu("Give Test Items")]
    public void GiveTestItems()
    {
        if (allItems != null && allItems.Length > 0)
        {
            foreach (var item in allItems)
            {
                if (item != null)
                {
                    AddItem(item, 5);
                }
            }
        }

        AddMoney(500);
    }
}