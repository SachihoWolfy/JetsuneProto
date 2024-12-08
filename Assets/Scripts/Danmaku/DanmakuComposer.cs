using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanmakuComposer : MonoBehaviour
{
    public List<DanmakuPattern> patterns = new List<DanmakuPattern>(); // List of all patterns
    private int activePatternIndex = 0;                              // Current active pattern
    private bool isActive = false; // Tracks if firing should continue
    private Coroutine currentCoroutine; // Keep track of the current coroutine

    // This method is called to set the active pattern index externally (via animation event)
    public void SetActivePattern(int index)
    {
        if (index >= 0 && index < patterns.Count)
        {
            

            // Stop all coroutines before switching to the new pattern
            StopAllCoroutines();
            isActive = false; // Deactivate firing before starting new pattern

            activePatternIndex = index; // Set the new pattern
            isActive = true;  // Start firing when the pattern is set

            // Start the coroutine to spawn bullets for the selected pattern
            currentCoroutine = StartCoroutine(SpawnActivePattern());
        }
        else
        {
            Debug.LogError("Invalid pattern index!");
        }
    }

    // This function will spawn the bullets for the active pattern
    private IEnumerator SpawnActivePattern()
    {
        DanmakuPattern activePattern = patterns[activePatternIndex];

        foreach (var layer in activePattern.patternLayers)
        {
            layer.Initialize(); // Initialize layer properties, including cooldowns

            // Wait for the layer's delay before starting to spawn bullets
            yield return new WaitForSeconds(layer.layerDelay);

            if (layer.isBurst && layer.burstCount > 0)
            {
                StartCoroutine(FireBurstLayer(layer)); // Handle burst pattern
            }
            else
            {
                StartCoroutine(FireLayer(layer)); // Handle non-burst pattern
            }
        }
    }

    // Fire bullets for a layer without burst (just regular rate of fire)
    private IEnumerator FireLayer(PatternLayer layer)
    {

        float currentAngle = 0f; // Angle for spiral motion

        while (isActive)
        {
            // Set base position and rotation from the spawnLocation
            Vector3 spawnPosition = layer.spawnLocation.position;
            Quaternion spawnRotation = layer.spawnLocation.rotation;

            // Step 1: Apply SpawnMotion (adjusts spawnPosition only)
            switch (layer.spawnMotion)
            {
                case SpawnMotion.None:
                    break;

                case SpawnMotion.Spiral:
                    // Apply a local-space spiral around the spawnLocation
                    Vector3 localOffset = new Vector3(
                        Mathf.Cos(currentAngle * Mathf.Deg2Rad) * layer.settings.radius,
                        Mathf.Sin(currentAngle * Mathf.Deg2Rad) * layer.settings.radius,
                        0 // Forward direction offset (X-Z plane)
                    );

                    // Transform the offset to world space, relative to spawnLocationÅfs orientation
                    spawnPosition += layer.spawnLocation.TransformDirection(localOffset);
                    currentAngle += layer.motionIncrement; // Increment angle to create the spiral effect
                    break;

                case SpawnMotion.HorizontalWave:
                    spawnPosition += layer.spawnLocation.right * Mathf.Sin(Time.time * layer.waveSpeed) * layer.waveAmplitude;
                    break;

                case SpawnMotion.VerticalWave:
                    spawnPosition += layer.spawnLocation.up * Mathf.Sin(Time.time * layer.waveSpeed) * layer.waveAmplitude;
                    break;

                default:
                    Debug.LogWarning("Unhandled spawn motion type!");
                    break;
            }

            switch (layer.rotateMotion)
            {
                case RotateMotion.None:
                    break;
                case RotateMotion.Circular:
                    // Apply circular rotation regardless of spawnMotion
                    float angleOffset = layer.rotateSpeed * Time.deltaTime;
                    currentAngle += angleOffset;

                    // Apply rotation around both X-axis (up/down) and Z-axis (left/right)
                    Quaternion verticalRotation = Quaternion.Euler(currentAngle, 0, 0); // Rotation around X-axis
                    Quaternion horizontalRotation = Quaternion.Euler(0, currentAngle, 0); // Rotation around Y-axis

                    // Apply both rotations to the spawnRotation
                    spawnRotation *= verticalRotation * horizontalRotation;
                    break;
                case RotateMotion.Spray:
                    // Apply spray rotation
                    float sprayAngleOffset = Mathf.Sin(Time.time * layer.rotateSpeed) * layer.sprayAngle;
                    Quaternion sprayRotation = Quaternion.AngleAxis(sprayAngleOffset, layer.sprayAxis.normalized);
                    spawnRotation *= sprayRotation;
                    break;
                default:
                    Debug.LogWarning("Unhandled rotate motion type!");
                    break;
            }


            // Step 3: Spawn bullet shape with the computed position and rotation
            SpawnShapeAtPosition(spawnPosition, layer, spawnRotation);

            yield return new WaitForSeconds(1 / layer.rateOfFire); // Delay for the next firing cycle
        }
    }





    private void SpawnShapeAtPosition(Vector3 spawnPosition, PatternLayer layer, Quaternion spawnRotation)
    {
        AdvProjectileSpawner spawner = GetComponentInChildren<AdvProjectileSpawner>();

        Transform tempTransform = new GameObject("TempTransform").transform;
        tempTransform.position = spawnPosition;
        tempTransform.rotation = spawnRotation;

        // Spawn the bullet pattern according to the chosen shape
        switch (layer.bulletShape)
        {
            case BulletShape.None:
                break;
            case BulletShape.Line:
                spawner.SpawnLinePattern(tempTransform, layer.settings);
                break;
            case BulletShape.Circle:
                spawner.SpawnCirclePattern(tempTransform, layer.settings);
                break;
            case BulletShape.Hemisphere:
                spawner.SpawnHemispherePattern(tempTransform, layer.settings);
                break;
            case BulletShape.Sphere:
                spawner.SpawnSpherePattern(tempTransform, layer.settings);
                break;
            default:
                Debug.LogError("Unknown bullet shape type!");
                break;
        }

        Destroy(tempTransform.gameObject); // Cleanup
    }



    // Fire bullets for a burst pattern, considering burst cooldown
    private IEnumerator FireBurstLayer(PatternLayer layer)
    {

        int burstFired = 0; // Keep track of how many bursts have been fired

        while (isActive && burstFired < layer.burstCount)
        {

            // Fire bullets for the current burst
            for (int i = 0; i < layer.burstCount; i++)
            {
                SpawnShape(layer); // Spawn one bullet at a time
                yield return new WaitForSeconds(1 / layer.rateOfFire); // Control rate of fire between individual bullets
            }

            burstFired++; // Increase the count of fired bursts

            // Wait for burst cooldown before firing the next burst
            if (burstFired < layer.burstCount)
            {
                yield return new WaitForSeconds(layer.burstCooldown); // Cooldown after each burst
            }
        }
    }

    private void SpawnShape(PatternLayer layer)
    {
        AdvProjectileSpawner spawner = GetComponentInChildren<AdvProjectileSpawner>();

        if (spawner == null)
        {
            Debug.LogError("No AdvProjectileSpawner found!");
            return;
        }

        switch (layer.bulletShape)
        {
            case BulletShape.None:
                break;
            case BulletShape.Line:
                spawner.SpawnLinePattern(layer.spawnLocation, layer.settings);
                break;
            case BulletShape.Circle:
                spawner.SpawnCirclePattern(layer.spawnLocation, layer.settings);
                break;
            case BulletShape.Hemisphere:
                spawner.SpawnHemispherePattern(layer.spawnLocation, layer.settings);
                break;
            case BulletShape.Sphere:
                spawner.SpawnSpherePattern(layer.spawnLocation, layer.settings);
                break;
            default:
                Debug.LogError("Unknown bullet shape type!");
                break;
        }
    }
}













