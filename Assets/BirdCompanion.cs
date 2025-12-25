using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Grasu' the budgie - your seed-finding companion
/// Press E to send him out, he returns in 1-3 minutes with 3-5 seeds
/// </summary>
public class BirdCompanion : MonoBehaviour
{
    [Header("Bird Settings")]
    public string birdName = "Grasu'";
    public float minReturnTime = 60f;  // 1 minute
    public float maxReturnTime = 180f; // 3 minutes
    public int minSeeds = 3;
    public int maxSeeds = 5;

    [Header("Debug")]
    public bool debugFastMode = true; // Set to true for 10 second returns

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Visuals - assign if bird mesh is a child object")]
    public GameObject birdVisual; // Leave empty if mesh is on this object

    [Header("Audio")]
    public AudioClip chirpSound;
    public AudioClip flyAwaySound;
    public AudioClip returnSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;

    // State
    private bool isOut = false;
    private float returnTimer = 0f;
    private List<SeedItem> collectedSeeds = new List<SeedItem>();

    // Components
    private Camera playerCamera;
    private AudioSource audioSource;
    private InteractionUI interactionUI;
    private Collider birdCollider;

    // Singleton
    public static BirdCompanion Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerCamera = Camera.main;

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;

        // Collider
        birdCollider = GetComponent<Collider>();
        if (birdCollider == null)
        {
            // Add a small sphere collider for interaction
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 0.5f;
            birdCollider = sphere;
            Debug.Log("[Bird] Added SphereCollider");
        }

        // If no visual assigned, use this object
        if (birdVisual == null)
        {
            // Check if there are children - use first child with renderer
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Renderer>() != null || child.GetComponentInChildren<Renderer>() != null)
                {
                    birdVisual = child.gameObject;
                    Debug.Log("[Bird] Using child as visual: " + child.name);
                    break;
                }
            }
        }

        // Find UI
        interactionUI = FindFirstObjectByType<InteractionUI>();

        // Init database
        DrugDatabase.Initialize();

        Debug.Log("[Bird] " + birdName + " ready!");
    }

    void Update()
    {
        if (isOut)
        {
            returnTimer -= Time.deltaTime;
            if (returnTimer <= 0f)
                BirdReturns();
            return;
        }

        CheckInteraction();
    }

    void CheckInteraction()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
        bool inRange = dist <= interactionDistance;
        bool looking = false;

        if (inRange)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactionDistance))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    looking = true;
                }
            }
        }

        // UI
        if (looking && interactionUI != null)
        {
            if (collectedSeeds.Count > 0)
                interactionUI.SetPrompt("E to collect seeds from " + birdName);
            else
                interactionUI.SetPrompt("E to send " + birdName + " for seeds");
            interactionUI.Show();
        }
        else if (interactionUI != null)
        {
            interactionUI.Hide();
        }

        // Input
        if (looking && Input.GetKeyDown(interactKey))
        {
            if (collectedSeeds.Count > 0)
                CollectSeeds();
            else
                SendBirdOut();
        }
    }

    void SendBirdOut()
    {
        isOut = true;

        if (debugFastMode)
            returnTimer = 10f;
        else
            returnTimer = Random.Range(minReturnTime, maxReturnTime);

        // Hide
        SetBirdVisible(false);

        // Sound
        if (flyAwaySound != null)
            audioSource.PlayOneShot(flyAwaySound, soundVolume);

        if (interactionUI != null)
            interactionUI.Hide();

        Debug.Log("[Bird] " + birdName + " flew away! Returns in " + returnTimer.ToString("F0") + "s");
    }

    void BirdReturns()
    {
        isOut = false;

        // Generate seeds
        int numSeeds = Random.Range(minSeeds, maxSeeds + 1);
        collectedSeeds.Clear();

        for (int i = 0; i < numSeeds; i++)
        {
            SeedItem seed = SeedItem.CreateRandomSeed();
            collectedSeeds.Add(seed);
            Debug.Log("[Bird] Found: " + seed.GetDisplayName());
        }

        // Show
        SetBirdVisible(true);

        // Sound
        if (returnSound != null)
            audioSource.PlayOneShot(returnSound, soundVolume);
        else if (chirpSound != null)
            audioSource.PlayOneShot(chirpSound, soundVolume);

        Debug.Log("[Bird] " + birdName + " returned with " + numSeeds + " seeds!");
    }

    void SetBirdVisible(bool visible)
    {
        // If we have a separate visual object, toggle that
        if (birdVisual != null)
        {
            birdVisual.SetActive(visible);
        }
        else
        {
            // Otherwise toggle all renderers on this object and children
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = visible;
            }
        }

        // Always toggle collider
        if (birdCollider != null)
            birdCollider.enabled = visible;

        Debug.Log("[Bird] Visible: " + visible);
    }

    void CollectSeeds()
    {
        if (collectedSeeds.Count == 0) return;

        SeedChest chest = FindFirstObjectByType<SeedChest>();

        if (chest != null)
        {
            foreach (var seed in collectedSeeds)
            {
                chest.storage.AddItem(seed);
            }
            Debug.Log("[Bird] Added " + collectedSeeds.Count + " seeds to chest!");
        }
        else
        {
            Debug.Log("[Bird] No SeedChest found! Seeds lost.");
        }

        if (chirpSound != null)
            audioSource.PlayOneShot(chirpSound, soundVolume);

        collectedSeeds.Clear();
    }

    public bool IsOut() { return isOut; }
    public float GetReturnTime() { return isOut ? returnTimer : 0f; }
    public bool HasSeeds() { return collectedSeeds.Count > 0; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isOut ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}