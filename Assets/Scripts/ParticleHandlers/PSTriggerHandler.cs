using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PSTriggerHandler : MonoBehaviour
{
    [Header("Particle System")]
    public ParticleSystem ps;

    [Header("Trigger Zones")]
    public Collider innerTriggerZone;
    public Collider outerTriggerZone;
    public Collider broadTriggerZone;

    [Header("Cooldown Settings")]
    public float innerZoneCooldown = 0.1f;
    public float outerZoneCooldown = 0.3f;

    private float innerCooldownTimer = 0f;
    private float outerCooldownTimer = 0f;

    // Outer Woosher
    private BulletWoosh woosher;
    public List<AudioClip> audioClips;
    public AudioSource audioSource;
    private float volume = 0.4f;
    private float wooshRadius = 15f;
    private static bool canPlaySound = true;

    //Inner
    GrazeController grazer;

    private ParticleSystem.Particle[] particles;
    private FlightBehavior player;

    int damage = 1;
    float speedDamage = 40f;

    void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
        //Woosh
        woosher = FindObjectOfType<BulletWoosh>();
        outerTriggerZone = woosher.GetComponent<Collider>();
        if (woosher)
        {
            audioClips = new List<AudioClip>();
            audioClips.AddRange(woosher.audioClips);
            volume = woosher.volume;
        }
        // Graze
        grazer = FindAnyObjectByType<GrazeController>();
        innerTriggerZone = grazer.GetComponent<Collider>();
        // Broad
        var trigger = ps.trigger;
        trigger.enabled = true;
        trigger.inside = ParticleSystemOverlapAction.Callback;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        trigger.enter = ParticleSystemOverlapAction.Ignore;
        trigger.exit = ParticleSystemOverlapAction.Ignore;
        foreach (Transform child in grazer.gameObject.transform)
        {
            string objName = child.gameObject.name;
            if (objName.Contains("Broad"))
            {
                broadTriggerZone = child.GetComponent<Collider>();
            }
        }
        trigger.SetCollider(0, broadTriggerZone);
    }
    void LateUpdate()
    {
        audioSource.transform.position = grazer.transform.position;
        if (ps == null || innerTriggerZone == null || outerTriggerZone == null || broadTriggerZone == null)
        {
            Debug.Log("Something was Null in lateUpdate");
            return;
        }

        // Cooldown timers
        if (innerCooldownTimer > 0f) innerCooldownTimer -= Time.deltaTime;
        if (outerCooldownTimer > 0f) outerCooldownTimer -= Time.deltaTime;

        int numParticlesAlive = ps.GetParticles(particles);
        bool innerTriggered = false;
        bool outerTriggered = false;

        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 worldPos = GetWorldPosition(particles[i]);
            // Skip particles outside the broad zone
            if (!broadTriggerZone.bounds.Contains(worldPos))
            {
                continue;
            }

            if (innerCooldownTimer <= 0f && innerTriggerZone.bounds.Contains(worldPos))
            {
                innerTriggered = true;
                break; // We only need one particle to activate
            }

            if (!innerTriggered && outerCooldownTimer <= 0f && outerTriggerZone.bounds.Contains(worldPos))
            {
                outerTriggered = true;
                // Don't break here — a later particle might still hit inner
            }
        }

        // Handle zone logic
        if (innerTriggered)
        {
            innerCooldownTimer = innerZoneCooldown;
            OnInnerZoneActivated();
        }
        else if (outerTriggered)
        {
            outerCooldownTimer = outerZoneCooldown;
            OnOuterZoneActivated();
        }

        ps.SetParticles(particles, numParticlesAlive);
    }
    void OnInnerZoneActivated()
    {
        grazer.Graze();
    }

    void OnOuterZoneActivated()
    {
        if (!canPlaySound) return;
        PlayRandomWooshSound();
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

    private Vector3 GetWorldPosition(ParticleSystem.Particle particle)
    {
        var simSpace = ps.main.simulationSpace;

        switch (simSpace)
        {
            case ParticleSystemSimulationSpace.Local:
                return ps.transform.TransformPoint(particle.position);

            case ParticleSystemSimulationSpace.World:
                return particle.position;

            case ParticleSystemSimulationSpace.Custom:
                if (ps.main.customSimulationSpace != null)
                    return ps.main.customSimulationSpace.TransformPoint(particle.position);
                else
                    return particle.position;

            default:
                return particle.position;
        }
    }
    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.TakeDamage(damage, speedDamage);
        }
    }

}
