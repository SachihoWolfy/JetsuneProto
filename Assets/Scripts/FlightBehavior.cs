using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

public class FlightBehavior : MonoBehaviour
{
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public CinemachineVirtualCamera[] cameras;
    public bool simpleControls = true;
    public bool pitchInvert = false;
    public bool lookAtEnemy = false;

    public float blend = 0.9f;
    public float blendThrust = 0.9f;
    public float pitchSpeed = 120f;
    public float pitchSpeedMach = 80f;
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

    public TextMeshProUGUI hpText;

    private bool isDie;
    private bool isDead;

    private bool sonicBoomHappened = false;

    CinemachineVirtualCamera virtualCamera;

    public int hp = 1;
    public bool immunity = false;
    private void Start()
    {
        virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
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
        if (!simpleControls)
        {
            roll = Input.GetAxis("Horizontal") * (Time.fixedDeltaTime * rollSpeed);
            roll = blend * roll + (1 - blend) * oldRoll;
            oldRoll = roll;

            yaw = Input.GetAxis("Yaw") * (Time.fixedDeltaTime * yawSpeed);
            yaw = blend * yaw + (1 - blend) * oldYaw;
            oldYaw = yaw;
        }
        else
        {
            roll = 0;

            yaw = Input.GetAxis("Horizontal") * (Time.fixedDeltaTime * yawSpeed);
            yaw = blend * yaw + (1 - blend) * oldYaw;
            oldYaw = yaw;
        }

        if (curSpeed > 40)
        {
            pitch = Input.GetAxis("Vertical") * (Time.fixedDeltaTime * pitchSpeedMach);
        }
        else
        {
            pitch = Input.GetAxis("Vertical") * (Time.fixedDeltaTime * pitchSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lookAtEnemy = !lookAtEnemy;
            if (lookAtEnemy)
            {
                cameras[1].Priority = 20;
                cameras[0].Priority = 10;
            }
            else
            {
                cameras[1].Priority = 10;
                cameras[0].Priority = 20;
            }
        }
        if (pitchInvert) pitch = -pitch;
        pitch = blend * pitch + (1 - blend) * oldPitch;
        oldPitch = pitch;

        thrust = Input.GetAxis("Thrust") * (Time.deltaTime * thrustSpeed);
        thrust = blendThrust * thrust + (1 - blendThrust) * oldThrust - 0.0004f * thrustSpeed * 2f;
        oldThrust = thrust;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, FindAnyObjectByType<BossMovement>().transform.position);

        if (FindAnyObjectByType<BossMovement>().wonGame)
        {
            lineRenderer.enabled = false;
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, FindAnyObjectByType<BossMovement>().transform.position);
        }
        
    }

    void FixedUpdate()
    {
        if(hp <= 0 && !isDie)
        {
            isDie = true;
        }
        if (!isDie)
        {
            Quaternion AddRot = Quaternion.identity;

            AddRot.eulerAngles = new Vector3(pitch, yaw, -roll);
            transform.rotation *= AddRot;

            if (simpleControls)
            {
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);

            }
            if (lookAtEnemy)
            {
                targetLook.position = FindObjectOfType<BossMovement>().gameObject.transform.position;
            }
            else
            {
                targetLook.localPosition = new Vector3(yaw * lookSens + roll * lookSens / 2, -pitch * lookSens, 5f);
            }

            curSpeed = rb.velocity.magnitude;
            if (curSpeed < 2f) { curSpeed = 3f; }
            oldSpeed = curSpeed;
            curSpeed = Mathf.Clamp(curSpeed + (thrust * thrustSpeed) / (curSpeed), MINSPEED, maxSpeed);

            if (oldSpeed > curSpeed)
            {
                curSpeed = Mathf.Clamp(curSpeed + thrust * thrustSpeed / 5f, MINSPEED, maxSpeed);
            }
            else if (curSpeed > 40 && curSpeed < 60)
            {
                if (!sonicBoomHappened) PlaySound(0);
                curSpeed = Mathf.Clamp(curSpeed + thrust * thrustSpeed * 2, MINSPEED, maxSpeed);
            }

            rb.velocity = transform.forward * curSpeed;

            anim.SetFloat("CurSpeed", curSpeed);

            if (curSpeed < 10)
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
        else if(!isDead)
        {
            Die();
            isDead = true;
            Restart();
        }
        updateUI();
    }

    void Restart()
    {
        FindAnyObjectByType<BossMovement>().StartCoroutine("RestartGame");
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
        hpText.text = "HP: " + hp;
    }

    void Die()
    {
        cameras[2].Priority = 100;
        FindObjectOfType<HitStop>().Stop(0.5f);
        PlaySound(3);
        rb.useGravity = true;
        rb.freezeRotation = false;
        rb.AddExplosionForce(160f, targetLook.position, 20f);
        Vector3 spin = new Vector3(-50f, 0f, 0f);
        rb.AddRelativeTorque(spin,ForceMode.Impulse);
    }
    void CalculateState(float dt)
    {
        //var invRotation = Quaternion.Inverse(Rigidbody.rotation);
    }

    void ToggleSimpleControls()
    {
        simpleControls = !simpleControls;
    }

    void TogglePitchInvert()
    {
        pitchInvert = !pitchInvert;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Toggle Advanced Controls")) ToggleSimpleControls();
        if (GUILayout.Button("Toggle Pitch Invert")) TogglePitchInvert();
    }

    public void TakeDamage(int dmg, float spdDamage)
    {
        if (immunity) { return; }
        immunity = true;
        StartCoroutine(ImmunityReset());
        hp -= dmg;
        curSpeed = Mathf.Clamp(curSpeed - spdDamage,MINSPEED,maxSpeed);
        rb.velocity = transform.forward * curSpeed;
        Debug.Log("Damaged");
        anim.SetTrigger("Damage");
        PlaySound(2);
    }

    public IEnumerator ImmunityReset(float duration = 1f)
    {
        yield return new WaitForSeconds(duration);
        immunity = false;
    }

    public void PlaySound(int index = 0)
    {
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }
}
