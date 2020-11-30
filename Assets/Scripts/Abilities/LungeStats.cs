using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLunge",menuName = "Abilities/Lunge",order = 1)]
public class LungeStats : AbilityStats
{
    public float range;
    public float chargeTime;
    public float lungeForce;
    public float lungeFriction;
    public float coolDown;
    public float damage;
    //public float lockPercent;
    public float targetLockTime;

    public GameObject warningPrefab;

    public override Ability GetAbilityInstance() {
        return new Lunge(this);
    }
}
