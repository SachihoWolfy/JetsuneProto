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

    public float distanceSpawnMissile = 100f;
    public bool toggleProjectileAtDistance = false;
    public bool lookAtPlayer = false;

    public int dmg = 1;
    public float speedDmg = 40f;
    private float bulletSpeed = 10f;
    private float missileSpeed = 10f;
    public float speed = 10f;

    private void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        boss = FindAnyObjectByType<BossMovement>();
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
        Vector3 dir = spawnLocation.transform.forward;
        Vector3 pos = spawnLocation.transform.position;
        // spawn and orientate it
        GameObject bulletObj = Instantiate(missilePrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;
        // get missile script
        Missile bulletScript = bulletObj.GetComponent<Missile>();
        // initialize it and set the velocity
        bulletScript.Initialize(dmg, speedDmg, bulletSpeed);
        bulletScript.rb.velocity = -dir * bulletSpeed;
    }

    //Novaborn2 code
    public void SpawnBullet()
    {
        Vector3 dir = spawnLocation.transform.forward;
        Vector3 pos = spawnLocation.transform.position;
        // spawn and orientate it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;
        // get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        // initialize it and set the velocity
        bulletScript.Initialize(dmg, speedDmg);
        bulletScript.rb.velocity = dir * bulletSpeed;
    }
}
