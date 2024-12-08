using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxController : MonoBehaviour
{
    private FlightBehavior player;
    private CapsuleCollider triggerArea;
    public GameObject indicatorPrefab;
    private AllyBulletCollector bulletCollector;
    public float maxDistance;
    public int MaxTrackedBullets = 30;
    public int indicatorsActive;
    public float usefulnessTimer = 1f; // Timer for checking usefulness globally
    public List<ProximityIndicator> proximityIndicators = new List<ProximityIndicator>();
    private Queue<ProximityIndicator> indicatorPool = new Queue<ProximityIndicator>();
    private float originalMaxDistance;
    private bool toggleOn;

    void Start()
    {
        player = FindObjectOfType<FlightBehavior>();
        triggerArea = GetComponent<CapsuleCollider>();
        bulletCollector = FindObjectOfType<AllyBulletCollector>();
        originalMaxDistance = maxDistance;

        // Pre-instantiate indicators for object pooling
        for (int i = 0; i < MaxTrackedBullets; i++)
        {
            GameObject temp = Instantiate(indicatorPrefab, player.transform.position, Quaternion.identity, player.transform);
            ProximityIndicator indicator = temp.GetComponent<ProximityIndicator>();
            temp.SetActive(false);
            indicatorPool.Enqueue(indicator);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            toggleOn = !toggleOn;
        }
        // Dynamically adjust maxDistance based on active indicators
        AdjustMaxDistance();

        // Match player's position and rotation
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }

    void FixedUpdate()
    {
        if (toggleOn)
        {
            // Reset active indicators
            indicatorsActive = 0;

            // Process dangerous bullets
            int count = 0;
            foreach (var bullet in bulletCollector.dangerousBullets)
            {
                if (count >= MaxTrackedBullets) break;

                if (!IsBulletTracked(bullet))
                {
                    AssignIndicator(bullet);
                    count++;
                }
            }

            // Deactivate unused indicators
            for (int i = indicatorsActive; i < proximityIndicators.Count; i++)
            {
                proximityIndicators[i].gameObject.SetActive(false);
            }
        }
    }

    private void AdjustMaxDistance()
    {
        // Example adjustment: decrease maxDistance as indicatorsActive increases
        float decreaseFactor = Mathf.Clamp01(indicatorsActive / (float)MaxTrackedBullets);
        maxDistance = Mathf.Lerp(originalMaxDistance, originalMaxDistance * 0.5f, decreaseFactor);
    }

    private bool IsBulletTracked(Bullet bullet)
    {
        foreach (var indicator in proximityIndicators)
        {
            if (indicator.assignedBullet == bullet) return true;
        }
        return false;
    }

    private void AssignIndicator(Bullet bullet)
    {
        ProximityIndicator indicator;

        // Reuse indicator from pool or create a new one if necessary
        if (indicatorPool.Count > 0)
        {
            indicator = indicatorPool.Dequeue();
            indicator.gameObject.SetActive(true);
        }
        else
        {
            // Fallback in case pool is exhausted (unlikely with proper pooling)
            GameObject temp = Instantiate(indicatorPrefab, player.transform.position, Quaternion.identity, player.transform);
            indicator = temp.GetComponent<ProximityIndicator>();
        }

        indicator.assignedBullet = bullet;
        proximityIndicators.Add(indicator);
        indicatorsActive++;
    }
}


