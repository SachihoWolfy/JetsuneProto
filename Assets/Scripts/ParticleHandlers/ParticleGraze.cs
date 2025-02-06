using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGraze : MonoBehaviour
{
    GrazeController grazer;
    FlightBehavior player;
    int damage = 1;
    float speedDamage = 40f;
    public List<ParticleCollisionEvent> collisionEvents;
    public ParticleSystem part;
    private void Start()
    {
        grazer = FindAnyObjectByType<GrazeController>();
        player = FindAnyObjectByType<FlightBehavior>();
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }
    private void OnParticleTrigger()
    {
        grazer.Graze();
    }
    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.TakeDamage(damage, speedDamage);
        }
    }
}
