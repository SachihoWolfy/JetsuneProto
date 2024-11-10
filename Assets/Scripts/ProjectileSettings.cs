using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileSettings
{
    public int damage = 1;
    public float speedDamage = 40f;
    public float bulletSpeed = 10f;
    public float radius = 5f;
    public float inwardSpeed = 5f;
    public int projectileCount = 10;
    public bool useMaterial1 = true;
    public Material mat1;
}
