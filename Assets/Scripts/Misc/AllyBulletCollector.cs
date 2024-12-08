using System.Collections.Generic;
using UnityEngine;

public class AllyBulletCollector : MonoBehaviour
{
    public List<Bullet> bullets = new List<Bullet>();
    public List<Bullet> dangerousBullets = new List<Bullet>();
    private FlightBehavior player;
    public int maxObservableBulletsAtOnce = 3;
    public int maxInDangerList = 20;
    public Bullet mostDangerousBullet;

    private HashSet<Bullet> observedBullets = new HashSet<Bullet>(); // Prevent duplicates efficiently
    private Collider[] detectedColliders = new Collider[100]; // Buffer for overlap checks
    private int overlapCount;

    private const int MAX_COLLIDER_CACHE_SIZE = 100;

    void Start()
    {
        player = FindObjectOfType<FlightBehavior>();

        // Attach to Camera (assuming needed for your gameplay setup)
        Vector3 localLocation = transform.localPosition;
        transform.parent = Camera.main.transform;
        transform.localPosition = localLocation;
    }

    void FixedUpdate()
    {
        CleanupBullets(); // Remove inactive or out-of-trigger bullets
        CheckForNewBulletsInTrigger(); // Ensure no bullets inside the trigger are missed
        EvaluateBullets();

        // Limit the size of the dangerousBullets list
        while (dangerousBullets.Count > maxInDangerList)
        {
            dangerousBullets.RemoveAt(dangerousBullets.Count - 1); // Remove least dangerous
        }

        // Update most dangerous bullet
        mostDangerousBullet = dangerousBullets.Count > 0 ? dangerousBullets[0] : null;
    }

    private void CheckForNewBulletsInTrigger()
    {
        // Use non-allocating version to reduce GC pressure
        overlapCount = Physics.OverlapSphereNonAlloc(transform.position, GetComponent<SphereCollider>().radius, detectedColliders);

        // Loop through detected bullets in the sphere
        for (int i = 0; i < overlapCount; i++)
        {
            Collider other = detectedColliders[i];

            // Skip if not an enemy bullet
            if (!other.CompareTag("EnemyBullet"))
                continue;

            // Get the bullet from the collider
            if (other.transform.parent.gameObject.TryGetComponent<Bullet>(out Bullet bullet) && !bullet.allyBullet)
            {
                // Prioritize bullets in front of the player, but still detect other bullets if necessary
                if (observedBullets.Count < maxObservableBulletsAtOnce || IsBulletInFront(bullet))
                {
                    if (observedBullets.Add(bullet)) // Ensure no duplicates
                    {
                        bullets.Add(bullet);
                    }
                }
            }
        }
    }

    private void CleanupBullets()
    {
        // Use a simple check to avoid unnecessary updates to the list
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = bullets[i];
            // Efficiently check if bullet is still valid and inside the trigger
            if (bullet == null || !bullet.gameObject.activeInHierarchy || !IsBulletInsideTrigger(bullet))
            {
                observedBullets.Remove(bullet);
                bullets.RemoveAt(i);
            }
        }
    }

    private bool IsBulletInsideTrigger(Bullet bullet)
    {
        // Avoid expensive distance check for all bullets by caching trigger radius
        SphereCollider trigger = GetComponent<SphereCollider>();
        float distance = Vector3.Distance(transform.position, bullet.transform.position);
        return distance <= trigger.radius;
    }

    private void EvaluateBullets()
    {
        dangerousBullets.Clear(); // Clear the list each frame to ensure it's properly refilled

        // Go through each bullet in the observable list and check its danger weight
        foreach (var bullet in bullets)
        {
            float weight = bullet.GetDangerWeight(); // Get the danger weight for the bullet
            bullet.dangerWeight = weight; // Assign it to the bullet

            // Add bullet to dangerousBullets if its danger weight is high enough
            if (dangerousBullets.Count < maxInDangerList || weight > dangerousBullets[dangerousBullets.Count - 1].dangerWeight)
            {
                dangerousBullets.Add(bullet); // Add bullet to the list
            }
        }

        // Sort the list after adding all bullets
        dangerousBullets.Sort((b1, b2) => b2.dangerWeight.CompareTo(b1.dangerWeight)); // Descending order by danger weight

        // Limit the list size
        while (dangerousBullets.Count > maxInDangerList)
        {
            dangerousBullets.RemoveAt(dangerousBullets.Count - 1); // Remove least dangerous
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            if (other.transform.parent.gameObject.TryGetComponent<Bullet>(out Bullet bullet) && !bullet.allyBullet)
            {
                // Prioritize bullets in front of the player, but still detect other bullets if necessary
                if (observedBullets.Count < maxObservableBulletsAtOnce || IsBulletInFront(bullet))
                {
                    if (observedBullets.Add(bullet)) // Ensure no duplicates
                    {
                        bullets.Add(bullet);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            if (other.transform.parent.gameObject.TryGetComponent<Bullet>(out Bullet bullet) && observedBullets.Remove(bullet))
            {
                bullets.Remove(bullet);
            }
        }
    }

    private bool IsBulletInFront(Bullet bullet)
    {
        // Check if the bullet is in front of the player
        Vector3 playerForward = player.transform.forward;
        Vector3 bulletDirection = (bullet.transform.position - player.transform.position).normalized;

        // If the dot product is positive, the bullet is in front of the player
        return Vector3.Dot(playerForward, bulletDirection) > 0;
    }
}


