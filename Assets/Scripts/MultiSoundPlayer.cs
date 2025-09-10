using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class MultiSoundPlayer : MonoBehaviour
{
    public float volume;
    AudioSource audioSource;
    public AudioClip[] audioClips;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayRandomSound()
    {
        if (audioClips.Count() == 0 || audioSource == null) return;

        audioSource.volume = volume;
        int randomIndex = Random.Range(0, audioClips.Count());
        PlaySound(randomIndex);
    }
    public void PlaySound(int index = 0)
    {
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }
}
