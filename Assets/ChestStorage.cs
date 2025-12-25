using UnityEngine;

public class ChestStorage : MonoBehaviour
{
    public int chestWidth = 20;
    public int chestHeight = 9;
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Customization")]
    public string promptText = "E to Open Chest";
    public Sprite customIcon;

    public Inventory chestInventory { get; private set; }

    private static ChestStorage currentOpenChest;
    private Camera playerCamera;
    private bool playerLooking = false;
    private InteractionUI interactionUI;

    void Start()
    {
        chestInventory = new Inventory(chestWidth, chestHeight);
        playerCamera = Camera.main;

        // Ensure ChestUI exists
        if (ChestUI.Instance == null)
        {
            GameObject chestUIObj = new GameObject("ChestUI");
            chestUIObj.AddComponent<ChestUI>();
            Debug.Log("[ChestStorage] Created ChestUI");
        }

        // Find or create InteractionUI
        interactionUI = FindObjectOfType<InteractionUI>();
        if (interactionUI == null)
        {
            GameObject uiObj = new GameObject("InteractionUI");
            interactionUI = uiObj.AddComponent<InteractionUI>();
            Debug.Log("[ChestStorage] Created InteractionUI");
        }

        // Check for collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("[ChestStorage] No Collider found on " + gameObject.name + "! Add a BoxCollider or MeshCollider.");
        }

        Debug.Log("[ChestStorage] Initialized on " + gameObject.name);
    }

    void Update()
    {
        CheckPlayerLooking();
        HandleInteraction();
    }

    void CheckPlayerLooking()
    {
        playerLooking = false;

        // Don't show UI during transition
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning())
        {
            if (interactionUI != null) interactionUI.Hide();
            return;
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        if (distance > interactionDistance)
        {
            if (interactionUI != null && currentOpenChest == null) interactionUI.Hide();
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                playerLooking = true;

                Debug.Log("[ChestStorage] Player looking at chest! Distance: " + distance);

                if (interactionUI != null && currentOpenChest == null)
                {
                    interactionUI.SetPrompt(promptText);
                    if (customIcon != null)
                        interactionUI.SetIcon(customIcon);
                    interactionUI.Show();
                }
            }
            else
            {
                if (interactionUI != null && currentOpenChest == null) interactionUI.Hide();
            }
        }
        else
        {
            if (interactionUI != null && currentOpenChest == null) interactionUI.Hide();
        }
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("[ChestStorage] E pressed! PlayerLooking: " + playerLooking + ", CurrentOpenChest: " + (currentOpenChest != null ? currentOpenChest.name : "null"));

            if (currentOpenChest == this)
                CloseChest();
            else if (playerLooking && currentOpenChest == null)
                OpenChest();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && currentOpenChest == this)
            CloseChest();
    }

    void OpenChest()
    {
        Debug.Log("[ChestStorage] Opening chest!");
        currentOpenChest = this;

        if (interactionUI != null)
            interactionUI.Hide();

        if (ChestUI.Instance != null)
        {
            ChestUI.Instance.Open(this);
            Debug.Log("[ChestUI] Open called");
        }
        else
        {
            Debug.LogError("[ChestStorage] ChestUI.Instance is null!");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseChest()
    {
        currentOpenChest = null;

        if (ChestUI.Instance != null)
            ChestUI.Instance.Close();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static ChestStorage GetOpenChest() { return currentOpenChest; }
    public static bool IsAnyChestOpen() { return currentOpenChest != null; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}