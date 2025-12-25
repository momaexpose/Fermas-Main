using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quality levels - affects price
/// </summary>
public enum DrugQuality
{
    Schwag = 0,     // 0.5x
    Normal = 1,     // 1.0x
    Good = 2,       // 1.5x
    Premium = 3,    // 2.0x
    Legendary = 4   // 3.0x
}

/// <summary>
/// Rarity of finding seeds
/// </summary>
public enum DrugRarity
{
    Common,         // 60%
    Uncommon,       // 25%
    Rare,           // 12%
    Legendary       // 3%
}

/// <summary>
/// Category
/// </summary>
public enum DrugCategory
{
    Cannabis,
    Mushroom,
    Opioid,
    Stimulant,
    Psychedelic,
    Deliriant
}

/// <summary>
/// Drug type definition
/// </summary>
[System.Serializable]
public class DrugType
{
    public string id;
    public string displayName;
    public DrugCategory category;
    public DrugRarity rarity;
    public int basePrice;
    public float growTime;
    public int minYield;
    public int maxYield;
    public Color color;
    public string description;

    public DrugType(string id, string name, DrugCategory cat, DrugRarity rare, int price, float grow, int minY, int maxY, Color col, string desc)
    {
        this.id = id;
        this.displayName = name;
        this.category = cat;
        this.rarity = rare;
        this.basePrice = price;
        this.growTime = grow;
        this.minYield = minY;
        this.maxYield = maxY;
        this.color = col;
        this.description = desc;
    }
}

/// <summary>
/// Static database
/// </summary>
public static class DrugDatabase
{
    private static Dictionary<string, DrugType> drugs;
    private static List<DrugType> drugList;
    private static bool initialized = false;

