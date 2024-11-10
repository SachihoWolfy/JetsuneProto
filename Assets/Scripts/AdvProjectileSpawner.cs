using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvProjectileSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject missilePrefab;
    public Transform spawnLocation;
    public AudioSource[] shootSound;

    // Bullet and missile settings
    public int dmg = 1;
    public float speedDmg = 40f;
    public float bulletSpeed = 10f;
    public float fireRate = 1.0f;
    public float speed = 10f;

    // Toggle options
    public bool lookAtPlayer = false;
    public bool useAlternateMaterials = false;
    public Material material1;
    public Material material2;

    // Circle and sphere pattern settings
    public float radius = 5f;
    public int numberOfProjectiles = 10;
    public float inwardSpeed = 5f;
    public float hemisphereRadius = 5f;
    public int hemisphereProjectileCount = 30;
    public int numberSphereProjectiles = 100;
    public float radiusSphere = 2f;

    private float fireTimer;
    private bool useMaterial1 = true;
    private FlightBehavior player;
    private BossMovement boss;

    // The "Gun trigger"
    public bool isFiring;
    public bool disconnected;
    public bool lead;
    public float distanceToActive = 200f;
    private Vector3 aim;
    public int type = 0;

    [Header("Speed Over Time")]
    public float finalSpeed = 50f;
    public bool isSpdTime = false;

    [Header("Homing")]
    public bool isHoming = false;
    public float homingArmTime = 2f;
    public float damping = 3f;
    public float speedDamping = 3f;

    [Header("Oscillate")]
    public bool isOscillate = false;
    public float oscillateArmTime = 0.5f;

    [Header("Pause")]
    public bool isPause = false;
    public float pauseArmTime = 0.5f;
    public float pauseDisarmTime = 1f;

    [Header("Explode")]
    public bool isbulletExplode = false;
    public float explodeArmTime = 1f;
    public float explodeCooldownTime = 0.3f;
    public int explosionAmount = 1;

    // Parameters to control the sweep effect
    public float sweepAngle = 5f; // Maximum angle offset in degrees for sweeping
    public float sweepSpeed = 1f; // Speed of sweeping motion
    public float inaccuracyAngle = 3f; // Max inaccuracy angle in degrees

    private float sweepTime = 0f; // Tracks the time for the sweep effect

    public int[] patterns;
    public int patternIndex;

    private void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        boss = FindAnyObjectByType<BossMovement>();
    }

    private void Update()
    {
        if (!disconnected) bulletSpeed = -boss.speed + speed;
        else bulletSpeed = speed/(speed/player.curSpeed);
        fireTimer += Time.deltaTime;

        if (lookAtPlayer)
            transform.LookAt(player.transform.position);
        if (lead)
        {
            // Distance between the turret and the player
            Vector3 toPlayer = player.transform.position - transform.position;

            if(toPlayer.magnitude > distanceToActive || player.immunity)
            {
                isFiring = false;
                return;
            }
            else
            {
                isFiring = true;
            }

            // Estimate the player's current speed and direction
            Vector3 playerVelocity = player.rb.velocity;

            // Use iterative approach to refine the target position estimate
            float t = 0;
            const int maxIterations = 5;


            for (int i = 0; i < maxIterations; i++)
            {
                // Estimate the time it would take the projectile to reach the player's estimated position
                t = toPlayer.magnitude / (bulletSpeed);

                // Calculate a new future position based on time of flight
                Vector3 futurePos = player.transform.position + playerVelocity * t;

                // Recalculate the distance and update `toPlayer` for the next iteration
                toPlayer = futurePos - transform.position;
            }

            // Base aim direction
            Vector3 aim = toPlayer.normalized;

            // Calculate sweeping offset angle
            sweepTime += Time.deltaTime * sweepSpeed;
            float sweepOffset = Mathf.Sin(sweepTime) * sweepAngle;

            // Apply sweeping rotation
            Quaternion baseRotation = Quaternion.LookRotation(aim);
            Quaternion sweepRotation = Quaternion.AngleAxis(sweepOffset, Vector3.up);

            // Apply inaccuracy rotation
            float randomAngle = Random.Range(-inaccuracyAngle, inaccuracyAngle);
            Quaternion inaccuracyRotation = Quaternion.AngleAxis(randomAngle, Vector3.up);

            // Combine rotations
            transform.rotation = baseRotation * sweepRotation * inaccuracyRotation;
        }


        if (fireTimer >= fireRate && isFiring)
        {
            fireTimer = 0f;
            spawnPattern(patternIndex);
        }
    }

    void spawnPattern(int value)
    {
        switch (patternIndex)
        {
            case 0:
                isHoming = false;
                SpawnLinePattern();
                break;
            case 1:
                isHoming = false;
                SpawnCirclePattern();
                break;
            case 2:
                isHoming = false;
                SpawnHemispherePattern();
                break;
            case 3:
                isHoming = false;
                SpawnSpherePattern();
                break;
        }
    }

    void SetSettings(Bullet bullet)
    {
        bullet.isHoming = isHoming;
        bullet.isOscillate = isOscillate;
        bullet.isPause = isPause;
        bullet.isbulletExplode = isbulletExplode;

        if (isHoming)
        {
            bullet.homingArmTime = 2f;
            bullet.damping = 3f;
            bullet.speedDamping = 3f;
        }
        //implement other things later.
    }

    // Spawns bullets in a line pattern (single direction)
    private void SpawnLinePattern()
    {
        Vector3 dir = spawnLocation.transform.forward;
        Vector3 pos = spawnLocation.transform.position;
        // spawn and orientate it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;
        // Get bullet script and initialize it
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        //alternate mats
        MeshRenderer bulletRenderer = bulletScript.mr;
        bulletRenderer.material = material2;
        // initialize it and set the velocity
        if (isSpdTime) bulletScript.Initialize(dmg, speedDmg, finalSpeed);
        else bulletScript.Initialize(dmg, speedDmg);
        SetSettings(bulletScript);
        bulletScript.rb.velocity = dir * bulletSpeed;
        bulletScript.initialSpeed = bulletSpeed;
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

    public void SpawnLinePattern(Transform customSpawn, ProjectileSettings projectileSettings)
    {
        Vector3 dir = customSpawn.forward;
        Vector3 pos = customSpawn.position;
        // spawn and orientate it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;
        // Get bullet script and initialize it
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        //alternate mats
        MeshRenderer bulletRenderer = bulletScript.mr;
        if(projectileSettings.mat1 != null) bulletRenderer.material = projectileSettings.mat1;
        else bulletRenderer.material = material2;
        // initialize it and set the velocity
        bulletScript.Initialize(projectileSettings.damage, projectileSettings.speedDamage);
        SetSettings(bulletScript);
        bulletScript.rb.velocity = dir * (projectileSettings.bulletSpeed - boss.speed);
        bulletScript.initialSpeed = (projectileSettings.bulletSpeed - boss.speed);
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

    // Spawns bullets in a circle around the spawn point
    private void SpawnCirclePattern()
    {
        for (int i = 0; i < numberOfProjectiles; i++)
        {

            float angle = (2 * Mathf.PI * i) / numberOfProjectiles;
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            Vector3 localPosition = new Vector3(x, y, 0);
            Vector3 pos = spawnLocation.position + spawnLocation.TransformDirection(localPosition);
            Vector3 dir = spawnLocation.forward;
            Vector3 inwardDir = -(spawnLocation.position - pos).normalized;
            Vector3 combinedDir = dir + (inwardDir * inwardSpeed);
            combinedDir.Normalize();
            //spawn and orientate it
            GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
            bulletObj.transform.forward = dir;

            // Get bullet script and initialize it
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            //alternate mats
            MeshRenderer bulletRenderer = bulletScript.mr;
            bulletRenderer.material = material2;

            // initialize it and set the velocity
            if (isSpdTime) bulletScript.Initialize(dmg, speedDmg, finalSpeed);
            else bulletScript.Initialize(dmg, speedDmg);
            SetSettings(bulletScript);
            bulletScript.rb.velocity = combinedDir * bulletSpeed;
            bulletScript.initialSpeed = bulletSpeed;
            bulletScript.isHoming = isHoming;
        }
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }
    public void SpawnCirclePattern(Transform customSpawn, ProjectileSettings projectileSettings)
    {
        for (int i = 0; i < projectileSettings.projectileCount; i++)
        {

            float angle = (2 * Mathf.PI * i) / projectileSettings.projectileCount;
            float x = projectileSettings.radius * Mathf.Cos(angle);
            float y = projectileSettings.radius * Mathf.Sin(angle);
            Vector3 localPosition = new Vector3(x, y, 0);
            Vector3 pos = customSpawn.position + customSpawn.TransformDirection(localPosition);
            Vector3 dir = customSpawn.forward;
            Vector3 inwardDir = -(customSpawn.position - pos).normalized;
            Vector3 combinedDir = dir + (inwardDir * projectileSettings.inwardSpeed);
            combinedDir.Normalize();
            //spawn and orientate it
            GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
            bulletObj.transform.forward = dir;

            // Get bullet script and initialize it
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            //alternate mats
            MeshRenderer bulletRenderer = bulletScript.mr;
            if (projectileSettings.mat1 != null) bulletRenderer.material = projectileSettings.mat1;
            else bulletRenderer.material = material2;

            // initialize it and set the velocity
            bulletScript.Initialize(projectileSettings.damage, projectileSettings.speedDamage);
            SetSettings(bulletScript);
            bulletScript.rb.velocity = combinedDir * (projectileSettings.bulletSpeed - boss.speed);
            bulletScript.initialSpeed = (projectileSettings.bulletSpeed - boss.speed);
        }
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

    // Spawns bullets in a hemisphere pattern
    private void SpawnHemispherePattern()
    {
        int numLayers = Mathf.CeilToInt(Mathf.Sqrt(hemisphereProjectileCount)); // Determine the number of layers
        int projectilesPerLayer = hemisphereProjectileCount / numLayers; // Projectiles per layer

        for (int layer = 0; layer < numLayers; layer++)
        {
            float theta = Mathf.Acos(1 - (layer + 1) / (float)(numLayers + 1)); // Polar angle

            for (int j = 0; j < projectilesPerLayer; j++)
            {
                // Calculate the azimuthal angle (phi)
                float phi = (j * (2 * Mathf.PI)) / projectilesPerLayer; // Azimuthal angle

                // Calculate the position using spherical coordinates
                float x = hemisphereRadius * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = hemisphereRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = hemisphereRadius * Mathf.Cos(theta); // Assuming z is up

                Vector3 localPosition = new Vector3(x, y, z);

                // Transform the local position into world space based on spawnLocation's orientation
                Vector3 pos = spawnLocation.position + spawnLocation.TransformDirection(localPosition);

                // Instantiate the bullet at the calculated position
                GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
                // Get bullet script and initialize it
                Bullet bulletScript = bulletObj.GetComponent<Bullet>();
                //alternate mats
                MeshRenderer bulletRenderer = bulletScript.mr;
                bulletRenderer.material = material2;

                // Calculate the direction from the spawn location to the bullet's position
                Vector3 direction = (pos - spawnLocation.position).normalized;
                Vector3 dir = spawnLocation.forward;
                Vector3 inwardDir = -(spawnLocation.position - pos).normalized;
                Vector3 combinedDir = dir + (inwardDir * inwardSpeed);

                
                // initialize it and set the velocity
                if (isSpdTime) bulletScript.Initialize(dmg, speedDmg, finalSpeed);
                else bulletScript.Initialize(dmg, speedDmg);
                SetSettings(bulletScript);
                bulletScript.rb.velocity = combinedDir * bulletSpeed;
                bulletScript.initialSpeed = bulletSpeed;
            }
        }
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }
    public void SpawnHemispherePattern(Transform customSpawn, ProjectileSettings projectileSettings)
    {
        int numLayers = Mathf.CeilToInt(Mathf.Sqrt(hemisphereProjectileCount)); // Determine the number of layers
        int projectilesPerLayer = projectileSettings.projectileCount / numLayers; // Projectiles per layer

        for (int layer = 0; layer < numLayers; layer++)
        {
            float theta = Mathf.Acos(1 - (layer + 1) / (float)(numLayers + 1)); // Polar angle

            for (int j = 0; j < projectilesPerLayer; j++)
            {
                // Calculate the azimuthal angle (phi)
                float phi = (j * (2 * Mathf.PI)) / projectilesPerLayer; // Azimuthal angle

                // Calculate the position using spherical coordinates
                float x = projectileSettings.radius * Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = projectileSettings.radius * Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = projectileSettings.radius * Mathf.Cos(theta); // Assuming z is up

                Vector3 localPosition = new Vector3(x, y, z);

                // Transform the local position into world space based on customTransform's orientation
                Vector3 pos = customSpawn.position + customSpawn.TransformDirection(localPosition);

                // Instantiate the bullet at the calculated position
                GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
                // Get bullet script and initialize it
                Bullet bulletScript = bulletObj.GetComponent<Bullet>();
                //alternate mats
                MeshRenderer bulletRenderer = bulletScript.mr;
                if (projectileSettings.mat1 != null) bulletRenderer.material = projectileSettings.mat1;
                else bulletRenderer.material = material2;

                // Calculate the direction from the spawn location to the bullet's position
                Vector3 direction = (pos - customSpawn.position).normalized;
                Vector3 dir = customSpawn.forward;
                Vector3 inwardDir = -(customSpawn.position - pos).normalized;
                Vector3 combinedDir = dir + (inwardDir * projectileSettings.inwardSpeed);


                // initialize it and set the velocity
                bulletScript.Initialize(projectileSettings.damage, projectileSettings.speedDamage);
                SetSettings(bulletScript);
                bulletScript.rb.velocity = combinedDir * (projectileSettings.bulletSpeed - boss.speed);
                bulletScript.initialSpeed = (projectileSettings.bulletSpeed - boss.speed);
            }
        }
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

    // Spawns bullets in a full spherical pattern
    private void SpawnSpherePattern()
    {
        float phiIncrement = Mathf.PI * (3 - Mathf.Sqrt(5));

        for (int i = 0; i < numberSphereProjectiles; i++)
        {
            float y = 1 - (i / (float)(numberSphereProjectiles - 1)) * 2;
            float radiusAtY = Mathf.Sqrt(1 - y * y);
            float theta = phiIncrement * i;

            float x = radiusAtY * Mathf.Cos(theta);
            float z = radiusAtY * Mathf.Sin(theta);

            Vector3 localPosition = new Vector3(x, y, z) * radiusSphere;
            Vector3 pos = spawnLocation.position + spawnLocation.TransformDirection(localPosition);
            Vector3 dir = spawnLocation.forward;
            Vector3 inwardDir = -(spawnLocation.position - pos).normalized;
            Vector3 combinedDir = dir + (inwardDir * inwardSpeed);
            combinedDir.Normalize();

            // Spawn and orientate it
            GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
            bulletObj.transform.forward = dir;

            // Get bullet script and initialize it
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            //alternate mats
            MeshRenderer bulletRenderer = bulletScript.mr;
            if (bulletRenderer != null)
            {
                // Alternate materials
                bulletRenderer.material = useMaterial1 ? material1 : material2;
                useMaterial1 = !useMaterial1; // Toggle the material for the next bullet
            }
            else
            {
                Debug.LogWarning("MeshRenderer not found in bullet prefab's children!");
            }

            // Initialize it and set the velocity
            if (isSpdTime) bulletScript.Initialize(dmg, speedDmg, finalSpeed);
            else bulletScript.Initialize(dmg, speedDmg);
            SetSettings(bulletScript);
            bulletScript.rb.velocity = combinedDir * bulletSpeed;
            bulletScript.initialSpeed = bulletSpeed;
        }
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

    public void SpawnSpherePattern(Transform customSpawn, ProjectileSettings projectileSettings)
    {
        float phiIncrement = Mathf.PI * (3 - Mathf.Sqrt(5));

        for (int i = 0; i < projectileSettings.projectileCount; i++)
        {
            float y = 1 - (i / (float)(projectileSettings.projectileCount - 1)) * 2;
            float radiusAtY = Mathf.Sqrt(1 - y * y);
            float theta = phiIncrement * i;

            float x = radiusAtY * Mathf.Cos(theta);
            float z = radiusAtY * Mathf.Sin(theta);

            Vector3 localPosition = new Vector3(x, y, z) * projectileSettings.radius;
            Vector3 pos = customSpawn.position + customSpawn.TransformDirection(localPosition);
            Vector3 dir = customSpawn.forward;
            Vector3 inwardDir = -(customSpawn.position - pos).normalized;
            Vector3 combinedDir = dir + (inwardDir * projectileSettings.inwardSpeed);
            combinedDir.Normalize();

            // Spawn and orientate it
            GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
            bulletObj.transform.forward = dir;

            // Get bullet script and initialize it
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            //alternate mats
            MeshRenderer bulletRenderer = bulletScript.mr;
            if (bulletRenderer != null)
            {
                // Alternate materials
                if (projectileSettings.mat1 != null) bulletRenderer.material = useMaterial1 ? projectileSettings.mat1 : material2;
                else bulletRenderer.material = useMaterial1 ? material1 : material2;
                useMaterial1 = !useMaterial1; // Toggle the material for the next bullet
            }
            else
            {
                Debug.LogWarning("MeshRenderer not found in bullet prefab's children!");
            }

            // Initialize it and set the velocity
            bulletScript.Initialize(projectileSettings.damage, projectileSettings.speedDamage);
            SetSettings(bulletScript);
            bulletScript.rb.velocity = combinedDir * (projectileSettings.bulletSpeed - boss.speed);
            bulletScript.initialSpeed = (projectileSettings.bulletSpeed - boss.speed);
        }
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

    // Helper method to play a random shoot sound
    private void PlayRandomShootSound()
    {
        int randomIndex = Random.Range(0, shootSound.Length);
        shootSound[randomIndex].Play();
    }
}

