using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSize = 1f;
    public Rigidbody rb;
    private int damage;
    private Vector3 oldPosition;

    public float speedDamage = 40f;
    public float projectileTime = 5f;
    private FlightBehavior player;

    public void Initialize(int damage, float spdDamage)
    {
        this.damage = damage;
        speedDamage = spdDamage;
        Destroy(gameObject, projectileTime);
    }

    private void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        oldPosition = transform.position;
    }
    private void Update()
    {
        // Shoot a ray between the old and new position to detect collisions
        /*RaycastHit hit;
        if (Physics.Raycast(oldPosition, transform.position - oldPosition, out hit, (transform.position - oldPosition).magnitude))
        {
            // Handle the collision
            //Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                player.TakeDamage(damage, speedDamage);
            }
            if (!hit.collider.gameObject.CompareTag("Enemy"))
                Destroy(gameObject);
        }*/
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
}
