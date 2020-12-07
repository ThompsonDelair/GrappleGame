using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarShootAbility : Ability
{
    MortarShootStats stats;
    float timestamp;
    bool shot;
    bool lockedon;
    Vector2 lockedPos;

    float retryTimestamp;

    public  MortarShootAbility(MortarShootStats s) {
        stats = s;
    }


    public override bool RunAbilityUpdate(Actor a,GameData data) {

        if (Time.time > timestamp && shot) {
            timestamp = Time.time + stats.coolDown;
            
            if (stats.animate) {
                a.currMovement = Movement.WALKING;
            }

            return false; // ability finished
        }

        if (timestamp - Time.time <= stats.targetLockTime && !shot && !lockedon) {
            // set the position we're going to shoot the mortar at
            lockedon = true;
            lockedPos = GameManager.main.GameData.player.position2D;
        }

        if (!lockedon) {
            // face our target if we're animating this ability
            if (stats.animate) {
                a.transform.LookAt(data.player.position3D);
            }
        }

        if (Time.time > timestamp) // shoot after finished charging time
        {
            if (stats.animate) {
                a.transform.GetComponent<Animator>().SetTrigger("Attack");
            }

            Bullet b = GameManager.main.SpawnBullet(stats.bulletPrefab,a.position2D);
            b.transform.rotation = a.transform.rotation;
            ((LobBulletBehavior)b.behavior).end = lockedPos;
            ((LobBulletBehavior)b.behavior).start = a.position2D;
            timestamp = Time.time + stats.windDown;
            shot = true;
            //Debug.Log("CALLING FIRE CAST");
            SoundManager.PlayOneClipAtLocation(AudioClips.singleton.fireCast, a.position2D, 6f);
        }

        return true; // Ability still casting
    }

    // check if target is in range, the ability is not on cooldown, and there is clear line of sight to target
    public override bool StartAbilityCheck(Actor a,GameData data) {
        if (timestamp < Time.time) // Charge setup
{
            if (Vector2.Distance(a.position2D,data.player.position2D) >= stats.range) // Range check
            {
                return false; // OUT OF RANGE
            }

            if(retryTimestamp > Time.time) {
                return false;
            }

            if (!data.map.ClearSightLine(a.position2D,data.player.position2D)) {
                retryTimestamp = Time.time + 0.3f;
                return false;
            }

            timestamp = Time.time + stats.chargeTime;
            a.currMovement = Movement.NONE;

            if (stats.animate) {
                a.transform.GetComponent<Animator>().SetTrigger("StartAttack");
            }
            shot = false;
            lockedon = false;
            return true;
        }
        return false;
    }
}
