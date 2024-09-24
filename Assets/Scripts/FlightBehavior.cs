using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlightBehavior : MonoBehaviour
{
    public Animator anim;

    public float blend = 0.9f;
    public float blendThrust = 0.9f;
    public float pitchSpeed = 120f;
    public float rollSpeed = 120f;
    public float yawSpeed = 40f;
    public float thrustSpeed;
    private float MINSPEED = 3f;

    public float maxSpeed = 100f;
    public float lookSens = 20f;

    public Rigidbody rb;
    public Transform targetLook;

    private float roll;
    private float yaw;
    private float pitch;
    private float thrust;
    public float curSpeed;

    float oldRoll;
    float oldPitch;
    float oldYaw;
    float oldThrust;

    float oldSpeed;

    public Slider speedGauge;
    public Image fillImage;
    LineRenderer lineRenderer;
    public Color c1 = Color.yellow;
    public Color c2 = Color.red;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.2f, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
    }

    void Update()
    {
        // get input movement
        roll = Input.GetAxis("Horizontal") * (Time.fixedDeltaTime * rollSpeed);
        roll = blend * roll + (1 - blend) * oldRoll;
        oldRoll = roll;

        pitch = Input.GetAxis("Vertical") * (Time.fixedDeltaTime * pitchSpeed);
        pitch = blend * pitch + (1 - blend) * oldPitch;
        oldPitch = pitch;

        yaw = Input.GetAxis("Yaw") * (Time.fixedDeltaTime * yawSpeed);
        yaw = blend * yaw + (1 - blend) * oldYaw;
        oldYaw = yaw;

        thrust = Input.GetAxis("Thrust") * (Time.deltaTime * thrustSpeed);
        thrust = blendThrust * thrust + (1 - blendThrust) * oldThrust - 0.0004f * thrustSpeed * 2f;
        oldThrust = thrust;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, FindAnyObjectByType<BossMovement>().transform.position);
        
    }

    void FixedUpdate()
    {
        Quaternion AddRot = Quaternion.identity;

        AddRot.eulerAngles = new Vector3(pitch, yaw, -roll);
        transform.rotation *= AddRot;

        targetLook.localPosition = new Vector3(yaw * lookSens + roll * lookSens/2, -pitch * lookSens, 5f);

        curSpeed = rb.velocity.magnitude;
        if(curSpeed < 2f) { curSpeed = 3f; }
        oldSpeed = curSpeed;
        curSpeed = Mathf.Clamp(curSpeed + (thrust * thrustSpeed)/(curSpeed), MINSPEED, maxSpeed);

        if(oldSpeed > curSpeed)
        {
            curSpeed = Mathf.Clamp(curSpeed + thrust * thrustSpeed/5f, MINSPEED, maxSpeed);
        }
        else if (curSpeed > 40 && curSpeed < 60)
        {
            curSpeed = Mathf.Clamp(curSpeed + thrust * thrustSpeed * 2, MINSPEED, maxSpeed);
        }

        rb.velocity = transform.forward * curSpeed;

        anim.SetFloat("CurSpeed", curSpeed);

        if(curSpeed < 10)
        {
            anim.ResetTrigger("Fly");
            anim.SetTrigger("Hover");
        }
        else
        {
            anim.ResetTrigger("Hover");
            anim.SetTrigger("Fly");
        }
        updateUI();
    }
    public void updateUI()
    {
        speedGauge.value = curSpeed;
        if (curSpeed > 60)
        {
            fillImage.color = Color.yellow;
        }
        else if(curSpeed > 40)
        {
            fillImage.color = Color.cyan;
        }
        else
        {
            fillImage.color = Color.red;
        }
    }
    void CalculateState(float dt)
    {
        //var invRotation = Quaternion.Inverse(Rigidbody.rotation);
    }
}
