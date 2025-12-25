using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Grid-based inventory storage.
/// </summary>
[System.Serializable]
public class Inventory
{
    public int width;
    public int height;
    public ItemStack[] slots;

    public int TotalSlots => width * height;

    public Inventory(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.slots = new ItemStack[width * height];

        // Initialize empty slots
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemStack(null, 0);
        }
    }

    /// <summary>
    /// Get slot at grid position
    /// </summary>
    public ItemStack GetSlot(int x, int y)
    {
        int index = y * width + x;
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    /// <summary>
    /// Get slot at index
    /// </summary>
    public ItemStack GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    /// <summary>
    /// Set slot contents
    /// </summary>
    public void SetSlot(int index, ItemStack stack)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index] = stack ?? new ItemStack(null, 0);
    }

    /// <summary>
    /// Add item to inventory. Returns amount that couldn't fit.
    /// </summary>
    public int AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return amount;

        int remaining = amount;

        // First try to stack with existing items
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i].item == item && !slots[i].IsFull())
            {
                remaining = slots[i].Add(remaining);
            }
        }

        // Then fill empty slots
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i] = new ItemStack(item, 0);
                remaining = slots[i].Add(remaining);
            }
        }

        return remaining;
    }

    /// <summary>
    /// Remove item from inventory. Returns amount actually removed.
    /// </summary>
    public int RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return 0;

        int toRemove = amount;
        int removed = 0;

        for (int i = 0; i < slots.Length && toRemove > 0; i++)
        {
            if (slots[i].item == item)
            {
                int r = slots[i].Remove(toRemove);
                removed += r;
                toRemove -= r;
            }
        }

        return removed;
    }

    /// <summary>
    /// Remove item by ID
    /// </summary>
    public int RemoveItemByID(string itemID, int amount)
    {
        if (string.IsNullOrEmpty(itemID) || amount <= 0) return 0;

        int toRemove = amount;
        int removed = 0;

        for (int i = 0; i < slots.Length && toRemove > 0; i++)
        {
            if (slots[i].item != null && slots[i].item.itemID == itemID)
            {
                int r = slots[i].Remove(toRemove);
                removed += r;
                toRemove -= r;
            }
        }

        return removed;
    }

    /// <summary>
    /// Count total amount of an item
    /// </summary>
    public int CountItem(ItemData item)
    {
        if (item == null) return 0;

        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == item)
            {
                count += slots[i].amount;
            }
        }
        return count;
    }

    /// <summary>
    /// Count by item ID
    /// </summary>
    public int CountItemByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return 0;

        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null && slots[i].item.itemID == itemID)
            {
                count += slots[i].amount;
            }
        }
        return count;
    }

    /// <summary>
    /// Check if has enough of an item
    /// </summary>
    public bool HasItem(ItemData item, int amount)
    {
        return CountItem(item) >= amount;
    }

    /// <summary>
    /// Check by ID
    /// </summary>
    public bool HasItemByID(string itemID, int amount)
    {
        return CountItemByID(itemID) >= amount;
    }

    /// <summary>
    /// Get all unique items and their counts
    /// </summary>
    public Dictionary<string, int> GetAllItems()
    {
        Dictionary<string, int> items = new Dictionary<string, int>();

        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty())
            {
                string id = slots[i].item.itemID;
                if (items.ContainsKey(id))
                {
                    items[id] += slots[i].amount;
                }
                else
                {
                    items[id] = slots[i].amount;
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Check if inventory has any empty slots
    /// </summary>
    public bool HasEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty()) return true;
        }
        return false;
    }

    /// <summary>
    /// Count empty slots
    /// </summary>
    public int CountEmptySlots()
    {
        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty()) count++;
        }
        return count;
    }

    /// <summary>
    /// Clear all slots
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemStack(null, 0);
        }
    }
}