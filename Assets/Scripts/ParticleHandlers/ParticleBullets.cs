using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBullets : MonoBehaviour
{
    [SerializeField]
    ParticleSystem ps;
    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Particle collided with: " + other.name);
        // Example: Apply a damage effect or spawn an explosion
    }
}
