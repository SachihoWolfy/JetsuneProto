using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject missilePrefab;
    public Transform spawnLocation;
    public Animator anim;
    private FlightBehavior player;
    private BossMovement boss;
    public AudioSource[] shootSound;

    public Material material1;
    public Material material2;
    private bool useMaterial1 = true;

    public float distanceSpawnMissile = 100f;
    public bool toggleProjectileAtDistance = false;
    public bool lookAtPlayer = false;

    public int dmg = 1;
    public float speedDmg = 40f;
    private float bulletSpeed = 10f;
    private float missileSpeed = 10f;
    public float speed = 10f;

    public float radius = 5f;
    public int numberOfProjectiles = 10;
    public float inwardSpeed = 5f;

    public float hemisphereRadius = 5f;
    public float explosionForce = 10f;
    public int hemisphereProjectileCount = 30;

    public int numberSphereProjectiles = 100;
    public float radiusSphere = 2f;
    private bool isPooled;

    private void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        boss = FindAnyObjectByType<BossMovement>();
        if (FindAnyObjectByType<BulletPoolManager>())
        {
            isPooled = true;
        }
    }

    private void FixedUpdate()
    {
        bulletSpeed = -boss.speed + speed;
        missileSpeed = -boss.speed + speed;
        if (lookAtPlayer)
        {
            transform.LookAt(player.transform.position);
        }
    }

    public void SpawnMissile()
    {
        return;
        /* Vector3 dir = spawnLocation.transform.forward;
        Vector3 pos = spawnLocation.transform.position;
        // spawn and orientate it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.position = pos;
        bulletObj.transform.forward = dir;
        // get missile script
        Missile bulletScript = bulletObj.GetComponent<Missile>();
        // initialize it and set the velocity
        bulletScript.Initialize(dmg, speedDmg, bulletSpeed);
        bulletScript.rb.velocity = -dir * bulletSpeed; */
    }

    //Novaborn2 code
    public void SpawnBullet()
    {
        Vector3 dir = spawnLocation.transform.forward;
        Vector3 pos = spawnLocation.transform.position;
        // spawn and orientate it
        GameObject bulletObj;
        // spawn and orientate it
        if (isPooled) bulletObj = BulletPoolManager.Instance.GetBullet();
        else bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.position = pos;
        bulletObj.transform.forward = dir;
        // get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        // initialize it and set the velocity
        MeshRenderer bulletRenderer = bulletScript.mr;
        bulletRenderer.material = material2;
        bulletScript.Initialize(dmg, speedDmg);
        bulletScript.rb.velocity = dir * bulletSpeed;
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }



    public void SpawnProjectilesInCircle()
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
            GameObject bulletObj;
            // spawn and orientate it
            if (isPooled) bulletObj = BulletPoolManager.Instance.GetBullet();
            else bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
            bulletObj.transform.position = pos;
            bulletObj.transform.forward = dir;

            // Get bullet script
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            //alternate mats
            MeshRenderer bulletRenderer = bulletScript.mr;
            bulletRenderer.material = material2;
            // initialize it and set the velocity
            bulletScript.Initialize(dmg, speedDmg);
            bulletScript.rb.velocity = combinedDir * bulletSpeed;
        }
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

    public void SpawnProjectilesInHemisphere()
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
                GameObject bulletObj;
                // spawn and orientate it
                if (isPooled) bulletObj = BulletPoolManager.Instance.GetBullet();
                else bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
                bulletObj.transform.position = pos;

            // Calculate the direction from the spawn location to the bullet's position
            Vector3 direction = (pos - spawnLocation.position).normalized;
            Vector3 dir = spawnLocation.forward;
            Vector3 inwardDir = -(spawnLocation.position - pos).normalized;
            Vector3 combinedDir = dir + (inwardDir * inwardSpeed);

            // Get bullet script and initialize it
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
                MeshRenderer bulletRenderer = bulletScript.mr;
                bulletRenderer.material = material2;
                // initialize it and set the velocity
                bulletScript.Initialize(dmg, speedDmg);
            bulletScript.rb.velocity = combinedDir * bulletSpeed;
            }
    }

        // Play random shooting sound
        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }
    public void SpawnProjectilesInSphere()
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
            GameObject bulletObj;
            // spawn and orientate it
            if (isPooled) bulletObj = BulletPoolManager.Instance.GetBullet();
            else bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
            bulletObj.transform.position = pos;
            bulletObj.transform.forward = dir;

            // Get bullet script
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
            bulletScript.Initialize(dmg, speedDmg);
            bulletScript.rb.velocity = combinedDir * bulletSpeed;
        }

        int rando = Random.Range(0, shootSound.Length);
        shootSound[rando].Play();
    }

}
