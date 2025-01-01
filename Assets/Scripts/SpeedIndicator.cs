using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedIndicator : MonoBehaviour
{
    FlightBehavior player;
    public TextMeshProUGUI speedText;
    // Start is called before the first frame update
    void Start()
    {
        player = FindFirstObjectByType<FlightBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        float measuredSpeed = player.curSpeed * 5;
        speedText.text = measuredSpeed.ToString("F0");
    }
}
