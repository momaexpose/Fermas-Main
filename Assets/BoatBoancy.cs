using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Realistic boat buoyancy. Uses multiple float points to calculate forces.
/// Includes splash effects and sounds.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BoatBuoyancy : MonoBehaviour
{
    [Header("Buoyancy")]
    public float buoyancyForce = 20f;
    public float waterDrag = 3f;
    public float waterAngularDrag = 2f;

    [Header("Float Points")]
    public Transform[] floatPoints;
    public float floatPointDepth = 1f;

    [Header("Stability")]
    public float stabilityForce = 2f;
    public float maxTiltAngle = 25f;

    [Header("Splash Effects")]
    public ParticleSystem splashParticles;
    public float splashThreshold = 2f;
    public float splashCooldown = 0.3f;

    [Header("Audio - Splashes")]
    public AudioSource splashAudioSource;
    public AudioClip[] splashSounds;
    [Range(0f, 1f)]
    public float splashVolume = 0.6f;

    [Header("Audio - Creaking")]
    public AudioSource creakAudioSource;
    public AudioClip[] creakSounds;
    [Range(0f, 1f)]
    public float creakVolume = 0.3f;
    public float creakThreshold = 5f;

    [Header("Audio - Ambient Ocean")]
    public AudioSource ambientAudioSource;
    public AudioClip oceanAmbientLoop;
    [Range(0f, 1f)]
    public float ambientVolume = 0.4f;

    [Header("Audio - Waves")]
    public AudioSource waveAudioSource;
    public AudioClip[] waveSounds;
    [Range(0f, 1f)]
    public float waveVolume = 0.5f;
    public float waveInterval = 3f;

    private Rigidbody rb;
    private OceanWaves oceanWaves;
    private float[] lastFloatHeights;
    private float[] splashCooldowns;
    private float lastCreakTime;
    private float lastWaveTime;
    private float lastAngularVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;

        oceanWaves = OceanWaves.Instance;
        if (oceanWaves == null)
        {
            oceanWaves = FindObjectOfType<OceanWaves>();
        }

        // Auto-create float points if not assigned
        if (floatPoints == null || floatPoints.Length == 0)
        {
            CreateDefaultFloatPoints();
        }

        lastFloatHeights = new float[floatPoints.Length];
        splashCooldowns = new float[floatPoints.Length];

        SetupAudio();
        CreateSplashParticles();
    }

    void CreateDefaultFloatPoints()
    {
        // Create 4 corner float points
        floatPoints = new Transform[4];

        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-3f, 0, 5f),   // Front left
            new Vector3(3f, 0, 5f),    // Front right
            new Vector3(-3f, 0, -5f),  // Back left
            new Vector3(3f, 0, -5f)    // Back right
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject fp = new GameObject("FloatPoint_" + i);
            fp.transform.SetParent(transform);
            fp.transform.localPosition = offsets[i];
            floatPoints[i] = fp.transform;
        }
    }

    void SetupAudio()
    {
        // Splash audio
        if (splashAudioSource == null)
        {
            GameObject obj = new GameObject("SplashAudio");
            obj.transform.SetParent(transform);
            splashAudioSource = obj.AddComponent<AudioSource>();
            splashAudioSource.spatialBlend = 1f;
            splashAudioSource.playOnAwake = false;
        }

        // Creak audio
        if (creakAudioSource == null)
        {
            GameObject obj = new GameObject("CreakAudio");
            obj.transform.SetParent(transform);
            creakAudioSource = obj.AddComponent<AudioSource>();
            creakAudioSource.spatialBlend = 1f;
            creakAudioSource.playOnAwake = false;
        }

        // Ambient ocean audio
        if (ambientAudioSource == null)
        {
            GameObject obj = new GameObject("AmbientOceanAudio");
            obj.transform.SetParent(transform);
            ambientAudioSource = obj.AddComponent<AudioSource>();
            ambientAudioSource.spatialBlend = 0f; // 2D sound
            ambientAudioSource.loop = true;
            ambientAudioSource.playOnAwake = false;
        }

        if (oceanAmbientLoop != null)
        {
            ambientAudioSource.clip = oceanAmbientLoop;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.Play();
        }

        // Wave audio
        if (waveAudioSource == null)
        {
            GameObject obj = new GameObject("WaveAudio");
            obj.transform.SetParent(transform);
            waveAudioSource = obj.AddComponent<AudioSource>();
            waveAudioSource.spatialBlend = 0.5f;
            waveAudioSource.playOnAwake = false;
        }
    }

    void CreateSplashParticles()
    {
        if (splashParticles != null) return;

        GameObject splashObj = new GameObject("BoatSplash");
        splashObj.transform.SetParent(transform);
        splashObj.transform.localPosition = Vector3.zero;

        splashParticles = splashObj.AddComponent<ParticleSystem>();
        splashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = splashParticles.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.8f, 0.85f, 0.9f, 0.7f);
        main.gravityModifier = 1f;
        main.maxParticles = 100;
        main.playOnAwake = false;

        var emission = splashParticles.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });
        emission.rateOverTime = 0;

        var shape = splashParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.5f;

        var sizeOverLife = splashParticles.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = splashParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", new Color(0.8f, 0.85f, 0.9f, 0.7f));
    }

    void FixedUpdate()
    {
        if (oceanWaves == null) return;

        ApplyBuoyancy();
        ApplyWaterDrag();
        ApplyStability();
        CheckForSplashes();
        UpdateCooldowns();
    }

    void Update()
    {
        CheckForCreaking();
        PlayRandomWaves();
    }

    void ApplyBuoyancy()
    {
        for (int i = 0; i < floatPoints.Length; i++)
        {
            Transform fp = floatPoints[i];
            Vector3 worldPos = fp.position;

            float waterHeight = oceanWaves.GetWaveHeight(worldPos);
            float depth = waterHeight - worldPos.y;

            if (depth > 0)
            {
                // Submerged - apply upward force
                float forceMagnitude = buoyancyForce * Mathf.Clamp01(depth / floatPointDepth);
                Vector3 force = Vector3.up * forceMagnitude;
                rb.AddForceAtPosition(force, worldPos, ForceMode.Force);
            }

            // Store height for splash detection
            lastFloatHeights[i] = depth;
        }
    }

    void ApplyWaterDrag()
    {
        // Check if any point is in water
        bool inWater = false;
        foreach (Transform fp in floatPoints)
        {
            float waterHeight = oceanWaves.GetWaveHeight(fp.position);
            if (fp.position.y < waterHeight)
            {
                inWater = true;
                break;
            }
        }

        if (inWater)
        {
            // Apply extra drag when in water
            rb.linearDamping = waterDrag;
            rb.angularDamping = waterAngularDrag;
        }
        else
        {
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.1f;
        }
    }

    void ApplyStability()
    {
        // Get target up direction from wave normal at boat center
        Vector3 waveNormal = oceanWaves.GetWaveNormal(transform.position, 3f);

        // Blend between wave normal and world up for stability
        Vector3 targetUp = Vector3.Lerp(Vector3.up, waveNormal, 0.5f).normalized;

        // Calculate torque to align boat
        Vector3 currentUp = transform.up;
        Vector3 torqueAxis = Vector3.Cross(currentUp, targetUp);
        float angle = Vector3.Angle(currentUp, targetUp);

        if (angle > 0.1f)
        {
            Vector3 torque = torqueAxis * angle * stabilityForce;
            rb.AddTorque(torque, ForceMode.Force);
        }
    }

    void CheckForSplashes()
    {
        for (int i = 0; i < floatPoints.Length; i++)
        {
            if (splashCooldowns[i] > 0) continue;

            Transform fp = floatPoints[i];
            float waterHeight = oceanWaves.GetWaveHeight(fp.position);

            // Check velocity of this point
            Vector3 pointVelocity = rb.GetPointVelocity(fp.position);
            float verticalSpeed = Mathf.Abs(pointVelocity.y);

            // Check if hitting water surface with speed
            bool atSurface = Mathf.Abs(fp.position.y - waterHeight) < 0.5f;

            if (atSurface && verticalSpeed > splashThreshold)
            {
                SpawnSplash(fp.position, verticalSpeed);
                splashCooldowns[i] = splashCooldown;
            }
        }
    }

    void SpawnSplash(Vector3 position, float intensity)
    {
        if (splashParticles != null)
        {
            splashParticles.transform.position = position;

            var main = splashParticles.main;
            main.startSpeed = 3f + intensity;

            var burst = new ParticleSystem.Burst(0f, (short)(20 + intensity * 10));
            var emission = splashParticles.emission;
            emission.SetBurst(0, burst);

            splashParticles.Play();
        }

        PlaySplashSound(intensity);
    }

    void PlaySplashSound(float intensity)
    {
        if (splashSounds != null && splashSounds.Length > 0 && splashAudioSource != null)
        {
            AudioClip clip = splashSounds[Random.Range(0, splashSounds.Length)];
            float volume = splashVolume * Mathf.Clamp01(intensity / 5f);
            splashAudioSource.pitch = Random.Range(0.9f, 1.1f);
            splashAudioSource.PlayOneShot(clip, volume);
        }
    }

    void CheckForCreaking()
    {
        if (creakSounds == null || creakSounds.Length == 0) return;
        if (Time.time - lastCreakTime < 2f) return;

        // Creak when angular velocity changes (boat straining)
        float angularChange = Mathf.Abs(rb.angularVelocity.magnitude - lastAngularVelocity);
        lastAngularVelocity = rb.angularVelocity.magnitude;

        if (angularChange > creakThreshold * 0.01f)
        {
            PlayCreakSound();
            lastCreakTime = Time.time;
        }
    }

    void PlayCreakSound()
    {
        if (creakAudioSource != null && creakSounds.Length > 0)
        {
            AudioClip clip = creakSounds[Random.Range(0, creakSounds.Length)];
            creakAudioSource.pitch = Random.Range(0.8f, 1.2f);
            creakAudioSource.PlayOneShot(clip, creakVolume);
        }
    }

    void PlayRandomWaves()
    {
        if (waveSounds == null || waveSounds.Length == 0) return;
        if (waveAudioSource == null) return;

        if (Time.time - lastWaveTime > waveInterval)
        {
            AudioClip clip = waveSounds[Random.Range(0, waveSounds.Length)];
            waveAudioSource.pitch = Random.Range(0.9f, 1.1f);
            waveAudioSource.PlayOneShot(clip, waveVolume);

            lastWaveTime = Time.time + Random.Range(-1f, 1f);
        }
    }

    void UpdateCooldowns()
    {
        for (int i = 0; i < splashCooldowns.Length; i++)
        {
            if (splashCooldowns[i] > 0)
            {
                splashCooldowns[i] -= Time.fixedDeltaTime;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw float points
        if (floatPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform fp in floatPoints)
            {
                if (fp != null)
                {
                    Gizmos.DrawWireSphere(fp.position, 0.3f);
                }
            }
        }
    }
}