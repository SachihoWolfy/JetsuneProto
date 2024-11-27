using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CutsceneDollyEvent : MonoBehaviour
{
    public CinemachineSplineCart polarisCart;
    public CinemachineSplineCart sachiCart;
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
        if (sachiCart == null) { sachiCart = fb.gameObject.GetComponent<CinemachineSplineCart>(); }
        if(polarisCart == null) { polarisCart = bm.cart; }
        if (SceneManager.GetActiveScene().buildIndex == 2)
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
        sachiCart.SplinePosition = pos;
    }
    public void SetPosPolaris(float pos)
    {
        polarisCart.SplinePosition = pos;
    }
    public void SetSachiOnPolarisTrack()
    {
        sachiCart.Spline = polarisCart.Spline;
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
    public void ResetRoll()
    {
        // Get the current euler angles
        Vector3 euler = fb.transform.eulerAngles;

        // Normalize pitch to the range of -180 to 180
        if (euler.x > 180)
            euler.x -= 360;

        // Clamp the pitch (x-axis) between -85 and 85 degrees
        euler.x = Mathf.Clamp(euler.x, -85f, 85f);

        // Apply the clamped pitch and maintain the yaw
        fb.transform.eulerAngles = new Vector3(euler.x, euler.y, 0f); // Keep roll as 0 if needed
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
