using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightBehavior : MonoBehaviour
{
    public Animator anim;

    public float blend = 0.9f;
    public float blendThrust = 0.9f;
    public float pitchSpeed = 120f;
    public float rollSpeed = 120f;
    public float yawSpeed = 40f;
    public float thrustSpeed;

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
        thrust = blendThrust * thrust + (1 - blendThrust) * oldThrust - 0.001f * thrustSpeed * 2f;
        oldThrust = thrust;
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
        curSpeed = Mathf.Clamp(curSpeed + (thrust * thrustSpeed)/(curSpeed), 0, maxSpeed);

        if(oldSpeed > curSpeed)
        {
            curSpeed = Mathf.Clamp(curSpeed + thrust * thrustSpeed/5f, 0, maxSpeed);
        }

        rb.velocity = transform.forward * curSpeed;

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
    }

    void CalculateState(float dt)
    {
        //var invRotation = Quaternion.Inverse(Rigidbody.rotation);
    }
}
