using UnityEngine;

/// <summary>
/// Manages pirate arrivals - plays horn sound, tracks if pirates are waiting
/// Put this in your main game scene
/// </summary>
public class PirateManager : MonoBehaviour
{
    public static PirateManager Instance;

    [Header("Pirate Arrival")]
    [Tooltip("Time between pirate visits (seconds)")]
    public float timeBetweenVisits = 300f; // 5 minutes
    [Tooltip("Random extra time added")]
    public float randomExtraTime = 120f; // 0-2 minutes extra

    [Header("Horn Sound")]
    public AudioClip hornSound;
    [Range(0f, 10f)]
    public float hornVolume = 1f;

    [Header("Debug")]
    public bool piratesWaitingOnStart = false;
    public float debugArrivalTime = 10f; // Set to >0 to force arrival in X seconds

    // State
    public static bool PiratesWaiting { get; private set; } = false;
    private float nextArrivalTime;
    private float elapsedTime = 0f;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Ensure GameData exists
        if (GameData.Instance == null)
        {
            GameObject obj = new GameObject("GameData");
            obj.AddComponent<GameData>();
        }

        // Load saved state
        PiratesWaiting = GameData.GetPiratesWaiting();
        elapsedTime = GameData.GetPirateTimer();

        if (piratesWaitingOnStart)
            PiratesWaiting = true;

        ScheduleNextArrival();

        Debug.Log("[PirateManager] Ready. Pirates waiting: " + PiratesWaiting);
    }

    void Update()
    {
        // Track elapsed time
        elapsedTime += Time.deltaTime;

        // Check if pirates should arrive
        if (!PiratesWaiting && Time.time >= nextArrivalTime)
        {
            PiratesArrive();
        }
    }

    void OnDisable()
    {
        SaveToGameData();
    }

    void OnApplicationQuit()
    {
        SaveToGameData();
    }

    void SaveToGameData()
    {
        GameData.SavePirateState(PiratesWaiting, elapsedTime);
    }

    void ScheduleNextArrival()
    {
        if (debugArrivalTime > 0)
        {
            nextArrivalTime = Time.time + debugArrivalTime;
        }
        else
        {
            float delay = timeBetweenVisits + Random.Range(0f, randomExtraTime);
            nextArrivalTime = Time.time + delay;
        }
    }

    void PiratesArrive()
    {
        PiratesWaiting = true;

        Debug.Log("[PirateManager] PIRATES ARRIVED! Playing horn...");

        // Play horn sound
        if (hornSound != null)
        {
            audioSource.PlayOneShot(hornSound, hornVolume);
        }
        else
        {
            Debug.LogWarning("[PirateManager] No horn sound assigned!");
        }

        SaveToGameData();
    }

    /// <summary>
    /// Call this when player finishes trading with pirates
    /// </summary>
    public static void PiratesLeave()
    {
        PiratesWaiting = false;

        if (Instance != null)
        {
            Instance.ScheduleNextArrival();
            Instance.SaveToGameData();
            Debug.Log("[PirateManager] Pirates left. Next arrival scheduled.");
        }
    }

    /// <summary>
    /// Force pirates to arrive now (for testing)
    /// </summary>
    [ContextMenu("Force Pirates Arrive")]
    public void ForceArrive()
    {
        PiratesArrive();
    }

    /// <summary>
    /// Update demands based on current location (called by TravelManager)
    /// </summary>
    public void UpdateLocationDemands()
    {
        // Reset timer when location changes
        ScheduleNextArrival();
        Debug.Log("[PirateManager] Location changed, demands updated");
    }
}