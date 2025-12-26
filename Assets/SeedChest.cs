using UnityEngine;

/// <summary>
/// Chest for storing seeds and drugs
/// </summary>
public class SeedChest : MonoBehaviour
{
    [Header("Storage")]
    public int maxSlots = 30;
    public int maxStackSize = 99;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public string promptText = "E to Open Seed Chest";
    public Sprite customIcon;

    // Storage
    public SeedStorage storage { get; private set; }

    // State
    private static SeedChest currentOpenChest;
    private Camera playerCamera;
    private bool playerLooking = false;
    private InteractionUI interactionUI;

    // Prevent instant close - skip frames after opening
    private int skipFrames = 0;

    void Start()
    {
        storage = new SeedStorage(maxSlots, maxStackSize);
        playerCamera = Camera.main;

        DrugDatabase.Initialize();

        // Ensure GameData exists
        if (GameData.Instance == null)
        {
            GameObject obj = new GameObject("GameData");
            obj.AddComponent<GameData>();
        }

        // Load saved seeds
        LoadFromGameData();

        // Create UI if needed
        if (SeedChestUI.Instance == null)
        {
            GameObject uiObj = new GameObject("SeedChestUI");
            uiObj.AddComponent<SeedChestUI>();
            Debug.Log("[SeedChest] Created SeedChestUI");
        }

        interactionUI = FindFirstObjectByType<InteractionUI>();

        // Add collider if missing
        if (GetComponent<Collider>() == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = Vector3.one;
            Debug.Log("[SeedChest] Added BoxCollider");
        }

        Debug.Log("[SeedChest] Ready with " + maxSlots + " slots, " + storage.items.Count + " items loaded");
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
        if (storage != null)
        {
            GameData.SaveSeeds(storage.items);
        }
    }

    void LoadFromGameData()
    {
        var savedItems = GameData.LoadSeeds();
        foreach (var item in savedItems)
        {
            storage.AddItem(item);
        }
    }

    void Update()
    {
        // Don't do anything if other UIs are open
        if (PlantPotUI.IsOpen || ProductStorageUI.IsOpen) return;

        // Handle open chest
        if (currentOpenChest == this)
        {
            // Skip frames to prevent instant close
            if (skipFrames > 0)
            {
                skipFrames--;
                return;
            }

            // Check for close
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(interactKey))
            {
                CloseChest();
            }
            return;
        }

        // Not open - check for interaction
        CheckPlayerLooking();

        if (playerLooking && Input.GetKeyDown(interactKey) && currentOpenChest == null)
        {
            OpenChest();
        }
    }

    void CheckPlayerLooking()
    {
        playerLooking = false;

        if (currentOpenChest != null)
        {
            if (interactionUI != null) interactionUI.Hide();
            return;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        if (dist > interactionDistance)
        {
            if (interactionUI != null) interactionUI.Hide();
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                playerLooking = true;

                if (interactionUI != null)
                {
                    interactionUI.SetPrompt(promptText);
                    if (customIcon != null)
                        interactionUI.SetIcon(customIcon);
                    interactionUI.Show();
                }
            }
            else
            {
                if (interactionUI != null) interactionUI.Hide();
            }
        }
        else
        {
            if (interactionUI != null) interactionUI.Hide();
        }
    }

    void OpenChest()
    {
        Debug.Log("[SeedChest] OPENING");
        currentOpenChest = this;
        skipFrames = 5; // Skip 5 frames before allowing close

        if (interactionUI != null)
            interactionUI.Hide();

        if (SeedChestUI.Instance != null)
        {
            SeedChestUI.Instance.Open(this);
        }
        else
        {
            Debug.LogError("[SeedChest] No SeedChestUI.Instance!");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseChest()
    {
        Debug.Log("[SeedChest] CLOSING");
        currentOpenChest = null;

        if (SeedChestUI.Instance != null)
            SeedChestUI.Instance.Close();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static SeedChest GetOpenChest() { return currentOpenChest; }
    public static bool IsAnyChestOpen() { return currentOpenChest != null; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}