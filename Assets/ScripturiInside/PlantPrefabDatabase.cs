using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Database of plant prefabs for each drug type
/// Add this to a GameObject and assign prefabs in Inspector
/// </summary>
public class PlantPrefabDatabase : MonoBehaviour
{
    public static PlantPrefabDatabase Instance;

    [System.Serializable]
    public class PlantPrefabEntry
    {
        public string drugId;
        public GameObject prefab;
    }

    [Header("Plant Prefabs")]
    [Tooltip("Assign a prefab for each drug type you want custom models for")]
    public List<PlantPrefabEntry> plantPrefabs = new List<PlantPrefabEntry>();

    [Header("Category Defaults")]
    [Tooltip("Default prefab for cannabis plants")]
    public GameObject cannabisDefault;
    [Tooltip("Default prefab for mushroom plants")]
    public GameObject mushroomDefault;
    [Tooltip("Default prefab for opioid plants (poppies)")]
    public GameObject opioidDefault;
    [Tooltip("Default prefab for stimulant plants (coca)")]
    public GameObject stimulantDefault;
    [Tooltip("Default prefab for psychedelic plants")]
    public GameObject psychedelicDefault;
    [Tooltip("Default prefab for deliriant plants (datura)")]
    public GameObject deliriantDefault;

    [Header("Universal Fallback")]
    public GameObject fallbackPrefab;

    private Dictionary<string, GameObject> prefabDict;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build dictionary
        prefabDict = new Dictionary<string, GameObject>();
        foreach (var entry in plantPrefabs)
        {
            if (!string.IsNullOrEmpty(entry.drugId) && entry.prefab != null)
            {
                prefabDict[entry.drugId] = entry.prefab;
            }
        }

        Debug.Log("[PlantPrefabDatabase] Loaded " + prefabDict.Count + " plant prefabs");
    }

    /// <summary>
    /// Get prefab for a drug ID
    /// </summary>
    public static GameObject GetPrefab(string drugId)
    {
        if (Instance == null) return null;

        // Check specific prefab
        if (Instance.prefabDict.ContainsKey(drugId))
        {
            return Instance.prefabDict[drugId];
        }

        // Check category default
        DrugType drugType = DrugDatabase.GetDrug(drugId);
        if (drugType != null)
        {
            switch (drugType.category)
            {
                case DrugCategory.Cannabis:
                    if (Instance.cannabisDefault != null) return Instance.cannabisDefault;
                    break;
                case DrugCategory.Mushroom:
                    if (Instance.mushroomDefault != null) return Instance.mushroomDefault;
                    break;
                case DrugCategory.Opioid:
                    if (Instance.opioidDefault != null) return Instance.opioidDefault;
                    break;
                case DrugCategory.Stimulant:
                    if (Instance.stimulantDefault != null) return Instance.stimulantDefault;
                    break;
                case DrugCategory.Psychedelic:
                    if (Instance.psychedelicDefault != null) return Instance.psychedelicDefault;
                    break;
                case DrugCategory.Deliriant:
                    if (Instance.deliriantDefault != null) return Instance.deliriantDefault;
                    break;
            }
        }

        // Fallback
        return Instance.fallbackPrefab;
    }

    /// <summary>
    /// Add a prefab at runtime
    /// </summary>
    public static void RegisterPrefab(string drugId, GameObject prefab)
    {
        if (Instance == null || prefab == null) return;
        Instance.prefabDict[drugId] = prefab;
    }
}