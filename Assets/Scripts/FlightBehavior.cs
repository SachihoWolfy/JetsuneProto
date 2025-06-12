using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using UnityEngine.Splines;

public class FlightBehavior : MonoBehaviour
{
    public GameObject visualObj;
    public Transform sachiVisual;
    public GameObject bulletDeleter;
    public bool isNight;
    private bool navLightsEnabled;
    public ParticleSystem[] wingTips;
    public GameObject[] navLights;
    public BossMovement boss;
    public bool isCutscene = false;
    public int cutsceneID = 0;
    public Animator cutsceneAnim;
    public Animator anim;
    public AudioSource audioSource;
    public AudioSource[] deathAudio;
    public AudioSource spinAudio;
    public AudioSource attackAudio;
    public CinemachineCamera deathCam;
    public CinemachineCamera hitCam;
    public GameObject deathExplosion;
    public ParticleSystem smoke;
    public ParticleSystem fire;
    public ParticleSystem sonicBoom;
    public ParticleSystem nearSonicBoom;
    public AudioClip[] audioClips;
    public CinemachineCamera[] cameras;
    public bool simpleControls = true;
    public bool pitchInvert = false;
    public bool lookAtEnemy;

    public float blend = 0.9f;
    public float blendThrust = 0.9f;
    public float pitchSpeed = 120f;
    public float pitchSpeedMach = 80f;
    public float rollSpeed = 120f;
    public float maxLeanAngle = 45f; // Lean
    public float maxPitchAngle = 30f; // Leeeeean
    public float leanSpeed = 5f;
    public float yawSpeed = 40f;
    public float thrustSpeed;
    private float MINSPEED = 0f;

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

    private HUDController hud;

    public Slider speedGauge;
    public Image fillImage;
    public LineRenderer lineRenderer;
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
    public TextMeshProUGUI bossDistanceText;

    private bool isDie;
    private bool isDead;

    private bool sonicBoomHappened = false;

    private float awayScale = 40f;
    private float defaultScale = 11.26f;
    private float pursuitMod = -20f;
    private float curScale;
    public bool doCutsceneScaling = false;

    CinemachineCamera virtualCamera;
    CinemachineSplineCart dolly;

    public int hp = 1;
    public bool immunity = false;

    public bool tryingPowerup;
    public bool isPowerup;
    public bool disablePower;
    public static int curPowerAmount;
    int dampingPowerUp = 2;
    public float powerSpeed = 67;

    public static int score = 0;
    public static int scoreP = 0;
    int oldScore;
    int oldScoreP;
    public int lifeScore = 10000;
    public int powerScore = 5000;
    private bool canPointDestroy = true;

    private ScoreDisplay scoreDisplay;
    public GameObject backupSettings;
    public AllyController ally;

    bool speedLock;
    private bool GODMODE = false;
    public bool devSachi = false;
    public GameObject devTerrain;
    public bool NOWOOSH = false;
    public void LockSpeed(bool value)
    {
        speedLock = value;
    }

