using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class BossMovement : MonoBehaviour
{
    public float distanceToMaintain = 300f;
    public int hp = 5;
    public float speed;
    public float engageSpeed = 60f;
    public float warningDistance;
    public float pursuitMod;
    public FlightBehavior player;
    public CinemachineDollyCart cart;
    public TextMeshProUGUI winText;

    public bool engaging = false;

    private void Start()
    {
        cart = GetComponent<CinemachineDollyCart>();
        player = FindAnyObjectByType<FlightBehavior>();
        winText.text = "Engage Boss - Boss HP: " + hp;
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
            speed = engageSpeed - 5f;
            distanceToMaintain = Mathf.Clamp(distanceToMaintain - 0.1f, 50f, distanceToMaintain);
        }
        cart.m_Speed = speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && hp > 0)
        {
            player.rb.velocity = new Vector3(0, 0, 0);
            player.curSpeed = 3f;
            player.anim.SetTrigger("Attack");
            hp += -1;
        }
        winText.text = "Boss HP: " + hp.ToString();
        if(hp == 0)
        {
            winText.text = "-[Insert Win Here]-";
        }
    }
}
