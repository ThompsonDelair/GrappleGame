using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLunge",menuName = "Abilities/Lunge",order = 1)]
public class LungeStats : AbilityStats
{
    public float range;
    public float chargeTime;
    // speed of the lunge
    public float lungeForce;
    // how fast lunge speed decays
    public float lungeFriction;
    public float coolDown;
    public float damage;
    // how many seconds before lunging we lock our lunge direction
    public float targetLockTime;

    // a object spawned as a visual warning of the lunge area when lunge is used
    public GameObject warningPrefab;

    public override Ability GetAbilityInstance() {
        return new Lunge(this);
    }
}
