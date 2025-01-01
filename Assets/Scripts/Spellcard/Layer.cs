using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Layer : MonoBehaviour
{
    public Spawner[] spawners;
    public List<Bullet> activeBullets = new List<Bullet>();
    public BonusBehavior[] bonusBehaviors; 

    private float timer = 0f;   // Shared timer
    public float Timer => timer; // Public getter for the timer

    DebugSpeedSlider debugSpeedSlider;
    bool debug;
    [Header("Layer Delay")]
    public float layerDelay;

    [Header("Burst Settings")]
    public bool burst = false;
    public int bulletsPerBurst = 3;      // Number of bullets in a single burst
    public float waitBetweenBullets = 0.2f; // Seconds per bullet;
    public float burstDelay = 0.5f;     // Delay between bursts
    private bool isBurstFiring = false; // Flag to track if burst firing is active

    [Header("Sequential Firing")]
    public bool sequentialFiring;
    public float waitBetweenSpawn = 0.1f;


    BossMovement boss;

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
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        ActivateLayerBehaviors();
    }
    public float GetSpeed(Spawner spawner)
    {
        float speed;
        if (debug)
        {
            speed = spawner.projectileSettings.bulletSpeed - debugSpeedSlider.curSpeed;
        }
        else
        {
            // Calculate the boss's velocity vector
            Vector3 bossVelocity = boss.transform.forward * boss.speed;

            // Calculate the bullet's final velocity
            float bulletSpeed = spawner.projectileSettings.bulletSpeed;
            Vector3 bulletVelocity = spawner.transform.forward * bulletSpeed + bossVelocity;

            speed = bulletVelocity.magnitude;
        }
        return speed;
    }
    public float GetSpeed(Bullet bullet)
    {
        float speed;
        if (debug)
        {
            speed = bullet.spawnedFrom.projectileSettings.bulletSpeed - debugSpeedSlider.curSpeed;
        }
        else
        {
            // Calculate the boss's velocity vector
            Vector3 bossVelocity = boss.transform.forward * boss.speed;

            // Calculate the bullet's final velocity
            float bulletSpeed = bullet.spawnedFrom.projectileSettings.bulletSpeed;
            Vector3 bulletVelocity = bullet.transform.forward * bulletSpeed + bossVelocity;

            speed = bulletVelocity.magnitude;
        }
        return speed;
    }

    public Vector3 GetVelocity(Bullet bullet)
    {
        float speed;
        Vector3 bulletVelocity;
        if (debug)
        {
            speed = bullet.spawnedFrom.projectileSettings.bulletSpeed - debugSpeedSlider.curSpeed;
            bulletVelocity = bullet.transform.forward * speed;
        }
        else
        {
            // Calculate the boss's velocity vector
            Vector3 bossVelocity = boss.transform.forward * boss.speed;

            // Calculate the bullet's final velocity
            float bulletSpeed = bullet.spawnedFrom.projectileSettings.bulletSpeed;
            bulletVelocity = bullet.rb.velocity + bossVelocity;
        }
        return bulletVelocity;
    }

    public void ResetTimer()
    {
        timer = 0f;
    }
    public void StartLayer()
    {
        if (burst)
        {
            StartBurstFire();
        }
        else
        {
            StartCoroutine(NormalFireCoroutine());
        }
    }

    IEnumerator NormalFireCoroutine()
    {
        if (layerDelay > 0)
        {
            yield return new WaitForSeconds(layerDelay);
        }
        foreach (var spawner in spawners)
        {
            spawner.StartSpawning(this);
            if (sequentialFiring)
            {
                yield return new WaitForSeconds(waitBetweenSpawn);
            }
        }
    }
    public void StopLayer()
    {
        StopBurstFire();

        foreach (var spawner in spawners)
        {
            spawner.StopSpawning();
        }
        ClearActiveBullets();
    }
    private IEnumerator BurstFireCoroutine()
    {
        if (layerDelay > 0)
        {
            yield return new WaitForSeconds(layerDelay);
        }
        isBurstFiring = true;

        while (isBurstFiring)
        {
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                // Spawn bullets from all spawners
                foreach (var spawner in spawners)
                {
                    spawner.Spawn(this);
                    if (sequentialFiring)
                    {
                        yield return new WaitForSeconds(waitBetweenSpawn);
                    }
                }
                yield return new WaitForSeconds(waitBetweenBullets);
                yield return null;
            }

            // Wait for the burst delay before starting the next burst
            yield return new WaitForSeconds(burstDelay);
        }
    }
    public void StartBurstFire()
    {
        if (!isBurstFiring)
        {
            StartCoroutine(BurstFireCoroutine());
        }
    }

    public void StopBurstFire()
    {
        isBurstFiring = false;
        StopCoroutine(BurstFireCoroutine());
    }

    public void ActivateLayerBehaviors()
    {
        foreach (var bullet in activeBullets)
        {
            bullet.TriggerAllBehaviors();
        }
    }

    public void RegisterBullet(Bullet bullet)
    {
        activeBullets.Add(bullet);

        // Apply bonus behaviors to the bullet
        foreach (var behavior in bonusBehaviors)
        {
            var behaviorInstance = Instantiate(behavior);
            bullet.AddBonusBehavior(behaviorInstance);
        }
    }

    public void UnregisterBullet(Bullet bullet)
    {
        activeBullets.Remove(bullet);
    }

    private void ClearActiveBullets()
    {
        activeBullets.Clear();
    }
}

