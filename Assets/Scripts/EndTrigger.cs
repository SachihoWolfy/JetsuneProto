using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EndTrigger : MonoBehaviour
{
    private FlightBehavior player;
    private void Start()
    {
        player = FindObjectOfType<FlightBehavior>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (FindAnyObjectByType<LevelTimer>())
            {
                LevelTimer levelTimer = FindAnyObjectByType<LevelTimer>();
                levelTimer.StopTimer();
                levelTimer.SaveTimeToSettings(true, FindAnyObjectByType<LevelManager>().levelIndex); // true for GPS.
                levelTimer.ResetTimer();
            }
            Settings.playerHP = player.hp;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
