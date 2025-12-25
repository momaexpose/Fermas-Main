using UnityEngine;

/// <summary>
/// Creates thick eerie fog atmosphere.
/// Sets up lighting for a dark, moody storm.
/// </summary>
public class EerieStormAtmosphere : MonoBehaviour
{
    [Header("Fog")]
    public Color fogColor = new Color(0.12f, 0.14f, 0.16f, 1f);
    public float fogDensity = 0.035f;

    [Header("Ambient Light")]
    public Color ambientColor = new Color(0.08f, 0.09f, 0.11f, 1f);

    [Header("Directional Light Settings")]
    public Color sunColor = new Color(0.3f, 0.35f, 0.4f, 1f);
    public float sunIntensity = 0.15f;

    [Header("Sky")]
    public Color skyColor = new Color(0.1f, 0.12f, 0.15f, 1f);

    private Light directionalLight;

    void Start()
    {
        SetupFog();
        SetupLighting();
        SetupSky();
    }

    void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }

    void SetupLighting()
    {
        // Ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;

        // Find or create directional light
        directionalLight = FindObjectOfType<Light>();
        if (directionalLight != null && directionalLight.type == LightType.Directional)
        {
            directionalLight.color = sunColor;
            directionalLight.intensity = sunIntensity;
        }
    }

    void SetupSky()
    {
        // Set sky color (affects reflection and ambient if using skybox)
        RenderSettings.ambientSkyColor = skyColor;

        // If you want solid color sky instead of skybox:
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = skyColor;
    }
}