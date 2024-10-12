using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;

public class BossMovement : MonoBehaviour
{
    public bool isCutscene = false;
    public int cutsceneID = 0;
    public float distanceToMaintain = 300f;
    public int hp = 5;
    public float speed;
    public float engageSpeed = 60f;
    public float engageFlightSpeed = 60f;
    public float warningDistance;
    public float pursuitMod = 1f;
    public FlightBehavior player;
    public CinemachineDollyCart cart;
    public TextMeshProUGUI winText;
    public Transform playerOffset;
    public GameObject targetLook;
    public AudioClip[] clips;
    public AudioSource audioSource;
    public Animator anim;
    public Animator visualAnim;
    private bool isMach;
    public GameObject enemyVisual;
    private float defaultScale = 11.26f;
    public float awayScale = 20f;
    public float enemyScale;

    public bool attackSequence = false;
    public bool wonGame = false;

    public ProjectileSpawner[] spawners;

    public bool engaging = false;
    private bool preventAttack = false;

    private void Start()
    {
        cart = GetComponent<CinemachineDollyCart>();
        player = FindAnyObjectByType<FlightBehavior>();
        winText.text = "Engage Boss - Boss HP: " + hp;
        anim.SetInteger("Cutscene", cutsceneID);
        anim.SetBool("IsCutscene", isCutscene);
    }

    private void Update()
    {
        if (attackSequence)
        {
            player.transform.position = playerOffset.position;
            player.transform.LookAt(transform.position);
        }
    }
    private void FixedUpdate()
    {
        enemyScale = Mathf.Clamp(Vector3.Distance(player.transform.position, transform.position) + pursuitMod, defaultScale, awayScale);
        enemyVisual.transform.localScale = new Vector3(enemyScale, enemyScale, enemyScale);
        targetLook.transform.position = player.transform.position;
        if (isCutscene)
        {
            speed = cart.m_Speed - 5f;
            return;
        }
        engaging = player.curSpeed >= engageSpeed;

        if (!engaging)
        {
            speed = player.curSpeed;
            if (isMach) { isMach = false; }
        }

        if (Vector3.Distance(player.transform.position, transform.position) < distanceToMaintain && !engaging)
        {
            speed = player.curSpeed * distanceToMaintain / Vector3.Distance(player.transform.position, transform.position);
        }
        if (Vector3.Distance(player.transform.position, transform.position) > distanceToMaintain && !engaging)
        {
            speed = player.curSpeed * distanceToMaintain / Vector3.Distance(player.transform.position, transform.position);
        }
        if (engaging)
        {
            speed = engageFlightSpeed;
            distanceToMaintain = Mathf.Clamp(distanceToMaintain - 0.1f, 50f, distanceToMaintain);
            if (!isMach) { isMach = true; }
        }
        visualAnim.SetBool("IsMach", isMach);
        cart.m_Speed = speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCutscene)
        {
            return;
        }
        if (other.CompareTag("Player") && hp > 0 && !wonGame && !preventAttack)
        {
            preventAttack = true;
            player.transform.position = playerOffset.position;
            player.anim.SetTrigger("Attack");
            StartCoroutine(SlowPlayer());
            StartCoroutine(ResetAttack());
            hp += -1;
            winText.text = "Boss HP: " + hp.ToString();
        }
        if(hp == 0 && !wonGame)
        {
            wonGame = true;
            player.cameras[0].Priority = 100;
            anim.SetBool("Died", wonGame);
            PlaySound(4);
            StartCoroutine(EndGame());
        }
    }
    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(2f);
        preventAttack = false;
    }
    IEnumerator SlowPlayer()
    {
        attackSequence = true;
        player.immunity = true;
        StartCoroutine(player.ImmunityReset(3f));
        player.cameras[1].Priority = 30;
        yield return new WaitForSeconds(0.3f);
        player.PlaySound(1);
        attackSequence = false;
        player.rb.velocity = new Vector3(0, 0, 0);
        player.curSpeed = 3f;
        yield return new WaitForSeconds(2f);
        FindObjectOfType<CinemachineBrain>().m_DefaultBlend.m_Time = 0.7f;
        player.cameras[1].Priority = 10;
        player.cameras[0].Priority = 20;
        player.lookAtEnemy = false;
        yield return new WaitForSeconds(1f);
        FindObjectOfType<CinemachineBrain>().m_DefaultBlend.m_Time = 0.2f;
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
        SceneManager.LoadScene(1);
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

    public void PlaySound(int index = 0)
    {
        audioSource.clip = clips[index];
        audioSource.Play();
    }
}
