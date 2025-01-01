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
    private static ExplosionPoolManager explosionPool;
    private float timeAlive;


    private void Awake()
    {
        if (explosionPool == null)
        {
            explosionPool = FindFirstObjectByType<ExplosionPoolManager>();
        }
        if (explosionPool != null)
        {
            isPooled = true;
        }
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        int rando = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[rando];
        audioSource.Play();
    }
    public void Initialize(bool fromPool)
    {
        pooledExplosion = fromPool;
        timeAlive = 0f;
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
    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive >= timeDestroy)
        {
            ReturnBullet();
        }
    }
}
