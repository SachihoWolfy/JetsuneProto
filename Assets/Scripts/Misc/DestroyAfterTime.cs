using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class DestroyAfterTime : MonoBehaviour
{
    public float timeDestroy = 2f;
    AudioSource audioSource;
    public AudioClip [] audioClips;
    bool pooledExplosion;
    bool isPooled;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        int rando = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[rando];
        audioSource.Play();
        if (FindAnyObjectByType<ExplosionPoolManager>())
        {
            isPooled = true;
        }
    }
    public void Initialize(bool fromPool)
    {
        pooledExplosion = fromPool;
        StartCoroutine(ReturnExplosionAfterTime(timeDestroy));
    }
    public IEnumerator ReturnExplosionAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ReturnBullet();
    }
    public void ReturnBullet()
    {
        if (isPooled && pooledExplosion)
        {
            ExplosionPoolManager.Instance.ReturnBullet(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void ForceDestroy()
    {
        Destroy(gameObject);
    }
}
