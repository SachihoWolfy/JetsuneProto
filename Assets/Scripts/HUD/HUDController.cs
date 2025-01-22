using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("UI elements")]
    public Slider speedGauge;
    public Image speedFillImage;
    public TMP_Text hpText;
    public TMP_Text powerupText;
    public TMP_Text scoreText;
    public Slider powerGauge;
    public Image fillImageP;
    public TMP_Text bossDistanceText;
    public TMP_Text gpsProgressText;
    public TMP_Text winText;
    public Slider bossHPSlider;
    public Image bossHPSliderFillImage;
    private LineRenderer lineRenderer;
    public Image[] cosmetics;
    public Image[] proxCosmetics;

    [Header("References and data")]
    private FlightBehavior player;
    private BossMovement boss;
    public ScoreDisplay miniScore;
    public float maxDistance = 100f;
    public float minDistance = 10f;
    public float minFlashSpeed = 1f;
    public float maxFlashSpeed = 5f;
    public float alphaLerpSpeed = 5f;

    [Header("Colors")]
    public Color pGuageColor1 = Color.red;
    public Color pGuageColor2 = Color.yellow;
    public Color pGuageColor3 = Color.green;
    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    private Color healthColor;



    [Header("Health Colors")]
    public Color healthColor4;
    public Color healthColor3;
    public Color healthColor2;
    public Color healthColor1;

    [Header("Boss Info")]
    public string bossName;
    public Color bossColor;
    // Internal state
    private float currentAlpha = 1f;
    private float previousDistance = float.MaxValue;
    private float distanceChangeRate;

    private void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<FlightBehavior>();
        }
        if (boss == null)
        {
            boss = FindFirstObjectByType<BossMovement>();
            if (boss.isWaypoint)
            {
                bossHPSlider.maxValue = 100;
            }
            else
            {
                bossHPSlider.maxValue = boss.hp;
            }
            bossHPSliderFillImage.color = bossColor;
        }
        if (lineRenderer == null && player != null)
        {
            lineRenderer = player.lineRenderer;
        }
        healthColor = hpText.color;
    }

    public void UpdateUI()
    {
        UpdateSpeedGauge();
        UpdateHealthUI();
        UpdatePowerUpUI();
        UpdateScoreUI();
        UpdateBossDistanceUI();
        UpdateWinText();
        UpdateMini();
    }

    private void UpdateSpeedGauge()
    {
        speedGauge.value = player.curSpeed;

        if (player.curSpeed > 60)
        {
            speedFillImage.color = Color.yellow;
        }
        else if (player.curSpeed > 40)
        {
            speedFillImage.color = Color.cyan;
        }
        else
        {
            speedFillImage.color = Color.red;
        }
    }

    private void UpdateHealthUI()
    {
        if (player.hp > 0)
        {
            hpText.text = "x" + player.hp;
            switch (player.hp)
            {
                case 4:
                    hpText.color = healthColor4;
                    break;
                case 3:
                    hpText.color = healthColor3;
                    break;
                case 2:
                    hpText.color = healthColor2;
                    break;
                case 1:
                    hpText.color = healthColor1;
                    break;
                default:
                    hpText.color = healthColor;
                    break;
            }
            foreach(Image cosmetic in cosmetics)
            {
                Color target = hpText.color;
                target.a = 0.5f;
                cosmetic.color = target;
            }
        }
        else
        {
            hpText.text = "<!>";
            hpText.color = FlashColor();
            foreach (Image cosmetic in cosmetics)
            {
                cosmetic.color = hpText.color;
            }
        }
    }

    private void UpdatePowerUpUI()
    {
        if (powerGauge.maxValue != player.powerScore)
        {
            powerGauge.maxValue = player.powerScore;
        }

        powerGauge.value = FlightBehavior.scoreP;

        if (FlightBehavior.curPowerAmount >= 1)
        {
            fillImageP.color = pGuageColor2;
        }
        else if (player.isPowerup)
        {
            fillImageP.color = pGuageColor3;
        }
        else
        {
            fillImageP.color = pGuageColor1;
        }

        powerupText.text = FlightBehavior.curPowerAmount >= 1 ? ">Ready!<" : ">>P>>";
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "" + FlightBehavior.score;
    }

    private void UpdateBossDistanceUI()
    {
        if (boss == null || bossDistanceText == null)
            return;
        if (lineRenderer == null)
        {
            lineRenderer = player.lineRenderer;
        }
        float distance = Mathf.Clamp(Vector3.Distance(boss.transform.position, player.transform.position) - 6.4f, 0, float.MaxValue);

        if (previousDistance != float.MaxValue)
        {
            distanceChangeRate = previousDistance - distance;
        }

        if (distance < maxDistance && player.curSpeed > 40)
        {
            if (distance == 0)
            {
                bossDistanceText.color = Color.yellow;
                bossDistanceText.text = "BULLSEYE!";
            }
            else
            {
                float flashSpeed = Mathf.Lerp(minFlashSpeed, maxFlashSpeed, Mathf.InverseLerp(maxDistance, minDistance, distance));
                float targetAlpha = Mathf.PingPong(Time.time * flashSpeed, 1.0f);
                currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * alphaLerpSpeed);

                Color targetColor = distanceChangeRate < 0 ? Color.Lerp(Color.white, Color.red, Mathf.Clamp01(Mathf.Abs(distanceChangeRate) * 5)) :
                                                              Color.Lerp(Color.white, Color.green, Mathf.Clamp01(Mathf.Abs(distanceChangeRate) * 5));

                targetColor.a = 1;
                bossDistanceText.color = targetColor;
                bossDistanceText.text = distance < previousDistance ? ">>Prox: " + distance.ToString("F1") + "<<" : "!Prox: " + distance.ToString("F1") + "!";
                lineRenderer.startColor = targetColor;
            }
        }
        else
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
            );
            lineRenderer.colorGradient = gradient;
            bossDistanceText.color = new Color(bossDistanceText.color.r, bossDistanceText.color.g, bossDistanceText.color.b, 0);
        }
        foreach(Image cosmetic in proxCosmetics)
        {
            cosmetic.color = bossDistanceText.color;
        }
        previousDistance = distance;
    }

    private void UpdateWinText()
    {
        if (boss.isWaypoint)
        {
            if (Vector3.Distance(player.transform.position, boss.transform.position) < boss.distanceToMaintain * 2 || !FindAnyObjectByType<SplineRenderer>())
            {
                boss.gpsProgress = (boss.cart.SplinePosition / boss.cart.Spline.Spline.GetLength() * 100);
                winText.text = "Follow GPS: " + boss.gpsProgress.ToString("F0") + "%";
                bossHPSlider.value = boss.gpsProgress;
            }
            else
            {
                winText.text = "<!>GPS ERROR<!>";
            }
        }
        else
        {
            winText.text = bossName;
            bossHPSlider.value = boss.hp;
        }
    }

    private Color FlashColor()
    {
        return Color.Lerp(Color.yellow, Color.red, Mathf.PingPong(Time.time, 1));
    }

    private void UpdateMini()
    {
        miniScore.UpdateScore(FlightBehavior.score);
    }
}
