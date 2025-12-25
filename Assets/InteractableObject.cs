using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    /// <summary>
    /// Base class for interactable objects. Inherit from this for easy interaction setup.
    /// Implements IInteractable interface with common functionality.
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        #region Settings
        [Header("Interaction Settings")]
        [Tooltip("Text shown when player looks at this object")]
        [SerializeField] protected string promptText = "Press E to interact";

        [Tooltip("Can this object be interacted with?")]
        [SerializeField] protected bool isInteractable = true;

        [Tooltip("Disable after first interaction")]
        [SerializeField] protected bool singleUse = false;

        [Tooltip("Cooldown between interactions (0 = no cooldown)")]
        [SerializeField] protected float interactionCooldown = 0f;
        #endregion

        #region Visual Feedback
        [Header("Visual Feedback")]
        [Tooltip("Outline or highlight when looked at")]
        [SerializeField] protected bool enableHighlight = true;

        [Tooltip("Renderer to highlight (auto-found if not set)")]
        [SerializeField] protected Renderer targetRenderer;

        [Tooltip("Highlight color")]
        [SerializeField] protected Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

        [Tooltip("Original material emission color")]
        protected Color originalEmissionColor;
        protected bool hasEmission;
        #endregion

        #region Audio
        [Header("Audio")]
        [Tooltip("Sound played on interaction")]
        [SerializeField] protected AudioClip interactionSound;

        [Tooltip("Volume of interaction sound")]
        [Range(0f, 1f)]
        [SerializeField] protected float soundVolume = 1f;

        protected AudioSource audioSource;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent OnInteracted;
        public UnityEvent OnLookedAt;
        public UnityEvent OnLookedAway;
        public UnityEvent OnBecameInteractable;
        public UnityEvent OnBecameNonInteractable;
        #endregion

        #region State
        protected bool isBeingLookedAt;
        protected float cooldownTimer;
        protected bool hasBeenUsed;
        #endregion

        #region IInteractable Implementation
        public virtual bool CanInteract => isInteractable && !hasBeenUsed && cooldownTimer <= 0;

        public virtual string InteractionPrompt => promptText;

        public virtual void Interact(GameObject interactor)
        {
            if (!CanInteract) return;

            // Play sound
            PlayInteractionSound();

            // Fire event
            OnInteracted?.Invoke();

            // Handle single use
            if (singleUse)
            {
                hasBeenUsed = true;
                SetInteractable(false);
            }

            // Start cooldown
            if (interactionCooldown > 0)
            {
                cooldownTimer = interactionCooldown;
            }

            // Call virtual method for derived classes
            OnInteract(interactor);
        }

        public virtual void OnLookAt()
        {
            if (isBeingLookedAt) return;

            isBeingLookedAt = true;

            if (enableHighlight)
            {
                ApplyHighlight();
            }

            OnLookedAt?.Invoke();
        }

        public virtual void OnLookAway()
        {
            if (!isBeingLookedAt) return;

            isBeingLookedAt = false;

            if (enableHighlight)
            {
                RemoveHighlight();
            }

            OnLookedAway?.Invoke();
        }
        #endregion

        #region Unity Callbacks
        protected virtual void Awake()
        {
            // Get or add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && interactionSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
            }

            // Find renderer for highlighting
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
                if (targetRenderer == null)
                {
                    targetRenderer = GetComponentInChildren<Renderer>();
                }
            }

            // Store original emission
            if (targetRenderer != null && targetRenderer.material.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = targetRenderer.material.GetColor("_EmissionColor");
                hasEmission = true;
            }
        }

        protected virtual void Update()
        {
            // Update cooldown
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        protected virtual void OnDisable()
        {
            // Clean up highlight if disabled while being looked at
            if (isBeingLookedAt)
            {
                RemoveHighlight();
                isBeingLookedAt = false;
            }
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Override this in derived classes to handle interaction
        /// </summary>
        protected virtual void OnInteract(GameObject interactor)
        {
            // Override in derived classes
        }
        #endregion

        #region Highlight
        protected virtual void ApplyHighlight()
        {
            if (targetRenderer == null) return;

            // Simple emission-based highlight
            if (targetRenderer.material.HasProperty("_EmissionColor"))
            {
                targetRenderer.material.EnableKeyword("_EMISSION");
                targetRenderer.material.SetColor("_EmissionColor", highlightColor * 0.3f);
            }
        }

        protected virtual void RemoveHighlight()
        {
            if (targetRenderer == null) return;

            if (hasEmission)
            {
                targetRenderer.material.SetColor("_EmissionColor", originalEmissionColor);
            }
            else if (targetRenderer.material.HasProperty("_EmissionColor"))
            {
                targetRenderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
        #endregion

        #region Audio
        protected virtual void PlayInteractionSound()
        {
            if (interactionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(interactionSound, soundVolume);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Set whether this object can be interacted with
        /// </summary>
        public virtual void SetInteractable(bool interactable)
        {
            bool wasInteractable = isInteractable;
            isInteractable = interactable;

            if (interactable && !wasInteractable)
            {
                OnBecameInteractable?.Invoke();
            }
            else if (!interactable && wasInteractable)
            {
                OnBecameNonInteractable?.Invoke();
            }
        }

        /// <summary>
        /// Set the interaction prompt text
        /// </summary>
        public virtual void SetPromptText(string text)
        {
            promptText = text;
        }

        /// <summary>
        /// Reset single-use state
        /// </summary>
        public virtual void ResetUse()
        {
            hasBeenUsed = false;
            cooldownTimer = 0;
            SetInteractable(true);
        }

        /// <summary>
        /// Check if player is currently looking at this
        /// </summary>
        public bool IsBeingLookedAt()
        {
            return isBeingLookedAt;
        }
        #endregion
    }

    /// <summary>
    /// Base class for hold-to-interact objects
    /// </summary>
    public class HoldInteractableObject : InteractableObject, IHoldInteractable
    {
        #region Hold Settings
        [Header("Hold Interaction")]
        [Tooltip("Time required to hold for full interaction")]
        [SerializeField] protected float holdDuration = 2f;

        [Tooltip("Does progress reset when released early?")]
        [SerializeField] protected bool resetOnRelease = true;

        [Tooltip("Progress decay rate when not holding (if not resetting)")]
        [SerializeField] protected float progressDecayRate = 0.5f;
        #endregion

        #region Hold Events
        [Header("Hold Events")]
        public UnityEvent<float> OnHoldProgress; // 0-1 progress
        public UnityEvent OnHoldComplete;
        public UnityEvent OnHoldCancelled;
        #endregion

        #region State
        protected float currentHoldProgress;
        protected bool isHolding;
        #endregion

        #region IHoldInteractable Implementation
        public float HoldDuration => holdDuration;
        public float HoldProgress => currentHoldProgress;

        public virtual void OnHoldInteract(float deltaTime)
        {
            if (!CanInteract) return;

            isHolding = true;
            currentHoldProgress += deltaTime / holdDuration;
            currentHoldProgress = Mathf.Clamp01(currentHoldProgress);

            OnHoldProgress?.Invoke(currentHoldProgress);

            if (currentHoldProgress >= 1f)
            {
                CompleteHoldInteraction();
            }
        }

        public virtual void OnReleaseInteract()
        {
            if (!isHolding) return;

            isHolding = false;

            if (currentHoldProgress < 1f)
            {
                OnHoldCancelled?.Invoke();

                if (resetOnRelease)
                {
                    currentHoldProgress = 0f;
                    OnHoldProgress?.Invoke(0f);
                }
            }
        }
        #endregion

        #region Override
        public override void Interact(GameObject interactor)
        {
            // For hold interactables, regular interact starts the hold
            // The actual interaction happens in CompleteHoldInteraction
        }

        protected override void Update()
        {
            base.Update();

            // Decay progress if not holding and not resetting
            if (!isHolding && !resetOnRelease && currentHoldProgress > 0)
            {
                currentHoldProgress -= progressDecayRate * Time.deltaTime;
                currentHoldProgress = Mathf.Max(0, currentHoldProgress);
                OnHoldProgress?.Invoke(currentHoldProgress);
            }
        }
        #endregion

        #region Hold Completion
        protected virtual void CompleteHoldInteraction()
        {
            // Play sound
            PlayInteractionSound();

            // Fire events
            OnHoldComplete?.Invoke();
            OnInteracted?.Invoke();

            // Handle single use
            if (singleUse)
            {
                hasBeenUsed = true;
                SetInteractable(false);
            }

            // Reset progress
            currentHoldProgress = 0f;
            isHolding = false;

            // Start cooldown
            if (interactionCooldown > 0)
            {
                cooldownTimer = interactionCooldown;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reset hold progress
        /// </summary>
        public virtual void ResetHoldProgress()
        {
            currentHoldProgress = 0f;
            isHolding = false;
            OnHoldProgress?.Invoke(0f);
        }

        /// <summary>
        /// Set hold duration
        /// </summary>
        public virtual void SetHoldDuration(float duration)
        {
            holdDuration = Mathf.Max(0.1f, duration);
        }
        #endregion
    }
}