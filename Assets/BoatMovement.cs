using UnityEngine;

/// <summary>
/// Gentle boat wobble. Very slow, very subtle.
/// </summary>
public class BoatWaveMovement : MonoBehaviour
{
    [Header("Vertical Bobbing")]
    public float bobAmount = 0.15f;        // Meters up/down (very small)
    public float bobSpeed = 0.2f;          // Very slow

    [Header("Tilt - Roll (Side to Side)")]
    public float rollAmount = 1f;          // Degrees (very small)
    public float rollSpeed = 0.15f;        // Very slow

    [Header("Tilt - Pitch (Front to Back)")]
    public float pitchAmount = 0.5f;       // Degrees (very small)
    public float pitchSpeed = 0.18f;       // Very slow

    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        startPosition = transform.position;
        // Random offset so multiple boats don't sync
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float t = Time.time + timeOffset;

        // Very gentle vertical bob
        float bob = Mathf.Sin(t * bobSpeed * Mathf.PI * 2f) * bobAmount;

        // Very gentle roll (side to side tilt)
        float roll = Mathf.Sin(t * rollSpeed * Mathf.PI * 2f) * rollAmount;

        // Very gentle pitch (front to back tilt)
        float pitch = Mathf.Sin(t * pitchSpeed * Mathf.PI * 2f + 1f) * pitchAmount;

        // Apply position (only Y changes)
        transform.position = startPosition + new Vector3(0, bob, 0);

        // Apply rotation
        transform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, roll);
    }

    /// <summary>
    /// Call this if you need to reset the base position
    /// </summary>
    public void SetBasePosition(Vector3 pos)
    {
        startPosition = pos;
    }
}