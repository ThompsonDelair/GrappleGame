using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// shoots using a bullet prefab
// plays animation and locks movement while ability is casting
public class ShootWithAnimation : Ability
{
    ShootWithAnimationStats stats;
    float timestamp;
    bool shot;
    bool lockedon;

    public ShootWithAnimation(ShootWithAnimationStats s) {
        stats = s;
    }

    public override bool StartAbilityCheck(Actor a,GameData data) {
        if (timestamp < Time.time) // check if ability is on cool down
        {

            if (Vector2.Distance(a.position2D,data.player.position2D) >= stats.range) // Range check
            {
                return false; // OUT OF RANGE
            }

            // check line of sight
            if (!data.map.ClearSightLine(a.position2D,data.player.position2D)) {
                return false;
            }

            timestamp = Time.time + stats.chargeTime;
            // Disable movement
            a.currMovement = Movement.NONE;
            a.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

            a.transform.GetComponent<Animator>().SetTrigger("StartAttack");
            shot = false;
            lockedon = false;
            a.transform.LookAt(data.player.position3D);
            return true; // ability is ready to execute
        }
        return false; // ability is not ready to execute
    }

    public override bool RunAbilityUpdate(Actor a,GameData data) {


        if (Time.time > timestamp && shot) {
            // ability is finished
            timestamp = Time.time + stats.coolDown;
            a.currMovement = Movement.WALKING;
            return false;
        } 

        if(timestamp - Time.time <= stats.targetLockTime && !shot && !lockedon) {
            // stop tracking target movement if we reach lock on time
            lockedon = true;
        }

        if (!lockedon) {
            // rotate to try to face target
            float step = 0.85f * Time.deltaTime;
            Vector3 target = data.player.position3D - a.position3D;
            Vector3 newDir = Vector3.RotateTowards(a.transform.forward,target,step,0.0f);
            a.transform.rotation = Quaternion.LookRotation(newDir);
        }


        if (Time.time > timestamp) // shoot
        {
            a.transform.GetComponent<Animator>().SetTrigger("Attack");
            SoundManager.PlayOneClipAtLocation(AudioClips.singleton.enemyShot, a.position2D, 6f);
            Bullet b = GameManager.main.SpawnBullet(stats.bulletPrefab,a.position2D);
            b.transform.rotation = a.transform.rotation;
              
            timestamp = Time.time + stats.windDown;
            shot = true;
        }

        return true; // Ability still casting
    }
}
