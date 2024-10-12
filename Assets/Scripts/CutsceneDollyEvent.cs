using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CutsceneDollyEvent : MonoBehaviour
{
    public CinemachineDollyCart polarisCart;
    public CinemachineDollyCart sachiCart;
    public CinemachinePathBase polarisTrack;
    public FlightBehavior fb;
    public BossMovement bm;
    public Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetInteger("CutsceneID", fb.cutsceneID);
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            StartCoroutine(PlayDemoAfter57());
        }
    }

    IEnumerator PlayDemoAfter57()
    {
        yield return new WaitForSeconds(57f);
        SceneManager.LoadScene(0);
    }
    public void SetPosSachi(float pos)
    {
        sachiCart.m_Position = pos;
    }
    public void SetPosPolaris(float pos)
    {
        polarisCart.m_Position = pos;
    }
    public void SetSachiOnPolarisTrack()
    {
        sachiCart.m_Path = polarisTrack;
    }
    public void triggerAttack()
    {
        fb.anim.Play("Sachi_Attack");
    }

    public void Slomo(float newTime = 0.5f)
    {
        Time.timeScale = newTime;
    }
    public void ResetTime()
    {
        Time.timeScale = 1f;
    }
    public void GivePriorityToCam(int index)
    {
        fb.GivePriorityToCam(index);
    }

    public void SendToScene(int index = 1)
    {
        SceneManager.LoadScene(index);
    }

    public void PolarisAttack()
    {
        bm.visualAnim.Play("Polaris_Threat");
    }
}
