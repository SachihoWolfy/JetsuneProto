using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPoolManager : MonoBehaviour
{
    public static ExplosionPoolManager Instance;  // Singleton instance for easy access

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] public int poolSize = 50;  // Number of bullets to pool

    private Queue<GameObject> explosionPool;

    private void Awake()
    {
        // Initialize the singleton instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Initialize the pool
        explosionPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(explosionPrefab);
            bullet.SetActive(false);
            explosionPool.Enqueue(bullet);
        }
    }
    public GameObject GetBullet()
    {
        if (explosionPool.Count > 0)
        {
            GameObject bullet = explosionPool.Dequeue();
            bullet.SetActive(true);
            var bulletScript = bullet.GetComponent<DestroyAfterTime>();
            bulletScript.Initialize(true);
            return bullet;
        }
        else
        {
            // Optional: Instantiate more bullets if pool is empty
            GameObject bullet = Instantiate(explosionPrefab);
            var bulletScript = bullet.GetComponent<DestroyAfterTime>();
            bulletScript.Initialize(false);
            return bullet;
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        explosionPool.Enqueue(bullet);
    }
}
