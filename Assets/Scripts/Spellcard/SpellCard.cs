using UnityEngine;

public class SpellCard : MonoBehaviour
{
    public Layer[] layers;
    private void Start()
    {
        if (FindFirstObjectByType<DebugSpeedSlider>())
        {
            ActivateSpellCard();
        }
    }
    public void ActivateSpellCard()
    {
        foreach (var layer in layers)
        {
            layer.StartLayer();
        }
    }

    public void DeactivateSpellCard()
    {
        foreach (var layer in layers)
        {
            layer.StopLayer();
        }
    }
}

