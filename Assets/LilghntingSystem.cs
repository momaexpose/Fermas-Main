using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Procedural lightning that illuminates dark clouds.
/// </summary>
public class LightningSystem : MonoBehaviour
{
    [Header("Timing")]
    public float minTimeBetweenStrikes = 4f;
    public float maxTimeBetweenStrikes = 15f;

    [Header("Position")]
    public float minDistance = 80f;
    public float maxDistance = 300f;
    public float strikeHeight = 120f;
    public float boltLength = 150f;

    [Header("Appearance")]
    public Color lightningColor = new Color(0.7f, 0.8f, 1f, 1f);
    public float boltWidth = 3f;
    public int segments = 12;
    public float jaggedness = 20f;

    [Header("Branching")]
    public bool enableBranching = true;
    public float branchChance = 0.25f;
    public int maxBranches = 3;

    [Header("Flash")]
    public float flashIntensity = 2f;

    [Header("Thunder")]
    public AudioClip[] thunderSounds;
    public float thunderVolume = 0.8f;


    private Light flashLight;
    private AudioSource thunderAudio;
    private float nextStrikeTime;
    private Material boltMaterial;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        CreateFlashLight();
        CreateAudioSource();
        CreateBoltMaterial();

        ScheduleNextStrike();
    }

    void CreateFlashLight()
    {
        GameObject lightObj = new GameObject("LightningFlash");
        lightObj.transform.SetParent(transform);
        flashLight = lightObj.AddComponent<Light>();
        flashLight.type = LightType.Directional;
        flashLight.color = lightningColor;
        flashLight.intensity = 0;
    }

    void CreateAudioSource()
    {
        thunderAudio = gameObject.AddComponent<AudioSource>();
        thunderAudio.spatialBlend = 0f;
        thunderAudio.playOnAwake = false;
    }

    void CreateBoltMaterial()
    {
        boltMaterial = new Material(Shader.Find("Sprites/Default"));
        boltMaterial.SetColor("_Color", lightningColor);
    }

    void Update()
    {
        if (Time.time >= nextStrikeTime)
        {
            StartCoroutine(Strike());
            ScheduleNextStrike();
        }
    }

    void ScheduleNextStrike()
    {
        nextStrikeTime = Time.time + Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
    }

    IEnumerator Strike()
    {
        // Random position around camera
        Vector3 strikePos = GetRandomPosition();

        // Create bolt
        List<LineRenderer> bolts = new List<LineRenderer>();
        LineRenderer mainBolt = CreateBolt(strikePos, strikePos + Vector3.down * boltLength);
        bolts.Add(mainBolt);

        // Branches
        if (enableBranching)
        {
            Vector3[] points = new Vector3[mainBolt.positionCount];
            mainBolt.GetPositions(points);

            int branches = 0;
            for (int i = 2; i < points.Length - 2 && branches < maxBranches; i++)
            {
                if (Random.value < branchChance)
                {
                    Vector3 dir = (Random.insideUnitSphere + Vector3.down * 0.5f).normalized;
                    LineRenderer branch = CreateBolt(points[i], points[i] + dir * boltLength * 0.4f, true);
                    bolts.Add(branch);
                    branches++;
                }
            }
        }

        // Flash light
        StartCoroutine(Flash());

    

        // Thunder (delayed by distance)
        float dist = Vector3.Distance(mainCam.transform.position, strikePos);
        float delay = dist / 343f; // Speed of sound
        delay = Mathf.Clamp(delay, 0.3f, 3f);
        StartCoroutine(PlayThunder(delay, dist));

        // Flicker
        yield return new WaitForSeconds(0.08f);
        SetBoltsVisible(bolts, false);
        yield return new WaitForSeconds(0.04f);
        SetBoltsVisible(bolts, true);
        yield return new WaitForSeconds(0.06f);
        SetBoltsVisible(bolts, false);
        yield return new WaitForSeconds(0.03f);
        SetBoltsVisible(bolts, true);
        yield return new WaitForSeconds(0.05f);

        // Cleanup
        foreach (var b in bolts)
        {
            if (b != null) Destroy(b.gameObject);
        }
    }

    Vector3 GetRandomPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(minDistance, maxDistance);

        Vector3 camPos = mainCam.transform.position;

        return new Vector3(
            camPos.x + Mathf.Cos(angle) * dist,
            strikeHeight,
            camPos.z + Mathf.Sin(angle) * dist
        );
    }

    LineRenderer CreateBolt(Vector3 start, Vector3 end, bool isBranch = false)
    {
        GameObject obj = new GameObject("Bolt");
        obj.transform.SetParent(transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.material = boltMaterial;
        lr.startColor = Color.white;
        lr.endColor = lightningColor;
        lr.startWidth = isBranch ? boltWidth * 0.5f : boltWidth;
        lr.endWidth = isBranch ? boltWidth * 0.2f : boltWidth * 0.3f;
        lr.positionCount = isBranch ? segments / 2 : segments;

        // Generate jagged path
        Vector3[] points = new Vector3[lr.positionCount];
        points[0] = start;
        points[points.Length - 1] = end;

        Vector3 step = (end - start) / (points.Length - 1);

        for (int i = 1; i < points.Length - 1; i++)
        {
            Vector3 basePos = start + step * i;
            Vector3 offset = new Vector3(
                Random.Range(-jaggedness, jaggedness),
                Random.Range(-jaggedness * 0.3f, jaggedness * 0.3f),
                Random.Range(-jaggedness, jaggedness)
            );

            float t = Mathf.Min(i, points.Length - 1 - i) / (float)(points.Length / 2);
            offset *= t;

            points[i] = basePos + offset;
        }

        lr.SetPositions(points);
        return lr;
    }

    void SetBoltsVisible(List<LineRenderer> bolts, bool visible)
    {
        foreach (var b in bolts)
        {
            if (b != null) b.enabled = visible;
        }
    }

    IEnumerator Flash()
    {
        // Quick flashes
        flashLight.intensity = flashIntensity;
        yield return new WaitForSeconds(0.05f);
        flashLight.intensity = flashIntensity * 0.3f;
        yield return new WaitForSeconds(0.03f);
        flashLight.intensity = flashIntensity * 0.8f;
        yield return new WaitForSeconds(0.04f);
        flashLight.intensity = 0;
    }

    IEnumerator PlayThunder(float delay, float distance)
    {
        yield return new WaitForSeconds(delay);

        if (thunderSounds != null && thunderSounds.Length > 0)
        {
            AudioClip clip = thunderSounds[Random.Range(0, thunderSounds.Length)];
            float vol = thunderVolume * Mathf.Lerp(1f, 0.4f, distance / maxDistance);
            thunderAudio.PlayOneShot(clip, vol);
        }
    }

    /// <summary>
    /// Manually trigger lightning
    /// </summary>
    public void TriggerLightning()
    {
        StartCoroutine(Strike());
    }
}