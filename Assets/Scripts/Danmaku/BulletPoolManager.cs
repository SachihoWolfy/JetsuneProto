using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : MonoBehaviour
{
    public static BulletPoolManager Instance;  // Singleton instance for easy access

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 200;  // Number of bullets to pool

    private Queue<GameObject> bulletPool;

    private void Awake()
    {
        // Initialize the singleton instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Initialize the pool
        bulletPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        else
        {
            // Optional: Instantiate more bullets if pool is empty
            GameObject bullet = Instantiate(bulletPrefab);
            return bullet;
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}

