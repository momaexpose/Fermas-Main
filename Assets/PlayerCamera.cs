using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    /// <summary>
    /// First-person camera controller with smooth mouse look.
    /// Features: Adjustable sensitivity, smoothing, vertical clamping, FOV effects, and camera shake.
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        #region Mouse Settings
        [Header("Mouse Look")]
        [Tooltip("Mouse sensitivity for horizontal rotation")]
        [SerializeField] private float sensitivityX = 2f;

        [Tooltip("Mouse sensitivity for vertical rotation")]
        [SerializeField] private float sensitivityY = 2f;

        [Tooltip("Multiplier applied to both sensitivities")]
        [SerializeField] private float sensitivityMultiplier = 1f;

        [Tooltip("Smoothing amount for mouse look (0 = no smoothing)")]
        [Range(0f, 0.3f)]
        [SerializeField] private float smoothing = 0.05f;

        [Tooltip("Invert Y axis")]
        [SerializeField] private bool invertY = false;

        [Tooltip("Invert X axis")]
        [SerializeField] private bool invertX = false;
        #endregion

        #region Clamp Settings
        [Header("Rotation Limits")]
        [Tooltip("Maximum angle the player can look up")]
        [SerializeField] private float maxVerticalAngle = 89f;

        [Tooltip("Maximum angle the player can look down")]
        [SerializeField] private float minVerticalAngle = -89f;

        [Tooltip("Limit horizontal rotation (0 = no limit)")]
        [SerializeField] private float maxHorizontalAngle = 0f;
        #endregion

        #region FOV Settings
        [Header("Field of View")]
        [Tooltip("Default field of view")]
        [SerializeField] private float baseFOV = 75f;

        [Tooltip("Field of view when sprinting")]
        [SerializeField] private float sprintFOV = 85f;

        [Tooltip("Field of view when crouching")]
        [SerializeField] private float crouchFOV = 70f;

        [Tooltip("How fast FOV transitions")]
        [SerializeField] private float fovTransitionSpeed = 8f;
        #endregion

        #region Camera Positioning
        [Header("Camera Position")]
        [Tooltip("Height offset from player pivot when standing")]
        [SerializeField] private float standingCameraHeight = 1.7f;

        [Tooltip("Height offset from player pivot when crouching")]
        [SerializeField] private float crouchingCameraHeight = 0.9f;

        [Tooltip("Speed of camera height transitions")]
        [SerializeField] private float heightTransitionSpeed = 8f;
        #endregion

        #region Camera Shake Settings
        [Header("Camera Shake")]
        [Tooltip("Maximum trauma value (affects shake intensity)")]
        [SerializeField] private float maxTrauma = 1f;

        [Tooltip("How fast trauma decays")]
        [SerializeField] private float traumaDecay = 1.5f;

        [Tooltip("Maximum rotation offset for shake")]
        [SerializeField] private float maxShakeAngle = 5f;

        [Tooltip("Frequency of shake oscillation")]
        [SerializeField] private float shakeFrequency = 25f;
        #endregion

        #region References
        [Header("References")]
        [Tooltip("The player body transform to rotate horizontally")]
        [SerializeField] private Transform playerBody;

        [Tooltip("Reference to PlayerController (auto-found if not set)")]
        [SerializeField] private PlayerController playerController;

        private Camera playerCamera;
        #endregion

        #region State
        private float xRotation;
        private float yRotation;
        private float currentXRotation;
        private float currentYRotation;
        private float xRotationVelocity;
        private float yRotationVelocity;
        private float targetFOV;
        private float targetHeight;
        private float trauma;
        private float shakeTime;
        private Vector3 originalLocalPosition;
        private bool isLocked = true;
        #endregion

        #region Input Cache
        private Vector2 mouseInput;
        private Vector2 smoothedMouseInput;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<float, float> OnLook; // Passes X and Y rotation
        #endregion

        #region Properties
        /// <summary>Current horizontal rotation</summary>
        public float YRotation => yRotation;

        /// <summary>Current vertical rotation</summary>
        public float XRotation => xRotation;

        /// <summary>Look direction as a normalized vector</summary>
        public Vector3 LookDirection => transform.forward;

        /// <summary>Current camera FOV</summary>
        public float CurrentFOV => playerCamera != null ? playerCamera.fieldOfView : baseFOV;

        /// <summary>Is cursor locked?</summary>
        public bool IsCursorLocked => isLocked;

        /// <summary>Current mouse sensitivity</summary>
        public float Sensitivity => sensitivityMultiplier;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }

            // Auto-find player body if not set
            if (playerBody == null)
            {
                playerBody = transform.parent;
            }

            // Auto-find player controller
            if (playerController == null && playerBody != null)
            {
                playerController = playerBody.GetComponent<PlayerController>();
            }

            // Store original local position
            originalLocalPosition = transform.localPosition;
            targetHeight = standingCameraHeight;
            targetFOV = baseFOV;

            // Initialize rotation from current transform
            Vector3 currentRotation = transform.localEulerAngles;
            xRotation = currentRotation.x;
            if (xRotation > 180f) xRotation -= 360f;

            if (playerBody != null)
            {
                yRotation = playerBody.eulerAngles.y;
            }

            currentXRotation = xRotation;
            currentYRotation = yRotation;
        }

        private void Start()
        {
            LockCursor();

            if (playerCamera != null)
            {
                playerCamera.fieldOfView = baseFOV;
            }

            // Subscribe to player events
            if (playerController != null)
            {
                playerController.OnStartSprint.AddListener(OnSprintStart);
                playerController.OnStopSprint.AddListener(OnSprintStop);
                playerController.OnStartCrouch.AddListener(OnCrouchStart);
                playerController.OnStopCrouch.AddListener(OnCrouchStop);
                playerController.OnLand.AddListener(OnPlayerLand);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (playerController != null)
            {
                playerController.OnStartSprint.RemoveListener(OnSprintStart);
                playerController.OnStopSprint.RemoveListener(OnSprintStop);
                playerController.OnStartCrouch.RemoveListener(OnCrouchStart);
                playerController.OnStopCrouch.RemoveListener(OnCrouchStop);
                playerController.OnLand.RemoveListener(OnPlayerLand);
            }
        }

        private void Update()
        {
            if (!isLocked) return;

            GatherInput();
            ProcessMouseLook();
            ApplyRotation();
            UpdateFOV();
            UpdateCameraHeight();
            ProcessCameraShake();
        }

        private void LateUpdate()
        {
            // LateUpdate ensures camera follows player after movement
            ApplyFinalCameraPosition();
        }
        #endregion

        #region Input
        private void GatherInput()
        {
            mouseInput = new Vector2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            );

            // Toggle cursor lock with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isLocked)
                {
                    UnlockCursor();
                }
            }

            // Re-lock on click
            if (Input.GetMouseButtonDown(0) && !isLocked)
            {
                LockCursor();
            }
        }

        /// <summary>
        /// Set mouse input externally (for new input system)
        /// </summary>
        public void SetLookInput(Vector2 input)
        {
            mouseInput = input;
        }
        #endregion

        #region Mouse Look
        private void ProcessMouseLook()
        {
            // Apply sensitivity
            float mouseX = mouseInput.x * sensitivityX * sensitivityMultiplier;
            float mouseY = mouseInput.y * sensitivityY * sensitivityMultiplier;

            // Apply inversion
            if (invertX) mouseX = -mouseX;
            if (invertY) mouseY = -mouseY;

            // Apply smoothing
            if (smoothing > 0)
            {
                smoothedMouseInput.x = Mathf.SmoothDamp(smoothedMouseInput.x, mouseX, ref xRotationVelocity, smoothing);
                smoothedMouseInput.y = Mathf.SmoothDamp(smoothedMouseInput.y, mouseY, ref yRotationVelocity, smoothing);
            }
            else
            {
                smoothedMouseInput.x = mouseX;
                smoothedMouseInput.y = mouseY;
            }

            // Update rotation values
            yRotation += smoothedMouseInput.x;
            xRotation -= smoothedMouseInput.y;

            // Clamp vertical rotation
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            // Clamp horizontal rotation if limited
            if (maxHorizontalAngle > 0)
            {
                yRotation = Mathf.Clamp(yRotation, -maxHorizontalAngle, maxHorizontalAngle);
            }

            // Smooth current rotation towards target
            currentXRotation = Mathf.LerpAngle(currentXRotation, xRotation, 1f - smoothing);
            currentYRotation = Mathf.LerpAngle(currentYRotation, yRotation, 1f - smoothing);

            OnLook?.Invoke(currentXRotation, currentYRotation);
        }

        private void ApplyRotation()
        {
            // Apply vertical rotation to camera
            transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);

            // Apply horizontal rotation to player body
            if (playerBody != null)
            {
                playerBody.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
            }
        }
        #endregion

        #region FOV
        private void UpdateFOV()
        {
            if (playerCamera == null) return;

            // Smooth FOV transition
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                fovTransitionSpeed * Time.deltaTime
            );
        }

        private void OnSprintStart()
        {
            targetFOV = sprintFOV;
        }

        private void OnSprintStop()
        {
            if (playerController != null && playerController.IsCrouching)
            {
                targetFOV = crouchFOV;
            }
            else
            {
                targetFOV = baseFOV;
            }
        }

        private void OnCrouchStart()
        {
            targetFOV = crouchFOV;
            targetHeight = crouchingCameraHeight;
        }

        private void OnCrouchStop()
        {
            targetFOV = baseFOV;
            targetHeight = standingCameraHeight;
        }

        /// <summary>
        /// Set a custom FOV (for aiming, etc.)
        /// </summary>
        public void SetFOV(float fov)
        {
            targetFOV = fov;
        }

        /// <summary>
        /// Reset FOV to appropriate value based on state
        /// </summary>
        public void ResetFOV()
        {
            if (playerController != null)
            {
                if (playerController.IsSprinting)
                {
                    targetFOV = sprintFOV;
                }
                else if (playerController.IsCrouching)
                {
                    targetFOV = crouchFOV;
                }
                else
                {
                    targetFOV = baseFOV;
                }
            }
            else
            {
                targetFOV = baseFOV;
            }
        }
        #endregion

        #region Camera Height
        private void UpdateCameraHeight()
        {
            Vector3 targetPosition = originalLocalPosition;
            targetPosition.y = targetHeight;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                heightTransitionSpeed * Time.deltaTime
            );
        }

        private void ApplyFinalCameraPosition()
        {
            // This runs in LateUpdate to apply after any physics/movement
            // Currently just ensures position is correct after shake
        }
        #endregion

        #region Camera Shake
        /// <summary>
        /// Add trauma to camera (causes shake)
        /// </summary>
        /// <param name="amount">Trauma amount (0-1)</param>
        public void AddTrauma(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }

        /// <summary>
        /// Immediately shake camera with specific intensity
        /// </summary>
        public void Shake(float intensity, float duration = 0.5f)
        {
            AddTrauma(intensity);
            // Duration is handled by trauma decay
        }

        private void ProcessCameraShake()
        {
            if (trauma > 0)
            {
                shakeTime += Time.deltaTime * shakeFrequency;

                // Calculate shake using Perlin noise for smooth randomness
                float shake = trauma * trauma; // Square for more dramatic falloff

                float offsetX = maxShakeAngle * shake * (Mathf.PerlinNoise(shakeTime, 0) * 2 - 1);
                float offsetY = maxShakeAngle * shake * (Mathf.PerlinNoise(0, shakeTime) * 2 - 1);
                float offsetZ = maxShakeAngle * shake * 0.5f * (Mathf.PerlinNoise(shakeTime, shakeTime) * 2 - 1);

                // Apply shake offset to rotation
                transform.localRotation = Quaternion.Euler(
                    currentXRotation + offsetX,
                    offsetY,
                    offsetZ
                );

                // Decay trauma
                trauma = Mathf.Max(0, trauma - traumaDecay * Time.deltaTime);
            }
        }

        private void OnPlayerLand()
        {
            // Add small shake on landing
            if (playerController != null)
            {
                float fallSpeed = Mathf.Abs(playerController.Velocity.y);
                if (fallSpeed > 5f)
                {
                    AddTrauma(Mathf.Clamp01((fallSpeed - 5f) / 15f) * 0.3f);
                }
            }
        }
        #endregion

        #region Cursor Control
        /// <summary>
        /// Lock cursor and enable camera control
        /// </summary>
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isLocked = true;
        }

        /// <summary>
        /// Unlock cursor and disable camera control
        /// </summary>
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isLocked = false;
        }

        /// <summary>
        /// Toggle cursor lock state
        /// </summary>
        public void ToggleCursor()
        {
            if (isLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
        #endregion

        #region Sensitivity
        /// <summary>
        /// Set mouse sensitivity multiplier
        /// </summary>
        public void SetSensitivity(float multiplier)
        {
            sensitivityMultiplier = Mathf.Max(0.1f, multiplier);
        }

        /// <summary>
        /// Set individual X and Y sensitivity
        /// </summary>
        public void SetSensitivity(float x, float y)
        {
            sensitivityX = Mathf.Max(0.1f, x);
            sensitivityY = Mathf.Max(0.1f, y);
        }
        #endregion

        #region Rotation Control
        /// <summary>
        /// Force camera to look at a specific point
        /// </summary>
        public void LookAt(Vector3 worldPosition)
        {
            Vector3 direction = worldPosition - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Vector3 euler = targetRotation.eulerAngles;

            yRotation = euler.y;
            xRotation = euler.x;
            if (xRotation > 180f) xRotation -= 360f;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            currentXRotation = xRotation;
            currentYRotation = yRotation;
        }

        /// <summary>
        /// Set rotation directly
        /// </summary>
        public void SetRotation(float horizontal, float vertical)
        {
            yRotation = horizontal;
            xRotation = Mathf.Clamp(vertical, minVerticalAngle, maxVerticalAngle);
            currentXRotation = xRotation;
            currentYRotation = yRotation;
        }

        /// <summary>
        /// Add rotation offset (for recoil, etc.)
        /// </summary>
        public void AddRotationOffset(float horizontal, float vertical)
        {
            yRotation += horizontal;
            xRotation = Mathf.Clamp(xRotation - vertical, minVerticalAngle, maxVerticalAngle);
        }
        #endregion
    }
}