using UnityEngine;

public class MotionShake : MonoBehaviour
{
    public Rigidbody targetRigidbody;
    public float decelerationThreshold = 3f; // Minimum deceleration to trigger shaking
    public float maxShakeIntensity = 0.5f; // Maximum shake offset
    public float shakeFrequency = 20f; // Higher values make it more jittery

    private float previousSpeed;
    private Vector3 shakeOffset;

    void Update()
    {
        if (targetRigidbody == null) return;

        // Get current speed
        float currentSpeed = targetRigidbody.velocity.magnitude;

        // Calculate deceleration
        float deceleration = (previousSpeed - currentSpeed) / Time.deltaTime;

        // Determine shake intensity based on deceleration threshold
        float shakeStrength = 0f;
        if (deceleration > decelerationThreshold)
        {
            // Normalize and scale shake intensity
            shakeStrength = Mathf.Clamp01(deceleration / decelerationThreshold) * maxShakeIntensity;
        }

        // Apply Perlin noise for smooth random movement
        shakeOffset = new Vector3(
            (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f,
            (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f,
            (Mathf.PerlinNoise(Time.time * shakeFrequency, Time.time * shakeFrequency) - 0.5f) * 2f
        ) * shakeStrength;

        // Apply shake to object's position
        transform.localPosition = shakeOffset;

        // Store speed for next frame
        previousSpeed = currentSpeed;
    }
}
