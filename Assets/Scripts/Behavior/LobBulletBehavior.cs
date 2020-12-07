using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this bullet behavior makes the bullet travel to a set location over period of time
//     defined by the min/max travel speed of its stats
// it will spawn a burning hazard on impact
public class LobBulletBehavior : BulletBehavior
{
    public LobBulletStats stats;

    public Vector2 start;
    public Vector2 end;
    float timestamp;
    float traveltime;

    // the warning visual before bullet impact
    GameObject warning;
       
    public LobBulletBehavior(LobBulletStats s) {
        stats = s;
        timestamp = Time.time;
    }

    public override void BulletBehaviorUpdate(Bullet b,GameData data) {

        if(traveltime == 0) {
            float diff = stats.travelTimeMax - stats.travelTimeMin;
            float percent = Vector2.Distance(start,end) / stats.travelTimeMaxDist;
            if (percent > 1f) {
                percent = 1f;
            }
            traveltime = stats.travelTimeMin + diff * percent;
        }

        if(warning == null) {
            warning = GameObject.Instantiate(stats.warningPrefab);
            warning.transform.position = Utils.Vector2XZToVector3(end);
        }

        float lerpVal = (Time.time - timestamp) / traveltime;

        if(lerpVal > 1f) {
            OnExpire(b,data);
            return;
        }

        // this block of code makes the visual for the bullet travel in an arc to its destinatino
        Transform bulletGraphics = b.transform.Find("BulletGraphics");
        Vector2 midpoint2D = end + (start - end) / 2;
        Utils.debugStarPoint(Utils.Vector2XZToVector3(midpoint2D),1f,CustomColors.orange);
        Vector3 midpoint3D = Utils.Vector2XZToVector3(midpoint2D) + Vector3.up * stats.lobHeight;
        Vector3 start3D = Utils.Vector2XZToVector3(start);
        Vector3 end3D = Utils.Vector2XZToVector3(end);
        Vector3 curve1 = Vector3.Lerp(start3D,midpoint3D,lerpVal);
        Vector3 curve2 = Vector3.Lerp(midpoint3D,end3D,lerpVal);
        bulletGraphics.position = Vector3.Lerp(curve1,curve2,lerpVal);
    }

    public override void OnActorCollision(Bullet b,Actor a,GameData data) {
        
    }

    public override void OnExpire(Bullet b,GameData data) {
        GameObject.Destroy(warning);
        GameManager.main.SpawnArea(stats.burnArea,end);
        AudioSource.PlayClipAtPoint(stats.impactSound,b.position3D,6f);
        GameManager.main.DestroyBullet(b);
        ParticleEmitter.current.SpawnParticleEffect(GameManager.main.playerBulletHit,b.position3D,Quaternion.identity);
    }
}
