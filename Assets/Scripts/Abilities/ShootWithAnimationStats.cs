using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShootWithAnimation",menuName = "Abilities/ShootWithAnimation",order = 3)]
public class ShootWithAnimationStats : AbilityStats
{
    public float targetLockTime;
    public float range;
    public float chargeTime;
    public float coolDown;
    public float windDown;
    public float damage;
    public BulletStats bulletStats;

    public override Ability GetAbilityInstance() {
        return new ShootWithAnimation(this);
    }
}
