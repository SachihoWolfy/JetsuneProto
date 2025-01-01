using UnityEngine;
using System.Collections;

public class FogTrigger : MonoBehaviour
{
    [Header("Fog Settings")]
    public bool enableFogOnEnter = true;      // Whether to enable fog when the player enters
    public bool resetFogOnExit = true;       // Whether to reset fog to its previous state when the player exits
    public Color fogColor = Color.gray;      // Fog color when enabled
    public float fogDensity = 0.02f;         // Fog density when enabled
    public float transitionDuration = 2.0f; // Duration of the fog transition (in seconds)

    private bool originalFogState;           // Stores the initial fog state
    private Color originalFogColor;          // Stores the original fog color
    private float originalFogDensity;        // Stores the original fog density
    private Coroutine fogTransition;         // Tracks the current fog transition coroutine

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
            if (fogTransition != null) StopCoroutine(fogTransition);
            fogTransition = StartCoroutine(TransitionFog(enableFogOnEnter, fogColor, fogDensity));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            if (fogTransition != null) StopCoroutine(fogTransition);

            if (resetFogOnExit)
            {
                fogTransition = StartCoroutine(TransitionFog(originalFogState, originalFogColor, originalFogDensity));
            }
            else
            {
                fogTransition = StartCoroutine(TransitionFog(true, RenderSettings.fogColor, 0f));
            }
        }
    }

    private IEnumerator TransitionFog(bool targetFogState, Color targetFogColor, float targetFogDensity)
    {
        float elapsedTime = 0f;

        // Save current fog state as starting values
        bool startingFogState = RenderSettings.fog;
        Color startingFogColor = RenderSettings.fogColor;
        float startingFogDensity = RenderSettings.fogDensity;

        // Ensure fog is active during the transition
        RenderSettings.fog = true;

        // Gradually interpolate fog settings over the transition duration
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;

            // Keep fog color constant, but fade the density
            RenderSettings.fogColor = targetFogColor;
            RenderSettings.fogDensity = Mathf.Lerp(startingFogDensity, targetFogDensity, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalize fog settings
        RenderSettings.fog = targetFogState;
        RenderSettings.fogColor = targetFogColor;
        RenderSettings.fogDensity = targetFogDensity;

        fogTransition = null;
    }
}
