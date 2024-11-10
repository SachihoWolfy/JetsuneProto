using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public static Settings instance;
    [SerializeField]
    public static bool simpleControls;
    [SerializeField]
    public static bool invertPitch;
    [SerializeField]
    public static bool doTutorials;
    [SerializeField]
    public static bool doTips;

    public bool simplePub;
    public bool invertPub;
    public bool tutorialPub;
    public bool tipPub;

    void Start()
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(this);
            simpleControls = true;
            invertPitch = false;
            doTutorials = true;
            doTips = true;
        }
        Application.targetFrameRate = 60;
        SceneManager.LoadScene(1);
    }
    private void Update()
    {
        simplePub = simpleControls;
        invertPub = invertPitch;
        tipPub = doTips;
        tutorialPub = doTutorials;
        if (SceneManager.GetActiveScene().buildIndex > 2)
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                PauseMenu.instance.ToggleActive();
            }
        }
        else
        {
            PauseMenu.instance.SetPauseActive(false);
        }
    }
}
