using UnityEngine;

public class Telephone : MonoBehaviour
{
    [Header("Where does this telephone go?")]
    public string targetSceneName = "MainGame";

    [Header("Is this an EXIT telephone?")]
    [Tooltip("TRUE = Takes you BACK to the last main scene")]
    public bool isExitTelephone = false;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Audio")]
    public AudioClip ringSound;
    public AudioClip pickupSound;
    public AudioClip hangupSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("UI Settings")]
    public string promptText = "E to Use Telephone";
    public Sprite customIcon;

    private Camera playerCamera;
    private bool playerLooking = false;
    private InteractionUI interactionUI;
    private AudioSource audioSource;

    void Start()
    {
        playerCamera = Camera.main;

        // Find or create interaction UI
        interactionUI = FindFirstObjectByType<InteractionUI>();
        if (interactionUI == null)
        {
            GameObject uiObj = new GameObject("InteractionUI");
            interactionUI = uiObj.AddComponent<InteractionUI>();
        }

        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.playOnAwake = false;

        // Ensure transition manager exists
        if (SceneTransitionManager.Instance == null)
        {
            GameObject managerObj = new GameObject("SceneTransitionManager");
            managerObj.AddComponent<SceneTransitionManager>();
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

        if (playerLooking && Input.GetKeyDown(interactKey))
        {
            UseTelephone();
        }
    }

    void UseTelephone()
    {
        if (interactionUI != null)
            interactionUI.Hide();

        if (pickupSound != null)
            audioSource.PlayOneShot(pickupSound, soundVolume);

        string destination;

        if (isExitTelephone)
        {
            destination = SceneTransitionManager.Instance.GetLastMainScene();
            Debug.Log("[Telephone] EXIT: Going BACK to " + destination);
        }
        else
        {
            destination = targetSceneName;
            Debug.Log("[Telephone] Going to: " + destination);
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToSceneWithSounds(
                destination,
                ringSound,
                hangupSound,
                soundVolume
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}