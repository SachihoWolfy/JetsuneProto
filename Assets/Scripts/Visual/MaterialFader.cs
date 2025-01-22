using System.Collections.Generic;
using UnityEngine;

public class MaterialFader : MonoBehaviour
{
    public Renderer targetRenderer;    // Renderer with the materials
    public Material fadeToMaterial;    // Material to fade to (Metal2)
    public float fadeSpeed = 2f;       // Speed of fading
    public float speedThreshold = 10f; // Minimum speed to trigger fading

    private FlightBehavior player;     // Reference to player's script
    private List<Material> originalMaterials = new List<Material>();
    private Material[] currentMaterials; // Current materials applied to the renderer
    private int[] fadeMaterialIndices = { 1, 2, 3 }; // Indices of materials to fade
    private float previousSpeed = 0f;
    private float acceleration = 0f;

    private void Start()
    {
        // Find player if not assigned
        player = FindObjectOfType<FlightBehavior>();
        if (player == null)
        {
            Debug.LogError("FlightBehavior script not found in the scene!");
            return;
        }

        // Store original materials
        if (targetRenderer != null)
        {
            foreach (Material mat in targetRenderer.materials)
            {
                originalMaterials.Add(new Material(mat)); // Clone to ensure independence
            }

            currentMaterials = targetRenderer.materials;
        }
        else
        {
            Debug.LogError("Target renderer not assigned!");
        }
    }

    private void FixedUpdate()
    {
        // Calculate acceleration in FixedUpdate
        if (player != null)
        {
            float currentSpeed = player.curSpeed;
            acceleration = currentSpeed - previousSpeed;
            previousSpeed = currentSpeed;
        }
    }

    private void Update()
    {
        if (player == null || targetRenderer == null) return;

        if (acceleration < 0 && player.curSpeed > speedThreshold)
        {
            // Decelerating and above threshold: Fade to fadeToMaterial
            FadeMaterials(fadeToMaterial);
        }
        else
        {
            // Accelerating or below threshold: Revert to original materials
            FadeMaterialsBackToOriginal();
        }
    }

    private void FadeMaterials(Material targetMaterial)
    {
        for (int i = 0; i < fadeMaterialIndices.Length; i++)
        {
            int index = fadeMaterialIndices[i];
            Material current = currentMaterials[index];

            // Lerp color from current to target
            current.Lerp(current, targetMaterial, Time.deltaTime * fadeSpeed);
        }

        targetRenderer.materials = currentMaterials;
    }

    private void FadeMaterialsBackToOriginal()
    {
        for (int i = 0; i < fadeMaterialIndices.Length; i++)
        {
            int index = fadeMaterialIndices[i];
            Material current = currentMaterials[index];
            Material original = originalMaterials[index];

            // Lerp color back to original
            current.Lerp(current, original, Time.deltaTime * fadeSpeed);
        }

        targetRenderer.materials = currentMaterials;
    }
}
