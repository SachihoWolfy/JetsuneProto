using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceScale : MonoBehaviour
{
    public float defaultScale = 0.5f;  // Scale at minDistance
    public float awayScale = 1f;       // Scale at maxDistance

    public float maxDistance = 10f;    // Farthest point where scaling applies
    public float minDistance = 2f;     // Closest point where scaling starts

    private FlightBehavior player;

    void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
    }

    void FixedUpdate()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);

        // Normalize the distance within the min-max range
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);

        // Lerp the scale based on the distance factor
        float scale = Mathf.Lerp(defaultScale, awayScale, t);

        transform.localScale = new Vector3(scale, scale, scale);
    }
}
