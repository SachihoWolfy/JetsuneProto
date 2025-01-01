using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float spawnInterval = 0.5f;
    public bool isPooled = false; // Indicates whether bullets are pooled.
    public bool isAlly = false; // Indicates if bullets are ally bullets.
    public Transform customSpawn; // The spawn transform.
    public ProjectileSettings projectileSettings; // The settings for the projectile.
    public AudioSource[] shootSound; // Array of shoot sounds.
    public Material material2; // Alternate material.
    BossMovement boss;
    FlightBehavior player;
    bool debug;
    DebugSpeedSlider debugSpeedSlider;
    public float debugSpeed;
    public bool doCircularSpawnUpsFaceOutward;

    private Layer parentLayer;
    private bool isSpawning = false;

    public BulletShape shape;

    private void Start()
    {
        if (FindFirstObjectByType<DebugSpeedSlider>())
        {
            debugSpeedSlider = FindFirstObjectByType<DebugSpeedSlider>();
            debug = true;
        }
        else
        {
            boss = FindFirstObjectByType<BossMovement>();
            player = FindFirstObjectByType<FlightBehavior>();
        }
        if (FindAnyObjectByType<BulletPoolManager>())
        {
            isPooled = true;
        }
    }
    public void StartSpawning(Layer layer)
    {
        parentLayer = layer;
        isSpawning = true;
        InvokeRepeating(nameof(FireSpawner), 0, spawnInterval);
    }

    public void StopSpawning()
    {
        isSpawning = false;
        CancelInvoke(nameof(FireSpawner));
    }

    public void Spawn(Layer layer)
    {
        parentLayer = layer;
        isSpawning = true;
        FireSpawner();
    }

    private void FireSpawner()
    {
        switch (shape)
        {
            case BulletShape.None:
                SpawnBullet(transform);
                break;
            case BulletShape.Line:
                SpawnLine();
                break;
            case BulletShape.Circle:
                SpawnCircle();
                break;
            case BulletShape.Sphere:
                SpawnSphere();
                break;
            case BulletShape.Hemisphere:
                SpawnHemisphere();
                break;
            default:
                SpawnBullet(transform);
                break;
        }
    }

    private void SpawnLine()
    {
        if (projectileSettings == null || projectileSettings.projectileCount <= 0) return;

        for (int i = 0; i < projectileSettings.projectileCount; i++)
        {
            Transform spawnTransform = Instantiate(customSpawn ? customSpawn : transform);
            spawnTransform.position += spawnTransform.right * (i - (projectileSettings.projectileCount - 1) / 2f) * projectileSettings.radius;
            SpawnBullet(spawnTransform, -i * projectileSettings.inwardSpeed);
            Destroy(spawnTransform.gameObject); // Clean up temporary spawn point
        }
    }

    private void SpawnCircle()
    {
        if (projectileSettings == null || projectileSettings.projectileCount <= 0 || customSpawn == null) return;

        float angleStep = 360f / projectileSettings.projectileCount;

        for (int i = 0; i < projectileSettings.projectileCount; i++)
        {
            // Calculate the angle in radians for the current bullet
            float angle = i * angleStep * Mathf.Deg2Rad;

            // Determine the offset position in the local X-Y plane
            Vector3 localOffset = new Vector3(
                Mathf.Cos(angle) * projectileSettings.radius,
                Mathf.Sin(angle) * projectileSettings.radius,
                0f // No movement along the local Z-axis
            );

            // Convert the local offset to world space using the customSpawn's transform
            Vector3 worldPosition = customSpawn.TransformPoint(localOffset);

            // Create a temporary GameObject to set the orientation
            GameObject tempSpawnPoint = new GameObject("TempSpawnPoint");
            tempSpawnPoint.transform.position = worldPosition;
            tempSpawnPoint.transform.rotation = customSpawn.rotation;

            // Adjust the "up" direction if required
            if (doCircularSpawnUpsFaceOutward)
            {
                tempSpawnPoint.transform.up = (worldPosition - customSpawn.position).normalized;
            }

            // Spawn the bullet
            SpawnBullet(tempSpawnPoint.transform);

            // Clean up the temporary GameObject
            Destroy(tempSpawnPoint);
        }
    }



    private void SpawnSphere()
    {
        if (projectileSettings == null || projectileSettings.projectileCount <= 0 || customSpawn == null) return;

        int count = projectileSettings.projectileCount;
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2; // Approximation of the golden ratio

        for (int i = 0; i < count; i++)
        {
            // Calculate spherical coordinates using the golden angle for even distribution
            float theta = 2 * Mathf.PI * i / goldenRatio; // Longitude
            float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / count); // Latitude

            // Convert spherical coordinates to Cartesian
            Vector3 localOffset = new Vector3(
                Mathf.Sin(phi) * Mathf.Cos(theta),
                Mathf.Sin(phi) * Mathf.Sin(theta),
                Mathf.Cos(phi)
            ) * projectileSettings.radius;

            // Convert local offset to world position
            Vector3 worldPosition = customSpawn.TransformPoint(localOffset);

            // Create a temporary GameObject to set position and orientation
            GameObject tempSpawnPoint = new GameObject("TempSpawnPoint");
            tempSpawnPoint.transform.position = worldPosition;
            tempSpawnPoint.transform.rotation = customSpawn.rotation;

            // Adjust "up" direction to face outward
            tempSpawnPoint.transform.up = (worldPosition - customSpawn.position).normalized;

            // Spawn bullet with inward velocity
            float inwardSpeed = projectileSettings.inwardSpeed; // Negative for inward
            Vector3 velocityOffset = localOffset.normalized * inwardSpeed;
            SpawnBullet(tempSpawnPoint.transform, speedModifier: inwardSpeed);

            // Clean up
            Destroy(tempSpawnPoint);
        }
    }

    private void SpawnHemisphere()
    {
        if (projectileSettings == null || projectileSettings.projectileCount <= 0 || customSpawn == null) return;

        int count = projectileSettings.projectileCount / 2; // Half the bullets for hemisphere
        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;

        for (int i = 0; i < count; i++)
        {
            float theta = 2 * Mathf.PI * i / goldenRatio; // Longitude
            float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / count); // Latitude

            // Restrict to northern hemisphere
            if (phi > Mathf.PI / 2) continue;

            Vector3 localOffset = new Vector3(
                Mathf.Sin(phi) * Mathf.Cos(theta),
                Mathf.Sin(phi) * Mathf.Sin(theta),
                Mathf.Cos(phi)
            ) * projectileSettings.radius;

            Vector3 worldPosition = customSpawn.TransformPoint(localOffset);

            GameObject tempSpawnPoint = new GameObject("TempSpawnPoint");
            tempSpawnPoint.transform.position = worldPosition;
            tempSpawnPoint.transform.rotation = customSpawn.rotation;

            tempSpawnPoint.transform.up = (worldPosition - customSpawn.position).normalized;

            float inwardSpeed = projectileSettings.inwardSpeed;
            Vector3 velocityOffset = localOffset.normalized * inwardSpeed;
            SpawnBullet(tempSpawnPoint.transform, speedModifier: inwardSpeed);

            Destroy(tempSpawnPoint);
        }
    }



    private void SpawnBullet(Transform customSpawn, float speedModifier = 0)
    {
        if (!isSpawning || customSpawn == null || projectileSettings == null) return;

        Vector3 dir = customSpawn.forward;
        Vector3 pos = customSpawn.position;
        GameObject bulletObj;

        // Spawn and orientate it
        if (isPooled)
        {
            bulletObj = BulletPoolManager.Instance.GetBullet();
        }
        else
        {
            bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        }
        bulletObj.transform.position = pos;
        bulletObj.transform.forward = dir;
        bulletObj.transform.up = customSpawn.up;

        // Get bullet script and initialize it
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.allyBullet = isAlly;
        bulletScript.parentLayer = parentLayer;
        bulletScript.transform.parent = null;

        // Alternate materials
        MeshRenderer bulletRenderer = bulletScript.mr;
        if (projectileSettings.mat1 != null)
        {
            bulletRenderer.material = projectileSettings.mat1;
        }
        else
        {
            bulletRenderer.material = material2;
        }

        // Initialize the bullet and set its velocity
        bulletScript.Initialize(projectileSettings.damage, projectileSettings.speedDamage);
        if (debug)
        {
            debugSpeed = debugSpeedSlider.curSpeed;
            bulletScript.rb.velocity = dir * ((projectileSettings.bulletSpeed - debugSpeed) + speedModifier);
            bulletScript.initialSpeed = projectileSettings.bulletSpeed - debugSpeed + speedModifier;
        }
        else
        {
            // Calculate the boss's velocity vector
            Vector3 bossVelocity = boss.transform.forward * boss.speed;

            // Calculate the bullet's final velocity
            float bulletSpeed = projectileSettings.bulletSpeed + speedModifier;
            Vector3 bulletVelocity = dir * bulletSpeed + bossVelocity;

            bulletScript.rb.velocity = bulletVelocity;
            bulletScript.initialSpeed = bulletSpeed;
        }
        bulletScript.spawnedFrom = this;

        // Apply bonus scripts from the parent layer
        if (parentLayer != null)
        {
            foreach (var behavior in parentLayer.bonusBehaviors)
            {
                if (behavior is IBonusScript bonusScript)
                {
                    bulletScript.AddBonusBehavior(bonusScript);
                }
                else
                {
                    Debug.LogWarning("Bonus behavior is not of type IBonusScript");
                }
            }
        }

        // Play a random shoot sound
        if (shootSound.Length > 0)
        {
            int rando = Random.Range(0, shootSound.Length);
            shootSound[rando].Play();
        }

        // Register the bullet with the layer
        parentLayer?.RegisterBullet(bulletScript);
        bulletScript.isSet = true;
    }
}


