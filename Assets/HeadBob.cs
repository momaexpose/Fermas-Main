using UnityEngine;

namespace Player
{
    /// <summary>
    /// Realistic head bobbing effect that responds to player movement.
    /// Creates vertical and horizontal oscillation based on movement speed.
    /// </summary>
    public class HeadBob : MonoBehaviour
    {
        #region Bob Settings
        [Header("Bob Intensity")]
        [Tooltip("Vertical bob amount when walking")]
        [SerializeField] private float walkBobAmount = 0.035f;

        [Tooltip("Horizontal bob amount when walking")]
        [SerializeField] private float walkBobHorizontal = 0.02f;

        [Tooltip("Vertical bob amount when sprinting")]
        [SerializeField] private float sprintBobAmount = 0.06f;

        [Tooltip("Horizontal bob amount when sprinting")]
        [SerializeField] private float sprintBobHorizontal = 0.035f;

        [Tooltip("Vertical bob amount when crouching")]
        [SerializeField] private float crouchBobAmount = 0.02f;

        [Tooltip("Horizontal bob amount when crouching")]
        [SerializeField] private float crouchBobHorizontal = 0.015f;
        #endregion

        #region Speed Settings
        [Header("Bob Speed")]
        [Tooltip("Bob frequency when walking (cycles per second)")]
        [SerializeField] private float walkBobSpeed = 10f;

        [Tooltip("Bob frequency when sprinting")]
        [SerializeField] private float sprintBobSpeed = 14f;

        [Tooltip("Bob frequency when crouching")]
        [SerializeField] private float crouchBobSpeed = 7f;
        #endregion

        #region Smoothing Settings
        [Header("Smoothing")]
        [Tooltip("How quickly bob transitions between states")]
        [SerializeField] private float bobTransitionSpeed = 12f;

        [Tooltip("How quickly bob settles when stopping")]
        [SerializeField] private float returnSpeed = 6f;

        [Tooltip("Minimum speed to activate head bob")]
        [SerializeField] private float minimumVelocity = 0.5f;
        #endregion

        #region Tilt Settings
        [Header("Camera Tilt")]
        [Tooltip("Enable camera tilt when strafing")]
        [SerializeField] private bool enableTilt = true;

        [Tooltip("Maximum tilt angle when strafing")]
        [SerializeField] private float maxTiltAngle = 2f;

        [Tooltip("How fast tilt responds to input")]
        [SerializeField] private float tiltSpeed = 8f;
        #endregion

        #region Landing Impact Settings
        [Header("Landing Impact")]
        [Tooltip("Enable camera drop on landing")]
        [SerializeField] private bool enableLandingImpact = true;

        [Tooltip("How much camera drops on landing")]
        [SerializeField] private float landingDropAmount = 0.15f;

        [Tooltip("How fast camera recovers from landing")]
        [SerializeField] private float landingRecoverySpeed = 8f;
        #endregion

        #region References
        [Header("References")]
        [Tooltip("Reference to PlayerController (auto-found if not set)")]
        [SerializeField] private PlayerController playerController;

        [Tooltip("Reference to camera transform (auto-found if not set)")]
        [SerializeField] private Transform cameraTransform;
        #endregion

        #region State
        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        private float bobTimer;
        private float currentBobAmount;
        private float currentBobHorizontal;
        private float currentBobSpeed;
        private float targetBobAmount;
        private float targetBobHorizontal;
        private float targetBobSpeed;
        private float landingOffset;
        private float currentTilt;
        private bool isActive = true;
        #endregion

        #region Properties
        /// <summary>Is head bob currently active?</summary>
        public bool IsActive => isActive;

        /// <summary>Current bob position offset</summary>
        public Vector3 CurrentBobOffset { get; private set; }
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            // Auto-find references
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
            }

            if (cameraTransform == null)
            {
                cameraTransform = GetComponent<Camera>()?.transform ?? transform;
            }

