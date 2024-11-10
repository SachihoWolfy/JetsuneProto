using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CutsceneDollyEvent : MonoBehaviour
{
    public CinemachineDollyCart polarisCart;
    public CinemachineDollyCart sachiCart;
    public CinemachinePathBase polarisTrack;
    public FlightBehavior fb;
    public BossMovement bm;
    public Animator anim;
    public Toggle invertPitchToggle;
    public Toggle simpleControlsToggle;
    public Toggle tutorialToggle;
    public Toggle tipToggle;
    public bool stopToggleSwitching;

    private void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetInteger("CutsceneID", fb.cutsceneID);
        if(SceneManager.GetActiveScene().buildIndex == 2)
        {
            Time.timeScale = 1;
            StartCoroutine(PlayDemoAfter57());
            stopToggleSwitching = true;
            invertPitchToggle.isOn = Settings.invertPitch;
            simpleControlsToggle.isOn = Settings.simpleControls;
            tutorialToggle.isOn = Settings.doTutorials;
            tipToggle.isOn = Settings.doTips;
            stopToggleSwitching = false;
        }
    }

    IEnumerator PlayDemoAfter57()
    {
        yield return new WaitForSeconds(57f);
        SceneManager.LoadScene(1);
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

    public void ToggleObject(GameObject obj)
    {
        bool isActive = obj.activeSelf;
        obj.SetActive(!isActive);
    }
    public void ToggleInvert()
    {
        if(!stopToggleSwitching)
        Settings.invertPitch = !Settings.invertPitch;
    }

    public void ToggleControls()
    {
        if(!stopToggleSwitching)
        Settings.simpleControls = !Settings.simpleControls;
    }
    public void ToggleTutorials()
    {
        if (!stopToggleSwitching)
            Settings.doTutorials = !Settings.doTutorials;
    }
    public void ToggleTips()
    {
        if (!stopToggleSwitching)
            Settings.doTips = !Settings.doTips;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
