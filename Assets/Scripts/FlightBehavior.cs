using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;

public class FlightBehavior : MonoBehaviour
{
    public bool isCutscene = false;
    public int cutsceneID = 0;
    public Animator cutsceneAnim;
    public Animator anim;
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public CinemachineVirtualCamera[] cameras;
    public bool simpleControls = true;
    public bool pitchInvert = false;
    public bool lookAtEnemy;

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

    public Slider powerGauge;
    public Image fillImageP;
    public Color pGuageColor1 = Color.red;
    public Color pGuageColor2;
    public Color pGuageColor3 = Color.yellow;

    public TextMeshProUGUI hpText;
    public TextMeshProUGUI powerupText;
    public TextMeshProUGUI scoreText;

    private bool isDie;
    private bool isDead;

    private bool sonicBoomHappened = false;

    private float awayScale = 40f;
    private float defaultScale = 11.26f;
    private float pursuitMod = -20f;
    private float curScale;
    public bool doCutsceneScaling = false;


    CinemachineVirtualCamera virtualCamera;
    CinemachineDollyCart dolly;

    public int hp = 1;
    public bool immunity = false;

    public bool tryingPowerup;
    public bool isPowerup;
    public bool disablePower;
    static int curPowerAmount;
    int dampingPowerUp = 2;
    public float powerSpeed = 67;