    public static void Initialize()
    {
        if (initialized) return;

        drugs = new Dictionary<string, DrugType>();
        drugList = new List<DrugType>();

        // ==================== CANNABIS ====================
        // Common
        Add("indica", "Cannabis Indica", DrugCategory.Cannabis, DrugRarity.Common, 50, 120f, 3, 6, new Color(0.2f, 0.8f, 0.2f), "Relaxing body high");
        Add("sativa", "Cannabis Sativa", DrugCategory.Cannabis, DrugRarity.Common, 55, 130f, 3, 5, new Color(0.3f, 0.9f, 0.3f), "Energizing head high");
        Add("ruderalis", "Cannabis Ruderalis", DrugCategory.Cannabis, DrugRarity.Common, 30, 90f, 4, 8, new Color(0.4f, 0.7f, 0.3f), "Fast, low potency");
        Add("schwag", "Schwag Weed", DrugCategory.Cannabis, DrugRarity.Common, 20, 60f, 5, 10, new Color(0.5f, 0.5f, 0.3f), "Brick weed");

        // Uncommon
        Add("purple_haze", "Purple Haze", DrugCategory.Cannabis, DrugRarity.Uncommon, 120, 180f, 2, 4, new Color(0.6f, 0.2f, 0.8f), "Purple legend");
        Add("northern_lights", "Northern Lights", DrugCategory.Cannabis, DrugRarity.Uncommon, 130, 200f, 2, 4, new Color(0.2f, 0.4f, 0.9f), "Award-winning indica");
        Add("og_kush", "OG Kush", DrugCategory.Cannabis, DrugRarity.Uncommon, 140, 170f, 2, 5, new Color(0.5f, 0.8f, 0.2f), "West coast classic");
        Add("sour_diesel", "Sour Diesel", DrugCategory.Cannabis, DrugRarity.Uncommon, 135, 175f, 2, 4, new Color(0.7f, 0.8f, 0.2f), "Pungent sativa");
        Add("blue_dream", "Blue Dream", DrugCategory.Cannabis, DrugRarity.Uncommon, 125, 165f, 2, 5, new Color(0.3f, 0.5f, 0.9f), "Balanced hybrid");
        Add("ak47", "AK-47", DrugCategory.Cannabis, DrugRarity.Uncommon, 145, 180f, 2, 4, new Color(0.6f, 0.6f, 0.3f), "Complex mellow");
        Add("skunk", "Skunk #1", DrugCategory.Cannabis, DrugRarity.Uncommon, 110, 150f, 3, 5, new Color(0.5f, 0.7f, 0.2f), "Pungent classic");
        Add("amnesia", "Amnesia Haze", DrugCategory.Cannabis, DrugRarity.Uncommon, 140, 190f, 2, 4, new Color(0.8f, 0.9f, 0.3f), "Strong sativa");

        // Rare
        Add("white_widow", "White Widow", DrugCategory.Cannabis, DrugRarity.Rare, 200, 220f, 2, 3, new Color(0.95f, 0.95f, 0.9f), "Dutch legend");
        Add("gsc", "Girl Scout Cookies", DrugCategory.Cannabis, DrugRarity.Rare, 220, 240f, 1, 3, new Color(0.6f, 0.5f, 0.3f), "Sweet potent hybrid");
        Add("gdp", "Granddaddy Purple", DrugCategory.Cannabis, DrugRarity.Rare, 210, 230f, 1, 3, new Color(0.5f, 0.1f, 0.6f), "Grape indica");
        Add("jack_herer", "Jack Herer", DrugCategory.Cannabis, DrugRarity.Rare, 230, 250f, 1, 3, new Color(0.8f, 0.7f, 0.3f), "Activist tribute");
        Add("trainwreck", "Trainwreck", DrugCategory.Cannabis, DrugRarity.Rare, 200, 210f, 2, 3, new Color(0.6f, 0.8f, 0.4f), "Potent sativa hybrid");
        Add("cheese", "UK Cheese", DrugCategory.Cannabis, DrugRarity.Rare, 190, 200f, 2, 4, new Color(0.9f, 0.85f, 0.4f), "Cheesy aroma");

        // Legendary
        Add("gorilla_glue", "Gorilla Glue #4", DrugCategory.Cannabis, DrugRarity.Legendary, 350, 280f, 1, 2, new Color(0.3f, 0.25f, 0.2f), "Sticky strong");
        Add("wedding_cake", "Wedding Cake", DrugCategory.Cannabis, DrugRarity.Legendary, 380, 300f, 1, 2, new Color(0.95f, 0.9f, 0.85f), "Sweet vanilla");
        Add("bruce_banner", "Bruce Banner", DrugCategory.Cannabis, DrugRarity.Legendary, 400, 320f, 1, 2, new Color(0.2f, 0.8f, 0.3f), "THC record");
        Add("godfather_og", "Godfather OG", DrugCategory.Cannabis, DrugRarity.Legendary, 420, 340f, 1, 2, new Color(0.3f, 0.3f, 0.3f), "Don of OGs");

        // ==================== MUSHROOMS ====================
        // Common
        Add("cubensis", "Psilocybe Cubensis", DrugCategory.Mushroom, DrugRarity.Common, 60, 100f, 4, 8, new Color(0.8f, 0.7f, 0.5f), "Common shroom");
        Add("thai_shroom", "Thai Cubensis", DrugCategory.Mushroom, DrugRarity.Common, 55, 95f, 4, 9, new Color(0.7f, 0.6f, 0.5f), "Thai variety");

        // Uncommon  
        Add("liberty_caps", "Liberty Caps", DrugCategory.Mushroom, DrugRarity.Uncommon, 100, 150f, 2, 5, new Color(0.7f, 0.6f, 0.4f), "Wild potent");
        Add("golden_teacher", "Golden Teacher", DrugCategory.Mushroom, DrugRarity.Uncommon, 90, 130f, 3, 6, new Color(0.9f, 0.8f, 0.3f), "Spiritual insight");
        Add("b_plus", "B+ Mushroom", DrugCategory.Mushroom, DrugRarity.Uncommon, 85, 120f, 3, 6, new Color(0.8f, 0.75f, 0.6f), "Beginner friendly");
        Add("mazatapec", "Mazatapec", DrugCategory.Mushroom, DrugRarity.Uncommon, 95, 140f, 2, 5, new Color(0.75f, 0.65f, 0.45f), "Mexican sacred");

        // Rare
        Add("penis_envy", "Penis Envy", DrugCategory.Mushroom, DrugRarity.Rare, 180, 200f, 1, 3, new Color(0.9f, 0.85f, 0.7f), "Extremely potent");
        Add("blue_meanie", "Blue Meanie", DrugCategory.Mushroom, DrugRarity.Rare, 160, 180f, 2, 4, new Color(0.3f, 0.4f, 0.8f), "Strong visuals");
        Add("albino_avery", "Albino Avery", DrugCategory.Mushroom, DrugRarity.Rare, 190, 210f, 1, 3, new Color(0.95f, 0.95f, 0.95f), "Albino mutation");
        Add("ape", "Albino Penis Envy", DrugCategory.Mushroom, DrugRarity.Rare, 200, 220f, 1, 3, new Color(0.98f, 0.98f, 0.95f), "Albino potent");

        // Legendary
        Add("azurescens", "Psilocybe Azurescens", DrugCategory.Mushroom, DrugRarity.Legendary, 320, 280f, 1, 2, new Color(0.4f, 0.5f, 0.7f), "Most potent species");
        Add("enigma", "Enigma Blob", DrugCategory.Mushroom, DrugRarity.Legendary, 350, 300f, 1, 2, new Color(0.5f, 0.5f, 0.6f), "Mutation blob");

        // ==================== OPIOIDS ====================
        // Uncommon
        Add("opium_poppy", "Opium Poppy", DrugCategory.Opioid, DrugRarity.Uncommon, 150, 200f, 2, 4, new Color(0.9f, 0.3f, 0.3f), "Source of opium");
        Add("persian_poppy", "Persian Poppy", DrugCategory.Opioid, DrugRarity.Uncommon, 160, 210f, 2, 4, new Color(0.95f, 0.4f, 0.4f), "Ancient variety");
        Add("hungarian_poppy", "Hungarian Blue", DrugCategory.Opioid, DrugRarity.Uncommon, 155, 205f, 2, 4, new Color(0.4f, 0.4f, 0.8f), "Blue poppy");

        // Rare
        Add("afghan_poppy", "Afghan Poppy", DrugCategory.Opioid, DrugRarity.Rare, 250, 250f, 1, 3, new Color(0.8f, 0.2f, 0.4f), "High morphine");
        Add("turkish_poppy", "Turkish Poppy", DrugCategory.Opioid, DrugRarity.Rare, 260, 260f, 1, 3, new Color(0.85f, 0.25f, 0.35f), "Pharma grade");
        Add("tasmanian_poppy", "Tasmanian Poppy", DrugCategory.Opioid, DrugRarity.Rare, 280, 270f, 1, 3, new Color(0.9f, 0.5f, 0.5f), "Thebaine rich");
        Add("indian_poppy", "Indian Poppy", DrugCategory.Opioid, DrugRarity.Rare, 240, 240f, 2, 3, new Color(0.9f, 0.35f, 0.45f), "Traditional opium");

        // Legendary
        Add("persian_white", "Persian White", DrugCategory.Opioid, DrugRarity.Legendary, 500, 300f, 1, 2, new Color(0.95f, 0.9f, 0.9f), "Purest opium");
        Add("golden_crescent", "Golden Crescent", DrugCategory.Opioid, DrugRarity.Legendary, 550, 320f, 1, 2, new Color(0.95f, 0.85f, 0.4f), "Legendary strain");

        // ==================== COCAINE / STIMULANTS ====================
        // Uncommon
        Add("ephedra", "Ephedra", DrugCategory.Stimulant, DrugRarity.Uncommon, 80, 140f, 3, 6, new Color(0.6f, 0.7f, 0.2f), "Natural stimulant");
        Add("khat", "Khat Plant", DrugCategory.Stimulant, DrugRarity.Uncommon, 70, 130f, 3, 6, new Color(0.4f, 0.6f, 0.3f), "African stimulant");
        Add("betel", "Betel Nut", DrugCategory.Stimulant, DrugRarity.Uncommon, 60, 120f, 4, 7, new Color(0.6f, 0.4f, 0.2f), "Asian mild stim");

        // Rare
        Add("coca", "Coca Plant", DrugCategory.Stimulant, DrugRarity.Rare, 300, 280f, 2, 4, new Color(0.3f, 0.7f, 0.3f), "Cocaine source");
        Add("colombian_coca", "Colombian Coca", DrugCategory.Stimulant, DrugRarity.Rare, 320, 290f, 2, 3, new Color(0.35f, 0.75f, 0.35f), "High alkaloid");
        Add("peruvian_coca", "Peruvian Coca", DrugCategory.Stimulant, DrugRarity.Rare, 310, 285f, 2, 4, new Color(0.3f, 0.65f, 0.3f), "Traditional");

        // Legendary
        Add("bolivian_coca", "Bolivian Coca", DrugCategory.Stimulant, DrugRarity.Legendary, 450, 320f, 1, 2, new Color(0.4f, 0.8f, 0.4f), "Highest alkaloid");
        Add("erythroxylum", "Erythroxylum Novo", DrugCategory.Stimulant, DrugRarity.Legendary, 480, 340f, 1, 2, new Color(0.35f, 0.85f, 0.45f), "Rarest coca");

        // ==================== PSYCHEDELICS ====================
        // Common
        Add("morning_glory", "Morning Glory", DrugCategory.Psychedelic, DrugRarity.Common, 40, 80f, 5, 10, new Color(0.5f, 0.3f, 0.8f), "LSA seeds");
        Add("wild_dagga", "Wild Dagga", DrugCategory.Psychedelic, DrugRarity.Common, 35, 70f, 5, 10, new Color(0.9f, 0.5f, 0.2f), "Mild psychedelic");

        // Uncommon
        Add("san_pedro", "San Pedro", DrugCategory.Psychedelic, DrugRarity.Uncommon, 120, 300f, 1, 3, new Color(0.3f, 0.7f, 0.5f), "Mescaline cactus");
        Add("woodrose", "Hawaiian Woodrose", DrugCategory.Psychedelic, DrugRarity.Uncommon, 90, 120f, 3, 6, new Color(0.6f, 0.4f, 0.7f), "Potent LSA");
        Add("salvia", "Salvia Divinorum", DrugCategory.Psychedelic, DrugRarity.Uncommon, 100, 150f, 2, 5, new Color(0.2f, 0.6f, 0.3f), "Intense short");
        Add("iboga", "Iboga", DrugCategory.Psychedelic, DrugRarity.Uncommon, 140, 200f, 2, 4, new Color(0.5f, 0.4f, 0.3f), "African plant");
        Add("peruvian_torch", "Peruvian Torch", DrugCategory.Psychedelic, DrugRarity.Uncommon, 130, 280f, 1, 3, new Color(0.4f, 0.7f, 0.4f), "Mescaline");

        // Rare
        Add("peyote", "Peyote Cactus", DrugCategory.Psychedelic, DrugRarity.Rare, 200, 400f, 1, 2, new Color(0.4f, 0.6f, 0.4f), "Sacred mescaline");
        Add("ayahuasca", "Ayahuasca Vine", DrugCategory.Psychedelic, DrugRarity.Rare, 280, 350f, 1, 2, new Color(0.4f, 0.3f, 0.2f), "DMT vine");
        Add("chacruna", "Chacruna", DrugCategory.Psychedelic, DrugRarity.Rare, 250, 300f, 1, 3, new Color(0.3f, 0.5f, 0.3f), "Aya admixture");
        Add("mimosa", "Mimosa Hostilis", DrugCategory.Psychedelic, DrugRarity.Rare, 270, 320f, 1, 2, new Color(0.5f, 0.35f, 0.25f), "DMT bark");

        // Legendary
        Add("dmt_acacia", "DMT Acacia", DrugCategory.Psychedelic, DrugRarity.Legendary, 400, 400f, 1, 2, new Color(0.5f, 0.4f, 0.3f), "Pure DMT");
        Add("lophophora", "True Peyote", DrugCategory.Psychedelic, DrugRarity.Legendary, 450, 500f, 1, 1, new Color(0.5f, 0.6f, 0.5f), "Lophophora");

        // ==================== DELIRIANTS (DATURA etc) ====================
        // Common
        Add("datura", "Datura Stramonium", DrugCategory.Deliriant, DrugRarity.Common, 40, 100f, 4, 8, new Color(0.9f, 0.9f, 0.85f), "Jimsonweed");
        Add("datura_wrightii_common", "Common Datura", DrugCategory.Deliriant, DrugRarity.Common, 35, 90f, 5, 9, new Color(0.85f, 0.85f, 0.8f), "Wild datura");

        // Uncommon
        Add("datura_inoxia", "Datura Inoxia", DrugCategory.Deliriant, DrugRarity.Uncommon, 70, 130f, 3, 6, new Color(0.85f, 0.85f, 0.8f), "Toloache");
        Add("datura_metel", "Datura Metel", DrugCategory.Deliriant, DrugRarity.Uncommon, 80, 140f, 3, 5, new Color(0.8f, 0.7f, 0.9f), "Devil trumpet");
        Add("brugmansia", "Brugmansia", DrugCategory.Deliriant, DrugRarity.Uncommon, 90, 150f, 2, 5, new Color(0.95f, 0.85f, 0.7f), "Angel trumpet");
        Add("henbane", "Henbane", DrugCategory.Deliriant, DrugRarity.Uncommon, 75, 135f, 3, 6, new Color(0.7f, 0.7f, 0.5f), "Hyoscyamus");

        // Rare
        Add("belladonna", "Belladonna", DrugCategory.Deliriant, DrugRarity.Rare, 150, 180f, 2, 4, new Color(0.2f, 0.1f, 0.3f), "Deadly nightshade");
        Add("mandrake", "Mandrake", DrugCategory.Deliriant, DrugRarity.Rare, 180, 250f, 1, 3, new Color(0.5f, 0.4f, 0.2f), "Mystical root");
        Add("scopolia", "Scopolia", DrugCategory.Deliriant, DrugRarity.Rare, 160, 200f, 2, 3, new Color(0.4f, 0.3f, 0.5f), "Russian bella");

        // Legendary
        Add("sacred_datura", "Sacred Datura", DrugCategory.Deliriant, DrugRarity.Legendary, 300, 280f, 1, 2, new Color(0.95f, 0.95f, 0.95f), "Sacred toloache");
        Add("black_henbane", "Black Henbane", DrugCategory.Deliriant, DrugRarity.Legendary, 280, 260f, 1, 2, new Color(0.2f, 0.2f, 0.25f), "Rare black");

        initialized = true;
        Debug.Log("[DrugDatabase] Initialized with " + drugs.Count + " drugs");
    }

