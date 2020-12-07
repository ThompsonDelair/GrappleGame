using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMortarShootAbility",menuName = "Abilities/MortarShootAbility",order = 4)]
public class MortarShootStats : AbilityStats
{
    // how many seconds before lunging we lock our lunge direction
    public float targetLockTime;
    public float range;
    public float chargeTime;
    public float coolDown;
    public float windDown;
    public float damage;
    public GameObject bulletPrefab;
    // if true, then locks movement when casting and tries to play an animation
    public bool animate;   

    public override Ability GetAbilityInstance() {
        return new MortarShootAbility(this);
    }
}
