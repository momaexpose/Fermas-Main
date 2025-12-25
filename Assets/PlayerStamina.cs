using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    /// <summary>
    /// Stamina system for sprinting and other stamina-consuming actions.
    /// Integrates with PlayerController for sprint management.
    /// </summary>
    public class PlayerStamina : MonoBehaviour
    {
        #region Stamina Settings
        [Header("Stamina Configuration")]
        [Tooltip("Maximum stamina amount")]
        [SerializeField] private float maxStamina = 100f;

        [Tooltip("Starting stamina (set to max if 0)")]
        [SerializeField] private float startingStamina = 0f;

        [Tooltip("Stamina consumed per second while sprinting")]
        [SerializeField] private float sprintCost = 15f;

        [Tooltip("Stamina regeneration per second")]
        [SerializeField] private float regenRate = 10f;

        [Tooltip("Delay before stamina starts regenerating after use")]
        [SerializeField] private float regenDelay = 1f;

        [Tooltip("Minimum stamina required to start sprinting")]
        [SerializeField] private float minStaminaToSprint = 10f;

        [Tooltip("Regeneration rate multiplier when crouching")]
        [SerializeField] private float crouchRegenMultiplier = 1.5f;

        [Tooltip("Regeneration rate multiplier when standing still")]
        [SerializeField] private float idleRegenMultiplier = 1.25f;
        #endregion

        #region Exhaustion Settings
        [Header("Exhaustion")]
        [Tooltip("Enable exhaustion state when stamina depleted")]
        [SerializeField] private bool enableExhaustion = true;

        [Tooltip("Stamina percentage required to exit exhaustion")]
        [Range(0.1f, 0.5f)]
        [SerializeField] private float exhaustionRecoveryThreshold = 0.25f;

        [Tooltip("Movement speed multiplier when exhausted")]
        [SerializeField] private float exhaustionSpeedMultiplier = 0.7f;
        #endregion

        #region Visual Feedback Settings
        [Header("Visual Feedback")]
        [Tooltip("Enable screen effects when low stamina")]
        [SerializeField] private bool enableVisualFeedback = true;

        [Tooltip("Stamina percentage to start showing low stamina effects")]
        [Range(0.1f, 0.4f)]
        [SerializeField] private float lowStaminaThreshold = 0.2f;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<float> OnStaminaChanged; // Current stamina
        public UnityEvent<float> OnStaminaPercentChanged; // 0-1 percentage
        public UnityEvent OnStaminaDepleted;
        public UnityEvent OnStaminaFull;
        public UnityEvent OnExhaustionStart;
        public UnityEvent OnExhaustionEnd;
        public UnityEvent OnLowStamina;
        public UnityEvent OnStaminaRecovered;
        #endregion

        #region State
        private float currentStamina;
        private float regenDelayTimer;
        private bool isExhausted;
        private bool wasLowStamina;
        private bool isConsuming;
        #endregion

        #region References
        private PlayerController playerController;
        #endregion

        #region Properties
        /// <summary>Current stamina amount</summary>
        public float CurrentStamina => currentStamina;

        /// <summary>Maximum stamina amount</summary>
        public float MaxStamina => maxStamina;

        /// <summary>Current stamina as percentage (0-1)</summary>
        public float StaminaPercent => currentStamina / maxStamina;

        /// <summary>Is stamina full?</summary>
        public bool IsFull => currentStamina >= maxStamina;

        /// <summary>Is stamina empty?</summary>
        public bool IsEmpty => currentStamina <= 0;

        /// <summary>Can the player sprint?</summary>
        public bool CanSprint => !isExhausted && currentStamina >= minStaminaToSprint;

        /// <summary>Is player exhausted?</summary>
        public bool IsExhausted => isExhausted;

        /// <summary>Is stamina low?</summary>
        public bool IsLowStamina => StaminaPercent <= lowStaminaThreshold;

        /// <summary>Is stamina currently being consumed?</summary>
        public bool IsConsuming => isConsuming;

        /// <summary>Speed multiplier based on exhaustion state</summary>
        public float SpeedMultiplier => isExhausted ? exhaustionSpeedMultiplier : 1f;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            playerController = GetComponent<PlayerController>();

            // Initialize stamina
            currentStamina = startingStamina > 0 ? startingStamina : maxStamina;
        }

        private void Update()
        {
            HandleRegeneration();
            CheckLowStamina();
            isConsuming = false; // Reset each frame, set by consume methods
        }
        #endregion

        #region Consumption
        /// <summary>
        /// Consume stamina for sprinting (called by PlayerController)
        /// </summary>
        public void ConsumeSprint(float deltaTime)
        {
            ConsumeStamina(sprintCost * deltaTime);
        }

        /// <summary>
        /// Consume a specific amount of stamina
        /// </summary>
        public void ConsumeStamina(float amount)
        {
            if (amount <= 0) return;

            isConsuming = true;
            float previousStamina = currentStamina;
            currentStamina = Mathf.Max(0, currentStamina - amount);
            regenDelayTimer = regenDelay;

            // Fire events
            OnStaminaChanged?.Invoke(currentStamina);
            OnStaminaPercentChanged?.Invoke(StaminaPercent);

            // Check for depletion
            if (currentStamina <= 0 && previousStamina > 0)
            {
                OnStaminaDepleted?.Invoke();

                if (enableExhaustion)
                {
                    EnterExhaustion();
                }
            }
        }

        /// <summary>
        /// Check if we have enough stamina for an action
        /// </summary>
        public bool HasStamina(float amount)
        {
            return currentStamina >= amount;
        }

        /// <summary>
        /// Try to consume stamina, returns true if successful
        /// </summary>
        public bool TryConsumeStamina(float amount)
        {
            if (!HasStamina(amount)) return false;
            ConsumeStamina(amount);
            return true;
        }
        #endregion

        #region Regeneration
        private void HandleRegeneration()
        {
            // Don't regenerate while consuming or during delay
            if (isConsuming) return;

            if (regenDelayTimer > 0)
            {
                regenDelayTimer -= Time.deltaTime;
                return;
            }

            // Don't regenerate if full
            if (IsFull) return;

            // Calculate regeneration rate
            float currentRegenRate = regenRate;

            // Apply multipliers based on state
            if (playerController != null)
            {
                if (playerController.IsCrouching)
                {
                    currentRegenRate *= crouchRegenMultiplier;
                }
                else if (!playerController.IsMoving)
                {
                    currentRegenRate *= idleRegenMultiplier;
                }
            }

            // Apply regeneration
            float previousStamina = currentStamina;
            currentStamina = Mathf.Min(maxStamina, currentStamina + currentRegenRate * Time.deltaTime);

            // Fire events
            OnStaminaChanged?.Invoke(currentStamina);
            OnStaminaPercentChanged?.Invoke(StaminaPercent);

            // Check for full
            if (currentStamina >= maxStamina && previousStamina < maxStamina)
            {
                OnStaminaFull?.Invoke();
            }

            // Check for exhaustion recovery
            if (isExhausted && StaminaPercent >= exhaustionRecoveryThreshold)
            {
                ExitExhaustion();
            }
        }

        /// <summary>
        /// Add stamina directly (power-ups, items, etc.)
        /// </summary>
        public void AddStamina(float amount)
        {
            if (amount <= 0) return;

            float previousStamina = currentStamina;
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);

            OnStaminaChanged?.Invoke(currentStamina);
            OnStaminaPercentChanged?.Invoke(StaminaPercent);

            if (currentStamina >= maxStamina && previousStamina < maxStamina)
            {
                OnStaminaFull?.Invoke();
            }

            // Recovery from exhaustion if enough
            if (isExhausted && StaminaPercent >= exhaustionRecoveryThreshold)
            {
                ExitExhaustion();
            }
        }

        /// <summary>
        /// Instantly refill stamina to max
        /// </summary>
        public void RefillStamina()
        {
            currentStamina = maxStamina;
            regenDelayTimer = 0;

            if (isExhausted)
            {
                ExitExhaustion();
            }

            OnStaminaChanged?.Invoke(currentStamina);
            OnStaminaPercentChanged?.Invoke(StaminaPercent);
            OnStaminaFull?.Invoke();
        }
        #endregion

        #region Exhaustion
        private void EnterExhaustion()
        {
            if (isExhausted) return;

            isExhausted = true;
            OnExhaustionStart?.Invoke();
        }

        private void ExitExhaustion()
        {
            if (!isExhausted) return;

            isExhausted = false;
            OnExhaustionEnd?.Invoke();
        }
        #endregion

        #region Low Stamina Warning
        private void CheckLowStamina()
        {
            bool isLow = IsLowStamina;

            if (isLow && !wasLowStamina)
            {
                OnLowStamina?.Invoke();
            }
            else if (!isLow && wasLowStamina)
            {
                OnStaminaRecovered?.Invoke();
            }

            wasLowStamina = isLow;
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set maximum stamina (also adjusts current if above new max)
        /// </summary>
        public void SetMaxStamina(float newMax)
        {
            maxStamina = Mathf.Max(1, newMax);
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            OnStaminaChanged?.Invoke(currentStamina);
            OnStaminaPercentChanged?.Invoke(StaminaPercent);
        }

        /// <summary>
        /// Modify regeneration rate
        /// </summary>
        public void SetRegenRate(float newRate)
        {
            regenRate = Mathf.Max(0, newRate);
        }

        /// <summary>
        /// Modify sprint cost
        /// </summary>
        public void SetSprintCost(float newCost)
        {
            sprintCost = Mathf.Max(0, newCost);
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// Get stamina data for saving
        /// </summary>
        public StaminaSaveData GetSaveData()
        {
            return new StaminaSaveData
            {
                currentStamina = currentStamina,
                maxStamina = maxStamina,
                isExhausted = isExhausted
            };
        }

        /// <summary>
        /// Load stamina data
        /// </summary>
        public void LoadSaveData(StaminaSaveData data)
        {
            maxStamina = data.maxStamina;
            currentStamina = data.currentStamina;
            isExhausted = data.isExhausted;

            OnStaminaChanged?.Invoke(currentStamina);
            OnStaminaPercentChanged?.Invoke(StaminaPercent);
        }
        #endregion
    }

    /// <summary>
    /// Data structure for saving/loading stamina state
    /// </summary>
    [System.Serializable]
    public struct StaminaSaveData
    {
        public float currentStamina;
        public float maxStamina;
        public bool isExhausted;
    }
}