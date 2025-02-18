using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GPSProgress : MonoBehaviour
{
    private BossMovement boss;
    private FlightBehavior player;
    public List<TMP_Text> progressText;
    private string winText;
    // Start is called before the first frame update
    void Start()
    {
        player = FindAnyObjectByType<FlightBehavior>();
        boss = FindAnyObjectByType<BossMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (boss.isWaypoint)
        {
            if (Vector3.Distance(player.transform.position, boss.transform.position) < boss.distanceToMaintain * 2 || !FindAnyObjectByType<SplineRenderer>())
            {
                boss.gpsProgress = (boss.cart.SplinePosition / boss.cart.Spline.Spline.GetLength() * 100);
                winText = "GPS " + boss.gpsProgress.ToString("F0") + "%";
            }
            else
            {
                winText = "<!>GPS ERROR<!>";
            }
        }
        foreach(TMP_Text sign in progressText)
        {
            sign.text = winText;
        }
    }
}
