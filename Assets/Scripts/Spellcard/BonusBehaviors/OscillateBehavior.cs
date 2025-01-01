using UnityEngine;

[CreateAssetMenu(fileName = "OscillateBehavior", menuName = "Bonus Behaviors/Oscillate")]
public class OscillateBehavior : BonusBehavior
{
    [SerializeField] private float frequency = 2f; // Frequency of the oscillation
    [SerializeField] private float amplitude = 1f; // Amplitude of the oscillation
    private Bullet bullet;
    private Vector3 initialDirection;
    private float elapsedTime;

    public override void AttachToBullet(Bullet bullet)
    {
        this.bullet = bullet;
        initialDirection = bullet.rb.velocity.normalized;
        elapsedTime = 0f;
    }

    public override void Trigger()
    {
        if (bullet == null || bullet.rb == null) return;

        elapsedTime += Time.deltaTime;

        // Calculate the perpendicular oscillation direction
        Vector3 oscillationAxis = Vector3.Cross(initialDirection, bullet.transform.up).normalized;

        // Compute the oscillation offset
        Vector3 oscillation = oscillationAxis * Mathf.Sin(elapsedTime * frequency) * amplitude;

        // Combine the forward velocity with the oscillation
        Vector3 adjustedVelocity = initialDirection * bullet.initialSpeed + oscillation;

        // Ensure the bullet maintains its original speed
        adjustedVelocity = adjustedVelocity.normalized * bullet.initialSpeed;

        bullet.rb.velocity = adjustedVelocity;
    }
}


