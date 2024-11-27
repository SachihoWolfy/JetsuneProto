using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;  // Reference to the HUD text.
    private float elapsedTime;        // Tracks the time since the timer started.
    private bool isTimerRunning;      // Tracks whether the timer is running.

    public void StartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI(elapsedTime);
        }
    }

    private void UpdateTimerUI(float time)
    {
        // Format time as minutes:seconds.milliseconds (e.g., "1:23.45")
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 100) % 100);

        timerText.text = $"{minutes:0}:{seconds:00}.{milliseconds:00}";
    }

    public void SaveBestTime(int levelIndex)
    {
        if (Settings.bestTimes == null || Settings.bestTimes.Length <= levelIndex)
        {
            Debug.LogWarning("Settings.bestTimes is not properly initialized.");
            return;
        }

        if (Settings.bestTimes[levelIndex] == 0 || Settings.GetTotalTime(levelIndex) < Settings.bestTimes[levelIndex])
        {
            Settings.bestTimes[levelIndex] = Settings.GetTotalTime(levelIndex);
            Debug.Log($"New best time for level {levelIndex}: {elapsedTime:F2} seconds");
        }
    }
    public void SaveTimeToSettings(bool isGPS, int levelIndex)
    {
        if (isGPS)
        {
            Settings.gpsTimes[levelIndex] = elapsedTime;
            Debug.Log($"GPS Time for Level {levelIndex}: {elapsedTime:F2}");
        }
        else
        {
            Settings.bossTimes[levelIndex] = elapsedTime;
            Debug.Log($"Boss Time for Level {levelIndex}: {elapsedTime:F2}");
        }
    }
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerUI(0f);
    }
}

