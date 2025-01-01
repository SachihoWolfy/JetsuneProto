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
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            doBulletBossRelativity = !doBulletBossRelativity;
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
