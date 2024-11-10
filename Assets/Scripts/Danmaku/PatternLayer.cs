using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatternLayer
{
    public BulletShape bulletShape;
    [Header("Bullet Motion")]
    public SpawnMotion spawnMotion = SpawnMotion.None;  // Current spawn motion type
    public float motionIncrement = 10f;  // How much to increment each spawn (degrees)
    public float waveAmplitude = 15f;  // How much the wave moves (for Horizontal/Vertical wave)
    public float waveSpeed = 1f; // Speed of wave movement
    [Header("Rotation")]
    public RotateMotion rotateMotion = RotateMotion.None;
    public float rotateSpeed = 1f;
    public float rotateOffset = 0f; // For Circular
    public float rotateRadius = 15f;
    public bool clockwise = true; // For Circular
    public float sprayAngle = 45f; // For Spray
    public Vector3 sprayAxis = Vector3.up; // For Spray
    [Header("Firing Settings")]
    public Transform spawnLocation;
    public float layerDelay; // Delay before starting the layer's bullets
    public float rateOfFire; // How fast the bullets spawn in this layer
    [Header("Burst")]
    public bool isBurst; // Whether it's a burst pattern
    public int burstCount; // How many bullets in the burst
    public float burstCooldown; // Cooldown between bursts
    public ProjectileSettings settings; // Settings for the bullets

    public void Initialize()
    {
        //removed
    }
    public bool CanFireBurst()
    {
        // Add your cooldown check logic here
        return true; // For now, it always returns true
    }
}






