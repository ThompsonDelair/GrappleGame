using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShoot",menuName = "Abilities/Shoot",order = 2)]
public class ShootStats : AbilityStats
{
    public float timeBetweenShots;
    public float damage;
    public GameObject bullet;
    
    public override Ability GetAbilityInstance() {
        return new ShootWithNoAnimation(this);
    }
}