            originalLocalPosition = cameraTransform.localPosition;
            originalLocalRotation = cameraTransform.localRotation;
        }

        private void Start()
        {
            // Subscribe to landing event
            if (playerController != null && enableLandingImpact)
            {
                playerController.OnLand.AddListener(OnPlayerLand);
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnLand.RemoveListener(OnPlayerLand);
            }
        }

        private void Update()
        {
            if (!isActive || playerController == null) return;

            UpdateBobParameters();
            ApplyHeadBob();
            ApplyTilt();
            ApplyLandingImpact();
        }
        #endregion

        #region Bob Calculation
        private void UpdateBobParameters()
        {
            // Determine target bob parameters based on movement state
            if (playerController.IsGrounded && playerController.HorizontalSpeed > minimumVelocity)
            {
                if (playerController.IsSprinting)
                {
                    targetBobAmount = sprintBobAmount;
                    targetBobHorizontal = sprintBobHorizontal;
                    targetBobSpeed = sprintBobSpeed;
                }
                else if (playerController.IsCrouching)
                {
                    targetBobAmount = crouchBobAmount;
                    targetBobHorizontal = crouchBobHorizontal;
                    targetBobSpeed = crouchBobSpeed;
                }
                else
                {
                    targetBobAmount = walkBobAmount;
                    targetBobHorizontal = walkBobHorizontal;
                    targetBobSpeed = walkBobSpeed;
                }

                // Scale bob by actual speed ratio
                float speedRatio = playerController.HorizontalSpeed /
                    (playerController.IsSprinting ? 7f : playerController.IsCrouching ? 2f : 4f);
                speedRatio = Mathf.Clamp01(speedRatio);

                targetBobAmount *= speedRatio;
                targetBobHorizontal *= speedRatio;
            }
            else
            {
                // Not moving - no bob
                targetBobAmount = 0f;
                targetBobHorizontal = 0f;
            }

            // Smooth transitions
            currentBobAmount = Mathf.Lerp(currentBobAmount, targetBobAmount, bobTransitionSpeed * Time.deltaTime);
            currentBobHorizontal = Mathf.Lerp(currentBobHorizontal, targetBobHorizontal, bobTransitionSpeed * Time.deltaTime);
            currentBobSpeed = Mathf.Lerp(currentBobSpeed, targetBobSpeed, bobTransitionSpeed * Time.deltaTime);
        }

        private void ApplyHeadBob()
        {
            if (currentBobAmount < 0.001f && currentBobHorizontal < 0.001f)
            {
                // Reset to original position when not bobbing
                Vector3 targetPos = originalLocalPosition;
                targetPos.y += landingOffset; // Keep landing impact
                cameraTransform.localPosition = Vector3.Lerp(
                    cameraTransform.localPosition,
                    targetPos,
                    returnSpeed * Time.deltaTime
                );
                CurrentBobOffset = Vector3.zero;
                return;
            }

            // Advance bob timer
            bobTimer += Time.deltaTime * currentBobSpeed;

            // Calculate bob offset
            // Vertical: Full sine wave (up and down)
            float verticalOffset = Mathf.Sin(bobTimer) * currentBobAmount;

            // Horizontal: Half frequency (left-right each step)
            float horizontalOffset = Mathf.Cos(bobTimer * 0.5f) * currentBobHorizontal;

            // Apply offset
            Vector3 bobOffset = new Vector3(horizontalOffset, verticalOffset, 0f);
            CurrentBobOffset = bobOffset;

            Vector3 targetPosition = originalLocalPosition + bobOffset;
            targetPosition.y += landingOffset; // Add landing impact

            cameraTransform.localPosition = targetPosition;
        }
        #endregion

        #region Tilt
        private void ApplyTilt()
        {
            if (!enableTilt) return;

            // Get strafe input (horizontal movement)
            float horizontalInput = Input.GetAxisRaw("Horizontal");

            // Target tilt based on strafe direction
            float targetTilt = -horizontalInput * maxTiltAngle;

            // Only tilt when moving
            if (!playerController.IsMoving || !playerController.IsGrounded)
            {
                targetTilt = 0f;
            }

            // Smooth tilt
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

            // Apply tilt to rotation
            Vector3 currentEuler = cameraTransform.localEulerAngles;
            cameraTransform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, currentTilt);
        }
        #endregion

        #region Landing Impact
        private void OnPlayerLand()
        {
            if (!enableLandingImpact) return;

            // Calculate impact based on fall velocity
            float fallSpeed = Mathf.Abs(playerController.Velocity.y);
            float impactScale = Mathf.Clamp01((fallSpeed - 3f) / 10f);

            landingOffset = -landingDropAmount * impactScale;
        }

        private void ApplyLandingImpact()
        {
            if (Mathf.Abs(landingOffset) > 0.001f)
            {
                // Recover from landing impact
                landingOffset = Mathf.Lerp(landingOffset, 0f, landingRecoverySpeed * Time.deltaTime);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enable or disable head bob
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;

            if (!active)
            {
                // Reset to original position
                cameraTransform.localPosition = originalLocalPosition;
                cameraTransform.localRotation = originalLocalRotation;
                currentBobAmount = 0f;
                currentBobHorizontal = 0f;
                bobTimer = 0f;
                currentTilt = 0f;
                landingOffset = 0f;
            }
        }

        /// <summary>
        /// Set bob intensity multiplier (0-2, 1 = normal)
        /// </summary>
        public void SetIntensity(float multiplier)
        {
            multiplier = Mathf.Clamp(multiplier, 0f, 2f);

            walkBobAmount = 0.035f * multiplier;
            walkBobHorizontal = 0.02f * multiplier;
            sprintBobAmount = 0.06f * multiplier;
            sprintBobHorizontal = 0.035f * multiplier;
            crouchBobAmount = 0.02f * multiplier;
            crouchBobHorizontal = 0.015f * multiplier;
        }

        /// <summary>
        /// Reset bob state (useful for teleporting, cutscenes, etc.)
        /// </summary>
        public void Reset()
        {
            bobTimer = 0f;
            currentBobAmount = 0f;
            currentBobHorizontal = 0f;
            currentTilt = 0f;
            landingOffset = 0f;
            cameraTransform.localPosition = originalLocalPosition;
            cameraTransform.localRotation = originalLocalRotation;
        }
        #endregion
    }
}