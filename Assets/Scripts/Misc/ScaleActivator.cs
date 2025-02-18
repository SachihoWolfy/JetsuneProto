using UnityEngine;

public class ScaleActivator : MonoBehaviour
{
    public float decayRate = 0.1f;     // Speed of shrinking over time
    public float growthSpeed = 5f;     // Speed of growing when activated
    public float decaySpeed = 1f;      // Speed of decaying over time (slower than growth)
    public float growthAmount = 0.2f;  // How much scale increases per activation
    public float minScale = 0.001f;     // Minimum scale
    public float maxScale = 1f;        // Maximum scale

    private float targetScale = 0.01f; // Desired scale we move towards
    private float currentScale = 0.001f; // The actual scale applied

    void Start()
    {
        transform.localScale = Vector3.one * currentScale;
    }

    void Update()
    {
        // Gradually decrease target scale (decay)
        targetScale -= decayRate * Time.deltaTime;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        // Smoothly transition current scale towards target scale (Decay is slower than Growth)
        if (currentScale < targetScale)
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * growthSpeed);
        else
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * decaySpeed);

        transform.localScale = Vector3.one * currentScale;
    }

    public void Activate()
    {
        // Increase target scale
        targetScale += growthAmount;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
    }
}
