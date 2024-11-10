using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DanmakuPattern
{
    public string patternName;                       // Name of the pattern for identification
    public List<PatternLayer> patternLayers = new List<PatternLayer>(); // List of layers
}
public enum BulletShape
{
    None,
    Line,
    Circle,
    Hemisphere,
    Sphere
}
public enum SpawnMotion
{
    None,
    Spiral,
    HorizontalWave,
    VerticalWave
}
public enum RotateMotion
{
    None,
    Circular,
    Spray
}

