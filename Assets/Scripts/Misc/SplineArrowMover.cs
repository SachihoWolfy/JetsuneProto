using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineArrowMover : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float moveSpeed = 1f; // Speed of the arrows moving
    private float textureOffset = 0f; // Current texture offset

    void Update()
    {
        if (lineRenderer != null && lineRenderer.material != null)
        {
            // Adjust the texture offset over time based on moveSpeed
            textureOffset += moveSpeed * Time.deltaTime;

            // Use Mathf.Repeat to ensure the offset loops seamlessly
            textureOffset = Mathf.Repeat(textureOffset, 1f); // Ensures it stays within 0 to 1 range

            // Apply the texture offset to the material's main texture
            lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(textureOffset, 0f));
        }
    }
}
