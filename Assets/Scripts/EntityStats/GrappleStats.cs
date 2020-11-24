using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrappleStats",menuName = "ScriptableObjects/GrappleStats",order = 2)]
public class GrappleStats : ScriptableObject
{
    public float stopDist = 0.6f;
    public float launchSpeed = 60f;
    public float rewindSpeed = 30f;
    public float grappleRange = 20f;
}
