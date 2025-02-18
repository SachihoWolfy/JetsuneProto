using UnityEngine;

public class ScaleOverTime : MonoBehaviour
{
    public float duration = 0.2f; // Time in seconds
    private Vector3 startScale = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 targetScale = Vector3.one;
    private float elapsedTime = 0f;

    public void Activate()
    {
        transform.localScale = startScale;
        elapsedTime = 0f;
    }

    void Update()
    {
        if (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
        }
        else
        {
            transform.localScale = targetScale; // Ensure final scale is exactly 1
        }
    }
}
