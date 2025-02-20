using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting; // Required for List<T>

public class ParticleWoosh : MonoBehaviour
{
    public List<AudioClip> audioClips;
    public AudioSource audioSource;
    private float wooshRadius = 15f;
    private float volume = 0.4f;
    private FlightBehavior player;
    private static bool canPlaySound = true;
    private float cooldownDuration = 0.3f;

    private ParticleSystem ps;
    private List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>(); // Fix: Use List instead of array
    private BulletWoosh woosher;
    public float colliderSize = 0.007f;
    public ParticleCard card;

    void Start()
    {
        player = FindObjectOfType<FlightBehavior>();
        ps = GetComponent<ParticleSystem>();
        woosher = FindObjectOfType<BulletWoosh>();
        if (woosher)
        {
            audioClips = new List<AudioClip>();
            audioClips.AddRange(woosher.audioClips);
            wooshRadius = woosher.wooshRadius;
            volume = woosher.volume;
        }

        if (ps == null)
        {
            Debug.LogError("BulletWoosh: No Particle System found! Attach this script to a GameObject with a Particle System.");
        }
        var collider = ps.collision;
        collider.enabled = true;
        collider.radiusScale = colliderSize;
        collider.collidesWith = LayerMask.GetMask("Ground", "Player");
        // Ensure particle system has trigger module enabled
        var trigger = ps.trigger;
        trigger.enabled = true;
        trigger.radiusScale = colliderSize;
        trigger.inside = ParticleSystemOverlapAction.Ignore;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        trigger.enter = ParticleSystemOverlapAction.Callback;
        trigger.exit = ParticleSystemOverlapAction.Ignore;
        trigger.SetCollider(0, woosher.GetComponent<Collider>());
    }

    void Update()
    {
        // Ensure audio source follows the bullet's trigger location
        // If the audio source should be at the particle system's position, use the particle's position instead of the player
        if (audioSource != null)
        {
            audioSource.transform.position = player.transform.position;
        }
        if(woosher == null)
        {
            woosher = FindObjectOfType<BulletWoosh>();
            audioClips.AddRange(woosher.audioClips);
            wooshRadius = woosher.wooshRadius;
            volume = woosher.volume;
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
        if (audioClips.Count == 0 || audioSource == null) return;

        audioSource.volume = volume;
        audioSource.spatialBlend = 1;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = wooshRadius;
        audioSource.dopplerLevel = 1;

        int randomIndex = Random.Range(0, audioClips.Count);
        audioSource.clip = audioClips[randomIndex];
        audioSource.PlayOneShot(audioClips[randomIndex]);
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
