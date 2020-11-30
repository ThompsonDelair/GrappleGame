using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMortarShootAbility",menuName = "Abilities/MortarShootAbility",order = 4)]
public class MortarShootStats : AbilityStats
{
    public float targetLockTime;
    public float range;
    public float chargeTime;
    public float coolDown;
    public float windDown;
    public float damage;
    public GameObject bulletPrefab;
    public bool animate;   


    public override Ability GetAbilityInstance() {
        return new MortarShootAbility(this);
    }
}
