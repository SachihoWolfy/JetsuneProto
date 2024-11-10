using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSize = 1f;
    public Rigidbody rb;
    public GameObject visual;
    public MeshRenderer mr;
    private int damage;
    private Vector3 oldPosition;
    ProjectileSettings ps;

    public float speedDamage = 40f;
    public float projectileTime = 5f;
    private FlightBehavior player;
    private BossMovement boss;

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

    public void Initialize(int damage, float spdDamage)
    {
        this.damage = damage;
        speedDamage = spdDamage;
        Destroy(gameObject, projectileTime);
    }
    public void Initialize(int damage, float spdDamage, float spdTime)
    {
        this.damage = damage;
        speedDamage = spdDamage;
        finalSpd = spdTime;
        isSpdTime = true;
        startTime = Time.time;
        Destroy(gameObject, projectileTime);
    }

    private void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        boss = FindObjectOfType<BossMovement>();

        oldPosition = transform.position;
        if (isHoming)
        {
            StartCoroutine(ArmHoming());
        }
        if (isbulletExplode)
        {
            StartCoroutine(ArmExplode());
        }
        if (isPause)
        {
            StartCoroutine(ArmPause());
        }
        if (isOscillate)
        {
            StartCoroutine(ArmOscillate());
        }
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
        if (isbulletExplode)
        {
            StartCoroutine(ArmExplode());
            isbulletExplode = false;
        }
        if (isPause)
        {
            StartCoroutine(ArmPause());
            isPause = false;
        }
        if (isOscillate)
        {
            StartCoroutine(ArmOscillate());
            isOscillate = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.TakeDamage(damage, speedDamage);
        }
        if (!other.gameObject.CompareTag("Enemy"))
            Destroy(gameObject);
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
}
