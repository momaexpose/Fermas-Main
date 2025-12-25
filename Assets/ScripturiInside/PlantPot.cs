using UnityEngine;

/// <summary>
/// Simple plant pot - E to plant/harvest
/// </summary>
public class PlantPot : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionDistance = 5f;

    [Header("Plant Model")]
    public GameObject plantPrefab;
    public float plantHeight = 0.5f;
    public float plantScale = 1f;

    // State
    private bool isPlanted = false;
    private bool isReady = false;
    private SeedItem plantedSeed;
    private GameObject currentPlant;

    // Grow timer
    private float growTime = 0f;
    private float growTimer = 0f;

    // Cache
    private Camera cam;
    private Collider col;
    private InteractionUI ui;

    public static ProductStorage productStorage;

    void Start()
    {
        Debug.Log("[PlantPot] Started: " + gameObject.name);

        cam = Camera.main;
        ui = FindFirstObjectByType<InteractionUI>();

        col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            Debug.Log("[PlantPot] Added BoxCollider");
        }

        if (productStorage == null)
            productStorage = FindFirstObjectByType<ProductStorage>();

        DrugDatabase.Initialize();
    }

    void Update()
    {
        // Update grow timer
        if (isPlanted && !isReady)
        {
            growTimer += Time.deltaTime;
            if (growTimer >= growTime)
            {
                isReady = true;
                Debug.Log("[PlantPot] Plant ready to harvest!");
            }
        }

        // Get camera
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                cam = FindFirstObjectByType<Camera>();
        }
        if (cam == null) return;

        // Skip if UI open
        if (PlantPotUI.IsOpen || SeedChestUI.IsOpen || ProductStorageUI.IsOpen)
        {
            if (ui != null) ui.Hide();
            return;
        }

        // Check distance
        float dist = Vector3.Distance(cam.transform.position, transform.position);
        if (dist > interactionDistance)
        {
            if (ui != null) ui.Hide();
            return;
        }

        // Raycast from camera
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                // Show prompt
                if (ui != null)
                {
                    ui.SetPrompt(GetPrompt());
                    ui.Show();
                }

                // E key
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("[PlantPot] E pressed");

                    if (!isPlanted)
                        OpenSeeds();
                    else if (isReady)
                        Harvest();
                    else
                        Debug.Log("[PlantPot] Still growing...");
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

    string GetPrompt()
    {
        if (!isPlanted)
            return "E to Plant";
        else if (isReady)
            return "E to Harvest";
        else
        {
            float percent = (growTimer / growTime) * 100f;
            int remaining = Mathf.CeilToInt(growTime - growTimer);
            return "Growing " + percent.ToString("F0") + "% (" + remaining + "s)";
        }
    }

    void OpenSeeds()
    {
        Debug.Log("[PlantPot] OpenSeeds()");

        SeedChest chest = FindFirstObjectByType<SeedChest>();
        if (chest == null)
        {
            Debug.LogError("[PlantPot] NO SEEDCHEST FOUND!");
            return;
        }

        if (PlantPotUI.Instance == null)
        {
            GameObject obj = new GameObject("PlantPotUI");
            obj.AddComponent<PlantPotUI>();
        }

        PlantPotUI.Instance.Open(this, chest);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (ui != null) ui.Hide();
    }

    public void PlantSeed(SeedItem seed)
    {
        Debug.Log("[PlantPot] PlantSeed(): " + (seed != null ? seed.GetDisplayName() : "NULL"));

        if (seed == null) return;

        plantedSeed = seed;
        isPlanted = true;
        isReady = false;

        // Set grow time from drug type
        DrugType drug = seed.GetDrugType();
        if (drug != null)
            growTime = drug.growTime;
        else
            growTime = 60f; // Default 1 minute

        growTimer = 0f;

        Debug.Log("[PlantPot] Grow time: " + growTime + " seconds");

        // Destroy old plant
        if (currentPlant != null)
            Destroy(currentPlant);

        // Spawn new plant
        Vector3 pos = transform.position + Vector3.up * plantHeight;

        if (plantPrefab != null)
        {
            currentPlant = Instantiate(plantPrefab, pos, transform.rotation);
            currentPlant.transform.SetParent(transform);
            currentPlant.transform.localScale = Vector3.one * plantScale;
        }
        else
        {
            // Create placeholder
            currentPlant = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            currentPlant.transform.position = pos;
            currentPlant.transform.SetParent(transform);
            currentPlant.transform.localScale = Vector3.one * 0.3f * plantScale;
            Destroy(currentPlant.GetComponent<Collider>());

            Renderer r = currentPlant.GetComponent<Renderer>();
            if (r != null) r.material.color = Color.green;
        }
    }

    void Harvest()
    {
        Debug.Log("[PlantPot] Harvest()");

        if (plantedSeed == null) return;

        DrugType drug = plantedSeed.GetDrugType();
        int yield = (drug != null) ? Random.Range(drug.minYield, drug.maxYield + 1) : 1;

        SeedItem product = new SeedItem(plantedSeed.drugId, plantedSeed.quality, false, yield);

        if (productStorage == null)
            productStorage = FindFirstObjectByType<ProductStorage>();

        if (productStorage != null)
        {
            productStorage.storage.AddItem(product);
            Debug.Log("[PlantPot] Harvested: " + product.GetDisplayName() + " x" + yield);
        }

        // Reset
        isPlanted = false;
        isReady = false;
        plantedSeed = null;
        growTimer = 0f;
        growTime = 0f;

        if (currentPlant != null)
        {
            Destroy(currentPlant);
            currentPlant = null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isReady ? Color.green : (isPlanted ? Color.yellow : Color.gray);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}