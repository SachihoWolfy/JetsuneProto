using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;
    public Toggle invertPitchToggle;
    public Toggle simpleControlsToggle;
    public Toggle tipToggle;
    public bool stopToggleSwitching;
    void Start()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        stopToggleSwitching = true;
        invertPitchToggle.isOn = Settings.invertPitch;
        simpleControlsToggle.isOn = Settings.simpleControls;
        tipToggle.isOn = Settings.doTips;
        stopToggleSwitching = false;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    public void ToggleInvert()
    {
        if (!stopToggleSwitching)
            Settings.invertPitch = !Settings.invertPitch;
    }

    public void ToggleControls()
    {
        if (!stopToggleSwitching)
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
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SetPauseActive(false);
    }
    public void ExitGame()
    {
        SceneManager.LoadScene(2);
        SetPauseActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void SetPauseActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
