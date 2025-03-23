using UnityEngine;
using UnityEngine.UI;

public class EmissionBrightnessController : MonoBehaviour
{
    public Material targetMaterial; // Assign the material directly
    public Material backupMaterial; // Backup material to restore default values
    public Slider brightnessSlider; // Assign the UI slider
    public Image sliderFill; // Assign the fill image of the slider
    public float maxBrightness = 1f; // Maximum emission intensity

    void Start()
    {
        if (targetMaterial == null || backupMaterial == null || brightnessSlider == null || sliderFill == null)
        {
            Debug.LogError("Material, Backup Material, Slider, or Slider Fill is not assigned!");
            return;
        }

        // Ensure the material has emission enabled
        targetMaterial.EnableKeyword("_EMISSION");

        // Set initial slider value and emission color based on the backup material
        Color defaultEmission = backupMaterial.GetColor("_EmissionColor");
        float defaultBrightness = defaultEmission.maxColorComponent / maxBrightness;
        brightnessSlider.value = defaultBrightness;
        targetMaterial.SetColor("_EmissionColor", defaultEmission);

        // Update slider fill color
        sliderFill.color = Color.Lerp(Color.black, Color.white, brightnessSlider.value);

        brightnessSlider.onValueChanged.AddListener(UpdateEmission);
    }

    void UpdateEmission(float value)
    {
        if (targetMaterial != null)
        {
            Color emissionColor = Color.white * (value * maxBrightness);
            targetMaterial.SetColor("_EmissionColor", emissionColor);
        }

        // Update slider fill color
        if (sliderFill != null)
        {
            sliderFill.color = Color.Lerp(Color.black, Color.white, value);
        }
    }
}
