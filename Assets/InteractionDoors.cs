using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionDoor : MonoBehaviour
{
    [Header("Where does this door go?")]
    [Tooltip("The scene this door leads to. IGNORED if this is an exit door.")]
    public string targetSceneName = "Interior";

    [Header("Is this an EXIT door?")]
    [Tooltip("TRUE = This door takes you BACK to where you came from (use for doors INSIDE interiors)")]
    public bool isExitDoor = false;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public string promptText = "E to Open Door";
    public Sprite customIcon;

    [Header("Audio")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    private Camera playerCamera;
    private bool playerLooking = false;
    private bool playerInRange = false;
    private InteractionUI interactionUI;

    void Start()
    {
        playerCamera = Camera.main;

        interactionUI = FindFirstObjectByType<InteractionUI>();
        if (interactionUI == null)
        {
            GameObject uiObj = new GameObject("InteractionUI");
            interactionUI = uiObj.AddComponent<InteractionUI>();
        }

        if (SceneTransitionManager.Instance == null)
        {
            GameObject managerObj = new GameObject("SceneTransitionManager");
            managerObj.AddComponent<SceneTransitionManager>();
        }

        // Log on start what this door does
        if (isExitDoor)
            Debug.Log("[Door:" + gameObject.name + "] EXIT door - will go BACK");
        else
            Debug.Log("[Door:" + gameObject.name + "] ENTRY door - goes to: " + targetSceneName);
    }

    void Update()
    {
        CheckPlayerLooking();
        HandleInteraction();
    }

    void CheckPlayerLooking()
    {
        playerLooking = false;
        playerInRange = false;

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
            if (interactionUI != null) interactionUI.Hide();
            return;
        }

        playerInRange = true;

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

        if (playerLooking && playerInRange && Input.GetKeyDown(interactKey))
            OpenDoor();
    }

    void OpenDoor()
    {
        if (interactionUI != null)
            interactionUI.Hide();

        string currentScene = SceneManager.GetActiveScene().name;
        string destination;

        // ========== DECIDE WHERE TO GO ==========
        if (isExitDoor)
        {
            // EXIT DOOR - Go back to last main scene
            destination = SceneTransitionManager.Instance.GetLastMainScene();
            Debug.Log("[Door] EXIT: " + currentScene + " --> " + destination + " (going BACK)");
        }
        else
        {
            // ENTRY DOOR - Go to target scene
            destination = targetSceneName;
            Debug.Log("[Door] ENTRY: " + currentScene + " --> " + destination);
        }
        // =========================================

        // Do the transition
        AudioClip openSound = doorOpenSound ?? SceneTransitionManager.Instance.defaultOpenSound;
        AudioClip closeSound = doorCloseSound ?? SceneTransitionManager.Instance.defaultCloseSound;

        SceneTransitionManager.Instance.TransitionToSceneWithSounds(
            destination,
            openSound,
            closeSound,
            soundVolume
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isExitDoor ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}