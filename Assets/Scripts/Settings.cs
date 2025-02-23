using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

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

    public static bool doBulletBossRelativity;

    public static bool seenGPS;
    public static bool seenBoss;

    public GameObject pauseMenu;

    public bool simplePub;
    public bool invertPub;
    public bool tutorialPub;
    public bool tipPub;

    [Header("OtherObjects")]
    public GameObject explosion;
    public static GameObject explosionPrefab;

    [Header("Flags and vars")]
    public static int playerHP = 5;

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
            explosionPrefab = explosion;
        }
        filePath = Path.Combine(Application.persistentDataPath, "bestTimes.json");
        LoadBestTimes();
        Application.targetFrameRate = 60;
        if(SceneManager.GetActiveScene().buildIndex<1) SceneManager.LoadScene(1);
    }
    private void Update()
    {
        simplePub = simpleControls;
        invertPub = invertPitch;
        tipPub = doTips;
        tutorialPub = doTutorials;
        if (SceneManager.GetActiveScene().buildIndex > 2)
        {
            UpdateFrameRate();
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {
                PauseMenu.instance.ToggleActive();
            }
            else if (!Application.isFocused)
            {
                PauseMenu.instance.SetPauseActive(true);
            }
        }
        else
        {
            PauseMenu.instance.SetPauseActive(false);
            playerHP = 5;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            doBulletBossRelativity = !doBulletBossRelativity;
        }
    }
    //Declare these in your class
    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    public static float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;

    void UpdateFrameRate()
    {
        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense.
            m_lastFramerate = (float)m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;
        }
    }
    // Lets seperate this stuff so I don't get confused. This is for times!

    public static float[] gpsTimes = new float[14];  // GPS part times for 7 levels.
    public static float[] bossTimes = new float[14]; // Boss fight times for 7 levels.
    public static float[] bestTimes = new float[14]; // Default size for 14 level parts.
    private static string filePath;

    public static void SaveBestTimes()
    {
        BestTimesData data = new BestTimesData
        {
            gpsTimes = gpsTimes,
            bossTimes = bossTimes
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Best times saved to {filePath}");
    }

    public static void LoadBestTimes()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            BestTimesData data = JsonUtility.FromJson<BestTimesData>(json);
            gpsTimes = data.gpsTimes;
            bossTimes = data.bossTimes;
            Debug.Log("Best times loaded successfully.");
        }
        else
        {
            Debug.LogWarning("Best times file not found. Using default values.");
        }
    }
    public static float GetTotalTime(int levelIndex)
    {
        return gpsTimes[levelIndex] + bossTimes[levelIndex];
    }
}
