using UnityEngine;

public class ParticleBullets : MonoBehaviour
{
    public GameObject explosionPrefab; // Prefab for explosion effect
    public bool isPooled = false; // If using an object pool
    private ParticleSystem ps;

    private BulletWoosh woosher;
    public float colliderSize = 0.007f;
    public ParticleCard card;
    bool updated = false;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        woosher = FindObjectOfType<BulletWoosh>();
        var collider = ps.collision;
        collider.enabled = true;
        collider.radiusScale = colliderSize;
        collider.collidesWith = LayerMask.GetMask("Ground", "Player","BulletColliders");
        collider.sendCollisionMessages = true;
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

    void OnParticleCollision(GameObject other)
    {
        // Get collision point(s)
        ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[10];
        int collisionCount = ps.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < collisionCount; i++)
        {
            Vector3 collisionPoint = collisionEvents[i].intersection;
            if (Settings.m_lastFramerate > 27 || other.gameObject.CompareTag("Player")) { 
                Explode(collisionPoint); 
            }
        }
    }

    private void Explode(Vector3 position)
    {
        GameObject explosion;

        // Spawn from pool or instantiate
        if (isPooled)
            explosion = ExplosionPoolManager.Instance.GetBullet();
        else
            explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

        explosion.transform.position = position;

        // Set Particle Color from Bullet's Material (if applicable)
        ParticleSystem explosionParticles = explosion.GetComponentInChildren<ParticleSystem>();
        if (explosionParticles != null)
        {
            Material bulletMaterial = GetComponent<ParticleSystemRenderer>().material;
            if (bulletMaterial.HasProperty("_Color"))
            {
                Color bulletColor = bulletMaterial.color;
                var mainModule = explosionParticles.main;
                mainModule.startColor = bulletColor;

                // Set colors for all child particle systems
                ParticleSystem[] particleSystems = explosionParticles.gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in particleSystems)
                {
                    var psMain = ps.main;
                    psMain.startColor = bulletColor;
                }

                // Set emissive color
                Color emissiveColor = bulletMaterial.color;
                foreach (ParticleSystem ps in particleSystems)
                {
                    ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Material instanceMaterial = renderer.material;
                        if (instanceMaterial.HasProperty("_EmissionColor"))
                        {
                            instanceMaterial.EnableKeyword("_EMISSION");
                            instanceMaterial.SetColor("_EmissionColor", emissiveColor);
                            renderer.material = instanceMaterial;
                        }
                    }
                }
            }
        }
    }
}
