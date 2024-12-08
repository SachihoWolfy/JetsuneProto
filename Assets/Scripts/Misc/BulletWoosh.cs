using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWoosh : MonoBehaviour
{
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    public float wooshRadius;
    public float volume;
    FlightBehavior player;
    private bool canPlaySound = true;
    [SerializeField] private float cooldownDuration;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<FlightBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet") && canPlaySound)
        {
            if(other.gameObject.TryGetComponent<AudioSource>(out AudioSource audioSource))
            {
                return;
            }
            else
            {
                audioSource = other.gameObject.AddComponent<AudioSource>();
            }
            audioSource.volume = volume;
            audioSource.spatialBlend = 1;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = wooshRadius;
            audioSource.dopplerLevel = 1;
            int rando = Random.Range(0, audioClips.Length);
            PlaySound(rando, audioSource);
            TriggerCooldown(cooldownDuration);
        }
        
    }
    public void PlaySound(int index, AudioSource audio)
    {
        audio.clip = audioClips[index];
        audio.Play();
    }

    void TriggerCooldown(float duration)
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
