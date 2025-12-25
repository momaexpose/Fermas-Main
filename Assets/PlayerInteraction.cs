using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    /// <summary>
    /// First-person interaction system for detecting and interacting with world objects.
    /// Uses raycasting from camera center to detect IInteractable objects.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        #region Interaction Settings
        [Header("Raycast Settings")]
        [Tooltip("Maximum distance for interaction")]
        [SerializeField] private float interactionRange = 3f;

        [Tooltip("Layers that can be interacted with")]
        [SerializeField] private LayerMask interactionLayers = ~0;

        [Tooltip("Radius of the spherecast (0 for precise raycast)")]
        [SerializeField] private float raycastRadius = 0.1f;

        [Tooltip("How often to check for interactables (seconds)")]
        [SerializeField] private float detectionInterval = 0.05f;
        #endregion

        #region Input Settings
        [Header("Input")]
        [Tooltip("Key to interact")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Tooltip("Alternative key to interact")]
        [SerializeField] private KeyCode altInteractKey = KeyCode.F;

        [Tooltip("Allow holding interact key for continuous interaction")]
        [SerializeField] private bool allowHoldInteract = false;
        #endregion

        #region UI Settings
        [Header("Crosshair Response")]
        [Tooltip("Change crosshair when looking at interactable")]
        [SerializeField] private bool changeCrosshair = true;

        [Tooltip("Crosshair UI element to modify")]
        [SerializeField] private RectTransform crosshairUI;

        [Tooltip("Normal crosshair size")]
        [SerializeField] private float normalCrosshairSize = 20f;

        [Tooltip("Crosshair size when looking at interactable")]
        [SerializeField] private float interactableCrosshairSize = 30f;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<IInteractable> OnLookAtInteractable;
        public UnityEvent OnLookAwayFromInteractable;
        public UnityEvent<IInteractable> OnInteract;
        public UnityEvent<string> OnShowInteractionPrompt; // Passes prompt text
        public UnityEvent OnHideInteractionPrompt;
        #endregion

        #region References
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerController playerController;
        #endregion

        #region State
        private IInteractable currentInteractable;
        private GameObject currentInteractableObject;
        private float detectionTimer;
        private bool isInteracting;
        private bool wasLookingAtInteractable;
        #endregion

        #region Properties
        /// <summary>Currently targeted interactable (null if none)</summary>
        public IInteractable CurrentInteractable => currentInteractable;

        /// <summary>Is player currently looking at an interactable?</summary>
        public bool IsLookingAtInteractable => currentInteractable != null;

        /// <summary>Is player currently in an interaction?</summary>
        public bool IsInteracting => isInteracting;

        /// <summary>Current interaction range</summary>
        public float InteractionRange => interactionRange;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            // Auto-find references
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    playerCamera = Camera.main;
                }
            }

            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            DetectInteractables();
            HandleInteractionInput();
            UpdateCrosshair();
        }
        #endregion

        #region Detection
        private void DetectInteractables()
        {
            detectionTimer += Time.deltaTime;
            if (detectionTimer < detectionInterval) return;
            detectionTimer = 0f;

            // Raycast from camera center
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            bool hitSomething;

            if (raycastRadius > 0)
            {
                hitSomething = Physics.SphereCast(ray, raycastRadius, out hit, interactionRange, interactionLayers, QueryTriggerInteraction.Collide);
            }
            else
            {
                hitSomething = Physics.Raycast(ray, out hit, interactionRange, interactionLayers, QueryTriggerInteraction.Collide);
            }

            if (hitSomething)
            {
                // Check if hit object has IInteractable
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                // Also check parent
                if (interactable == null)
                {
                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                }

                if (interactable != null && interactable.CanInteract)
                {
                    SetCurrentInteractable(interactable, hit.collider.gameObject);
                }
                else
                {
                    ClearCurrentInteractable();
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }

        private void SetCurrentInteractable(IInteractable interactable, GameObject obj)
        {
            if (currentInteractable == interactable) return;

            // Clear previous
            if (currentInteractable != null)
            {
                currentInteractable.OnLookAway();
            }

            currentInteractable = interactable;
            currentInteractableObject = obj;
            currentInteractable.OnLookAt();

            OnLookAtInteractable?.Invoke(currentInteractable);
            OnShowInteractionPrompt?.Invoke(currentInteractable.InteractionPrompt);
            wasLookingAtInteractable = true;
        }

        private void ClearCurrentInteractable()
        {
            if (currentInteractable == null) return;

            currentInteractable.OnLookAway();
            currentInteractable = null;
            currentInteractableObject = null;

            OnLookAwayFromInteractable?.Invoke();
            OnHideInteractionPrompt?.Invoke();
            wasLookingAtInteractable = false;
        }
        #endregion

        #region Interaction
        private void HandleInteractionInput()
        {
            if (currentInteractable == null) return;

            bool interactPressed = Input.GetKeyDown(interactKey) || Input.GetKeyDown(altInteractKey);
            bool interactHeld = allowHoldInteract && (Input.GetKey(interactKey) || Input.GetKey(altInteractKey));

            if (interactPressed || (interactHeld && !isInteracting))
            {
                TryInteract();
            }

            // Handle hold interactions
            if (currentInteractable is IHoldInteractable holdInteractable)
            {
                if (Input.GetKey(interactKey) || Input.GetKey(altInteractKey))
                {
                    holdInteractable.OnHoldInteract(Time.deltaTime);
                    isInteracting = true;
                }
                else if (isInteracting)
                {
                    holdInteractable.OnReleaseInteract();
                    isInteracting = false;
                }
            }
        }

        private void TryInteract()
        {
            if (currentInteractable == null || !currentInteractable.CanInteract) return;

            currentInteractable.Interact(gameObject);
            OnInteract?.Invoke(currentInteractable);
        }

        /// <summary>
        /// Force an interaction with the current target
        /// </summary>
        public void ForceInteract()
        {
            TryInteract();
        }

        /// <summary>
        /// Interact with a specific object (bypasses raycast)
        /// </summary>
        public void InteractWith(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract)
            {
                interactable.Interact(gameObject);
                OnInteract?.Invoke(interactable);
            }
        }
        #endregion

        #region Crosshair
        private void UpdateCrosshair()
        {
            if (!changeCrosshair || crosshairUI == null) return;

            float targetSize = currentInteractable != null ? interactableCrosshairSize : normalCrosshairSize;
            float currentSize = crosshairUI.sizeDelta.x;
            float newSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * 10f);

            crosshairUI.sizeDelta = new Vector2(newSize, newSize);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set interaction range
        /// </summary>
        public void SetInteractionRange(float range)
        {
            interactionRange = Mathf.Max(0.1f, range);
        }

        /// <summary>
        /// Check if a specific object is interactable and in range
        /// </summary>
        public bool CanInteractWith(GameObject obj)
        {
            if (obj == null) return false;

            IInteractable interactable = obj.GetComponent<IInteractable>();
            if (interactable == null) return false;

            float distance = Vector3.Distance(playerCamera.transform.position, obj.transform.position);
            return distance <= interactionRange && interactable.CanInteract;
        }

        /// <summary>
        /// Get the closest interactable in range
        /// </summary>
        public IInteractable GetClosestInteractable()
        {
            Collider[] colliders = Physics.OverlapSphere(playerCamera.transform.position, interactionRange, interactionLayers);

            IInteractable closest = null;
            float closestDistance = float.MaxValue;

            foreach (Collider col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    float distance = Vector3.Distance(playerCamera.transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = interactable;
                    }
                }
            }

            return closest;
        }
        #endregion

        #region Debug
        private void OnDrawGizmosSelected()
        {
            if (playerCamera == null) return;

            Gizmos.color = currentInteractable != null ? Color.green : Color.yellow;
            Gizmos.DrawLine(
                playerCamera.transform.position,
                playerCamera.transform.position + playerCamera.transform.forward * interactionRange
            );

            if (raycastRadius > 0)
            {
                Gizmos.DrawWireSphere(
                    playerCamera.transform.position + playerCamera.transform.forward * interactionRange,
                    raycastRadius
                );
            }
        }
        #endregion
    }

    #region Interfaces
    /// <summary>
    /// Interface for objects that can be interacted with
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Can this object currently be interacted with?</summary>
        bool CanInteract { get; }

        /// <summary>Text shown to player when looking at this object</summary>
        string InteractionPrompt { get; }

        /// <summary>Called when player interacts with this object</summary>
        void Interact(GameObject interactor);

        /// <summary>Called when player starts looking at this object</summary>
        void OnLookAt();

        /// <summary>Called when player stops looking at this object</summary>
        void OnLookAway();
    }

    /// <summary>
    /// Extended interface for hold-to-interact objects
    /// </summary>
    public interface IHoldInteractable : IInteractable
    {
        /// <summary>Time required to hold for full interaction</summary>
        float HoldDuration { get; }

        /// <summary>Current hold progress (0-1)</summary>
        float HoldProgress { get; }

        /// <summary>Called each frame while holding interact</summary>
        void OnHoldInteract(float deltaTime);

        /// <summary>Called when interact is released</summary>
        void OnReleaseInteract();
    }
    #endregion
}