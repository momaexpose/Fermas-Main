using UnityEngine;

/// <summary>
/// Storage for harvested products
/// Pirates will buy from this storage
/// </summary>
public class ProductStorage : MonoBehaviour
{
    [Header("Storage")]
    public int maxSlots = 50;
    public int maxStackSize = 99;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public string promptText = "E to View Products";
    public Sprite customIcon;

    // Storage
    public SeedStorage storage { get; private set; }

    // State
    private static ProductStorage currentOpen;
    private Camera cam;
    private bool looking = false;
    private InteractionUI ui;
    private int skipFrames = 0;

    // Singleton
    public static ProductStorage Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        storage = new SeedStorage(maxSlots, maxStackSize);
        cam = Camera.main;
        ui = FindFirstObjectByType<InteractionUI>();

        // Ensure GameData exists
        if (GameData.Instance == null)
        {
            GameObject obj = new GameObject("GameData");
            obj.AddComponent<GameData>();
        }

        // Load saved products
        LoadFromGameData();

        // Create UI if needed
        if (ProductStorageUI.Instance == null)
        {
            GameObject uiObj = new GameObject("ProductStorageUI");
            uiObj.AddComponent<ProductStorageUI>();
        }

        if (GetComponent<Collider>() == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = Vector3.one;
        }

        DrugDatabase.Initialize();

        Debug.Log("[ProductStorage] Ready with " + maxSlots + " slots, " + storage.items.Count + " items loaded");
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
            GameData.SaveProducts(storage.items);
        }
    }

    void LoadFromGameData()
    {
        var savedItems = GameData.LoadProducts();
        foreach (var item in savedItems)
        {
            storage.AddItem(item);
        }
    }

    void Update()
    {
        // Don't do anything if other UIs are open
        if (PlantPotUI.IsOpen || SeedChestUI.IsOpen) return;

        // Handle open state
        if (currentOpen == this)
        {
            if (skipFrames > 0)
            {
                skipFrames--;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(interactKey))
            {
                CloseStorage();
            }
            return;
        }

        CheckLooking();

        if (looking && Input.GetKeyDown(interactKey) && currentOpen == null)
        {
            OpenStorage();
        }
    }

    void CheckLooking()
    {
        looking = false;

        if (currentOpen != null)
        {
            if (ui != null) ui.Hide();
            return;
        }

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        float dist = Vector3.Distance(cam.transform.position, transform.position);
        if (dist > interactionDistance)
        {
            if (ui != null) ui.Hide();
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                looking = true;

                if (ui != null)
                {
                    ui.SetPrompt(promptText);
                    if (customIcon != null)
                        ui.SetIcon(customIcon);
                    ui.Show();
                }
            }
            else
            {
                if (ui != null) ui.Hide();
            }
        }
        else
        {
            if (ui != null) ui.Hide();
        }
    }

    void OpenStorage()
    {
        Debug.Log("[ProductStorage] Opening");
        currentOpen = this;
        skipFrames = 5;

        if (ui != null) ui.Hide();

        if (ProductStorageUI.Instance != null)
            ProductStorageUI.Instance.Open(this);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseStorage()
    {
        Debug.Log("[ProductStorage] Closing");
        currentOpen = null;

        if (ProductStorageUI.Instance != null)
            ProductStorageUI.Instance.Close();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Open for selling (called by pirates)
    /// </summary>
    public void OpenForSelling(System.Action<SeedItem, int> onSellCallback)
    {
        Debug.Log("[ProductStorage] Opening for selling");
        currentOpen = this;
        skipFrames = 5;

        if (ui != null) ui.Hide();

        if (ProductStorageUI.Instance != null)
            ProductStorageUI.Instance.OpenForSelling(this, onSellCallback);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static ProductStorage GetOpen() { return currentOpen; }
    public static bool IsAnyOpen() { return currentOpen != null; }

    /// <summary>
    /// Get total value of all products
    /// </summary>
    public int GetTotalValue()
    {
        int total = 0;
        foreach (var item in storage.items)
        {
            total += item.GetValue() * item.amount;
        }
        return total;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}