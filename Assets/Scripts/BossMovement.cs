using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;

public class BossMovement : MonoBehaviour
{
    public float distanceToMaintain = 300f;
    public int hp = 5;
    public float speed;
    public float engageSpeed = 60f;
    public float engageFlightSpeed = 60f;
    public float warningDistance;
    public float pursuitMod;
    public FlightBehavior player;
    public CinemachineDollyCart cart;
    public TextMeshProUGUI winText;
    public Transform playerOffset;
    public AudioClip[] clips;
    public AudioSource audio;
    public Animator anim;

    public bool attackSequence = false;
    public bool wonGame = false;

    public ProjectileSpawner[] spawners;

    public bool engaging = false;

    private void Start()
    {
        cart = GetComponent<CinemachineDollyCart>();
        player = FindAnyObjectByType<FlightBehavior>();
        winText.text = "Engage Boss - Boss HP: " + hp;
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
        engaging = player.curSpeed >= engageSpeed;

        if(!engaging) { 
            speed = player.curSpeed;
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
        }
        cart.m_Speed = speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && hp > 0 && !wonGame)
        {
            player.transform.position = playerOffset.position;
            player.anim.SetTrigger("Attack");
            StartCoroutine(SlowPlayer());
            hp += -1;
            winText.text = "Boss HP: " + hp.ToString();
        }
        if(hp == 0 && !wonGame)
        {
            wonGame = true;
            anim.SetBool("Died", wonGame);
            PlaySound(4);
            StartCoroutine(RestartGame());
        }
    }

    IEnumerator SlowPlayer()
    {
        attackSequence = true;
        yield return new WaitForSeconds(0.3f);
        player.PlaySound(1);
        attackSequence = false;
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
        SceneManager.LoadScene(0);
    }

    public void SpawnBullet(int index = 0)
    {
        spawners[index].SpawnBullet();
    }
    public void SpawnMissile(int index = 0)
    {
        spawners[index].SpawnMissile();
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

    public void PlaySound(int index = 0)
    {
        audio.clip = clips[index];
        audio.Play();
    }
}
