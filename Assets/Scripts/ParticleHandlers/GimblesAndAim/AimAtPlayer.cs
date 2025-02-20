using UnityEngine;

public class AimAtPlayer : MonoBehaviour
{
    private static FlightBehavior player; // Shared across all instances

    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation
    [SerializeField] private Vector3 rotationOffsetEuler; // Allows for rotation adjustments
    private Quaternion rotationOffset;

    void Awake()
    {
        // Find the player only once and store it in the static variable
        if (player == null)
        {
            player = FindObjectOfType<FlightBehavior>();
            if (player == null)
            {
                Debug.LogError("No Sachi found in the scene! (missing FlightBehavior)");
            }
        }

        rotationOffset = Quaternion.Euler(rotationOffsetEuler);
    }

    void Update()
    {
        if (player != null)
        {
            // Get direction to player
            Vector3 direction = player.transform.position - transform.position;
            if (direction != Vector3.zero)
            {
                // Calculate target rotation with offset
                Quaternion targetRotation = Quaternion.LookRotation(direction) * rotationOffset;

                // Apply rotation smoothly
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // Dynamically update rotation offset
    public void SetRotationOffset(Vector3 newOffset)
    {
        rotationOffsetEuler = newOffset;
        rotationOffset = Quaternion.Euler(rotationOffsetEuler);
    }
}
