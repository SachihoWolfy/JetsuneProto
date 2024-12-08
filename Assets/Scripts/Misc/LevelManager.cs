using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public LevelTimer levelTimer;
    public TextMeshProUGUI bossTimerText;
    public TextMeshProUGUI GPSTimerText;
    public TextMeshProUGUI totalTimerText;
    public AudioSource endAudio;
    public AudioClip endMusicClip;
    public GameObject HUD;
    public GameObject EndScreen;
    public GameObject playerVis;
    public GameObject enemyVis;
    public AudioSource Music;
    public int levelIndex;
    public bool isEnding = false;

    public CinemachineCamera endCam;

    private void Start()
    {
        levelTimer = FindAnyObjectByType<LevelTimer>();
        EndScreen.SetActive(false);
        endAudio.volume = Music.volume;
        StartLevel();
    }

    private void StartLevel()
    {
        levelTimer.StartTimer();
    }

    public void EndLevel()
    {
        levelTimer.StopTimer();
        levelTimer.SaveTimeToSettings(false, levelIndex);
        levelTimer.SaveBestTime(levelIndex);

        float totalTime = Settings.GetTotalTime(levelIndex);
        Debug.Log($"Total Time for Level {levelIndex}: {totalTime:F2}");

        ShowEndScreen();
    }

    public void HideHud()
    {
        HUD.SetActive(false);
    }

    public void HideEntities()
    {
        playerVis = FindAnyObjectByType<FlightBehavior>().anim.gameObject;
        enemyVis = FindAnyObjectByType<BossMovement>().visualAnim.gameObject;
        if (FindAnyObjectByType<AllyController>())
        {
            var ally = FindAnyObjectByType<AllyController>();
            ally.activeAlly = false;
        }
        playerVis.SetActive(false);
        enemyVis.SetActive(false);
        FindAnyObjectByType<JetSoundController>().gameObject.SetActive(false);
    }
    public void StopMusic()
    {
        FadeOutAndStop(Music, 2f);
    }
    public void PlayEndMusic()
    {
        endAudio.clip = endMusicClip;
        endAudio.Play();
    }
    public void FadeOutAndStop(AudioSource audioSource, float fadeDuration)
    {
        if (audioSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(audioSource, fadeDuration));
        }
    }
    private string SecondstoTime(float time)
    {
        // Format time as minutes:seconds.milliseconds (e.g., "1:23.45")
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 100) % 100);

        string timeString = $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        return timeString;
    }

    // Coroutine for fading out the volume
    private IEnumerator FadeOutCoroutine(AudioSource audioSource, float fadeDuration)
    {
        float startVolume = audioSource.volume;

        // Gradually reduce the volume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null; // Wait until the next frame
        }

        // Ensure the volume reaches zero
        audioSource.volume = 0;

        // Stop the audio source
        audioSource.Stop();

        // Restore the original volume in case the audio source is reused
        audioSource.volume = startVolume;
    }
    private void ShowEndScreen()
    {
        bossTimerText.text = SecondstoTime(Settings.bossTimes[levelIndex]);
        GPSTimerText.text = SecondstoTime(Settings.gpsTimes[levelIndex]);
        totalTimerText.text = SecondstoTime(Settings.GetTotalTime(levelIndex));
        isEnding = true;
        endCam.Prioritize();
        endCam.Priority = 230;
        EndScreen.SetActive(true);
        HideEntities();
        // Transition to the end screen and show the final time.
        Debug.Log("End Screen!");
        StartCoroutine(ToNextLevel());
    }
    private void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void ToMenu()
    {
        SceneManager.LoadScene(2);
    }

    IEnumerator ToNextLevel()
    {
        yield return new WaitForSeconds(15f);
        Settings.gpsTimes[levelIndex] = 0f;
        Settings.bossTimes[levelIndex] = 0f;
        if (SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings)
            NextLevel();
        else
            ToMenu();
    }
}
