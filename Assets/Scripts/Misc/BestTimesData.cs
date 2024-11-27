using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BestTimesData
{
    public float[] gpsTimes = new float[14];
    public float[] bossTimes = new float[14];
    public float[] bestTimes = new float[14]; // Array to store times for 14 level parts.
}
