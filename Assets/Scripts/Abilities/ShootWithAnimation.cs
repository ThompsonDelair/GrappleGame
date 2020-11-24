using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootWithAnimation : Ability
{
    ShootWithAnimationStats stats;
    float timestamp;
    Vector2 dir;
    bool shot;
    bool lockedon;

    //public override bool attacking() {
    //    throw new System.NotImplementedException();
    //}

    public ShootWithAnimation(ShootWithAnimationStats s) {
        stats = s;
    }

    public override bool StartAbilityCheck(Actor a,GameData data) {
        if (timestamp < Time.time) // Charge setup
        {


            if (Vector2.Distance(a.position2D,data.player.position2D) >= stats.range) // Range check
            {
                return false; // OUT OF RANGE
            }

            timestamp = Time.time + stats.chargeTime;
            // Disable movement
            a.currMovement = Movement.NONE;
            a.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

            a.transform.GetComponent<Animator>().SetTrigger("StartAttack");
            shot = false;
            lockedon = false;
            //Debug.Log("start lunge");
            return true;
        }
        return false;
    }

    public override bool RunAbilityUpdate(Actor a,GameData data) {
        //Debug.Log("LUNGE: " + actor.transform.gameObject.name + " attack status through actor: " + actor.ability.attacking + " attack status through local attacking: " + attacking);

        if (Time.time > timestamp && shot) {
            timestamp = Time.time + stats.coolDown;
            a.currMovement = Movement.WALKING;
            return false;
        } 

        if(timestamp - Time.time <= stats.targetLockTime && !shot && !lockedon) {
            dir = (data.player.position2D - a.position2D);
            lockedon = true;
        }

        if (!lockedon) {
            a.transform.LookAt(data.player.position3D);
        }


        if (Time.time > timestamp) // Lunges after finished charging time
        {
            a.transform.GetComponent<Animator>().SetTrigger("Attack");

            GameObject gameObjBullet = GameManager.main.GetNewSentryBullet(); // Since ability does not derive from MonoBehaviour we have to get another mono class to do the work of instantiation for us.
            Bullet bullet = new Bullet(gameObjBullet.transform,stats.bulletStats.radius,Team.PLAYER,stats.damage);
            bullet.speed = stats.bulletStats.speed;
            bullet.position3D = a.transform.position;
            bullet.transform.LookAt(Utils.Vector2XZToVector3(a.position2D + dir));
            GameManager.main.gameData.bullets.Add(bullet);

            //GetComponent<Actor>().PlayAudioClip(AudioClips.singleton.gunShot);
            // Set to shoot in that direction
            bullet.transform.rotation = a.transform.rotation;
            timestamp = Time.time + stats.windDown;
            shot = true;

        }



        return true; // Ability still casting
    }
}
