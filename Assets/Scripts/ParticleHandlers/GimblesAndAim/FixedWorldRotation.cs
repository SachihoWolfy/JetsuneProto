using UnityEngine;

public class FixedWorldRotation : MonoBehaviour
{
    private Quaternion initialRotation;
    public Vector3 rotationOffsetEuler; // Editable in the Inspector
    private Quaternion rotationOffset;

    void Start()
    {
        // Store the initial world rotation
        initialRotation = transform.rotation;
        rotationOffset = Quaternion.Euler(rotationOffsetEuler);
    }

    void LateUpdate()
    {
        // Maintain the adjusted world rotation
        transform.rotation = initialRotation * rotationOffset;
    }

    // Call this to update the rotation offset dynamically
    public void SetRotationOffset(Vector3 newOffset)
    {
        rotationOffsetEuler = newOffset;
        rotationOffset = Quaternion.Euler(rotationOffsetEuler);
    }
}
