using UnityEngine;

[CreateAssetMenu(fileName = "RotateAndLaunchBehavior", menuName = "Bonus Behaviors/RotateAndLaunch")]
public class RotateAndLaunchBehavior : BonusBehavior
{
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private float movementSpeed = 5f;  // Speed of the bullet during rotation
    [SerializeField] private float rotationDuration = 2f; // Duration of rotation in seconds
    [SerializeField] private bool counterClockwise = false;
    [SerializeField] private float behaviorDelay = 0f;

    private Bullet bullet;
    private Layer parentLayer;
    private Transform originalParent;
    private float rotationTimer;
    private float behaviorTimer;
    private float rotationDirection = 1f;

    public override void AttachToBullet(Bullet bullet)
    {
        this.bullet = bullet;

        parentLayer = bullet.parentLayer; // Use the parentLayer from the bullet
        originalParent = null; // Store the original parent
        rotationTimer = 0f;
        behaviorTimer = 0f;

        // Parent the bullet to the layer
        bullet.transform.SetParent(parentLayer.transform, true);
        if (behaviorDelay <= 0)
        {
            bullet.transform.localPosition = bullet.spawnedFrom.transform.localPosition;
            bullet.transform.forward = bullet.spawnedFrom.transform.forward;
            bullet.transform.rotation = bullet.spawnedFrom.transform.rotation;
            // Set the initial velocity for the bullet
            bullet.rb.velocity = bullet.spawnedFrom.transform.forward * (movementSpeed);
        }
        if (counterClockwise)
        {
            rotationDirection = -1f;
        }
    }

    public override void Trigger()
    {
        behaviorTimer += Time.deltaTime;
        if (bullet == null || bullet.rb == null || parentLayer == null || behaviorTimer < behaviorDelay) return;
        // Rotate the bullet's velocity during the rotation duration
        if(bullet.transform.parent == null)
        {
            bullet.transform.SetParent(parentLayer.transform, true);
            if (behaviorDelay <= 0)
            {
                bullet.transform.localPosition = bullet.spawnedFrom.transform.localPosition;
                bullet.transform.forward = bullet.spawnedFrom.transform.forward;
                bullet.transform.rotation = bullet.spawnedFrom.transform.rotation;
                // Set the initial velocity for the bullet
                bullet.rb.velocity = bullet.spawnedFrom.transform.forward * (movementSpeed);
            }
            else
            {
                bullet.rb.velocity = bullet.rb.velocity.normalized * (movementSpeed);
            }
        }
        if (rotationTimer < rotationDuration)
        {
            // Increment the timer
            rotationTimer += Time.deltaTime;
            // Calculate the rotation axis (local "up" of the bullet)
            Vector3 rotationAxis = bullet.transform.up;

            // Rotate the current velocity vector
            Quaternion rotation = Quaternion.AngleAxis(rotationDirection * rotationSpeed * Time.deltaTime, rotationAxis);
            bullet.rb.velocity = rotation * bullet.rb.velocity;
        }
        else
        {
            // Unparent the bullet
            bullet.transform.SetParent(originalParent, true);

            // Rotate the velocity vector 180 degrees
            //Quaternion flipRotation = Quaternion.AngleAxis(180f, bullet.transform.up);
            //bullet.rb.velocity = flipRotation * bullet.rb.velocity;

            // Set the new speed based on the layer's speed
            //float layerSpeed = parentLayer.GetSpeed(bullet);
            bullet.rb.velocity = parentLayer.GetVelocity(bullet);
            // Behavior complete: Detach this behavior from the bullet
            Detach();
        }
    }

    private void Detach()
    {
        bullet = null;
        parentLayer = null;
    }
}