    private void Start()
    {
        if (!Settings.instance)
        {
            Instantiate(backupSettings);
        }
        if(Settings.playerHP < 1)
        {
            Settings.playerHP = 1;
        }
        hp = Settings.playerHP;
        if (curPowerAmount >= 1)
        {
            scoreP = powerScore;
        }
        virtualCamera = FindAnyObjectByType<CinemachineCamera>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        scoreDisplay = FindAnyObjectByType<ScoreDisplay>();
        boss = FindFirstObjectByType<BossMovement>();
        ally = GetComponentInChildren<AllyController>();
        hud = FindFirstObjectByType<HUDController>();
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
        if (isNight)
        {
            EnableNavLights();
        }
        else
        {
            DisableNavLights();
        }
        if (isCutscene)
        {
            cutsceneAnim.SetInteger("Cutscene", cutsceneID);
            cutsceneAnim.SetBool("IsCutscene", isCutscene);
        }
        if (GetComponent<CinemachineSplineCart>())
        {
            dolly = GetComponent<CinemachineSplineCart>();
        }
        simpleControls = Settings.simpleControls;
        pitchInvert = Settings.invertPitch;
        if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            Debug.Log("Reseting Score");
            score = 0;
            scoreP = 0;
            curPowerAmount = 0;
            hp = 5;
            Settings.seenGPS = false;
            Settings.seenBoss = false;
        }
        var mainModule = wingTips[0].main;
        initialWingtipColor = mainModule.startColor.color;
    }
    public BossMovement GetBoss()
    {
        return boss;
    }
    public void EnableNavLights()
    {
        foreach(GameObject navLight in navLights)
        {
            navLight.SetActive(true);
        }
    }
    public void DisableNavLights()
    {
        foreach (GameObject navLight in navLights)
        {
            navLight.SetActive(false);
        }
    }

    public void SetNavLights(bool value)
    {
        foreach (GameObject navLight in navLights)
        {
            navLight.SetActive(value);
        }
    }
    IEnumerator DoSachiPowerUp()
    {
        anim.Play("Sachi_Javilin");
        spinAudio.Play();
        PlaySound(0);
        curPowerAmount--;
        powerupText.text = ">>P>>";
        immunity = true;
        isPowerup = true;
        anim.SetBool("IsPowerup", true);
        gameObject.GetComponent<Animator>().SetBool("IsPowerup", true);
        if(hp < 1)
        {
            hp = 1;
        }
        yield return new WaitForSeconds(7);
        spinAudio.Stop();
        anim.SetBool("IsPowerup", false);
        gameObject.GetComponent<Animator>().SetBool("IsPowerup", false);
        StartCoroutine(ImmunityReset(2));
        isPowerup = false;
        scoreP = 0;
    }

    public void StopPowerup()
    {
        StopCoroutine(DoSachiPowerUp());
        spinAudio.Stop();
        anim.SetBool("IsPowerup", false);
        GetComponent<Animator>().SetBool("IsPowerup", false);
        StartCoroutine(ImmunityReset(2));
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
            FindAnyObjectByType<GrazeController>().PlaySound(1);
            Mathf.Clamp(hp++,0,9);
        }
        if ((scoreP % powerScore) < (oldScoreP % powerScore) && curPowerAmount < 1)
        {
            FindAnyObjectByType<GrazeController>().PlaySound(2);
            FindAnyObjectByType<TipsHandler>().StartCoroutine(FindAnyObjectByType<TipsHandler>().ShowPowerupPrompt());
            Mathf.Clamp(curPowerAmount++,0,1);
        }
        /*if ((score % lifeScore) < (oldScore % lifeScore)|| (score % powerScore) < (oldScore % powerScore))
        {
            FindAnyObjectByType<GrazeController>().PlaySound(1);
        }*/
    }

    void AccelerateToEnemy()
    {
        if(isPowerup)
        immunity = true;
        Vector3 bossPos = boss.gameObject.transform.position;
        var lookPos = bossPos - transform.position;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampingPowerUp);
        curSpeed = powerSpeed;
        rb.velocity = transform.forward * curSpeed;
    }
    void DoLineRender()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, boss.transform.position);
        if (boss.wonGame)
        {
            lineRenderer.enabled = false;
        }
        else
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, boss.transform.position);
        }
    }
    private bool spaceTapped = false;
    private float holdDuration = 0.3f; 
    private float holdTime = 0f; 
    private bool spaceHeld = false;
    private bool isLookStored = false;
    void Update()
    {
        if (isCutscene)
        {
            return;
        }
        DoLineRender();
        //Cam Stuff
        if (Input.GetKey(KeyCode.Space))
        {
            holdTime += Time.deltaTime;
            if (holdTime >= holdDuration)
            {
                spaceHeld = true;
            }
        }
        else if(Input.GetKey(KeyCode.Mouse3)|| Input.GetKey(KeyCode.Mouse4)|| Input.GetKey(KeyCode.Mouse5)|| Input.GetKey(KeyCode.Mouse6))
        {
            spaceHeld = true;
        }
        else
        {
            holdTime = 0f;
            spaceHeld = false;
        }
        spaceTapped = Input.GetKeyUp(KeyCode.Space);
        CinemachineCore.GetInputAxis = CinemachineCore.GetInputAxis = GetAxisCustom;
        bool storedLookAtEnemy = lookAtEnemy;
        if (spaceTapped)
        {
            if (lookAtEnemy)
            {
                lookAtEnemy = false;
            }
            else
            {
                lookAtEnemy = true;
            }
        }
        if (spaceHeld && cameras[4].IsValid && !FindAnyObjectByType<LevelManager>().isEnding) {
            cameras[4].Priority = 30;
            if (!isLookStored)
            {
                storedLookAtEnemy = lookAtEnemy;
                isLookStored = true;
            }
        }
        else
        {
            if(cameras[4].Priority > 5)
            {
                lookAtEnemy = storedLookAtEnemy;
                isLookStored = false;
            }
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
            cameras[4].Priority = 5;
        }
        // Do the Powerup stuff
        //powerupText.text = "P x" + curPowerAmount;
        if (curPowerAmount >= 1) { powerupText.text = ">Ready!<"; }
        else { powerupText.text = ">>P>>"; }
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

        if (curSpeed > 60)
        {
            pitch = Input.GetAxis("Vertical") * (Time.fixedDeltaTime * pitchSpeedMach);
        }
        else if (curSpeed > 40)
        {
            pitch = Input.GetAxis("Vertical") * (Time.fixedDeltaTime * (pitchSpeedMach + 10));
        }
        else
        {
            pitch = Input.GetAxis("Vertical") * (Time.fixedDeltaTime * pitchSpeed);
        }
        if (pitchInvert) pitch = -pitch;
        pitch = blend * pitch + (1 - blend) * oldPitch;
        oldPitch = pitch;

        thrust = Input.GetAxis("Thrust") * (Time.fixedDeltaTime * thrustSpeed);
        thrust = blendThrust * thrust + (1 - blendThrust) * oldThrust - 0.0004f * thrustSpeed * 2f;
        oldThrust = thrust;
        if (Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1) && !isPowerup && curPowerAmount > 0)
        {
            StartCoroutine(DoSachiPowerUp());
        }
        if(devSachi)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                GODMODE = !GODMODE;
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                devTerrain.SetActive(!devTerrain.activeSelf);
                FindAnyObjectByType<SplineContainer>().gameObject.GetComponent<LineRenderer>().enabled = !devTerrain.activeSelf;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                PauseMenu.instance.RestartGame();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                NOWOOSH = !NOWOOSH;
            }
        }
       
        /* if(Input.GetKeyDown(KeyCode.Mouse1) && !Input.GetKey(KeyCode.Mouse0) && curSpeed > 40 && canParry)
        {
            DoParry();
        } */
        
    }
    /*
    private bool isParrying;
    private bool canParry = true;
    private bool successfulParry;
     void DoParry()
    {
        isParrying = true;
        canParry = false;
        anim.Play("Sachi_Attack");
        StartCoroutine(CooldownParry());
    } 
    IEnumerator CooldownParry()
    {
        yield return new WaitForSeconds(0.5f);
        isParrying = false;
        canParry = true;
        if (successfulParry)
        {
            curSpeed = 59;
        }
        else
        {
            rb.velocity = rb.velocity.normalized * 25f;
            curSpeed = 30f;
        }
    }
    */
    public float GetAxisCustom(string axisName)
    {
        if (axisName == "Mouse X")
        {
            if (spaceHeld)
            {
                return Input.GetAxis("Mouse X");
            }
            else
            {
                return 0;
            }
        }
        else if (axisName == "Mouse Y")
        {
            if (spaceHeld)
            {
                return Input.GetAxis("Mouse Y");
            }
            else
            {
                return 0;
            }
        }
        return Input.GetAxis(axisName);
    }

    void FixedUpdate()
    {
        anim.SetFloat("CurSpeed", curSpeed);
        simpleControls = Settings.simpleControls;
        pitchInvert = Settings.invertPitch;
        if (lookAtEnemy)
        {
            targetLook.position = boss.enemyVisual.transform.position;
        }
        else
        {
            targetLook.localPosition = new Vector3(yaw * lookSens + roll * lookSens / 2, -pitch * lookSens, 5f);
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
        if(hp <= 0 && !isDying)
        {
            isDying = true;
            StartCoroutine(DieSequence());
        }
        if (!isDie)
        {
            if (!isPowerup)
            {
                DoRotate();
                if (!speedLock)
                {
                    DoSpeedThings();
                }
            }
        }
        else
        {
            rb.velocity = new Vector3(0, 0, 0);
        }
        /*else if(!isDead)
        {
            Die();
            isDead = true;
            Restart();
        }*/
        //updateUI();
        hud.UpdateUI();
    }
    void DoCutsceneThings()
    {
        var autodolly = dolly.AutomaticDolly.Method as SplineAutoDolly.FixedSpeed;
        if (doCutsceneScaling)
        {
            curScale = Mathf.Clamp(Vector3.Distance(transform.position, Camera.main.transform.position) + pursuitMod, defaultScale, awayScale);
            anim.gameObject.transform.localScale = new Vector3(curScale, curScale, curScale);
        }
        else
        {
            anim.gameObject.transform.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
        }
        if (autodolly != null)
        {
            autodolly.Speed = curSpeed;
        }
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
        LeanSachi();
    }
    void LeanSachi()
    {
        if (sachiVisual != null)
        {
            // Calculate target angles for roll and pitch
            float targetLeanAngle;
            if(simpleControls) targetLeanAngle = Mathf.Clamp(yaw * -maxLeanAngle, -maxLeanAngle, maxLeanAngle);
            else targetLeanAngle = Mathf.Clamp(roll * -maxLeanAngle, -maxLeanAngle, maxLeanAngle);
            float targetPitchAngle = Mathf.Clamp(pitch * maxPitchAngle, -maxPitchAngle, maxPitchAngle);

            float targetYawAngle = 0f;
            if (!simpleControls)
            {
                targetPitchAngle = Mathf.Clamp(pitch * maxPitchAngle, -10, 10);
                targetLeanAngle = Mathf.Clamp(roll * -maxLeanAngle, -5, 5);
                targetYawAngle = Mathf.Clamp(yaw * maxPitchAngle, -5, 5);
            }

            // Smoothly interpolate current angles towards target angles
            float currentLeanAngle = Mathf.LerpAngle(sachiVisual.localEulerAngles.z, targetLeanAngle, Time.deltaTime * leanSpeed);
            float currentPitchAngle = Mathf.LerpAngle(sachiVisual.localEulerAngles.x, targetPitchAngle, Time.deltaTime * leanSpeed);
            float currentYawAngle = Mathf.LerpAngle(sachiVisual.localEulerAngles.y, targetYawAngle, Time.deltaTime * leanSpeed);

            // Create separate quaternions for pitch, yaw, and roll
            Quaternion pitchRotation = Quaternion.Euler(currentPitchAngle, 0, 0);
            Quaternion yawRotation = Quaternion.Euler(0, currentYawAngle, 0);
            Quaternion rollRotation = Quaternion.Euler(0, 0, currentLeanAngle);

            // Combine pitch, yaw, and roll rotations
            sachiVisual.localRotation = pitchRotation * yawRotation * rollRotation;
        }
    }
    void DoSpeedThings()
    {
        curSpeed = rb.velocity.magnitude;

        // Handle smooth acceleration from near-zero speed
        if (curSpeed < 3f && Mathf.Abs(thrust) > 0.01f)
        {
            curSpeed += thrust * Time.deltaTime * 5; // Gradually accelerate based on thrust
        }
        else if (curSpeed < 3f)
        {
            curSpeed = Mathf.Max(0f, curSpeed - 0.1f * Time.deltaTime *5); // Gradually decelerate to hover
        }

        oldSpeed = curSpeed;

        // Adjust thrust impact on speed
        float adjustedThrust = thrust;
        if (curSpeed > 3)
        {
            curSpeed = Mathf.Clamp(curSpeed + adjustedThrust / Mathf.Max(curSpeed, 0.1f), MINSPEED, maxSpeed);
        }

        // Handle deceleration
        if (oldSpeed > curSpeed && curSpeed > 3)
        {
            curSpeed = Mathf.Clamp(curSpeed + adjustedThrust / 5f, MINSPEED, maxSpeed);
        }
        // Handle acceleration boost for specific ranges
        else if (curSpeed > 40 && curSpeed < 60)
        {
            if (!sonicBoomHappened)
            {
                if (!sonicBoom.IsAlive())
                    sonicBoom.Play();
                PlaySound(0);
            }
            curSpeed = Mathf.Clamp(curSpeed + adjustedThrust * 2, MINSPEED, maxSpeed);
        }

        // Handle near-sonic boom effects
        if (oldSpeed <= curSpeed && curSpeed > 38 && curSpeed < 40 && !nearSonicBoom.isPlaying)
        {
            nearSonicBoom.Play();
        }
        else if (nearSonicBoom.isPlaying)
        {
            nearSonicBoom.Stop();
        }

        // Apply the updated velocity
        rb.velocity = transform.forward * curSpeed;

        // Update animation parameters
        anim.SetFloat("CurSpeed", curSpeed);
        anim.SetBool("Decelerating", oldSpeed > curSpeed);

        // Update animation triggers based on speed
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

    public float minDistance = 5f;
    public float maxDistance = 30f;
    public float minFlashSpeed = 0.5f;  // Speed when far
    public float maxFlashSpeed = 5.0f;  // Speed when close
    public float alphaLerpSpeed = 5f;
    private float currentFlashSpeed;
    private float currentAlpha = 0f; // Current alpha value for smoother transitions

    // Variables to track the distance change rate
    private float previousDistance = float.MaxValue;
    private float distanceChangeRate = 0f;
    Color color1 = Color.red; // First color (flashing color 1)
    Color color2 = Color.yellow; // Second color (flashing color 2)
    public float flashSpeed = 1f; // Speed at which the colors interpolate
    private float lerpTime = 0f; // Time used to interpolate between colors
    private Color initialWingtipColor;
    private bool clearedWingtips;
    public Color FlashColor()
    {
        lerpTime += Time.fixedDeltaTime * flashSpeed;
        if (lerpTime > 1f) lerpTime = 0f;
        Color flashColor = Color.Lerp(color1, color2, Mathf.PingPong(lerpTime, 1f));
        return flashColor;
    }
    void ColorTheWingTips(Color pGC)
    {
        if (!clearedWingtips)
        {
            ClearWingtips();
        }
        Color emissiveColor = pGC;

        // Loop through each particle system
        foreach (ParticleSystem ps in wingTips)
        {
            // Access the renderer for this particle system
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();

            // Ensure the renderer has a material
            if (renderer != null && renderer.material != null)
            {
                // Create an instance of the material to avoid affecting the original
                Material instanceMaterial = renderer.trailMaterial;
                instanceMaterial.color = pGC;
                // Check if the material supports emissive color changes
                if (instanceMaterial.HasProperty("_EmissionColor"))
                {
                    // Enable emission keyword if necessary (for Standard shaders)
                    instanceMaterial.EnableKeyword("_EMISSION");

                    // Set the emissive color
                    instanceMaterial.SetColor("_EmissionColor", emissiveColor);

                    // Assign the instanced material back to the renderer
                    renderer.material = instanceMaterial;
                }
            }
        }
    }
    void ClearWingtips()
    {
        foreach (ParticleSystem ps in wingTips)
        {
            ps.Clear();
            ps.Play();
        }
        clearedWingtips = true;
    }
    private int curPowerState;
    private int oldPowerState;
    public void updateUI()
    {
        speedGauge.value = curSpeed;
        if (curSpeed > 60)
        {
            fillImage.color = Color.yellow;
        }
        else if (curSpeed > 40)
        {
            fillImage.color = Color.cyan;
        }
        else
        {
            fillImage.color = Color.red;
        }
        if (hp > 0)
        {
            hpText.text = "HP: " + hp;
            hpText.color = Color.white;
        }
        else
        {
            hpText.text = "<!>";
            hpText.color = FlashColor();
        }
        //powerupText.text = "P x" + curPowerAmount;
        if (curPowerAmount >= 1) { powerupText.text = ">Ready!<"; }
        else { powerupText.text = ">>P>>"; }
        scoreText.text = "Score: " + score;
        if (powerGauge.maxValue != powerScore) powerGauge.maxValue = powerScore;
        powerGauge.value = scoreP;
        if(oldPowerState != curPowerState)
        {
            clearedWingtips = false;
        }
        oldPowerState = curPowerState;
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
        if (scoreDisplay != null) scoreDisplay.UpdateScore(score);
        if (bossDistanceText != null && boss != null)
        {
            // Calculate the distance between the player and the boss
            float distance = Mathf.Clamp(Vector3.Distance(boss.transform.position, transform.position) - 6.4f,0,float.MaxValue);

            // If within max distance, adjust flashing and visibility
            if (previousDistance != float.MaxValue)
            {
                distanceChangeRate = previousDistance - distance;
            }

            // Check if within the max visible range
            if (distance < maxDistance && curSpeed > 40)
            {
                // Update the text with the current proximity distance
                bossDistanceText.text = "!Prox: " + distance.ToString("F1") + "!";

                // Calculate flash speed based on distance (closer = faster)
                float flashSpeed = Mathf.Lerp(minFlashSpeed, maxFlashSpeed, Mathf.InverseLerp(maxDistance, minDistance, distance));

                // Calculate the target alpha for the flashing effect
                float targetAlpha = Mathf.PingPong(Time.time * flashSpeed, 1.0f);

                // Smoothly transition the current alpha to the target alpha
                currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * alphaLerpSpeed);

                // Clamp currentAlpha to ensure it reaches full transparency (0) or opacity (1)
                if (Mathf.Abs(currentAlpha - targetAlpha) < 0.01f)
                {
                    currentAlpha = targetAlpha;
                }

                // Determine color based on distance change rate (approaching vs moving away)
                Color targetColor;

                if (distance < previousDistance && curSpeed>40)
                {
                    // Player is getting closer (fade to green)
                    // Use the absolute rate of change (no need for speed)
                    float greenIntensity = Mathf.Clamp01(Mathf.Abs(distance-previousDistance)*5);
                    targetColor = Color.Lerp(Color.white, Color.green, greenIntensity);
                    if(greenIntensity > 0.5f) bossDistanceText.text = ">>Prox: " + distance.ToString("F1") + "<<";
                    else bossDistanceText.text = ">Prox: " + distance.ToString("F1") + "<";
                }
                else
                {
                    // Player is getting farther away (fade to red)
                    // Use the absolute rate of change (no need for speed)
                    float redIntensity = Mathf.Clamp01(Mathf.Abs(distance - previousDistance)*5);
                    targetColor = Color.Lerp(Color.white, Color.red, redIntensity);
                    if (redIntensity > 0.5f) bossDistanceText.text = "!! Prox: " + distance.ToString("F1") + " !!";
                    else bossDistanceText.text = "! Prox: " + distance.ToString("F1") + " !";
                }

                // Apply the smoothed alpha to the target color
                targetColor.a = currentAlpha;
                bossDistanceText.color = targetColor;
                lineRenderer.startColor = targetColor;
            }
            else
            {
                // Hide the text if beyond max distance
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.2f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
                );
                lineRenderer.colorGradient = gradient;
                bossDistanceText.color = new Color(bossDistanceText.color.r, bossDistanceText.color.g, bossDistanceText.color.b, 0);
            }
            previousDistance = distance;
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
    bool isDying;
    IEnumerator DieSequence()
    {
        //Start Warning
        deathAudio[0].Play();
        smoke.Play();
        yield return new WaitForSeconds(6f);
        deathAudio[0].Stop();
        if (hp < 1)
        {
            deathAudio[1].Play();
            fire.Play();
            yield return new WaitForSeconds(4f);
        }
        else
        {
            smoke.Stop();
        }
        //Explode and Spawn explosion, stop smoke, and do death cam.
        if (hp < 1)
        {
            immunity = true;
            isDie = true;
            lineRenderer.enabled = false;
            fire.Stop();
            smoke.Stop();
            deathAudio[1].Stop();
            deathAudio[2].Play();
            rb.velocity = new Vector3(0, 0, 0);
            curSpeed = 3f;
            visualObj.SetActive(false);
            deathExplosion.SetActive(true);
            deathCam.Priority = 300;
            yield return new WaitForSeconds(0.25f);
            deathAudio[3].Play();
            Restart();
        }
        else
        {
            isDie = false;
            isDying = false;
            fire.Stop();
            smoke.Stop();
            deathAudio[1].Stop();
        }
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

    public void TakeDamage(int dmg, float spdDamage, bool isGround = false)
    {
        if (GODMODE) { return; }
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
        hp = Mathf.Clamp(hp-dmg,0,7);
        curSpeed = Mathf.Clamp(curSpeed - spdDamage,MINSPEED,maxSpeed);
        rb.velocity = transform.forward * curSpeed;
        anim.Play("Damaged");
        if (isGround)
        {
            PlaySound(6);
        }
        else
        {
            PlaySound(2);
        }
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
        foreach(CinemachineCamera cam in cameras)
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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Ground") && curSpeed > 40)
        {
            TakeDamage(1, 37, true);
        }
    }
}
