using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public GameObject miniScore;
    public TextMeshProUGUI scoreText;
    private FlightBehavior player;
    public float displayDuration = 2f;

    private int startingScore = 0;
    private int accumulatedScoreDifference = 0;
    private int lastScore = 0;
    private float timer = 0f;
    private bool isScoreChanging = false;

    public Slider powerGauge;
    public Image fillImageP;
    public TextMeshProUGUI powerupText;

    void Start()
    {
        if (miniScore != null)
            miniScore.SetActive(false);  // Initially hide the object
        if (powerGauge != null)
            powerGauge.gameObject.SetActive(false);
        player = FindAnyObjectByType<FlightBehavior>();
    }

    public void UpdateScore(int currentScore)
    {
        // Check if score has changed
        if (currentScore != lastScore)
        {
            if (!isScoreChanging)
            {
                // Set the starting score the first time the score changes
                startingScore = lastScore;
                accumulatedScoreDifference = 0;
            }

            // Calculate the difference between the starting score and current score
            accumulatedScoreDifference = currentScore - startingScore;

            // Show the score object and reset the timer
            if (miniScore != null)
                miniScore.SetActive(true);
            if (powerGauge != null)
                powerGauge.gameObject.SetActive(true);

            // Update the displayed score difference
            if (scoreText != null)
                scoreText.text = "+" + accumulatedScoreDifference;

            isScoreChanging = true;
            timer = displayDuration;
            lastScore = currentScore;
        }
        else if (isScoreChanging)
        {
            // Countdown timer while the score is not changing
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                // Hide the score object and reset the accumulated difference when the timer reaches zero
                if (miniScore != null)
                    miniScore.SetActive(false);
                if (powerGauge != null)
                    powerGauge.gameObject.SetActive(false);

                isScoreChanging = false;
                accumulatedScoreDifference = 0;
            }
        }
        if (FlightBehavior.curPowerAmount >= 1) { powerupText.text = ">Ready!<"; }
        else { powerupText.text = ">>P>>"; }
        if (powerGauge.maxValue != player.powerScore) powerGauge.maxValue = player.powerScore;
        powerGauge.value = FlightBehavior.scoreP;
        if (FlightBehavior.curPowerAmount >= 1)
        {
            fillImageP.color = player.pGuageColor2;
        }
        else if (player.isPowerup)
        {
            fillImageP.color = player.pGuageColor3;
        }
        else
        {
            fillImageP.color = player.pGuageColor1;
        }
    }
}

