using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lunge : Ability
{
    LungeStats stats;


    private bool isLunging = false; 

    bool lockedon;
    
    private float timestamp;

    private Vector2 dir;

    private PushInstance p;
    private GameObject warning;

    private Lunge() { }

    public Lunge(LungeStats s) {
        stats = s;
    }

    //public override bool attacking() { return isAttacking; }

    // Check for if ability's pre-cast requirements have been met
    public override bool StartAbilityCheck(Actor actor,GameData data) // Position is of the thing they want to attack
    {
        
        if (timestamp < Time.time) // Charge setup
        {
            if (Vector2.Distance(actor.position2D,data.player.position2D) >= stats.range) // Range check
            {
                return false; // OUT OF RANGE
            }

            timestamp = Time.time + stats.chargeTime;
            // Disable movement
            actor.currMovement = Movement.NONE;
            actor.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            
            actor.transform.GetComponent<Animator>().SetTrigger("StartAttack");
            actor.transform.LookAt(data.player.position3D);
            if (stats.warningPrefab != null) {
                warning = GameObject.Instantiate(stats.warningPrefab,actor.position3D,actor.transform.rotation,actor.transform);


                //warning.transform.position = actor.position3D;
                //warning.transform.rotation = actor.transform.rotation;
                //warning.transform.parent = actor.transform;
                
            }


            //Debug.Log("start lunge");
            return true;
        }
        return false;
    }

    // Updates the ability
    public override bool RunAbilityUpdate(Actor actor, GameData data)
    {
        //Debug.Log("LUNGE: " + actor.transform.gameObject.name + " attack status through actor: " + actor.ability.attacking + " attack status through local attacking: " + attacking);
       
        if (Time.time > timestamp) // Lunges after finished charging time
        {
            actor.transform.GetComponent<Animator>().SetTrigger("Attack");
            isLunging = true;
            timestamp = Time.time + stats.coolDown;
            actor.currMovement = Movement.WALKING;
            //Debug.Log("Direction when lunging: " + dir);
            p = actor.StartNewPush(Utils.Vector3ToVector2XZ(actor.transform.forward),stats.lungeForce,stats.lungeFriction); // Using direction assigned at start of charge time
            if (stats.warningPrefab != null) {
                warning.transform.parent = null;
            }
        }

        if (timestamp - Time.time <= stats.targetLockTime && !isLunging && !lockedon) {
            //dir = (data.player.position2D - actor.position2D);
            lockedon = true;
        }

        if (!lockedon) {
            //actor.transform.LookAt(data.player.position3D);

            float step = 0.85f * Time.deltaTime;
            Vector3 target = data.player.position3D - actor.position3D;
            Vector3 newDir = Vector3.RotateTowards(actor.transform.forward,target,step,0.0f);
            actor.transform.rotation = Quaternion.LookRotation(newDir);

        }

        if (isLunging) {
            float dist = Vector2.Distance(actor.position2D,data.player.position2D);
            float threshold = actor.collider.Radius + data.player.collider.Radius + 0.6f;
            if(dist < threshold) {
                DamageSystem.DealDamage(data.player,stats.damage);
            }
        }
        if (!actor.pushForces.Contains(p) && isLunging) // Decreases momentum and return to non attack state
        {
            actor.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
            isLunging = false;
            lockedon = false;
            if(stats.warningPrefab != null) {
                GameObject.Destroy(warning);
            }
            
            return false; // Finished the ability
        }

        return true; // Ability still casting
    }
}
