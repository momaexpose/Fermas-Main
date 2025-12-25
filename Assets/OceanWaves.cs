using UnityEngine;

/// <summary>
/// Simulates ocean waves. Call GetWaveHeight() to get water level at any position.
/// </summary>
public class OceanWaves : MonoBehaviour
{
    public static OceanWaves Instance { get; private set; }

    [Header("Wave Settings")]
    public float baseWaterLevel = 0f;

    [Header("Wave 1 - Large Swell")]
    public float wave1Amplitude = 1.5f;
    public float wave1Wavelength = 40f;
    public float wave1Speed = 8f;
    public Vector2 wave1Direction = new Vector2(1f, 0f);

    [Header("Wave 2 - Medium Waves")]
    public float wave2Amplitude = 0.8f;
    public float wave2Wavelength = 20f;
    public float wave2Speed = 12f;
    public Vector2 wave2Direction = new Vector2(0.7f, 0.7f);

    [Header("Wave 3 - Small Chop")]
    public float wave3Amplitude = 0.3f;
    public float wave3Wavelength = 8f;
    public float wave3Speed = 15f;
    public Vector2 wave3Direction = new Vector2(-0.5f, 0.8f);

    [Header("Storm Intensity")]
    [Range(0f, 1f)]
    public float stormIntensity = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Get water height at world position
    /// </summary>
    public float GetWaveHeight(Vector3 worldPosition)
    {
        return GetWaveHeight(worldPosition.x, worldPosition.z);
    }

    /// <summary>
    /// Get water height at x,z coordinates
    /// </summary>
    public float GetWaveHeight(float x, float z)
    {
        float time = Time.time;
        float height = baseWaterLevel;

        // Wave 1
        float w1 = CalculateWave(x, z, time, wave1Amplitude, wave1Wavelength, wave1Speed, wave1Direction);

        // Wave 2
        float w2 = CalculateWave(x, z, time, wave2Amplitude, wave2Wavelength, wave2Speed, wave2Direction);

        // Wave 3
        float w3 = CalculateWave(x, z, time, wave3Amplitude, wave3Wavelength, wave3Speed, wave3Direction);

        height += (w1 + w2 + w3) * stormIntensity;

        return height;
    }

    float CalculateWave(float x, float z, float time, float amplitude, float wavelength, float speed, Vector2 direction)
    {
        direction = direction.normalized;
        float k = 2f * Mathf.PI / wavelength;
        float phase = k * (direction.x * x + direction.y * z) - speed * time;
        return amplitude * Mathf.Sin(phase);
    }

    /// <summary>
    /// Get wave normal at position (for tilting boat)
    /// </summary>
    public Vector3 GetWaveNormal(Vector3 worldPosition, float sampleDistance = 1f)
    {
        float x = worldPosition.x;
        float z = worldPosition.z;

        float heightCenter = GetWaveHeight(x, z);
        float heightRight = GetWaveHeight(x + sampleDistance, z);
        float heightForward = GetWaveHeight(x, z + sampleDistance);

        Vector3 right = new Vector3(sampleDistance, heightRight - heightCenter, 0);
        Vector3 forward = new Vector3(0, heightForward - heightCenter, sampleDistance);

        return Vector3.Cross(forward, right).normalized;
    }
}