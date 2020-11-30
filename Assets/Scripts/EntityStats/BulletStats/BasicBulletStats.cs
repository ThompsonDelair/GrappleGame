using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBasicBulletStatsBulletStats",menuName = "Stats/BasicBulletStats",order = 2)]
public class BasicBulletStats : BulletStats
{
    public int damage;

    public override BulletBehavior GetBehavior() {
        return new BasicBulletBehavior(this);
    }
}
