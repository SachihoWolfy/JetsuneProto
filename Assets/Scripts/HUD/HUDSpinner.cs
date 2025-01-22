using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDSpinner : MonoBehaviour
{
    public RectTransform[] hudElements; // The UI element to rotate
    private FlightBehavior player;   // Reference to the player's script
    public float maxRotationSpeed = 360f; // Maximum rotation speed in degrees per second
    public float smoothTime = 0.2f;  // Time to smooth the rotation
    public float accelerationScale = 10f; // Scale for acceleration contribution
    public float speedScale = 1f; // Scale for speed contribution

    private float previousSpeed = 0f; // To store the player's previous speed
    private float currentRotationSpeed = 0f; // Current rotation speed
    private float targetRotationSpeed = 0f; // Target rotation speed
    private float rotationVelocity = 0f; // For smoothing

    private void Start()
    {
        // Find the player reference if not assigned
        if (player == null)
        {
            player = FindObjectOfType<FlightBehavior>();
            if (player == null)
            {
                Debug.LogError("FlightBehavior not found. Please ensure a FlightBehavior script is in the scene.");
            }
        }
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            // Get the current speed and calculate acceleration
            float currentSpeed = player.curSpeed;
            float acceleration = (currentSpeed - previousSpeed) / Time.fixedDeltaTime; // Rate of change of speed
            previousSpeed = currentSpeed; // Update previous speed for the next frame

            // Determine the target rotation speed
            if (acceleration < 0) // Deceleration
            {
                // Use acceleration exclusively for deceleration
                targetRotationSpeed = Mathf.Clamp(acceleration * accelerationScale, -maxRotationSpeed, maxRotationSpeed);
            }
            else // Acceleration
            {
                // Combine speed and acceleration for acceleration
                float speedContribution = Mathf.Clamp(currentSpeed * speedScale, 0f, maxRotationSpeed);
                float accelerationContribution = Mathf.Clamp(acceleration * accelerationScale, 0f, maxRotationSpeed);
                targetRotationSpeed = Mathf.Clamp(speedContribution + accelerationContribution, 0f, maxRotationSpeed);
            }
        }
    }

    private void Update()
    {
        if (hudElements != null)
        {
            // Smoothly interpolate to the target rotation speed
            currentRotationSpeed = Mathf.SmoothDamp(currentRotationSpeed, targetRotationSpeed, ref rotationVelocity, smoothTime);

            // Apply the rotation to the HUD element
            foreach(RectTransform hudElement in hudElements)
            hudElement.Rotate(Vector3.forward, -currentRotationSpeed * Time.deltaTime); // Adjusted for correct rotation direction
        }
    }
}
