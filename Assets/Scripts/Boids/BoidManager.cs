using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public GameObject boidPrefab;
    public int boidCount = 50;
    public Vector3 bounds = new Vector3(10, 10, 10);
    public float spawnRadius = 5f;

    [HideInInspector] public List<Boid> boids = new List<Boid>();

    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 position = transform.position + Random.insideUnitSphere * spawnRadius;
            GameObject boidObj = Instantiate(boidPrefab, position, Quaternion.identity);
            Boid boid = boidObj.GetComponent<Boid>();
            boid.manager = this;
            boids.Add(boid);
        }
    }
}
