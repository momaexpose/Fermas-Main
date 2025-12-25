using UnityEngine;
using System.Collections.Generic;

public class TravelManager : MonoBehaviour
{
    public static TravelManager Instance { get; private set; }

    [System.Serializable]
    public class Location
    {
        public string locationName;
        public string description;
        public string[] wantedDrugs; // What pirates want here
        public float priceMultiplier = 1f; // How much more/less pirates pay
    }

    [System.Serializable]
    public class Home
    {
        public string homeName;
        public string sceneName;
        public bool owned = false;
        public int purchasePrice = 0;
    }

    [Header("Current State")]
    public string currentLocationName = "Black Sea";
    public int currentLocationIndex = 0;

    [Header("Selected Destination")]
    public bool hasSelectedDestination = false;
    public bool isHomeDestination = false;
    public int selectedDestinationIndex = -1;

    [Header("Locations")]
    public List<Location> locations = new List<Location>();

    [Header("Homes")]
    public List<Home> homes = new List<Home>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (locations.Count == 0)
            CreateDefaultLocations();

        if (homes.Count == 0)
            CreateDefaultHomes();
    }

    void CreateDefaultLocations()
    {
        locations.Add(new Location
        {
            locationName = "Black Sea",
            description = "Calm waters, moderate demand",
            wantedDrugs = new string[] { "cannabis", "opium", "mushrooms" },
            priceMultiplier = 1f
        });

        locations.Add(new Location
        {
            locationName = "Caribbean",
            description = "High risk, high reward",
            wantedDrugs = new string[] { "cocaine", "pills", "cannabis_premium" },
            priceMultiplier = 1.5f
        });

        locations.Add(new Location
        {
            locationName = "Mediterranean",
            description = "Tourist season, diverse demand",
            wantedDrugs = new string[] { "mushrooms", "lsa", "peyote" },
            priceMultiplier = 1.2f
        });

        locations.Add(new Location
        {
            locationName = "South China Sea",
            description = "Dangerous but lucrative",
            wantedDrugs = new string[] { "opium", "salvia", "sanpedro" },
            priceMultiplier = 1.8f
        });

        locations.Add(new Location
        {
            locationName = "Baltic Sea",
            description = "Cold waters, steady customers",
            wantedDrugs = new string[] { "pills", "cannabis", "mushrooms" },
            priceMultiplier = 0.9f
        });
    }

    void CreateDefaultHomes()
    {
        homes.Add(new Home
        {
            homeName = "Constanta",
            sceneName = "constanta",
            owned = true,
            purchasePrice = 0
        });

        homes.Add(new Home
        {
            homeName = "Havana",
            sceneName = "havana",
            owned = false,
            purchasePrice = 50000
        });

        homes.Add(new Home
        {
            homeName = "Barcelona",
            sceneName = "barcelona",
            owned = false,
            purchasePrice = 75000
        });

        homes.Add(new Home
        {
            homeName = "Hong Kong",
            sceneName = "hongkong",
            owned = false,
            purchasePrice = 100000
        });
    }

    /// <summary>
    /// Get current location data
    /// </summary>
    public Location GetCurrentLocation()
    {
        if (currentLocationIndex >= 0 && currentLocationIndex < locations.Count)
            return locations[currentLocationIndex];
        return null;
    }

    /// <summary>
    /// Get drugs wanted at current location
    /// </summary>
    public string[] GetWantedDrugsAtCurrentLocation()
    {
        Location loc = GetCurrentLocation();
        if (loc != null)
            return loc.wantedDrugs;
        return new string[] { "cannabis", "opium" };
    }

    /// <summary>
    /// Get price multiplier at current location
    /// </summary>
    public float GetPriceMultiplier()
    {
        Location loc = GetCurrentLocation();
        if (loc != null)
            return loc.priceMultiplier;
        return 1f;
    }

    /// <summary>
    /// Select a location to travel to
    /// </summary>
    public void SelectLocation(int index)
    {
        if (index >= 0 && index < locations.Count)
        {
            hasSelectedDestination = true;
            isHomeDestination = false;
            selectedDestinationIndex = index;
            Debug.Log("[Travel] Selected location: " + locations[index].locationName);
        }
    }

    /// <summary>
    /// Select a random different location
    /// </summary>
    public void SelectRandomLocation()
    {
        if (locations.Count <= 1) return;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, locations.Count);
        }
        while (newIndex == currentLocationIndex);

        SelectLocation(newIndex);
    }

    /// <summary>
    /// Select a home to travel to
    /// </summary>
    public void SelectHome(int index)
    {
        if (index >= 0 && index < homes.Count && homes[index].owned)
        {
            hasSelectedDestination = true;
            isHomeDestination = true;
            selectedDestinationIndex = index;
            Debug.Log("[Travel] Selected home: " + homes[index].homeName);
        }
    }

    /// <summary>
    /// Clear selected destination
    /// </summary>
    public void ClearDestination()
    {
        hasSelectedDestination = false;
        isHomeDestination = false;
        selectedDestinationIndex = -1;
    }

    /// <summary>
    /// Get selected destination name
    /// </summary>
    public string GetSelectedDestinationName()
    {
        if (!hasSelectedDestination) return "None";

        if (isHomeDestination && selectedDestinationIndex >= 0 && selectedDestinationIndex < homes.Count)
            return homes[selectedDestinationIndex].homeName + " (Home)";

        if (!isHomeDestination && selectedDestinationIndex >= 0 && selectedDestinationIndex < locations.Count)
            return locations[selectedDestinationIndex].locationName;

        return "Unknown";
    }

    /// <summary>
    /// Execute travel to selected destination (called by lever)
    /// </summary>
    public void ExecuteTravel(System.Action onComplete)
    {
        if (!hasSelectedDestination)
        {
            Debug.Log("[Travel] No destination selected!");
            return;
        }

        if (isHomeDestination)
        {
            // Travel to home - scene change
            Home home = homes[selectedDestinationIndex];
            ClearDestination();

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(home.sceneName);
            }
        }
        else
        {
            // Travel to location - just fade, no scene change
            int destIndex = selectedDestinationIndex;
            ClearDestination();

            currentLocationIndex = destIndex;
            currentLocationName = locations[destIndex].locationName;

            // Update pirate demands
            if (PirateManager.Instance != null)
            {
                PirateManager.Instance.UpdateLocationDemands();
            }

            // Fade out and back
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeOutAndBack(onComplete);
            }
            else
            {
                if (onComplete != null) onComplete();
            }

            Debug.Log("[Travel] Arrived at: " + currentLocationName);
        }
    }

    /// <summary>
    /// Buy a home
    /// </summary>
    public bool BuyHome(int index)
    {
        if (index < 0 || index >= homes.Count) return false;

        Home home = homes[index];
        if (home.owned) return false;

        if (InventoryManager.Instance != null && InventoryManager.Instance.money >= home.purchasePrice)
        {
            InventoryManager.Instance.RemoveMoney(home.purchasePrice);
            home.owned = true;
            Debug.Log("[Travel] Bought home: " + home.homeName);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get list of owned homes
    /// </summary>
    public List<Home> GetOwnedHomes()
    {
        List<Home> owned = new List<Home>();
        foreach (var home in homes)
        {
            if (home.owned)
                owned.Add(home);
        }
        return owned;
    }
}