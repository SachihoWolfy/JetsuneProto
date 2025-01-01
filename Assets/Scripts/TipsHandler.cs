using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TipsHandler : MonoBehaviour
{
    BossMovement boss;
    Transform bossTransform;  // Reference to the boss
    public TextMeshProUGUI promptText; // The prompt UI text element
    public float offScreenDelay = 3.0f;  // Time in seconds to wait before showing prompt
    public float timeWindow = 5f; // Time window for detecting oscillations (in seconds)
    public int maxOscillations = 3;  // Number of times the boss goes off-screen to show the "steady yourself" tip
    public float distanceThreshold = 40f; // Distance threshold to trigger oscillation detection
    public float oscillationTipDuration = 3f; // Duration for which the oscillation tip will show
    public Color color1 = Color.red; // First color (flashing color 1)
    public Color color2 = Color.yellow; // Second color (flashing color 2)
    public Color color3 = Color.white;
    public float flashSpeed = 1f; // Speed at which the colors interpolate
    public bool isDoingTutorial = false;

    private Camera mainCamera;
    private bool isBossOffScreen = false;
    private float offScreenTimer = 0f;

    private float oscillationTipTimer = 0f; // Timer for showing the oscillation tip
    private float lerpTime = 0f; // Time used to interpolate between colors

    public AudioSource audioSource;
    public AudioClip[] audioClips;

    void Start()
    {
        // Cache the main camera reference
        mainCamera = Camera.main;
        boss = FindFirstObjectByType<BossMovement>();
        bossTransform = boss.transform;
        // Hide the prompt initially
        if(promptText!=null) promptText.gameObject.SetActive(false);
        if (Settings.doTutorials)
        {
            if (!boss.isWaypoint && !boss.isCutscene && !Settings.seenBoss)
            {
                StartCoroutine(ShowBossTutorialPrompt());
            }
            else if (boss.isWaypoint && !Settings.seenGPS)
            {
                StartCoroutine(ShowTutorialPrompt());
            }
        }
    }

    void Update()
    {
        if (boss.wonGame) return;
        boss.anim.SetBool("TutorialActive", isDoingTutorial);
        if (boss.isWaypoint || boss.isCutscene)
        {
            return;
        }
        boss.anim.SetBool("TutorialActive", isDoingTutorial);
        if (Settings.doTips)
        {
            // Check for boss off-screen status and display appropriate prompt
            CheckBossOffScreen();

            // Detect oscillations (boss going off-screen) and show the "steady yourself" tip if necessary
            DetectOscillation();
        }

        // Handle the oscillation tip duration, hide it after a few seconds
        if (oscillationTipTimer > 0f)
        {
            oscillationTipTimer -= Time.deltaTime;
            if (oscillationTipTimer <= 0f)
            {
                HidePrompt(); // Hide the prompt after the duration ends
            }
        }

        // If the oscillation tip is showing, lerp between the colors
        if (oscillationTipTimer > 0f)
        {
            // Lerp between the two colors over time
            lerpTime += Time.deltaTime * flashSpeed;

            // Ensure lerpTime stays within the range of [0, 1]
            if (lerpTime > 1f) lerpTime = 0f;

            // Lerp between color1 and color2
            promptText.color = Color.Lerp(color1, color2, Mathf.PingPong(lerpTime, 1f));
        }
    }

    // Check if the boss is off-screen
    bool IsBossOffScreen()
    {
        Vector3 bossViewportPos = mainCamera.WorldToViewportPoint(bossTransform.position);
        return bossViewportPos.x < 0 || bossViewportPos.x > 1 || bossViewportPos.y < 0 || bossViewportPos.y > 1;
    }

    // Show the prompt on screen with a customizable message
    void ShowPrompt(string message)
    {
        promptText.gameObject.SetActive(true);
        promptText.text = message;
        promptText.color = color3; // Set the initial color when the tip is first shown
    }
    IEnumerator ShowBossTutorialPrompt()
    {
        int curBossHP = FindAnyObjectByType<BossMovement>().hp;
        isDoingTutorial = true;
        ShowPrompt("This is a boss");
        yield return new WaitForSeconds(5f);
        ShowPrompt("They will attack with Magic");
        yield return new WaitForSeconds(5f);
        ShowPrompt("Sachi Has no magic");
        yield return new WaitForSeconds(5f);
        ShowPrompt("You must ram into the boss to do damage\nYou can only approach at \"Mach\" (yellow speed)");
        yield return new WaitForSeconds(5f);
        ShowPrompt("> Get In Range <");
        yield return new WaitForSeconds(5f);
        while (Vector3.Distance(FindAnyObjectByType<FlightBehavior>().transform.position, boss.transform.position) > 45) 
        {
            yield return new WaitForSeconds(0.2f);
        }
        promptText.color = Color.green;
        yield return new WaitForSeconds(1f);
        ShowPrompt("The line will turn green when getting closer\nStay in the green");
        yield return new WaitForSeconds(5f);
        ShowPrompt("> Hit the boss <");
        while (boss.hp >= curBossHP)
        {
            yield return new WaitForSeconds(1f);
        }
        ShowPrompt("Good Work!");
        promptText.color = Color.green;
        yield return new WaitForSeconds(5f);
        ShowPrompt("Keep it up and good luck!");
        yield return new WaitForSeconds(5f);
        HidePrompt();
        isDoingTutorial = false;
        Settings.seenBoss = true;
    }

    IEnumerator ShowTutorialPrompt()
    {
        isDoingTutorial = true;
        ShowPrompt("Use W/S to pitch");
        while(!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)))
        {
            yield return new WaitForSeconds(1);
        }
        promptText.color = Color.green;
        yield return new WaitForSeconds(1);
        if (Settings.simpleControls)
        {
            ShowPrompt("Use A/D to turn");
            while (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                yield return new WaitForSeconds(1);
            }
        }
        else
        {
            ShowPrompt("Use Q/E to Yaw");
            while (!(Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E)))
            {
                yield return new WaitForSeconds(1);
            }
            promptText.color = Color.green;
            yield return new WaitForSeconds(1);
            ShowPrompt("Use A/D to Roll");
            while (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                yield return new WaitForSeconds(1);
            }
        }
        promptText.color = Color.green;
        yield return new WaitForSeconds(1);
        ShowPrompt("Hold M1 to Accelerate");
        while (FindAnyObjectByType<FlightBehavior>().curSpeed < 20)
        {
            yield return new WaitForSeconds(1);
        }
        promptText.color = Color.green;
        yield return new WaitForSeconds(1);
        ShowPrompt("Hold M2 to Decelerate");
        while (FindAnyObjectByType<FlightBehavior>().curSpeed > 15)
        {
            yield return new WaitForSeconds(1);
        }
        promptText.color = Color.green;
        yield return new WaitForSeconds(1);
        ShowPrompt("Press Space to Change Camera");
        while (!FindAnyObjectByType<FlightBehavior>().lookAtEnemy)
        {
            yield return new WaitForSeconds(1);
        }
        promptText.color = Color.green;
        yield return new WaitForSeconds(2);
        ShowPrompt("This is your target");
        yield return new WaitForSeconds(5f);
        ShowPrompt("You will chase down what the line points at.");
        yield return new WaitForSeconds(5f);
        ShowPrompt("Good Luck!");
        promptText.color = Color.green;
        yield return new WaitForSeconds(5f);
        HidePrompt();
        Settings.seenGPS = true;
        isDoingTutorial = false;
    }


    public IEnumerator ShowPowerupPrompt()
    {
        ShowPrompt("Use Power: M1 + M2");
        yield return new WaitForSeconds(7f);
        HidePrompt();
    }

    // Hide the prompt
    void HidePrompt()
    {
        promptText.gameObject.SetActive(false);
    }

    // Check for boss being off-screen (for "Boss out of sight" tip)
    void CheckBossOffScreen()
    {
        // If the boss is off-screen, start the timer and handle prompt showing
        if (IsBossOffScreen())
        {
            if (!isBossOffScreen)
            {
                isBossOffScreen = true;
                offScreenTimer = 0f; // Reset the timer
            }

            // Update the timer while the boss is off-screen
            offScreenTimer += Time.deltaTime;

            // If the timer exceeds the threshold, show the prompt
            if (offScreenTimer >= offScreenDelay)
            {
                PlaySound(1);
                promptText.color = Color.white;
                ShowPrompt("Press [Space] to look at target"); // Display prompt
            }
        }
        else
        {
            // If the boss is back on-screen, reset the timer and hide the prompt
            if (isBossOffScreen && !(oscillationTipTimer>0f))
            {
                isBossOffScreen = false;
                offScreenTimer = 0f; // Reset the timer
                HidePrompt();
            }
        }
    }
    private bool wasBossOffScreen;
    // Detect oscillation (boss going off-screen multiple times in a time window)
    void DetectOscillation()
    {
        /*bool isPitchUp = Input.GetAxis("Vertical") > 0; // Assuming Vertical axis is mapped for up/down pitch

        // Track pitch change (from up to down or down to up)
        if (isPitchUp != wasPitchUp)
        {
            wasPitchUp = isPitchUp; // Update pitch state
        }

        // Detect when the boss goes off-screen (only trigger once per transition)
        if (IsBossOffScreen() && !wasBossOffScreen)
        {
            // The boss has just gone off-screen (transition from on-screen to off-screen)
            offScreenCount++;

            // Reset the time window timer
            timeSinceLastOffScreen = 0f;

            // Set flag to track that the boss is now off-screen
            wasBossOffScreen = true; // Boss is off-screen now
        }
        else if (!IsBossOffScreen() && wasBossOffScreen)
        {
            // The boss has just come back on-screen (transition from off-screen to on-screen)
            wasBossOffScreen = false; // Boss is now back on-screen
        }

        // Increment time since last off-screen event
        timeSinceLastOffScreen += Time.deltaTime;

        // If we've exceeded the time window, reset the oscillation count
        if (timeSinceLastOffScreen > timeWindow)
        {
            offScreenCount = 0;
        }

        // If the boss has gone off-screen enough times within the time window, show the "steady yourself" tip
        if (offScreenCount >= maxOscillations)
        {
            PlaySound(0);
            ShowPrompt("Steady yourself!");
            offScreenCount = 0;
            oscillationTipTimer = oscillationTipDuration; // Set the duration to show the tip
        }*/
    }

    private bool canPlay = true;
    public void PlaySound(int index = 0)
    {
        audioSource.clip = audioClips[index];
        if (canPlay)
        {
            canPlay = false;
            StartCoroutine(ResetPlayClip());
            audioSource.Play();
        }
    }

    IEnumerator ResetPlayClip()
    {
        yield return new WaitForSeconds(5);
        canPlay = true;
    }
}
