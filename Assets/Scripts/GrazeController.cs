using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrazeController : MonoBehaviour
{
    public int bulletsGrazed;
    public int grazeAmountToReward;
    public int machBonus = 100;
    public float cooldown = 0.1f;
    public AudioSource audioSource;

    public AudioClip[] audioClips;
    FlightBehavior player;
    bool canGraze = true;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<FlightBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && canGraze && !player.immunity)
        {
            StartCoroutine(grazeOnCooldown());
            bulletsGrazed++;
            player.AddScore(grazeAmountToReward);
            if (player.curSpeed >= 60)
            {
                player.AddScore(machBonus);
            }
            PlaySound(0);
        }
    }
    IEnumerator grazeOnCooldown()
    {
        canGraze = false;
        yield return new WaitForSeconds(cooldown);
        canGraze = true;
    }
    public void PlaySound(int index = 0)
    {
        audioSource.PlayOneShot(audioClips[index]);
    }
}
