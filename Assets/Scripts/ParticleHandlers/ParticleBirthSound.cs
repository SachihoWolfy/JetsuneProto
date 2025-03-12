using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(SphereCollider))]
public class ParticleBirthSound : MonoBehaviour
{
    public List<AudioClip> audioClips;
    public bool shouldVary = true;
    public bool shouldOverlap = true;
    public float volume;
    private AudioSource audioSource;
    private ParticleSystem ps;
    private SphereCollider colliderTrigger;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1;
        audioSource.dopplerLevel = 0;
        audioSource.spatialBlend = 1;
        ps = GetComponent<ParticleSystem>();
        colliderTrigger = GetComponent<SphereCollider>();
        colliderTrigger.radius = 200f;
        colliderTrigger.center = new Vector3(0,0,0);
        var main = ps.main;
        main.startLifetime = 0.1f;
        var trigger = ps.trigger;
        trigger.enabled = true;
        trigger.radiusScale = 0.007f;
        trigger.inside = ParticleSystemOverlapAction.Ignore;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        trigger.enter = ParticleSystemOverlapAction.Callback;
        trigger.exit = ParticleSystemOverlapAction.Ignore;
        trigger.SetCollider(0, colliderTrigger);
    }

    private void OnParticleTrigger()
    {
        PlaySound();
    }

    private void PlaySound()
    {
        audioSource.clip = audioClips[Random.Range(0, audioClips.Count)];
        if (shouldVary)
        {
            VaryPitch();
        }
        if (shouldOverlap)
        {
            audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Count)]);
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void SetVolume(float targetVolume)
    {
        audioSource.volume = targetVolume;
    }
    public void SetClip(AudioClip ac)
    {
        audioSource.clip = ac;
    }
    public void SetClips(AudioClip[] audioClipsIncoming)
    {
        audioClips = new List<AudioClip>(audioClipsIncoming);
    }
    private void VaryPitch()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
    }
}
