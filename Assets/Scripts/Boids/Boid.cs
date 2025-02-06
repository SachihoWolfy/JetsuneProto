using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float speed = 5f;
    public float neighborRadius = 3f;
    public float separationDistance = 1f;
    public float alignmentStrength = 1f;
    public float cohesionStrength = 1f;
    public float separationStrength = 1.5f;
    public float boundsStrength = 2f;
    public float obstacleAvoidanceStrength = 5f; // Strength of avoidance
    public float obstacleAvoidanceDistance = 3f; // Distance to detect obstacles

    [HideInInspector] public BoidManager manager;
    private Vector3 velocity;

    void Start()
    {
        velocity = Random.insideUnitSphere * speed;
    }

    void FixedUpdate()
    {
        ApplyFlocking();
        AvoidObstacles();
        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity.normalized; // Orient boid direction
    }

    void ApplyFlocking()
    {
        List<Boid> neighbors = GetNeighbors();
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        if (neighbors.Count > 0)
        {
            foreach (var boid in neighbors)
            {
                Vector3 offset = transform.position - boid.transform.position;
                if (offset.magnitude < separationDistance)
                {
                    separation += offset.normalized / offset.magnitude; // Avoid nearby boids
                }
                alignment += boid.velocity; // Average velocity
                cohesion += boid.transform.position; // Average position
            }

            alignment /= neighbors.Count;
            cohesion = (cohesion / neighbors.Count - transform.position).normalized;
        }

        // Keep boids within bounds
        Vector3 boundsForce = Vector3.zero;
        Vector3 toCenter = manager.transform.position - transform.position;
        if (toCenter.magnitude > manager.bounds.magnitude)
        {
            boundsForce = toCenter.normalized * boundsStrength;
        }

        // Apply forces
        Vector3 acceleration = (separation * separationStrength) +
                               (alignment.normalized * alignmentStrength) +
                               (cohesion * cohesionStrength) +
                               boundsForce;

        velocity = Vector3.Lerp(velocity, velocity + acceleration, Time.deltaTime * speed);
        velocity = velocity.normalized * speed;
    }

    void AvoidObstacles()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity.normalized, out hit, obstacleAvoidanceDistance))
        {
            Vector3 avoidDirection = Vector3.Reflect(velocity.normalized, hit.normal);
            velocity += avoidDirection * obstacleAvoidanceStrength;
            velocity = velocity.normalized * speed;
        }
    }

    List<Boid> GetNeighbors()
    {
        List<Boid> neighbors = new List<Boid>();
        foreach (var boid in manager.boids)
        {
            if (boid != this && Vector3.Distance(transform.position, boid.transform.position) < neighborRadius)
            {
                neighbors.Add(boid);
            }
        }
        return neighbors;
    }
}