    static void Add(string id, string name, DrugCategory cat, DrugRarity rare, int price, float grow, int minY, int maxY, Color col, string desc)
    {
        DrugType drug = new DrugType(id, name, cat, rare, price, grow, minY, maxY, col, desc);
        drugs[id] = drug;
        drugList.Add(drug);
    }

    public static DrugType GetDrug(string id)
    {
        Initialize();
        return drugs.ContainsKey(id) ? drugs[id] : null;
    }

    public static List<DrugType> GetAllDrugs()
    {
        Initialize();
        return new List<DrugType>(drugList);
    }

    public static List<DrugType> GetByRarity(DrugRarity rarity)
    {
        Initialize();
        List<DrugType> result = new List<DrugType>();
        foreach (var d in drugList)
            if (d.rarity == rarity) result.Add(d);
        return result;
    }

    public static List<DrugType> GetByCategory(DrugCategory cat)
    {
        Initialize();
        List<DrugType> result = new List<DrugType>();
        foreach (var d in drugList)
            if (d.category == cat) result.Add(d);
        return result;
    }

    public static DrugType GetRandomDrug()
    {
        Initialize();
        float roll = Random.value * 100f;
        DrugRarity rarity;

        if (roll < 3f) rarity = DrugRarity.Legendary;
        else if (roll < 15f) rarity = DrugRarity.Rare;
        else if (roll < 40f) rarity = DrugRarity.Uncommon;
        else rarity = DrugRarity.Common;

        var options = GetByRarity(rarity);
        if (options.Count == 0) options = GetByRarity(DrugRarity.Common);
        return options[Random.Range(0, options.Count)];
    }

