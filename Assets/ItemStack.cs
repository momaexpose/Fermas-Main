using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public ItemData item;
    public int amount;

    public ItemStack(ItemData item, int amount)
    {
        this.item = item;
        this.amount = Mathf.Clamp(amount, 0, item != null ? item.maxStack : 9);
    }

    public ItemStack Clone()
    {
        return new ItemStack(item, amount);
    }

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public bool IsFull()
    {
        if (item == null) return false;
        return amount >= item.maxStack;
    }

    public int GetSpace()
    {
        if (item == null) return 0;
        return item.maxStack - amount;
    }

    public int Add(int toAdd)
    {
        if (item == null) return toAdd;

        int canFit = item.maxStack - amount;
        int adding = Mathf.Min(toAdd, canFit);
        amount += adding;
        return toAdd - adding;
    }

    public int Remove(int toRemove)
    {
        int removing = Mathf.Min(toRemove, amount);
        amount -= removing;

        if (amount <= 0)
        {
            item = null;
            amount = 0;
        }

        return removing;
    }
}