using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxBeep : MonoBehaviour
{
    private FlightBehavior player;
    private BossMovement boss;
    public AudioSource audioSource;
    public float maxDistance;
    public float minDistance;
    public float offset;
    private float distance;
    void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        boss = FindAnyObjectByType<BossMovement>();
    }
    void FixedUpdate()
    {
        CalculateDistance();
        if(distance < maxDistance && player.curSpeed > 40)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            AdjustAudio();
        }
        else
        {
            audioSource.Stop();
        }
    }
    void CalculateDistance()
    {
        distance = Vector3.Distance(player.transform.position, boss.transform.position)-offset;
    }

    void AdjustAudio()
    {
        // Clamp the distance between minDistance and maxDistance
        float clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Inverse Lerp to get a normalized value (0 at maxDistance, 1 at minDistance)
        float t = 1 - Mathf.InverseLerp(maxDistance, minDistance, clampedDistance);

        // Lerp to map the normalized value to the pitch range (0.5 to 1.5)
        audioSource.pitch = Mathf.Lerp(1.5f, 0.7f, t);
    }

}
