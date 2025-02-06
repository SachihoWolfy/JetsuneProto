using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for List<T>

public class ParticleWoosh : MonoBehaviour
{
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    public float wooshRadius;
    public float volume;
    private FlightBehavior player;
    private bool canPlaySound = true;
    [SerializeField] private float cooldownDuration;

    private ParticleSystem ps;
    private List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>(); // Fix: Use List instead of array

    void Start()
    {
        player = FindObjectOfType<FlightBehavior>();
        ps = GetComponent<ParticleSystem>();

        if (ps == null)
        {
            Debug.LogError("BulletWoosh: No Particle System found! Attach this script to a GameObject with a Particle System.");
        }

        // Ensure particle system has trigger module enabled
        var trigger = ps.trigger;
        trigger.enabled = true;
    }

    void Update()
    {
        // Ensure audio source follows the bullet's trigger location
        // If the audio source should be at the particle system's position, use the particle's position instead of the player
        if (audioSource != null)
        {
            audioSource.transform.position = player.transform.position;
        }
    }

    void OnParticleTrigger()
    {
        if (!canPlaySound) return;

        enterParticles.Clear(); // Clear the list before getting new particles
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles); // Fix: Use List instead of array

        if (numEnter > 0)
        {
            PlayRandomWooshSound();
            TriggerCooldown(cooldownDuration);
        }
    }

    private void PlayRandomWooshSound()
    {
        if (audioClips.Length == 0 || audioSource == null || audioSource.isPlaying) return;

        audioSource.volume = volume;
        audioSource.spatialBlend = 1;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = wooshRadius;
        audioSource.dopplerLevel = 1;

        int randomIndex = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[randomIndex];
        audioSource.Play();
    }

    private void TriggerCooldown(float duration)
    {
        if (canPlaySound)
        {
            canPlaySound = false;
            StartCoroutine(Cooldown(duration, () => canPlaySound = true));
        }
    }

    private IEnumerator Cooldown(float cooldownTime, System.Action onCooldownComplete)
    {
        yield return new WaitForSeconds(cooldownTime);
        onCooldownComplete?.Invoke();
    }
}
