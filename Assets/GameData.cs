using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Persistent game data - survives scene changes
/// Stores: money, seeds, products, plant states
/// </summary>
public class GameData : MonoBehaviour
{
    public static GameData Instance;

    // Money
    public int money = 0;

    // Current scene to return to (main game scene)
    public string currentMainScene = "Crimeea";

    // Seeds in chest
    public List<SeedItemData> seeds = new List<SeedItemData>();

    // Products in storage
    public List<SeedItemData> products = new List<SeedItemData>();

    // Plant pot states (by pot ID)
    public List<PlantPotData> plantPots = new List<PlantPotData>();

    // Pirate state
    public bool piratesWaiting = false;
    public float pirateTimer = 0f;

    [System.Serializable]
    public class SeedItemData
    {
        public string drugId;
        public string quality; // Store as string
        public bool isSeed;
        public int amount;

        public SeedItemData() { }

        public SeedItemData(SeedItem item)
        {
            drugId = item.drugId;
            quality = item.quality.ToString(); // Convert enum to string
            isSeed = item.isSeed;
            amount = item.amount;
        }

        public SeedItem ToSeedItem()
        {
            // Convert string back to enum
            DrugQuality qual = DrugQuality.Schwag;
            try
            {
                qual = (DrugQuality)System.Enum.Parse(typeof(DrugQuality), quality);
            }
            catch { }

            return new SeedItem(drugId, qual, isSeed, amount);
        }
    }

    [System.Serializable]
    public class PlantPotData
    {
        public string potId;
        public bool isPlanted;
        public bool isReady;
        public float growTimer;
        public float growTime;
        public SeedItemData plantedSeed;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[GameData] Initialized");
    }

    // ===== MONEY =====

    public static void SetMoney(int amount)
    {
        if (Instance != null)
            Instance.money = amount;
    }

    public static int GetMoney()
    {
        return Instance != null ? Instance.money : 0;
    }

    // ===== SEEDS =====

    public static void SaveSeeds(List<SeedItem> items)
    {
        if (Instance == null) return;

        Instance.seeds.Clear();
        foreach (var item in items)
        {
            Instance.seeds.Add(new SeedItemData(item));
        }
        Debug.Log("[GameData] Saved " + items.Count + " seeds");
    }

    public static List<SeedItem> LoadSeeds()
    {
        List<SeedItem> items = new List<SeedItem>();
        if (Instance == null) return items;

        foreach (var data in Instance.seeds)
        {
            items.Add(data.ToSeedItem());
        }
        Debug.Log("[GameData] Loaded " + items.Count + " seeds");
        return items;
    }

    // ===== PRODUCTS =====

    public static void SaveProducts(List<SeedItem> items)
    {
        if (Instance == null) return;

        Instance.products.Clear();
        foreach (var item in items)
        {
            Instance.products.Add(new SeedItemData(item));
        }
        Debug.Log("[GameData] Saved " + items.Count + " products");
    }

    public static List<SeedItem> LoadProducts()
    {
        List<SeedItem> items = new List<SeedItem>();
        if (Instance == null) return items;

        foreach (var data in Instance.products)
        {
            items.Add(data.ToSeedItem());
        }
        Debug.Log("[GameData] Loaded " + items.Count + " products");
        return items;
    }

    // ===== PLANT POTS =====

    public static void SavePlantPot(string potId, bool isPlanted, bool isReady, float growTimer, float growTime, SeedItem seed)
    {
        if (Instance == null) return;

        // Find or create entry
        PlantPotData data = Instance.plantPots.Find(p => p.potId == potId);
        if (data == null)
        {
            data = new PlantPotData { potId = potId };
            Instance.plantPots.Add(data);
        }

        data.isPlanted = isPlanted;
        data.isReady = isReady;
        data.growTimer = growTimer;
        data.growTime = growTime;
        data.plantedSeed = seed != null ? new SeedItemData(seed) : null;

        Debug.Log("[GameData] Saved pot: " + potId + " planted=" + isPlanted);
    }

    public static PlantPotData LoadPlantPot(string potId)
    {
        if (Instance == null) return null;
        return Instance.plantPots.Find(p => p.potId == potId);
    }

    // ===== PIRATES =====

    public static void SavePirateState(bool waiting, float timer)
    {
        if (Instance == null) return;
        Instance.piratesWaiting = waiting;
        Instance.pirateTimer = timer;
    }

    public static bool GetPiratesWaiting()
    {
        return Instance != null ? Instance.piratesWaiting : false;
    }

    public static float GetPirateTimer()
    {
        return Instance != null ? Instance.pirateTimer : 0f;
    }

    // ===== CURRENT SCENE =====

    public static void SetMainScene(string sceneName)
    {
        if (Instance != null)
        {
            Instance.currentMainScene = sceneName;
            Debug.Log("[GameData] Main scene set to: " + sceneName);
        }
    }

    public static string GetMainScene()
    {
        return Instance != null ? Instance.currentMainScene : "Crimeea";
    }
}