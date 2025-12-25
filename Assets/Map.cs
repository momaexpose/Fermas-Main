using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Settings")]
    public string promptText = "E to View Map";
    public Sprite customIcon;

    private Camera playerCamera;
    private bool playerLooking = false;
    private InteractionUI interactionUI;
    private MapUI mapUI;

    void Start()
    {
        playerCamera = Camera.main;

        interactionUI = FindObjectOfType<InteractionUI>();
        if (interactionUI == null)
        {
            GameObject uiObj = new GameObject("InteractionUI");
            interactionUI = uiObj.AddComponent<InteractionUI>();
        }

        // Ensure TravelManager exists
        if (TravelManager.Instance == null)
        {
            GameObject managerObj = new GameObject("TravelManager");
            managerObj.AddComponent<TravelManager>();
            Debug.Log("[Map] Created TravelManager");
        }

        // Ensure MapUI exists
        mapUI = FindObjectOfType<MapUI>();
        if (mapUI == null)
        {
            GameObject mapUIObj = new GameObject("MapUI");
            mapUI = mapUIObj.AddComponent<MapUI>();
            Debug.Log("[Map] Created MapUI");
        }
    }

    void Update()
    {
        CheckPlayerLooking();
        HandleInteraction();
    }

    void CheckPlayerLooking()
    {
        playerLooking = false;

        // Don't show during transitions
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning())
        {
            if (interactionUI != null) interactionUI.Hide();
            return;
        }

        // Don't show if map is already open
        if (mapUI != null && mapUI.IsOpen())
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

    void HandleInteraction()
    {
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning())
            return;

        if (mapUI != null && mapUI.IsOpen())
            return;

        if (playerLooking && Input.GetKeyDown(interactKey))
        {
            OpenMap();
        }
    }

    void OpenMap()
    {
        if (interactionUI != null)
            interactionUI.Hide();

        if (mapUI != null)
        {
            mapUI.Open();
        }
        else
        {
            Debug.LogError("[Map] MapUI not found!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}