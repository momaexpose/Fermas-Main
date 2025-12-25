using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Storage for seeds and drugs
/// Supports stacking of same type/quality items
/// </summary>
[System.Serializable]
public class SeedStorage
{
    public int maxSlots;
    public int maxStackSize;
    public List<SeedItem> items;

    public SeedStorage(int slots = 20, int stackSize = 99)
    {
        maxSlots = slots;
        maxStackSize = stackSize;
        items = new List<SeedItem>();
    }

    /// <summary>
    /// Add a seed/drug to storage
    /// Returns amount that couldn't be added (0 if all added)
    /// </summary>
    public int AddItem(SeedItem item)
    {
        if (item == null || item.amount <= 0) return 0;

        int remaining = item.amount;

        // Try to stack with existing items first
        foreach (var existing in items)
        {
            if (existing.CanStackWith(item))
            {
                int canAdd = maxStackSize - existing.amount;
                if (canAdd > 0)
                {
                    int toAdd = Mathf.Min(canAdd, remaining);
                    existing.amount += toAdd;
                    remaining -= toAdd;

                    if (remaining <= 0) return 0;
                }
            }
        }

        // Create new stacks for remaining
        while (remaining > 0 && items.Count < maxSlots)
        {
            int toAdd = Mathf.Min(maxStackSize, remaining);
            SeedItem newItem = new SeedItem(item.drugId, item.quality, item.isSeed, toAdd);
            items.Add(newItem);
            remaining -= toAdd;
        }

        return remaining; // Return amount that couldn't fit
    }

    /// <summary>
    /// Add multiple seeds at once
    /// </summary>
    public int AddItems(List<SeedItem> newItems)
    {
        int notAdded = 0;
        foreach (var item in newItems)
        {
            notAdded += AddItem(item);
        }
        return notAdded;
    }

    /// <summary>
    /// Remove amount of specific item
    /// Returns actual amount removed
    /// </summary>
    public int RemoveItem(string drugId, DrugQuality quality, bool isSeed, int amount)
    {
        int toRemove = amount;

        for (int i = items.Count - 1; i >= 0 && toRemove > 0; i--)
        {
            var item = items[i];
            if (item.drugId == drugId && item.quality == quality && item.isSeed == isSeed)
            {
                int removing = Mathf.Min(item.amount, toRemove);
                item.amount -= removing;
                toRemove -= removing;

                if (item.amount <= 0)
                    items.RemoveAt(i);
            }
        }

        return amount - toRemove;
    }

    /// <summary>
    /// Remove item at specific index
    /// </summary>
    public SeedItem RemoveAt(int index, int amount = -1)
    {
        if (index < 0 || index >= items.Count) return null;

        var item = items[index];

        if (amount < 0 || amount >= item.amount)
        {
            items.RemoveAt(index);
            return item;
        }
        else
        {
            item.amount -= amount;
            return new SeedItem(item.drugId, item.quality, item.isSeed, amount);
        }
    }

    /// <summary>
    /// Get item at index
    /// </summary>
    public SeedItem GetItem(int index)
    {
        if (index < 0 || index >= items.Count) return null;
        return items[index];
    }

    /// <summary>
    /// Count total of specific item
    /// </summary>
    public int CountItem(string drugId, DrugQuality quality, bool isSeed)
    {
        int total = 0;
        foreach (var item in items)
        {
            if (item.drugId == drugId && item.quality == quality && item.isSeed == isSeed)
                total += item.amount;
        }
        return total;
    }

    /// <summary>
    /// Check if has space for more
    /// </summary>
    public bool HasSpace()
    {
        if (items.Count < maxSlots) return true;

        // Check for non-full stacks
        foreach (var item in items)
        {
            if (item.amount < maxStackSize) return true;
        }
        return false;
    }

    /// <summary>
    /// Get number of used slots
    /// </summary>
    public int UsedSlots()
    {
        return items.Count;
    }

    /// <summary>
    /// Clear all items
    /// </summary>
    public void Clear()
    {
        items.Clear();
    }

    /// <summary>
    /// Get all unique item types
    /// </summary>
    public List<SeedItem> GetUniqueItems()
    {
        Dictionary<string, SeedItem> unique = new Dictionary<string, SeedItem>();

        foreach (var item in items)
        {
            string key = item.drugId + "_" + item.quality + "_" + item.isSeed;
            if (unique.ContainsKey(key))
            {
                unique[key].amount += item.amount;
            }
            else
            {
                unique[key] = item.Clone();
            }
        }

        return new List<SeedItem>(unique.Values);
    }
}