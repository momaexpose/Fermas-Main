using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Settings")]
    public string noDestinationPrompt = "E to Pull Lever (No Destination)";
    public string hasDestinationPrompt = "E to Pull Lever - Travel to: ";
    public Sprite customIcon;

    [Header("Audio")]
    public AudioClip leverPullSound;
    public AudioClip travelSound;
    public AudioClip errorSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("Animation (Optional)")]
    public Transform leverHandle;
    public float pullAngle = 45f;
    public float pullSpeed = 5f;

    private Camera playerCamera;
    private bool playerLooking = false;
    private InteractionUI interactionUI;
    private AudioSource audioSource;
    private bool isPulled = false;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private MapUI mapUI;

    void Start()
    {
        playerCamera = Camera.main;

        interactionUI = FindFirstObjectByType<InteractionUI>();
        if (interactionUI == null)
        {
            GameObject uiObj = new GameObject("InteractionUI");
            interactionUI = uiObj.AddComponent<InteractionUI>();
        }

        mapUI = FindFirstObjectByType<MapUI>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        CheckPlayerLooking();
        HandleInteraction();
        UpdateLeverAnimation();
    }

    void CheckPlayerLooking()
    {
        playerLooking = false;

        // Don't show if map is open
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
                    // Show destination in prompt if one is selected
                    string prompt = noDestinationPrompt;
                    if (TravelManager.Instance != null && TravelManager.Instance.hasSelectedDestination)
                    {
                        prompt = hasDestinationPrompt + TravelManager.Instance.GetSelectedDestinationName();
                    }

                    interactionUI.SetPrompt(prompt);
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
        if (mapUI != null && mapUI.IsOpen())
            return;

        if (playerLooking && Input.GetKeyDown(interactKey))
        {
            PullLever();
        }
    }

    void PullLever()
    {
        if (interactionUI != null)
            interactionUI.Hide();

        // Check if destination is selected
        if (TravelManager.Instance == null || !TravelManager.Instance.hasSelectedDestination)
        {
            // No destination - play error sound
            if (errorSound != null)
                audioSource.PlayOneShot(errorSound, soundVolume);

            Debug.Log("[Lever] No destination selected! Use the map first.");
            return;
        }

        // Play lever sound
        if (leverPullSound != null)
            audioSource.PlayOneShot(leverPullSound, soundVolume);

        // Animate lever
        isPulled = true;
        targetAngle = pullAngle;

        // Execute travel after short delay
        Invoke("ExecuteTravel", 0.5f);
    }

    void ExecuteTravel()
    {
        // Play travel sound
        if (travelSound != null)
            audioSource.PlayOneShot(travelSound, soundVolume);

        // Execute the travel
        if (TravelManager.Instance != null)
        {
            TravelManager.Instance.ExecuteTravel(() =>
            {
                // Reset lever after travel
                ResetLever();
            });
        }
    }

    void ResetLever()
    {
        isPulled = false;
        targetAngle = 0f;
    }

    void UpdateLeverAnimation()
    {
        if (leverHandle == null) return;

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, pullSpeed * Time.deltaTime);
        leverHandle.localRotation = Quaternion.Euler(currentAngle, 0, 0);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}