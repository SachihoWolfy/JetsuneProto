using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;

public class BossMovement : MonoBehaviour
{
    public int charID = 0;
    public bool isCutscene = false;
    public bool isWaypoint = false;
    public float gpsProgress;
    public int cutsceneID = 0;
    public float distanceToMaintain = 300f;
    public int hp = 5;
    public float speed;
    public float engageSpeed = 60f;
    public float engageFlightSpeed = 60f;
    public float warningDistance;
    public float pursuitMod = 1f;
    public FlightBehavior player;
    public CinemachineSplineCart cart;
    public TextMeshProUGUI winText;
    public Transform playerOffset;
    public GameObject targetLook;
    public AudioClip[] clips;
    public AudioSource audioSource;
    public Animator anim;
    public Animator visualAnim;
    private bool isMach;
    public GameObject enemyVisual;
    public GameObject waypointVisual;
    private float defaultScale = 11.26f;
    public float awayScale = 20f;
    public float enemyScale;
    public GameObject summonCircle;

    public bool stickPlayerToOffset = false;
    public bool wonGame = false;

    public ProjectileSpawner[] spawners;
    public AdvProjectileSpawner[] advSpawners;

    public bool engaging = false;
    private bool preventAttack = false;
    public bool isTest;

    private void Start()
    {
        cart = GetComponent<CinemachineSplineCart>();
        player = FindAnyObjectByType<FlightBehavior>();
        anim.SetInteger("Cutscene", cutsceneID);
        anim.SetBool("IsCutscene", isCutscene);
        anim.SetBool("IsWaypoint", isWaypoint);
        anim.SetInteger("HP", hp);
        anim.SetInteger("CharID", charID);
        anim.SetBool("ComposerTest", isTest);
        visualAnim.SetInteger("HP", hp);
        if (isWaypoint)
        {
            enemyVisual.SetActive(false);
            waypointVisual.SetActive(true);
            anim.Play("Stop");
            foreach(ProjectileSpawner spawner in spawners)
            {
                spawner.gameObject.SetActive(false);
            }
            foreach (AdvProjectileSpawner spawner in advSpawners)
            {
                spawner.gameObject.SetActive(false);
            }
            winText.text = "Follow GPS";
        }
        else
        {
            winText.text = "Boss HP: " + hp;
            waypointVisual.SetActive(false);
        }
    }
    bool rotatePlayerToFaceBoss = false;
    private void Update()
    {
        if (stickPlayerToOffset)
        {
            // Update player position
            player.transform.position = playerOffset.position;
        }
        if (rotatePlayerToFaceBoss)
        {
            // Make the player look at the target while maintaining an upright orientation
            Vector3 lookDirection = enemyVisual.transform.position - player.transform.position;
            if (lookDirection.sqrMagnitude > 0.001f) // Ensure lookDirection is non-zero
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                player.transform.rotation = targetRotation;
            }
            player.rb.velocity = player.transform.forward * player.rb.velocity.magnitude;
        }
    }
    private void FixedUpdate()
    {
        var autodolly = cart.AutomaticDolly.Method as SplineAutoDolly.FixedSpeed;
        enemyScale = Mathf.Clamp(Vector3.Distance(player.transform.position, transform.position) + pursuitMod, defaultScale, awayScale);
        enemyVisual.transform.localScale = new Vector3(enemyScale, enemyScale, enemyScale);
        float relativescale = enemyScale / defaultScale;
        float scaleOffset = 1f;
        summonCircle.transform.localScale = new Vector3(relativescale + scaleOffset, relativescale + scaleOffset, relativescale + scaleOffset);
        targetLook.transform.position = player.transform.position;
        if (isWaypoint)
        {
            if (FindAnyObjectByType<SplineRenderer>())
            {
                if (!FindAnyObjectByType<SplineRenderer>().isShowing)
                    FindAnyObjectByType<SplineRenderer>().ShowLine();
            }
        }
        if (isCutscene)
        {
            if (autodolly != null)
            {
                speed = autodolly.Speed - 5f;
            }
            return;
        }
        engaging = player.curSpeed >= engageSpeed;

        if (!engaging)
        {
            speed = player.curSpeed;
            if (isMach) { isMach = false; }
        }

        if (Vector3.Distance(player.transform.position, transform.position) != distanceToMaintain && (!engaging||isWaypoint))
        {
            speed = player.curSpeed * distanceToMaintain / Vector3.Distance(player.transform.position, transform.position);
        }

        if (engaging && !isWaypoint)
        {
            speed = engageFlightSpeed;
            distanceToMaintain = Mathf.Clamp(distanceToMaintain - 0.1f, 50f, distanceToMaintain);
            if (!isMach) { isMach = true; }
        }

        visualAnim.SetBool("IsMach", isMach);
        if (autodolly != null)
        {
            autodolly.Speed = speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        anim.SetInteger("HP", hp);
        visualAnim.SetInteger("HP", hp);
        if (isCutscene || isWaypoint)
        {
            return;
        }
        if (other.CompareTag("Player") && hp > 0 && !wonGame && !preventAttack)
        {
            player.AddScore(2000);
            preventAttack = true;
            player.transform.position = playerOffset.position;
            hp += -1;
            StartCoroutine(SlowPlayer());
            StartCoroutine(ResetAttack());
        }
    }
    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(2f);
        preventAttack = false;
    }
    IEnumerator SlowPlayer()
    {
        player.LockSpeed(true);
        player.disablePower = true;
        stickPlayerToOffset = true;
        rotatePlayerToFaceBoss = true;
        player.immunity = true;

        if (player.ally)
        {
            player.ally.shootBoss = true;
        }

        StartCoroutine(player.ImmunityReset(5f));
        FindObjectOfType<CinemachineBrain>().DefaultBlend.Time = 0.7f;
        if (hp != 0)
        {
            visualAnim.Play("Hurt");
            player.hitCam.Priority = 30;
            yield return new WaitForSeconds(0.7f);
        }
        if (!player.isPowerup)
        {
            player.anim.Play("Sachi_Attack");
            if (player.GetComponentInChildren<AllyController>())
            {
                AllyController ally = player.GetComponentInChildren<AllyController>();
                ally.characterAnim.Play("Ally_Attack");
            }
        }
        else
        {
            player.anim.Play("JavAttack");
        }

        player.StopPowerup();
        yield return new WaitForSeconds(0.3f);
        player.PlaySound(1);
        player.sonicBoom.Play();
        if (hp <= 0)
        {
            player.LockSpeed(false);
            ResetPlayerSpeed();
            player.hitCam.Priority = 0;
        }
        else
        {
            player.attackAudio.Play();
            player.rb.velocity *= 0.6f;
        }
        if (hp == 0 && !wonGame)
        {
            wonGame = true;
            anim.SetBool("Died", wonGame);
            PlaySound(4);
            if (FindAnyObjectByType<LevelManager>())
            {
                StartCoroutine(AdvancedEndGame());
            }
            else
            {
                StartCoroutine(EndGame());
            }
        }

        yield return new WaitForSeconds(0.2f);
        stickPlayerToOffset = false;
        player.bulletDeleter.SetActive(true);
        yield return new WaitForFixedUpdate();
        player.bulletDeleter.SetActive(false);
        yield return new WaitForSeconds(1f);
        if(hp > 0)
        {
            player.LockSpeed(false);
            ResetPlayerSpeed();
            player.hitCam.Priority = 0;
        }
        yield return new WaitForSeconds(1f);

        rotatePlayerToFaceBoss = false;
        player.lookAtEnemy = false;
        yield return new WaitForSeconds(1f);

        if (player.ally)
        {
            player.ally.shootBoss = false;
        }

        player.disablePower = false;
        FindObjectOfType<CinemachineBrain>().DefaultBlend.Time = 0.2f;
    }
    IEnumerator GradualDeceleration(FlightBehavior player)
    {
        const float decelerationRate = 0.9f; // How much the velocity decreases per step
        const float decelerationDuration = 0.5f; // Total duration of deceleration
        const int steps = 3; // Number of deceleration steps

        float timePerStep = decelerationDuration / steps;

        for (int i = 0; i < steps; i++)
        {
            player.rb.velocity *= decelerationRate;
            yield return new WaitForSeconds(timePerStep);
        }
    }


    void ResetPlayerSpeed()
    {
        player.rb.velocity = new Vector3(0, 0, 0);
        player.curSpeed = 3f;
    }
    IEnumerator RestartGame()
    {
        int i = 5;
        winText.text = "Restarting in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Restarting in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Restarting in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Restarting in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Restarting in: " + i;
        PlaySound(6);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    IEnumerator EndGame()
    {
        int i = 5;
        winText.text = "Returning to menu in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Returning to menu in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Returning to menu in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Returning to menu in: " + i;
        i--;
        PlaySound(5);
        yield return new WaitForSeconds(1f);
        winText.text = "Returning to menu in: " + i;
        PlaySound(6);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(2);
    }

    IEnumerator AdvancedEndGame()
    {
        FindAnyObjectByType<LevelTimer>().StopTimer();
        FindAnyObjectByType<LevelManager>().HideHud();
        FindAnyObjectByType<LevelManager>().StopMusic();
        yield return new WaitForSeconds(3f);
        FindAnyObjectByType<LevelManager>().EndLevel();
        yield return new WaitForSeconds(1f);
        FindAnyObjectByType<LevelManager>().PlayEndMusic();
    }

    public void SpawnBullet(int index = 0)
    {
        spawners[index].SpawnBullet();
    }
    public void SpawnMissile(int index = 0)
    {
        spawners[index].SpawnMissile();
    }
    void SpawnProjectilesInCircle(int index = 0)
    {
        spawners[index].SpawnProjectilesInCircle();
    }

    public void ToggleLookAtPlayer(int index = 0)
    {
        spawners[index].lookAtPlayer = !spawners[index].lookAtPlayer;
    }

    public void DisableLookAtPlayer()
    {
        foreach(ProjectileSpawner spawner in spawners)
        {
            spawner.lookAtPlayer = false;
        }
    }
    public void FireAllBullets()
    {
        foreach(ProjectileSpawner spawner in spawners)
        {
            spawner.SpawnBullet();
        }
    }
    public void FireAllCircles()
    {
        foreach (ProjectileSpawner spawner in spawners)
        {
            spawner.SpawnProjectilesInCircle();
        }
    }

    public void SpawnProjectilesInHemisphere(int index = 0)
    {
        spawners[index].SpawnProjectilesInHemisphere();
    }

    public void SpawnProjectilesInSphere(int index = 0)
    {
        spawners[index].SpawnProjectilesInSphere();
    }

    public void FireTheAdvancedSpawner(int index = 0)
    {
        if(advSpawners[index])
        {
            advSpawners[index].isFiring = true;
        }
    }

    public void StopTheAdvancedSpawner(int index = 0)
    {
        if (advSpawners[index])
        {
            advSpawners[index].isFiring = false;
        }
    }

    public void PlaySound(int index = 0)
    {
        audioSource.clip = clips[index];
        audioSource.Play();
    }
}
