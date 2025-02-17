using UnityEngine;

public class BallisticTurret : MonoBehaviour
{
    public Transform target;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public float gravity = 9.81f;
    public float activateDistance = 50f;
    public float cooldownTime = 1.5f;

    private float cooldownTimer = 0f;

    private Vector3 launchVelocity;

    void Update()
    {
        if (!target) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= activateDistance && cooldownTimer <= 0f)
        {
            //Making this a boolean makes this easy to figure out if it can fire or not.
            if (CalculateLeadShot())
            {
                transform.forward = launchVelocity.normalized;

                Fire(launchVelocity);
                cooldownTimer = cooldownTime;
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    bool CalculateLeadShot()
    {
        launchVelocity = Vector3.zero;

        Vector3 targetPos = target.position;
        Vector3 targetVel = target.GetComponent<Rigidbody>()?.velocity ?? Vector3.zero;
        Vector3 turretPos = firePoint.position;
        float projectileSpeed = bulletSpeed;

        Vector3 displacement = targetPos - turretPos;
        float a = 0.5f * gravity;
        float b = projectileSpeed;
        float c = -displacement.y;
        // Quadratic ftw
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0) return false; 
        // math sucks
        float flightTime = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        if (flightTime < 0) flightTime = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

        Vector3 futureTargetPos = targetPos + targetVel * flightTime;
        //now I have you in my sights, time to get a firing solution.
        Vector3 horizontalDisplacement = new Vector3(futureTargetPos.x - turretPos.x, 0, futureTargetPos.z - turretPos.z);
        float horizontalSpeed = horizontalDisplacement.magnitude / flightTime;
        Vector3 horizontalVelocity = horizontalDisplacement.normalized * horizontalSpeed;

        float verticalVelocity = (futureTargetPos.y - turretPos.y) / flightTime + 0.5f * gravity * flightTime;
        // Done. This may be inconsistent speedwise, but it'll hit.

        launchVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
        // If we got here, it's a success!
        return true;
    }

    void Fire(Vector3 velocity)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = velocity;
    }
}
