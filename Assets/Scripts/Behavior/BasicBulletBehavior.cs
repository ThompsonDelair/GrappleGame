using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBulletBehavior : BulletBehavior
{
    public BasicBulletStats stats;

    public BasicBulletBehavior(BasicBulletStats stats) {
        this.stats = stats;
    }

    public override void BulletBehaviorUpdate(Bullet b, GameData data) {
        
    }

    public override void OnActorCollision(Bullet b, Actor a,GameData data) {
        DamageSystem.DealDamage(a,stats.damage);
        OnExpire(b,data);
    }

    public override void OnExpire(Bullet b,GameData data) {
        AudioSource.PlayClipAtPoint(stats.impactSound, b.position2D, 6f);
        GameManager.main.DestroyBullet(b);
        ParticleEmitter.current.SpawnParticleEffect(GameManager.main.playerBulletHit,b.position3D,Quaternion.identity);
    }
}
