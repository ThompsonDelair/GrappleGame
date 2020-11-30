using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletBehavior
{
    public abstract void BulletBehaviorUpdate(Bullet b, GameData data);
    public abstract void OnActorCollision(Bullet b,Actor a, GameData data);
    public abstract void OnExpire(Bullet b,GameData data);
}
