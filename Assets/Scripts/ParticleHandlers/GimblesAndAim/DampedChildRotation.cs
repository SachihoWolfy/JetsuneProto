using UnityEngine;

public class DampedChildRotation : MonoBehaviour
{
    public Transform parent;
    public float damping = 1f;

    private Quaternion targetRotation;

    void Start()
    {
        if (parent == null)
            parent = transform.parent;

        if (parent != null)
            targetRotation = parent.rotation; // Start at parent's world rotation
    }

    void LateUpdate()
    {
        if (parent == null)
            return;

        // Gradually move toward the parent's world rotation
        targetRotation = Quaternion.Slerp(targetRotation, parent.rotation, Time.deltaTime * damping);

        // Apply the smoothed world rotation
        transform.rotation = targetRotation;
    }
}
