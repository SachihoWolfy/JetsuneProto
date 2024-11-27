using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetSoundController : MonoBehaviour
{
    public AudioSource jetAudioSource; // Assign your jet noise audio source here
    public float minPitch = 0.5f; // Pitch at the lowest speed
    public float maxPitch = 2.0f; // Pitch at the highest speed
    public float maxSpeed = 100f; // The maximum speed that affects pitch
    public float stopThreshold = 5f; // Speed below which the audio stops

    private FlightBehavior player;

    private void Start()
    {
        // Find the FlightBehavior instance in the scene
        player = FindAnyObjectByType<FlightBehavior>();

        if (jetAudioSource == null)
        {
            Debug.LogError("JetSoundController: No AudioSource assigned!");
        }

        if (player == null)
        {
            Debug.LogError("JetSoundController: No FlightBehavior instance found!");
        }
    }

    private void Update()
    {
        if (player == null || jetAudioSource == null)
            return;

        float currentSpeed = player.curSpeed;

        // Adjust pitch based on speed
        if (currentSpeed > stopThreshold)
        {
            if (!jetAudioSource.isPlaying)
            {
                jetAudioSource.Play();
            }

            // Map the speed to the pitch range
            jetAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, currentSpeed / maxSpeed);
        }
        else
        {
            // Fade out and stop the audio when below the threshold
            if (jetAudioSource.isPlaying)
            {
                StartCoroutine(FadeOutAndStop(jetAudioSource, 1.0f)); // 1-second fade-out
            }
        }
    }

    // Coroutine for fading out audio
    private System.Collections.IEnumerator FadeOutAndStop(AudioSource audioSource, float fadeDuration)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
        audioSource.volume = startVolume; // Reset for next play
    }
}
