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

    public bool simplePub;
    public bool invertPub;

    void Start()
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(this);
            simpleControls = true;
            invertPitch = false;
        }
        SceneManager.LoadScene(1);
    }
    private void Update()
    {
        simplePub = simpleControls;
        invertPub = invertPitch;
    }
}
