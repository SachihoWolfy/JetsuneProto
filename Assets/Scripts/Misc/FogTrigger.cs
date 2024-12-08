using UnityEngine;

public class FogTrigger : MonoBehaviour
{
    [Header("Fog Settings")]
    public bool enableFogOnEnter = true; // Whether to enable fog when the player enters
    public bool resetFogOnExit = true;  // Whether to reset fog to its previous state when the player exits
    public Color fogColor = Color.gray; // Fog color when enabled
    public float fogDensity = 0.02f;    // Fog density when enabled

    private bool originalFogState;     // Stores the initial fog state
    private Color originalFogColor;    // Stores the original fog color
    private float originalFogDensity;  // Stores the original fog density

    private void Start()
    {
        // Store the initial fog settings
        originalFogState = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            RenderSettings.fog = enableFogOnEnter;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            if (resetFogOnExit)
            {
                RenderSettings.fog = originalFogState;
                RenderSettings.fogColor = originalFogColor;
                RenderSettings.fogDensity = originalFogDensity;
            }
            else
            {
                RenderSettings.fog = false; // Optionally turn off fog completely
            }
        }
    }
}

