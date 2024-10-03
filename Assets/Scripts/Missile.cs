using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    // Watched this Youtube Video: https://www.youtube.com/watch?v=Z6qBeuN-H1M&ab_channel=Tarodev
    public Rigidbody rb;
    public GameObject _explosionPrefab;
    private FlightBehavior player;

    private float speed = 15f;
    public float speedMod = 1f;
    public float rotateSpeed = 95f;

    public float maxDistancePredict = 100f;
    public float minDistancePredict = 5f;
    public float maxTimePrediction = 5f;
    private Vector3 standardPrediction;
    private Vector3 deviatedPrediction;

    public float deviationAmount = 50f;
    public float deviationSpeed = 2f;

    public float speedDamage = 40f;
    public int damage = 1;
    public float projectileTime = 5f;

    public void Initialize(int dmg, float spdDamage, float spd)
    {
        damage = dmg;
        speedDamage = spdDamage;
        speed = spd;
        Destroy(gameObject, projectileTime);
    }
    private void Start()
    {
        player = FindObjectOfType<FlightBehavior>();
        Destroy(gameObject, projectileTime);
    }

    private void FixedUpdate()
    {
        speed = Mathf.Clamp((player.curSpeed/speed)*speedMod + Time.deltaTime, 20, player.curSpeed + 7);
        rb.velocity = transform.forward * speed;
        var leadTimePercentage = Mathf.InverseLerp(minDistancePredict, maxDistancePredict, Vector3.Distance(transform.position, player.transform.position));
        PredictMovement(leadTimePercentage);
        AddDeviation(leadTimePercentage);
        RotateRocket();

    }

    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, maxTimePrediction, leadTimePercentage);
        standardPrediction = player.rb.position + player.rb.velocity * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = new Vector3(Mathf.Cos(Time.time * deviationSpeed), 0, 0);
        var predictionOffset = transform.TransformDirection(deviation) * deviationAmount * leadTimePercentage;
        deviatedPrediction = standardPrediction + predictionOffset;
    }

    private void RotateRocket()
    {
        var heading = deviatedPrediction - transform.position;
        var rotation = Quaternion.LookRotation(heading);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.TakeDamage(damage, speedDamage);
        }
        if (!collision.gameObject.CompareTag("Enemy"))
            Destroy(gameObject);
    }

}
