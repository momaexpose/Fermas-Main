using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    /// <summary>
    /// High-quality first-person character controller with realistic movement.
    /// Features: Walking, sprinting, crouching, jumping, smooth acceleration/deceleration.
    /// Designed for a realistic drug farming simulation game.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        #region Movement Settings
        [Header("Movement Speeds")]
        [Tooltip("Normal walking speed in units per second")]
        [SerializeField] private float walkSpeed = 4f;

        [Tooltip("Sprinting speed in units per second")]
        [SerializeField] private float sprintSpeed = 7f;

        [Tooltip("Crouching speed in units per second")]
        [SerializeField] private float crouchSpeed = 2f;

        [Tooltip("How quickly the player accelerates to target speed")]
        [SerializeField] private float acceleration = 10f;

        [Tooltip("How quickly the player decelerates when stopping")]
        [SerializeField] private float deceleration = 12f;

        [Tooltip("Speed multiplier when moving backwards")]
        [SerializeField] private float backwardSpeedMultiplier = 0.7f;

        [Tooltip("Speed multiplier when strafing")]
        [SerializeField] private float strafeSpeedMultiplier = 0.85f;
        #endregion

        #region Jump Settings
        [Header("Jumping")]
        [Tooltip("Initial upward velocity when jumping")]
        [SerializeField] private float jumpForce = 6f;

        [Tooltip("Gravity multiplier (Earth gravity = 1)")]
        [SerializeField] private float gravityMultiplier = 2f;

        [Tooltip("Extra gravity when falling for snappier feel")]
        [SerializeField] private float fallMultiplier = 2.5f;

        [Tooltip("Can the player jump?")]
        [SerializeField] private bool canJump = true;

        [Tooltip("Time after leaving ground where jump is still allowed")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Buffer window for jump input before landing")]
        [SerializeField] private float jumpBufferTime = 0.1f;
        #endregion

        #region Crouch Settings
        [Header("Crouching")]
        [Tooltip("Height of the character controller when standing")]
        [SerializeField] private float standingHeight = 2f;

        [Tooltip("Height of the character controller when crouching")]
        [SerializeField] private float crouchHeight = 1.2f;

        [Tooltip("How fast the crouch transition happens")]
        [SerializeField] private float crouchTransitionSpeed = 8f;

        [Tooltip("Toggle crouch instead of hold")]
        [SerializeField] private bool toggleCrouch = false;
        #endregion

        #region Ground Check Settings
        [Header("Ground Detection")]
        [Tooltip("Layers considered as ground")]
        [SerializeField] private LayerMask groundMask = ~0;

        [Tooltip("Distance to check for ground below player")]
        [SerializeField] private float groundCheckDistance = 0.2f;

        [Tooltip("Radius of the ground check sphere")]
        [SerializeField] private float groundCheckRadius = 0.3f;
        #endregion

        #region Slope Settings
        [Header("Slopes")]
        [Tooltip("Maximum angle the player can walk up")]
        [SerializeField] private float maxSlopeAngle = 45f;

        [Tooltip("Force applied to keep player grounded on slopes")]
        [SerializeField] private float slopeForce = 8f;

        [Tooltip("Length of ray to detect slopes")]
        [SerializeField] private float slopeRayLength = 1.5f;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent OnJump;
        public UnityEvent OnLand;
        public UnityEvent OnStartSprint;
        public UnityEvent OnStopSprint;
        public UnityEvent OnStartCrouch;
        public UnityEvent OnStopCrouch;
        public UnityEvent<float> OnMove; // Passes current speed as parameter
        #endregion

        #region References
        private CharacterController characterController;
        private PlayerStamina playerStamina;
        private Transform cameraTransform;
        #endregion

        #region State
        private Vector3 velocity;
        private Vector3 moveDirection;
        private float currentSpeed;
        private float targetHeight;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool wasGrounded;
        private bool isCrouching;
        private bool isSprinting;
        private bool jumpRequested;
        private RaycastHit slopeHit;
        #endregion

        #region Input Cache
        private Vector2 inputMovement;
        private bool inputJump;
        private bool inputSprint;
        private bool inputCrouch;
        private bool inputCrouchPressed;
        #endregion

        #region Properties
        /// <summary>Is the player currently on the ground?</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>Is the player currently sprinting?</summary>
        public bool IsSprinting => isSprinting;

        /// <summary>Is the player currently crouching?</summary>
        public bool IsCrouching => isCrouching;

        /// <summary>Is the player currently moving?</summary>
        public bool IsMoving => inputMovement.sqrMagnitude > 0.01f;

        /// <summary>Current movement speed</summary>
        public float CurrentSpeed => currentSpeed;

        /// <summary>Normalized movement speed (0-1 based on max speed)</summary>
        public float NormalizedSpeed => currentSpeed / sprintSpeed;

        /// <summary>Current velocity of the player</summary>
        public Vector3 Velocity => velocity;

        /// <summary>Horizontal velocity magnitude</summary>
        public float HorizontalSpeed => new Vector3(velocity.x, 0, velocity.z).magnitude;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerStamina = GetComponent<PlayerStamina>();

            // Find camera (child or tagged)
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            targetHeight = standingHeight;
        }

        private void Start()
        {
            // Lock cursor for FPS controls
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            GatherInput();
            UpdateGroundedState();
            UpdateCoyoteTime();
            UpdateJumpBuffer();
            HandleCrouch();
            HandleSprint();
            HandleJump();
            HandleMovement();
            ApplyGravity();
            ApplyFinalMovement();
            UpdateHeight();
        }
        #endregion

        #region Input
        private void GatherInput()
        {
            // Movement input
            inputMovement = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            // Jump input
            if (Input.GetButtonDown("Jump"))
            {
                inputJump = true;
                jumpBufferCounter = jumpBufferTime;
            }

            // Sprint input
            inputSprint = Input.GetKey(KeyCode.LeftShift);

            // Crouch input
            if (toggleCrouch)
            {
                if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
                {
                    inputCrouchPressed = true;
                }
            }
            else
            {
                inputCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            }
        }

        /// <summary>
        /// Set movement input externally (for new input system or custom input)
        /// </summary>
        public void SetMovementInput(Vector2 input)
        {
            inputMovement = input;
        }

        /// <summary>
        /// Trigger jump externally
        /// </summary>
        public void TriggerJump()
        {
            inputJump = true;
            jumpBufferCounter = jumpBufferTime;
        }

        /// <summary>
        /// Set sprint state externally
        /// </summary>
        public void SetSprint(bool sprint)
        {
            inputSprint = sprint;
        }

        /// <summary>
        /// Set crouch state externally
        /// </summary>
        public void SetCrouch(bool crouch)
        {
            if (toggleCrouch)
            {
                inputCrouchPressed = crouch;
            }
            else
            {
                inputCrouch = crouch;
            }
        }
        #endregion

        #region Ground Check
        private void UpdateGroundedState()
        {
            wasGrounded = IsGrounded;

            // Sphere cast for ground detection
            Vector3 spherePosition = transform.position + Vector3.up * groundCheckRadius;
            IsGrounded = Physics.SphereCast(
                spherePosition,
                groundCheckRadius,
                Vector3.down,
                out _,
                groundCheckDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

            // Landing event
            if (IsGrounded && !wasGrounded)
            {
                OnLand?.Invoke();
            }
        }

        private void UpdateCoyoteTime()
        {
            if (IsGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void UpdateJumpBuffer()
        {
            if (jumpBufferCounter > 0)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }
        #endregion

        #region Movement
        private void HandleMovement()
        {
            // Get camera-relative directions
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;

            // Flatten to horizontal plane
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calculate move direction
            moveDirection = (forward * inputMovement.y + right * inputMovement.x).normalized;

            // Determine target speed
            float targetSpeed = GetTargetSpeed();

            // Apply directional speed modifiers
            if (inputMovement.y < 0) // Moving backward
            {
                targetSpeed *= backwardSpeedMultiplier;
            }
            if (inputMovement.x != 0 && inputMovement.y == 0) // Pure strafe
            {
                targetSpeed *= strafeSpeedMultiplier;
            }

            // No input = no target speed
            if (inputMovement.sqrMagnitude < 0.01f)
            {
                targetSpeed = 0;
            }

            // Smooth speed transition
            float speedChangeRate = currentSpeed < targetSpeed ? acceleration : deceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);

            // Set horizontal velocity
            Vector3 horizontalVelocity = moveDirection * currentSpeed;
            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;

            // Fire move event
            if (currentSpeed > 0.1f)
            {
                OnMove?.Invoke(currentSpeed);
            }
        }

        private float GetTargetSpeed()
        {
            if (isCrouching)
            {
                return crouchSpeed;
            }
            if (isSprinting)
            {
                return sprintSpeed;
            }
            return walkSpeed;
        }
        #endregion

        #region Jump
        private void HandleJump()
        {
            if (!canJump) return;

            // Check if we can jump (grounded or coyote time, plus jump buffer)
            bool canJumpNow = (IsGrounded || coyoteTimeCounter > 0) && jumpBufferCounter > 0;

            if (canJumpNow && !isCrouching)
            {
                // Perform jump
                velocity.y = jumpForce;
                coyoteTimeCounter = 0;
                jumpBufferCounter = 0;
                inputJump = false;

                // Cancel crouch if jumping
                if (isCrouching)
                {
                    StopCrouch();
                }

                OnJump?.Invoke();
            }
        }
        #endregion

        #region Gravity
        private void ApplyGravity()
        {
            if (IsGrounded && velocity.y < 0)
            {
                // Small negative velocity to keep grounded
                velocity.y = -2f;
            }
            else
            {
                // Apply gravity with fall multiplier
                float multiplier = velocity.y < 0 ? fallMultiplier : gravityMultiplier;
                velocity.y += Physics.gravity.y * multiplier * Time.deltaTime;
            }

            // Apply extra force on slopes to prevent bouncing
            if (IsGrounded && IsOnSlope())
            {
                velocity.y -= slopeForce * Time.deltaTime;
            }
        }

        private bool IsOnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeRayLength))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle > 0 && angle <= maxSlopeAngle;
            }
            return false;
        }
        #endregion

        #region Sprint
        private void HandleSprint()
        {
            bool wantToSprint = inputSprint &&
                                inputMovement.y > 0 && // Only sprint forward
                                !isCrouching &&
                                IsGrounded;

            // Check stamina if available
            if (playerStamina != null)
            {
                wantToSprint = wantToSprint && playerStamina.CanSprint;
            }

            if (wantToSprint && !isSprinting)
            {
                StartSprint();
            }
            else if (!wantToSprint && isSprinting)
            {
                StopSprint();
            }

            // Consume stamina while sprinting
            if (isSprinting && playerStamina != null)
            {
                playerStamina.ConsumeSprint(Time.deltaTime);
            }
        }

        private void StartSprint()
        {
            isSprinting = true;
            OnStartSprint?.Invoke();
        }

        private void StopSprint()
        {
            isSprinting = false;
            OnStopSprint?.Invoke();
        }
        #endregion

        #region Crouch
        private void HandleCrouch()
        {
            if (toggleCrouch)
            {
                if (inputCrouchPressed)
                {
                    inputCrouchPressed = false;
                    if (isCrouching)
                    {
                        TryStandUp();
                    }
                    else
                    {
                        StartCrouch();
                    }
                }
            }
            else
            {
                if (inputCrouch && !isCrouching)
                {
                    StartCrouch();
                }
                else if (!inputCrouch && isCrouching)
                {
                    TryStandUp();
                }
            }
        }

        private void StartCrouch()
        {
            isCrouching = true;
            targetHeight = crouchHeight;

            // Stop sprinting when crouching
            if (isSprinting)
            {
                StopSprint();
            }

            OnStartCrouch?.Invoke();
        }

        private void StopCrouch()
        {
            isCrouching = false;
            targetHeight = standingHeight;
            OnStopCrouch?.Invoke();
        }

        private void TryStandUp()
        {
            // Check if there's room to stand
            float checkDistance = standingHeight - crouchHeight;
            Vector3 checkStart = transform.position + Vector3.up * crouchHeight;

            if (!Physics.SphereCast(checkStart, characterController.radius, Vector3.up, out _, checkDistance, groundMask))
            {
                StopCrouch();
            }
        }

        private void UpdateHeight()
        {
            // Smoothly transition character controller height
            if (Mathf.Abs(characterController.height - targetHeight) > 0.01f)
            {
                float previousHeight = characterController.height;
                characterController.height = Mathf.Lerp(
                    characterController.height,
                    targetHeight,
                    crouchTransitionSpeed * Time.deltaTime
                );

                // Adjust position so feet stay grounded
                float heightDifference = characterController.height - previousHeight;
                transform.position += Vector3.up * (heightDifference / 2f);

                // Update center
                characterController.center = Vector3.up * (characterController.height / 2f);
            }
        }
        #endregion

        #region Final Movement
        private void ApplyFinalMovement()
        {
            // Apply slope adjustment
            if (IsGrounded && IsOnSlope() && !inputJump)
            {
                // Project velocity onto slope
                Vector3 slopeDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
                velocity = slopeDirection * currentSpeed + Vector3.up * velocity.y;
            }

            characterController.Move(velocity * Time.deltaTime);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Teleport the player to a specific position
        /// </summary>
        public void Teleport(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            velocity = Vector3.zero;
            characterController.enabled = true;
        }

        /// <summary>
        /// Add external force to the player (knockback, explosions, etc.)
        /// </summary>
        public void AddForce(Vector3 force)
        {
            velocity += force;
        }

        /// <summary>
        /// Freeze/unfreeze player movement
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            this.enabled = enabled;
            if (!enabled)
            {
                velocity = Vector3.zero;
                currentSpeed = 0;
            }
        }
        #endregion

        #region Debug
        private void OnDrawGizmosSelected()
        {
            // Ground check visualization
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 spherePosition = transform.position + Vector3.up * groundCheckRadius;
            Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
            Gizmos.DrawLine(spherePosition, spherePosition + Vector3.down * groundCheckDistance);
        }
        #endregion
    }
}