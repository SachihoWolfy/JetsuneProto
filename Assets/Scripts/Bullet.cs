using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float bulletSize = 1f;
    public Rigidbody rb;
    public GameObject visual;
    public MeshRenderer mr;
    private int damage;
    private Vector3 oldPosition;
    ProjectileSettings ps;

    public float speedDamage = 40f;
    public float projectileTime = 5f;
    private static FlightBehavior player;
    private static BossMovement boss;

    public bool isHoming = false;
    private bool activeHoming = false;
    public float homingArmTime = 2f;
    public float damping = 3f;
    public float speedDamping = 3f;
    public float homingSpeed = 50f;

    public bool isOscillate = false;
    private bool activeOscillate = false;
    public float oscillateArmTime = 0.5f;
    public bool isPause = false;
    private bool activePause = false;
    public float pauseArmTime = 0.5f;
    public float pauseDisarmTime = 1f;
    public bool isbulletExplode = false;
    private bool activeExplode = false;
    public float explodeArmTime = 1f;
    public float explodeCooldownTime = 0.3f;
    public int explosionAmount = 1;

    public bool isSpdTime;
    private float oldSpeed;
    public float finalSpd;
    public float curSpeed;
    public float initialSpeed;
    private float startTime;
    private Vector3 dir;

    private bool isPooled;
    public ProximityIndicator tracker;
    public bool allyBullet = false;
    private SphereCollider col;
    public bool observed;
    public float dangerWeight;

    public void Initialize(int damage, float spdDamage, bool isAlly = false)
    {
        this.damage = damage;
        speedDamage = spdDamage;
        StartCoroutine(ReturnBulletAfterTime(projectileTime));
        col = GetComponentInChildren<SphereCollider>();
        if (isAlly)
        {
            col.tag = "AllyBullet";
            allyBullet = true;
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        }
        else
        {
            col.tag = "EnemyBullet";
            allyBullet = false;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        observed = false;
    }
    public void Initialize(int damage, float spdDamage, float spdTime, bool isAlly = false)
    {
        this.damage = damage;
        speedDamage = spdDamage;
        finalSpd = spdTime;
        isSpdTime = true;
        startTime = Time.time;
        StartCoroutine(ReturnBulletAfterTime(projectileTime));
        col = GetComponentInChildren<SphereCollider>();
        if (isAlly)
        {
            col.tag = "AllyBullet";
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            allyBullet = true;
        }
        else
        {
            col.tag = "EnemyBullet";
            allyBullet = false;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        observed = false;
    }
    private AllyBulletCollector collector;
    private void Start()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<FlightBehavior>();
        }
        if(boss == null)
        {
            boss = FindObjectOfType<BossMovement>();
        }
        if (FindAnyObjectByType<ExplosionPoolManager>())
        {
            isPooled = true;
        }
        if(collector == null)
        {
            collector = FindObjectOfType<AllyBulletCollector>();
        }
        col = GetComponentInChildren<SphereCollider>();
        oldPosition = transform.position;
    }
    public float GetDistanceToPlayer()
    {
        return (player.transform.position - transform.position).magnitude;
    }
    private void Update()
    {
        // Get the player's forward velocity component
        Vector3 playerForwardVelocity = player.transform.forward * player.curSpeed;

        // Calculate the bullet's velocity relative to the player's forward movement
        Vector3 relativeVelocity = rb.velocity - playerForwardVelocity;

        // Check if the relative velocity has a significant magnitude
        if (relativeVelocity.sqrMagnitude > 0.001f)
        {
            // Set the bullet's rotation based on the calculated relative direction
            visual.transform.rotation = Quaternion.LookRotation(relativeVelocity.normalized);
        }
        else
        {
            // If relative velocity is too small, fallback to the bullet's own velocity direction
            visual.transform.rotation = Quaternion.LookRotation(rb.velocity.normalized);
        }
        if (isSpdTime)
        {
            oldSpeed = curSpeed;
            curSpeed = Mathf.Lerp(curSpeed, finalSpd, ((Time.time-startTime) / (projectileTime*projectileTime*2)));
            if(curSpeed < 0 && oldSpeed > 0)
            {
                dir = -rb.velocity.normalized;
            }
            else
            {
                dir = rb.velocity.normalized;
            }
            rb.velocity = dir * Mathf.Abs(curSpeed);
        }
        if (activeHoming)
        {
            Debug.Log("Activating Homing");
            Vector3 playerPos = player.gameObject.transform.position;
            var lookPos = playerPos - transform.position;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
            rb.velocity = transform.forward * Mathf.Abs(-boss.speed + homingSpeed);
        }
        if (isHoming)
        {
            StartCoroutine(ArmHoming());
            isHoming = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (allyBullet)
        {
            if (other.gameObject.CompareTag("EnemyBullet"))
            {
                if (other.gameObject.transform.parent)
                {
                    if (other.gameObject.transform.parent.TryGetComponent<Bullet>(out Bullet bullet))
                    {
                        if (!other.gameObject.transform.parent.GetComponent<Bullet>().allyBullet)
                        {
                            other.gameObject.transform.parent.GetComponent<Bullet>().ReturnBullet();
                            player.AddScore(10);
                            col.tag = "EnemyBullet";
                            ReturnBullet();
                        }
                    }
                }
                
            }
            if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("BulletIgnore") && !other.gameObject.CompareTag("AllyBullet") && !other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("EnemyBullet"))
                ReturnBullet();
        }
        else
        {
            if (other.gameObject.CompareTag("Player"))
            {
                player.TakeDamage(damage, speedDamage);
            }
            if (!other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("BulletIgnore") && !other.gameObject.CompareTag("AllyBullet") && !other.gameObject.CompareTag("EnemyBullet"))
                ReturnBullet();
        }
    }
    private void FixedUpdate()
    {
        oldPosition = transform.position;
    }

    public IEnumerator ArmHoming()
    {
        Debug.Log("Activating Homing");
        yield return new WaitForSeconds(homingArmTime);
        transform.forward = -transform.forward;
        activeHoming = true;
    }

    public IEnumerator ArmPause()
    {
        yield return new WaitForSeconds(pauseArmTime);
        activePause = true;
        yield return new WaitForSeconds(pauseDisarmTime);
        activePause = false;
    }

    public IEnumerator ArmOscillate()
    {
        yield return new WaitForSeconds(oscillateArmTime);
        activeOscillate = true;
    }

    public IEnumerator ArmExplode()
    {
        yield return new WaitForSeconds(explodeArmTime);
        while(explosionAmount > 0)
        {
            FindAnyObjectByType<AdvProjectileSpawner>().SpawnCirclePattern(transform, ps);
            explosionAmount--;
            yield return new WaitForSeconds(explodeCooldownTime);
        }
    }

    public void ForceHoming()
    {
        isHoming = true;
        activeHoming = true;
    }
    public IEnumerator ReturnBulletAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ReturnBullet();
    }
    public bool IsOffScreen()
    {
        Vector3 bossViewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return bossViewportPos.x < 0 || bossViewportPos.x > 1 || bossViewportPos.y < 0 || bossViewportPos.y > 1;
    }
    public float GetDangerWeight()
    {
        // Get current and previous positions relative to the player
        Vector3 curPosToPlayer = transform.position - player.transform.position;

        // Calculate relative velocity (bullet velocity relative to player velocity)
        Vector3 bulletVelocity = rb.velocity; // Bullet's Rigidbody velocity
        Vector3 playerVelocity = player.rb.velocity; // Player's Rigidbody velocity
        Vector3 relativeVelocity = bulletVelocity - playerVelocity;

        // Determine how much the bullet is moving towards the player
        float relativeApproachSpeed = Vector3.Dot(relativeVelocity, -curPosToPlayer.normalized);

        // Weight based on proximity: closer bullets are exponentially more dangerous
        float proximityWeight = Mathf.Max(0.1f, curPosToPlayer.magnitude); // Prevent division by zero
        float weightedDistanceToPlayer = 1f / (proximityWeight * proximityWeight);

        // Add fine-tuning constants for different factors
        float proximityFactor = 0.3f;      // Influence of proximity
        float approachSpeedFactor = 0.7f;  // Influence of approach speed

        // Player's forward direction (assuming the player is facing the positive Z axis)
        Vector3 playerForward = player.transform.forward;

        // Calculate the direction of the bullet relative to the player
        Vector3 bulletDirection = (transform.position - player.transform.position).normalized;

        // Weight based on whether the bullet is in front of the player (dot product)
        float frontWeight = Mathf.Max(0f, Vector3.Dot(playerForward, bulletDirection));

        // Combine the weights
        float weight = (proximityFactor * weightedDistanceToPlayer) +
                       (approachSpeedFactor * Mathf.Max(0f, relativeApproachSpeed)) +
                       (frontWeight * 0.9f); // Add weight for being in front of the player

        return weight;
    }

    public void ReturnBullet()
    {
        if(tracker != null)
        {
            tracker.ReturnIndicator();
        }
        tracker = null;
        if (observed)
        {
            collector.bullets.Remove(this);
        }
        if (!IsOffScreen())
        {
            GameObject explosion;
            // spawn and orientate it
            if (isPooled) explosion = ExplosionPoolManager.Instance.GetBullet();
            else explosion = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
            explosion.transform.position = transform.position;
            // Set Partical Color
            Material bulletMaterial = mr.material;
            ParticleSystem explosionParticles = explosion.GetComponentInChildren<ParticleSystem>();
            if (bulletMaterial.HasProperty("_Color"))
            {
                Color bulletColor = bulletMaterial.color;
                var mainModule = explosionParticles.main;
                mainModule.startColor = bulletColor;



                // Get all particle systems under the parent, including the parent itself
                ParticleSystem[] particleSystems = explosionParticles.gameObject.GetComponentsInChildren<ParticleSystem>();

                // Loop through each particle system and set the start color
                foreach (ParticleSystem ps in particleSystems)
                {
                    mainModule = ps.main;
                    mainModule.startColor = bulletColor;

                }
                // Get the emissive color from the bullet material
                Color emissiveColor = bulletMaterial.color;

                // Loop through each particle system
                foreach (ParticleSystem ps in particleSystems)
                {
                    // Access the renderer for this particle system
                    ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();

                    // Ensure the renderer has a material
                    if (renderer != null && renderer.material != null)
                    {
                        // Create an instance of the material to avoid affecting the original
                        Material instanceMaterial = renderer.material;

                        // Check if the material supports emissive color changes
                        if (instanceMaterial.HasProperty("_EmissionColor"))
                        {
                            // Enable emission keyword if necessary (for Standard shaders)
                            instanceMaterial.EnableKeyword("_EMISSION");

                            // Set the emissive color
                            instanceMaterial.SetColor("_EmissionColor", emissiveColor);

                            // Assign the instanced material back to the renderer
                            renderer.material = instanceMaterial;
                        }
                    }
                }

            }

            else
            {
                Debug.LogWarning("The bullet material does not have a _Color property.");
            }
        }
        if (FindAnyObjectByType<BulletPoolManager>())
        {
            BulletPoolManager.Instance.ReturnBullet(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
