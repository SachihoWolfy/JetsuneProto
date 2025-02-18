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
    public float colliderSize = 0.007f;
    private ParticleSystem ps;
    private void Start()
    {
        grazer = FindAnyObjectByType<GrazeController>();
        player = FindAnyObjectByType<FlightBehavior>();
        ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        var collider = ps.collision;
        collider.enabled = true;
        collider.radiusScale = colliderSize;
        collider.collidesWith = LayerMask.GetMask("Ground", "Player", "BulletColliders");
        collider.sendCollisionMessages = true;
        var trigger = ps.trigger;
        trigger.enabled = true;
        trigger.inside = ParticleSystemOverlapAction.Callback;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        trigger.enter = ParticleSystemOverlapAction.Ignore;
        trigger.exit = ParticleSystemOverlapAction.Ignore;
        trigger.colliderQueryMode = ParticleSystemColliderQueryMode.One;
        trigger.SetCollider(0, grazer.GetComponent<Collider>());
        trigger.radiusScale = colliderSize;
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