    public static DrugQuality GetRandomQuality()
    {
        float roll = Random.value * 100f;
        if (roll < 2f) return DrugQuality.Legendary;
        if (roll < 10f) return DrugQuality.Premium;
        if (roll < 30f) return DrugQuality.Good;
        if (roll < 70f) return DrugQuality.Normal;
        return DrugQuality.Schwag;
    }

    public static float GetQualityMultiplier(DrugQuality q)
    {
        switch (q)
        {
            case DrugQuality.Schwag: return 0.5f;
            case DrugQuality.Normal: return 1f;
            case DrugQuality.Good: return 1.5f;
            case DrugQuality.Premium: return 2f;
            case DrugQuality.Legendary: return 3f;
            default: return 1f;
        }
    }

    public static Color GetQualityColor(DrugQuality q)
    {
        switch (q)
        {
            case DrugQuality.Schwag: return new Color(0.6f, 0.6f, 0.6f);
            case DrugQuality.Normal: return Color.white;
            case DrugQuality.Good: return new Color(0.3f, 0.9f, 0.3f);
            case DrugQuality.Premium: return new Color(0.3f, 0.6f, 1f);
            case DrugQuality.Legendary: return new Color(1f, 0.8f, 0.2f);
            default: return Color.white;
        }
    }

    public static Color GetRarityColor(DrugRarity r)
    {
        switch (r)
        {
            case DrugRarity.Common: return Color.white;
            case DrugRarity.Uncommon: return new Color(0.3f, 0.9f, 0.3f);
            case DrugRarity.Rare: return new Color(0.3f, 0.6f, 1f);
            case DrugRarity.Legendary: return new Color(1f, 0.8f, 0.2f);
            default: return Color.white;
        }
    }
}