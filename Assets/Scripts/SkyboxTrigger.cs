using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxTrigger : MonoBehaviour
{
    public Material desiredSkybox;
    public Color ambientColor;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.skybox = desiredSkybox;
            RenderSettings.ambientSkyColor = ambientColor;
            RenderSettings.subtractiveShadowColor = ambientColor;
        }
    }
}