    static int score = 0;
    static int scoreP = 0;
    int oldScore;
    int oldScoreP;
    public int lifeScore = 10000;
    public int powerScore = 5000;
    private bool canPointDestroy = true;
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
        if (isCutscene)
        {
            cutsceneAnim.SetInteger("Cutscene", cutsceneID);
            cutsceneAnim.SetBool("IsCutscene", isCutscene);
        }
        if (GetComponent<CinemachineDollyCart>())
        {
            dolly = GetComponent<CinemachineDollyCart>();
        }
        simpleControls = Settings.simpleControls;
        pitchInvert = Settings.invertPitch;
    }

    IEnumerator DoSachiPowerUp()
    {
        audioSource.PlayOneShot(audioClips[0]);
        curPowerAmount--;
        powerupText.text = "P x" + curPowerAmount;
        immunity = true;
        isPowerup = true;
        anim.SetBool("IsPowerup", true);
        gameObject.GetComponent<Animator>().SetBool("IsPowerup", true);
        PlaySound(4);
        yield return new WaitForSeconds(5);
        audioSource.Stop();
        anim.SetBool("IsPowerup", false);
        gameObject.GetComponent<Animator>().SetBool("IsPowerup", false);
        immunity = false;
        isPowerup = false;
        scoreP = 0;
    }

    public void StopPowerup()
    {
        StopCoroutine(DoSachiPowerUp());
        anim.SetBool("IsPowerup", false);
        GetComponent<Animator>().SetBool("IsPowerup", false);
        immunity = false;
        isPowerup = false;
    }

    public void AddScore(int value)
    {
        oldScore = score;
        score += value;
        if (curPowerAmount < 1)
        {
            oldScoreP = scoreP;
            scoreP += value;
        }
        if ((score % lifeScore) < (oldScore % lifeScore))
        {
            Mathf.Clamp(hp++,0,9);
        }
        if ((scoreP % powerScore) < (oldScoreP % powerScore) && curPowerAmount < 1)
        {
            FindAnyObjectByType<GrazeController>().PlaySound(2);
            Mathf.Clamp(curPowerAmount++,0,1);
        }
        if ((score % lifeScore) < (oldScore % lifeScore)|| (score % powerScore) < (oldScore % powerScore))
        {
            FindAnyObjectByType<GrazeController>().PlaySound(1);
        }
    }

    void AccelerateToEnemy()
    {
        if(isPowerup)
        immunity = true;
        Vector3 bossPos = FindObjectOfType<BossMovement>().gameObject.transform.position;
        var lookPos = bossPos - transform.position;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampingPowerUp);
        curSpeed = powerSpeed;
        rb.velocity = transform.forward * curSpeed;
    }
    void DoLineRender()
    {
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
    void Update()
    {
        if (isCutscene)
        {
            return;
        }
        DoLineRender();
        //Cam Shit
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (lookAtEnemy)
            {
                lookAtEnemy = false;
            }
            else
            {
                lookAtEnemy = true;
            }
            Debug.Log("Toggle Look: " + lookAtEnemy);
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
        // Do the Powerup stuff
        powerupText.text = "P x" + curPowerAmount;
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
        if (pitchInvert) pitch = -pitch;
        pitch = blend * pitch + (1 - blend) * oldPitch;
        oldPitch = pitch;

        thrust = Input.GetAxis("Thrust") * (Time.deltaTime * thrustSpeed);
        thrust = blendThrust * thrust + (1 - blendThrust) * oldThrust - 0.0004f * thrustSpeed * 2f;
        oldThrust = thrust;
        if (Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1) && !isPowerup && curPowerAmount > 0)
        {
            StartCoroutine(DoSachiPowerUp());
        }
        
    }

    void FixedUpdate()
    {
        anim.SetFloat("CurSpeed", curSpeed);
        simpleControls = Settings.simpleControls;
        pitchInvert = Settings.invertPitch;
        if (lookAtEnemy)
        {
            targetLook.position = FindObjectOfType<BossMovement>().gameObject.transform.position;
        }
        else
        {
            targetLook.localPosition = new Vector3(yaw * lookSens + roll * lookSens / 2, -pitch * lookSens, 5f);
        }
        if (lookAtEnemy)
        {
            targetLook.position = FindObjectOfType<BossMovement>().gameObject.transform.position;
        }
        if (isCutscene)
        {
            DoCutsceneThings();
            return;
        }
        // Do the Powerup stuff
        if (isPowerup && !disablePower)
        {
            AccelerateToEnemy();
        }
        if(hp <= 0 && !isDie)
        {
            isDie = true;
        }
        if (!isDie)
        {
            if (!isPowerup)
            {
                DoRotate();
                DoSpeedThings();
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
    void DoCutsceneThings()
    {
        if (doCutsceneScaling)
        {
            curScale = Mathf.Clamp(Vector3.Distance(transform.position, Camera.main.transform.position) + pursuitMod, defaultScale, awayScale);
            anim.gameObject.transform.localScale = new Vector3(curScale, curScale, curScale);
        }
        else
        {
            anim.gameObject.transform.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
        }
        dolly.m_Speed = curSpeed;
    }

    void DoRotate()
    {
        Quaternion AddRot = Quaternion.identity;
        AddRot.eulerAngles = new Vector3(pitch, yaw, -roll);
        transform.rotation *= AddRot;

        if (simpleControls)
        {
            // Get the current euler angles
            Vector3 euler = transform.eulerAngles;

            // Normalize pitch to the range of -180 to 180
            if (euler.x > 180)
                euler.x -= 360;

            // Clamp the pitch (x-axis) between -85 and 85 degrees
            euler.x = Mathf.Clamp(euler.x, -85f, 85f);

            // Apply the clamped pitch and maintain the yaw
            transform.eulerAngles = new Vector3(euler.x, euler.y, 0f); // Keep roll as 0 if needed
        }
    }
    void DoSpeedThings()
    {
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
        powerupText.text = "P x" + curPowerAmount;
        scoreText.text = "Score: " + score;
        if (powerGauge.maxValue != powerScore) powerGauge.maxValue = powerScore;
        powerGauge.value = scoreP;
        if (curPowerAmount >= 1)
        {
            fillImageP.color = pGuageColor2;
        }
        else if (isPowerup)
        {
            fillImageP.color = pGuageColor3;
        }
        else
        {
            fillImageP.color = pGuageColor1;
        }
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

    public void TakeDamage(int dmg, float spdDamage)
    {
        if (immunity) 
        { 
            if (canPointDestroy)
            {
                audioSource.PlayOneShot(audioClips[5]);
                AddScore(500);
                StartCoroutine(pointDestroyOnCooldown());
            }
            return; 
        }
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

    public void GivePriorityToCam(int index)
    {
        int curIndex = 0;
        cameras[index].Priority = 50;
        foreach(CinemachineVirtualCamera cam in cameras)
        {
            if(curIndex != index)
            {
                cam.Priority = 0;
            }
            curIndex++;
        }
    }

    IEnumerator pointDestroyOnCooldown()
    {
        canPointDestroy = false;
        yield return new WaitForSeconds(0.2f);
        canPointDestroy = true;
    }
}
