using UnityEngine;
using System;

/// <summary>
/// Represents a seed or harvested drug
/// </summary>
[System.Serializable]
public class SeedItem
{
    public string drugId;
    public DrugQuality quality;
    public bool isSeed; // true = seed, false = harvested product
    public int amount;

    public SeedItem(string id, DrugQuality qual, bool seed, int amt = 1)
    {
        drugId = id;
        quality = qual;
        isSeed = seed;
        amount = amt;
    }

    public DrugType GetDrugType()
    {
        return DrugDatabase.GetDrug(drugId);
    }

    public string GetDisplayName()
    {
        DrugType type = GetDrugType();
        if (type == null) return drugId;

        string qualStr = quality.ToString();
        string seedStr = isSeed ? " Seeds" : "";
        return qualStr + " " + type.displayName + seedStr;
    }

    public string GetShortName()
    {
        DrugType type = GetDrugType();
        if (type == null) return drugId;
        return type.displayName + (isSeed ? " Seeds" : "");
    }

    public int GetValue()
    {
        DrugType type = GetDrugType();
        if (type == null) return 0;

        float qualMult = DrugDatabase.GetQualityMultiplier(quality);
        float seedMult = isSeed ? 0.1f : 1f; // Seeds worth 10% of product

        return Mathf.RoundToInt(type.basePrice * qualMult * seedMult);
    }

    public Color GetColor()
    {
        DrugType type = GetDrugType();
        if (type == null) return Color.white;
        return type.color;
    }

    public Color GetQualityColor()
    {
        return DrugDatabase.GetQualityColor(quality);
    }

    public Color GetRarityColor()
    {
        DrugType type = GetDrugType();
        if (type == null) return Color.white;
        return DrugDatabase.GetRarityColor(type.rarity);
    }

    /// <summary>
    /// Create a random seed
    /// </summary>
    public static SeedItem CreateRandomSeed()
    {
        DrugType type = DrugDatabase.GetRandomDrug();
        DrugQuality quality = DrugDatabase.GetRandomQuality();
        return new SeedItem(type.id, quality, true, 1);
    }

    /// <summary>
    /// Clone this item
    /// </summary>
    public SeedItem Clone()
    {
        return new SeedItem(drugId, quality, isSeed, amount);
    }

    /// <summary>
    /// Check if can stack with another item
    /// </summary>
    public bool CanStackWith(SeedItem other)
    {
        if (other == null) return false;
        return drugId == other.drugId && quality == other.quality && isSeed == other.isSeed;
    }
}