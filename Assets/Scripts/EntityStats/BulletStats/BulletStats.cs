using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BulletStats : ScriptableObject
{
    public float speed;
    public float radius;
    public Team targetTeam;
    public AudioClip impactSound;

    public abstract BulletBehavior GetBehavior();
}
