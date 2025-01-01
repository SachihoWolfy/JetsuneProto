using UnityEngine;

[CreateAssetMenu(fileName = "StopAndGoBehavior", menuName = "Bonus Behaviors/StopAndGo")]
public class StopAndGoBehavior : BonusBehavior
{
    [SerializeField] private float stopDuration = 1f;  // Duration of the stop phase
    [SerializeField] private float goDuration = 2f;    // Duration of the go phase
    private Layer parentLayer;
    private Bullet bullet;
    private Transform originalParent;
    private bool isStopped;
    private Material originalMaterial;
    private Material grayscaleEmissiveMaterial;

    public override void AttachToBullet(Bullet bullet)
    {
        this.bullet = bullet;
        parentLayer = bullet.parentLayer;
        originalParent = bullet.transform.parent; // Store the original parent
        originalMaterial = bullet.mr.material;    // Store the original material
        isStopped = false;
    }

    public override void Trigger()
    {
        if (bullet == null || bullet.rb == null || parentLayer == null) return;

        float timer = parentLayer.Timer;
        float cycleDuration = stopDuration + goDuration;

        // Determine the current phase: stop or go
        bool shouldStop = timer % cycleDuration < stopDuration;

        if (shouldStop && !isStopped)
        {
            // Enter the stop phase
            FreezeBullet();
        }
        else if (!shouldStop && isStopped)
        {
            // Exit the stop phase
            ResumeBullet();
        }
    }

    private void FreezeBullet()
    {
        // Parent the bullet to the layer and stop its motion
        bullet.rb.velocity = Vector3.zero;
        bullet.transform.SetParent(parentLayer.transform, true);
        isStopped = true;

        // Create grayscale emissive material if it doesn't exist
        if (grayscaleEmissiveMaterial == null)
        {
            grayscaleEmissiveMaterial = CreateGrayscaleEmissiveMaterial(originalMaterial);
        }

        // Assign the grayscale emissive material
        bullet.mr.material = grayscaleEmissiveMaterial;
    }

    private void ResumeBullet()
    {
        // Unparent the bullet immediately and restore its motion
        bullet.transform.SetParent(originalParent, true);
        bullet.rb.velocity = bullet.transform.forward * bullet.initialSpeed;
        isStopped = false;

        // Restore the original material
        bullet.mr.material = originalMaterial;
    }

    private Material CreateGrayscaleEmissiveMaterial(Material original)
    {
        // Create a copy of the original material
        Material grayscaleMaterial = new Material(original);

        // Convert the original material's color to grayscale by averaging its RGB values
        Color originalColor = grayscaleMaterial.color;
        float grayValue = (originalColor.r + originalColor.g + originalColor.b) / 3f;

        // Apply the grayscale color to the material
        grayscaleMaterial.color = new Color(grayValue, grayValue, grayValue);

        // Adjust emissive color (tone down the glow for more natural lighting)
        Color emissiveColor = grayscaleMaterial.color * 1.2f; // Adjust the factor for a more subtle glow
        emissiveColor = Color.Lerp(grayscaleMaterial.color, emissiveColor, 0.5f); // Blend to avoid too strong a glow

        // Set the emissive color
        grayscaleMaterial.SetColor("_EmissionColor", emissiveColor);

        // Enable the emissive effect
        grayscaleMaterial.EnableKeyword("_EMISSION");

        return grayscaleMaterial;
    }
}
