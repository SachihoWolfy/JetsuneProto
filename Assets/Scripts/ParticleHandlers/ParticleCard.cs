using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleCard : MonoBehaviour
{
    private AudioSource wooshSource;
    private GameObject explosionPrefab;
    [TextArea(10, 20)]
    public string Notes =
        "This script will add the needed scripts at runtime, unless you add them manually.\n" +
        "\n" +
        "This system uses names to assign scripts.\n" +
        "- For the white inside part of the bullet, include 'Core' in the name\n" +
        "- For the colored outside part of the bullet, include 'Shell' in the name\n\n" +
        "Default Core 3D start size: \n" +
        "    x: 100    y: 100    z:130\n" +
        "Default Shell 3D start size:\n" +
        "    x: 150    y:150    z:150";
    void Start()
    {
        var audioHolder = new GameObject("wooshSource");
        wooshSource = audioHolder.AddComponent<AudioSource>();
        explosionPrefab = Settings.explosionPrefab;
        AttachComponentsRecursively(transform);
    }

    void AttachComponentsRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Check if the child has a ParticleSystem component
            if (child.GetComponent<ParticleSystem>())
            {
                string objName = child.gameObject.name;

                if (objName.Contains("Core"))
                {
                    if (!child.gameObject.GetComponent<ParticleGraze>())
                        child.gameObject.AddComponent<ParticleGraze>();
                }

                if (objName.Contains("Shell"))
                {
                    if (!child.gameObject.GetComponent<ParticleBullets>())
                    {
                        var exploder = child.gameObject.AddComponent<ParticleBullets>();
                        exploder.explosionPrefab = explosionPrefab;
                        exploder.isPooled = true;
                    }

                    if (!child.gameObject.GetComponent<ParticleWoosh>())
                    {
                        var pwoosh = child.gameObject.AddComponent<ParticleWoosh>();
                        pwoosh.card = this;
                        pwoosh.audioSource = wooshSource;
                    }
                }
                // Single Bullet System
                if (objName.Contains("SBS_"))
                {
                    if (!child.gameObject.GetComponent<ParticleBullets>())
                    {
                        var exploder = child.gameObject.AddComponent<ParticleBullets>();
                        exploder.explosionPrefab = explosionPrefab;
                        exploder.isPooled = true;
                        exploder.isSBS = true;
                    }
                    if (!child.gameObject.GetComponent<PSTriggerHandler>())
                    {
                        var TH = child.gameObject.AddComponent<PSTriggerHandler>();
                        TH.audioSource = wooshSource;
                        TH.bulletScript = child.gameObject.GetComponent<ParticleBullets>();
                    }
                }
            }

            // Recursively check children
            AttachComponentsRecursively(child);
        }
    }
    public void StopCard()
    {
        StopCardRecursively(transform);
    }
    void StopCardRecursively(Transform parent)
    {
        foreach(Transform child in parent)
        {
            if (child.GetComponent<ParticleSystem>())
            {
                child.GetComponent<ParticleSystem>().Stop();
            }
            StopCardRecursively(child);
        }
    }
    public void PlayCard()
    {
        PlayCardRecursively(transform);
    }
    void PlayCardRecursively(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.GetComponent<ParticleSystem>())
            {
                child.GetComponent<ParticleSystem>().Play();
            }
            PlayCardRecursively(child);
        }
    }
}
