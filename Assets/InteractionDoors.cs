using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class InteractionDoor : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public string promptText = "E to Open Door";
    public string piratePromptText = "E to Meet Pirates";
    public Sprite customIcon;

    [Header("Audio")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("Transition")]
    public float fadeOutTime = 0.5f;
    public float fadeInTime = 0.5f;

    private Camera playerCamera;
    private bool playerLooking = false;
    private bool playerInRange = false;
    private InteractionUI interactionUI;
    private AudioSource audioSource;

    // Static to persist across scenes
    private static bool isTransitioning = false;
    private static GameObject fadeCanvasObj;
    private static Image fadeImage;
    private static AudioSource persistentAudio;
    private static AudioClip pendingCloseSound;
    private static float pendingVolume;

    void Awake()
    {
        CreateFadeCanvas();
    }

    void Start()
    {
        playerCamera = Camera.main;

        interactionUI = FindFirstObjectByType<InteractionUI>();
        if (interactionUI == null)
        {
            GameObject uiObj = new GameObject("InteractionUI");
            interactionUI = uiObj.AddComponent<InteractionUI>();
        }

        // Audio source for door sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }

        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("[Door:" + gameObject.name + "] In scene: " + currentScene);

        // Fade in when scene loads (if we just transitioned)
        if (isTransitioning)
        {
            StartCoroutine(FadeIn());
        }
    }

    void CreateFadeCanvas()
    {
        if (fadeCanvasObj != null) return;

        fadeCanvasObj = new GameObject("DoorFadeCanvas");
        DontDestroyOnLoad(fadeCanvasObj);

        Canvas canvas = fadeCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        fadeCanvasObj.AddComponent<CanvasScaler>();
        fadeCanvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("FadePanel");
        panel.transform.SetParent(fadeCanvasObj.transform, false);

        fadeImage = panel.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        fadeImage.raycastTarget = false;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Persistent audio source for close sound
        persistentAudio = fadeCanvasObj.AddComponent<AudioSource>();
        persistentAudio.playOnAwake = false;
        persistentAudio.spatialBlend = 0f;
    }

    void Update()
    {
        if (isTransitioning)
        {
            if (interactionUI != null) interactionUI.Hide();
            return;
        }

        CheckPlayerLooking();
        HandleInteraction();
    }

    void CheckPlayerLooking()
    {
        playerLooking = false;
        playerInRange = false;

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
                    string currentScene = SceneManager.GetActiveScene().name.ToLower();
                    if (currentScene == "interior" && PiratesAreWaiting())
                    {
                        interactionUI.SetPrompt(piratePromptText);
                    }
                    else
                    {
                        interactionUI.SetPrompt(promptText);
                    }

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
        if (playerLooking && playerInRange && Input.GetKeyDown(interactKey))
            OpenDoor();
    }

    bool PiratesAreWaiting()
    {
        if (PirateManager.Instance != null)
            return PirateManager.PiratesWaiting;
        return false;
    }

    void OpenDoor()
    {
        if (isTransitioning) return;

        if (interactionUI != null)
            interactionUI.Hide();

        string currentScene = SceneManager.GetActiveScene().name.ToLower();
        string destination = GetDestination(currentScene);

        Debug.Log("[Door] " + currentScene + " --> " + destination);

        // Store close sound to play after scene loads
        pendingCloseSound = doorCloseSound;
        pendingVolume = soundVolume;

        StartCoroutine(TransitionToScene(destination));
    }

    IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;

        // Hide UI immediately
        if (interactionUI != null)
            interactionUI.Hide();

        // Play door open sound
        if (doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound, soundVolume);
        }

        // Fade to black
        float timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / fadeOutTime);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = Color.black;

        // Small delay at black
        yield return new WaitForSecondsRealtime(0.1f);

        // Load scene
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeIn()
    {
        // Ensure we start black
        if (fadeImage != null)
            fadeImage.color = Color.black;

        // Play door close sound
        if (pendingCloseSound != null && persistentAudio != null)
        {
            persistentAudio.PlayOneShot(pendingCloseSound, pendingVolume);
            pendingCloseSound = null;
        }

        // Small delay before fading in
        yield return new WaitForSecondsRealtime(0.1f);

        // Fade from black
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeInTime);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);

        isTransitioning = false;
    }

    string GetDestination(string currentScene)
    {
        switch (currentScene)
        {
            case "interior":
                if (PiratesAreWaiting())
                    return "Pirate";
                else
                    return "MainGame";

            case "constanta":
                return "CasaC";

            case "casac":
                return "Constanta";

            case "crimeea":
                return "Interior";

            case "maingame":
                return "Interior";

            case "pirate":
                return "MainGame";

            case "outdoors":
                return "Interior";

            default:
                Debug.LogWarning("[Door] Unknown scene: " + currentScene + ", going to MainGame");
                return "MainGame";
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}