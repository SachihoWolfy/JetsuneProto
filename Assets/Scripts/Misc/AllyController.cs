using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyController : MonoBehaviour
{
    public Animator characterAnim;
    public Animator allyAnim;
    FlightBehavior player;
    BossMovement boss;
    public Transform visualTransform;
    public Transform lookTransform;
    Transform targetToLook;
    public bool isAllyLevel;
    public bool activeAlly;
    private bool hasBeenActivated;
    public bool shootBoss;
    public AdvProjectileSpawner turret;
    bool oldActiveAlly;
    // Start is called before the first frame update
    void Start()
    {
        if (!isAllyLevel)
        {
            Destroy(gameObject);
        }
        player = GetComponentInParent<FlightBehavior>();
        boss = FindFirstObjectByType<BossMovement>();
        characterAnim.SetBool("IsAlly", true);
        LookAt(boss.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (boss.hp == 2 && !hasBeenActivated)
        {
            ActivateAlly();
        }
        if(boss.hp == 1 && hasBeenActivated)
        {
            activeAlly = false;
        }
        allyAnim.SetBool("Active", activeAlly);
        if (activeAlly)
        {
            turret.gameObject.SetActive(true);
            turret.targetBoss = shootBoss;
            characterAnim.SetBool("IsAlly", true);
            characterAnim.SetFloat("CurSpeed", player.curSpeed);
            allyAnim.SetBool("UsingPower", player.isPowerup);
            LeanAlly();
            if (lookTransform && targetToLook)
            {
                lookTransform.position = targetToLook.position;
            }
        }
        else
        {
            turret.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            activeAlly = !activeAlly;
        }
        if (oldActiveAlly && !activeAlly)
        {
            allyAnim.Play("Ally_Fallback");
        }
        oldActiveAlly = activeAlly;
    }

    void ActivateAlly()
    {
        hasBeenActivated = true;
        activeAlly = true;
    }

    void LeanAlly()
    {
        if(visualTransform)
        visualTransform.rotation = player.sachiVisual.rotation;
    }

    public void LookAt(Transform objectTransform)
    {
        targetToLook = objectTransform;
    }
}
