using System.Collections;
using UnityEngine;

public class ProximityIndicator : MonoBehaviour
{
    public Bullet assignedBullet;
    private FlightBehavior player;
    private ProxController controller;
    public GameObject visual;
    public float scaleMultiplier = 1f;
    private float minScale = 0.1f;
    private float maxScale = 1f;
    private float maxDistance;
    private float distanceToBullet;
    private Vector3 directionToBullet;
    private float usefulnessTimer;
    private float oldDistance;

    void Awake()
    {
        // Cache references instead of finding them in Start
        player = FindObjectOfType<FlightBehavior>();
        controller = FindObjectOfType<ProxController>();
    }

    void Start()
    {
        // Set parent and initial conditions
        transform.SetParent(player.transform);
        transform.position = player.transform.position;
        maxDistance = controller.maxDistance;
        usefulnessTimer = controller.usefulnessTimer;
        controller.indicatorsActive++;
        StartCoroutine(CheckUsefulness());
    }

    void Update()
    {
        if (assignedBullet != null)
        {
            CheckBulletDistance();
            RotateAndScale();
        }
        else
        {
            visual.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        oldDistance = distanceToBullet;
    }

    public void RotateAndScale()
    {
        if (assignedBullet == null || visual == null) return;

        assignedBullet.tracker = this;

        directionToBullet = assignedBullet.transform.position - transform.position;
        if (directionToBullet != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToBullet);
            transform.rotation = targetRotation;
        }

        distanceToBullet = directionToBullet.magnitude;
        float normalizedDistance = Mathf.Clamp01(distanceToBullet / maxDistance);
        float scaleFactor = Mathf.Lerp(maxScale, minScale, normalizedDistance);
        visual.transform.localScale = Vector3.one * scaleFactor * scaleMultiplier;
    }

    public void CheckBulletDistance()
    {
        if (assignedBullet == null) return;

        directionToBullet = assignedBullet.transform.position - transform.position;
        distanceToBullet = directionToBullet.magnitude;
        if (distanceToBullet > maxDistance || !assignedBullet.IsOffScreen())
        {
            visual.SetActive(false);
        }
        else
        {
            visual.SetActive(true);
        }
    }

    IEnumerator CheckUsefulness()
    {
        while (assignedBullet != null)
        {
            yield return new WaitForSeconds(usefulnessTimer);
            if (distanceToBullet > maxDistance || !assignedBullet)
            {
                ReturnIndicator();
            }
        }
    }

    public void ReturnIndicator()
    {
        if (assignedBullet)
        {
            assignedBullet.tracker = null;
        }

        // Instead of destroying, deactivate and return to pool
        controller.indicatorsActive--;
        visual.SetActive(false);
        controller.proximityIndicators.Remove(this);
        // Optionally return this object to a pool instead of destroying it.
    }
}
