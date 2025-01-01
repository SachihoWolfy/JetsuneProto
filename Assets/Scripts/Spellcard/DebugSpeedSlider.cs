using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugSpeedSlider : MonoBehaviour
{
    public float curSpeed = 3;
    public TextMeshProUGUI speedText;
    [SerializeField]
    public Slider slider;
    float thrust;
    float blendThrust = 0.9f;
    float oldThrust;
    float thrustSpeed = 63;

    float MINSPEED = 3;
    float maxSpeed = 61;

    public AudioSource audioSource;
    public bool speedToCamera;

    private void Update()
    {
        thrust = Input.GetAxis("Thrust") * (Time.fixedDeltaTime * thrustSpeed);
        thrust = blendThrust * thrust + (1 - blendThrust) * oldThrust - 0.0004f * thrustSpeed * 2f;
        oldThrust = thrust;
        DoSpeedThings();
        curSpeed = slider.value;
        speedText.text = curSpeed.ToString("F1");
        if (speedToCamera)
        {
            float FOVmod = NormalizeBetweenZeroAndOne(curSpeed, MINSPEED, maxSpeed) * 20;
            Camera.main.fieldOfView = FOVmod + 60;

            Camera.main.transform.position = new Vector3(0, 0, -curSpeed / 2);
        }
    }
    public float NormalizeBetweenZeroAndOne(float value, float minimum, float maximum)
    {
        return Mathf.InverseLerp(minimum, maximum, value);
    }

    void DoSpeedThings()
    {
        curSpeed = slider.value;
        if (curSpeed < 2f) { curSpeed = 3f; }
        var oldSpeed = curSpeed;
        float adjustedThrust = (thrust);
        curSpeed = Mathf.Clamp(curSpeed + adjustedThrust / curSpeed, MINSPEED, maxSpeed);
        if (oldSpeed > curSpeed)
        {
            curSpeed = Mathf.Clamp(curSpeed + adjustedThrust / 5f, MINSPEED, maxSpeed);
        }
        else if (curSpeed > 40 && curSpeed < 60)
        {
            PlaySound();
            curSpeed = Mathf.Clamp(curSpeed + adjustedThrust * 2, MINSPEED, maxSpeed);
        }
        slider.value = curSpeed;
    }

    void PlaySound()
    {
        audioSource.Play();
    }
}
