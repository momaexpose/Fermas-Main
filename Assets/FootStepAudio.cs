using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// Footstep audio system that plays sounds based on surface type and movement state.
    /// Detects surface via physics materials or tags and plays appropriate audio clips.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class FootstepAudio : MonoBehaviour
    {
        #region Timing Settings
        [Header("Step Timing")]
        [Tooltip("Time between footsteps when walking")]
        [SerializeField] private float walkStepInterval = 0.5f;

        [Tooltip("Time between footsteps when sprinting")]
        [SerializeField] private float sprintStepInterval = 0.35f;

        [Tooltip("Time between footsteps when crouching")]
        [SerializeField] private float crouchStepInterval = 0.7f;

        [Tooltip("Minimum player speed to trigger footsteps")]
        [SerializeField] private float minimumSpeedForSteps = 0.5f;
        #endregion

        #region Volume Settings
        [Header("Volume")]
        [Tooltip("Base volume for footsteps")]
        [Range(0f, 1f)]
        [SerializeField] private float baseVolume = 0.5f;

        [Tooltip("Volume multiplier when sprinting")]
        [SerializeField] private float sprintVolumeMultiplier = 1.3f;

        [Tooltip("Volume multiplier when crouching")]
        [SerializeField] private float crouchVolumeMultiplier = 0.5f;

        [Tooltip("Random volume variation")]
        [Range(0f, 0.3f)]
        [SerializeField] private float volumeVariation = 0.1f;
        #endregion

        #region Pitch Settings
        [Header("Pitch")]
        [Tooltip("Base pitch for footsteps")]
        [SerializeField] private float basePitch = 1f;

        [Tooltip("Pitch multiplier when sprinting")]
        [SerializeField] private float sprintPitchMultiplier = 1.1f;

        [Tooltip("Random pitch variation")]
        [Range(0f, 0.3f)]
        [SerializeField] private float pitchVariation = 0.1f;
        #endregion

        #region Surface Detection
        [Header("Surface Detection")]
        [Tooltip("How to detect surface type")]
        [SerializeField] private SurfaceDetectionMethod detectionMethod = SurfaceDetectionMethod.Tag;

        [Tooltip("Ray length for surface detection")]
        [SerializeField] private float raycastDistance = 1.5f;

        [Tooltip("Layers to check for surface detection")]
        [SerializeField] private LayerMask groundLayers = ~0;
        #endregion

        #region Audio Clips
        [Header("Default Footsteps")]
        [Tooltip("Default footstep sounds (used when no surface match)")]
        [SerializeField] private AudioClip[] defaultFootsteps;

        [Header("Surface Audio")]
        [Tooltip("Audio clips for different surface types")]
        [SerializeField] private SurfaceAudioSet[] surfaceAudioSets;

        [Header("Special Sounds")]
        [Tooltip("Sound when landing from a jump")]
        [SerializeField] private AudioClip[] landingSounds;

        [Tooltip("Sound when jumping")]
        [SerializeField] private AudioClip[] jumpSounds;
        #endregion

        #region References
        [Header("References")]
        [SerializeField] private PlayerController playerController;

        private AudioSource audioSource;
        private Dictionary<string, AudioClip[]> surfaceAudioMap;
        #endregion

        #region State
        private float stepTimer;
        private float currentStepInterval;
        private int lastClipIndex = -1;
        private SurfaceType currentSurface = SurfaceType.Default;
        private bool isActive = true;
        #endregion

        #region Enums
        public enum SurfaceDetectionMethod
        {
            Tag,
            PhysicMaterial,
            Layer
        }

        public enum SurfaceType
        {
            Default,
            Concrete,
            Wood,
            Metal,
            Grass,
            Dirt,
            Gravel,
            Water,
            Carpet,
            Tile,
            Sand
        }
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound for player's own footsteps

            // Auto-find player controller
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>();
            }

            // Build surface audio dictionary
            BuildSurfaceAudioMap();
        }

        private void Start()
        {
            // Subscribe to events
            if (playerController != null)
            {
                playerController.OnJump.AddListener(OnJump);
                playerController.OnLand.AddListener(OnLand);
            }
        }

        private void OnDestroy()
        {
            if (playerController != null)
            {
                playerController.OnJump.RemoveListener(OnJump);
                playerController.OnLand.RemoveListener(OnLand);
            }
        }

        private void Update()
        {
            if (!isActive || playerController == null) return;

            UpdateStepInterval();
            UpdateSurfaceDetection();
            HandleFootsteps();
        }
        #endregion

        #region Footstep Logic
        private void UpdateStepInterval()
        {
            if (playerController.IsSprinting)
            {
                currentStepInterval = sprintStepInterval;
            }
            else if (playerController.IsCrouching)
            {
                currentStepInterval = crouchStepInterval;
            }
            else
            {
                currentStepInterval = walkStepInterval;
            }
        }

        private void HandleFootsteps()
        {
            // Only play footsteps when grounded and moving
            if (!playerController.IsGrounded || playerController.HorizontalSpeed < minimumSpeedForSteps)
            {
                stepTimer = currentStepInterval * 0.5f; // Start halfway through when resuming
                return;
            }

            stepTimer += Time.deltaTime;

            if (stepTimer >= currentStepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }

        private void PlayFootstep()
        {
            AudioClip clip = GetFootstepClip();
            if (clip == null) return;

            // Calculate volume
            float volume = baseVolume;
            if (playerController.IsSprinting)
            {
                volume *= sprintVolumeMultiplier;
            }
            else if (playerController.IsCrouching)
            {
                volume *= crouchVolumeMultiplier;
            }
            volume += Random.Range(-volumeVariation, volumeVariation);
            volume = Mathf.Clamp01(volume);

            // Calculate pitch
            float pitch = basePitch;
            if (playerController.IsSprinting)
            {
                pitch *= sprintPitchMultiplier;
            }
            pitch += Random.Range(-pitchVariation, pitchVariation);

            // Play
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, volume);
        }

        private AudioClip GetFootstepClip()
        {
            AudioClip[] clips = GetClipsForCurrentSurface();
            if (clips == null || clips.Length == 0) return null;

            // Avoid playing same clip twice in a row
            int index;
            if (clips.Length == 1)
            {
                index = 0;
            }
            else
            {
                do
                {
                    index = Random.Range(0, clips.Length);
                } while (index == lastClipIndex && clips.Length > 1);
            }

            lastClipIndex = index;
            return clips[index];
        }

        private AudioClip[] GetClipsForCurrentSurface()
        {
            string surfaceKey = currentSurface.ToString();

            if (surfaceAudioMap.TryGetValue(surfaceKey, out AudioClip[] clips) && clips.Length > 0)
            {
                return clips;
            }

            return defaultFootsteps;
        }
        #endregion

        #region Surface Detection
        private void UpdateSurfaceDetection()
        {
            if (!playerController.IsGrounded)
            {
                return;
            }

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayers))
            {
                currentSurface = DetectSurface(hit);
            }
            else
            {
                currentSurface = SurfaceType.Default;
            }
        }

        private SurfaceType DetectSurface(RaycastHit hit)
        {
            switch (detectionMethod)
            {
                case SurfaceDetectionMethod.Tag:
                    return DetectByTag(hit.collider.gameObject);

                case SurfaceDetectionMethod.PhysicMaterial:
                    return DetectByPhysicMaterial(hit.collider);

                case SurfaceDetectionMethod.Layer:
                    return DetectByLayer(hit.collider.gameObject);

                default:
                    return SurfaceType.Default;
            }
        }

        private SurfaceType DetectByTag(GameObject obj)
        {
            string tag = obj.tag;

            // Try to parse tag as surface type
            if (System.Enum.TryParse(tag, true, out SurfaceType surface))
            {
                return surface;
            }

            // Check for common tag variations
            tag = tag.ToLower();
            if (tag.Contains("wood")) return SurfaceType.Wood;
            if (tag.Contains("metal")) return SurfaceType.Metal;
            if (tag.Contains("concrete") || tag.Contains("stone")) return SurfaceType.Concrete;
            if (tag.Contains("grass")) return SurfaceType.Grass;
            if (tag.Contains("dirt") || tag.Contains("mud")) return SurfaceType.Dirt;
            if (tag.Contains("gravel")) return SurfaceType.Gravel;
            if (tag.Contains("water")) return SurfaceType.Water;
            if (tag.Contains("carpet")) return SurfaceType.Carpet;
            if (tag.Contains("tile")) return SurfaceType.Tile;
            if (tag.Contains("sand")) return SurfaceType.Sand;

            return SurfaceType.Default;
        }

        private SurfaceType DetectByPhysicMaterial(Collider collider)
        {
            PhysicsMaterial material = collider.sharedMaterial;
            if (material == null) return SurfaceType.Default;

            string matName = material.name.ToLower();

            if (matName.Contains("wood")) return SurfaceType.Wood;
            if (matName.Contains("metal")) return SurfaceType.Metal;
            if (matName.Contains("concrete") || matName.Contains("stone")) return SurfaceType.Concrete;
            if (matName.Contains("grass")) return SurfaceType.Grass;
            if (matName.Contains("dirt") || matName.Contains("mud")) return SurfaceType.Dirt;
            if (matName.Contains("gravel")) return SurfaceType.Gravel;
            if (matName.Contains("water")) return SurfaceType.Water;
            if (matName.Contains("carpet")) return SurfaceType.Carpet;
            if (matName.Contains("tile")) return SurfaceType.Tile;
            if (matName.Contains("sand")) return SurfaceType.Sand;

            return SurfaceType.Default;
        }

        private SurfaceType DetectByLayer(GameObject obj)
        {
            string layerName = LayerMask.LayerToName(obj.layer).ToLower();

            if (layerName.Contains("wood")) return SurfaceType.Wood;
            if (layerName.Contains("metal")) return SurfaceType.Metal;
            if (layerName.Contains("concrete") || layerName.Contains("stone")) return SurfaceType.Concrete;
            if (layerName.Contains("grass")) return SurfaceType.Grass;
            if (layerName.Contains("dirt") || layerName.Contains("mud")) return SurfaceType.Dirt;
            if (layerName.Contains("gravel")) return SurfaceType.Gravel;
            if (layerName.Contains("water")) return SurfaceType.Water;
            if (layerName.Contains("carpet")) return SurfaceType.Carpet;
            if (layerName.Contains("tile")) return SurfaceType.Tile;
            if (layerName.Contains("sand")) return SurfaceType.Sand;

            return SurfaceType.Default;
        }

        private void BuildSurfaceAudioMap()
        {
            surfaceAudioMap = new Dictionary<string, AudioClip[]>();

            if (surfaceAudioSets != null)
            {
                foreach (var set in surfaceAudioSets)
                {
                    if (set.clips != null && set.clips.Length > 0)
                    {
                        surfaceAudioMap[set.surfaceType.ToString()] = set.clips;
                    }
                }
            }
        }
        #endregion

        #region Special Sounds
        private void OnJump()
        {
            if (jumpSounds != null && jumpSounds.Length > 0)
            {
                AudioClip clip = jumpSounds[Random.Range(0, jumpSounds.Length)];
                audioSource.pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
                audioSource.PlayOneShot(clip, baseVolume);
            }
        }

        private void OnLand()
        {
            if (landingSounds != null && landingSounds.Length > 0)
            {
                // Scale volume by fall speed
                float fallSpeed = Mathf.Abs(playerController.Velocity.y);
                float volume = baseVolume * Mathf.Clamp01((fallSpeed - 2f) / 10f + 0.5f);

                AudioClip clip = landingSounds[Random.Range(0, landingSounds.Length)];
                audioSource.pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
                audioSource.PlayOneShot(clip, volume);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Enable or disable footstep audio
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
        }

        /// <summary>
        /// Set master volume for footsteps
        /// </summary>
        public void SetVolume(float volume)
        {
            baseVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Play a footstep immediately (for animations, etc.)
        /// </summary>
        public void PlayFootstepNow()
        {
            PlayFootstep();
        }

        /// <summary>
        /// Play a specific surface type footstep
        /// </summary>
        public void PlayFootstepForSurface(SurfaceType surface)
        {
            SurfaceType previousSurface = currentSurface;
            currentSurface = surface;
            PlayFootstep();
            currentSurface = previousSurface;
        }

        /// <summary>
        /// Get current detected surface
        /// </summary>
        public SurfaceType GetCurrentSurface()
        {
            return currentSurface;
        }
        #endregion
    }

    /// <summary>
    /// Audio clip set for a specific surface type
    /// </summary>
    [System.Serializable]
    public class SurfaceAudioSet
    {
        public FootstepAudio.SurfaceType surfaceType;
        public AudioClip[] clips;
    }
}