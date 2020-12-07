using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 
public class Lunge : Ability
{
    LungeStats stats;

    private bool isLunging = false; 

    private bool lockedon;
    
    private float timestamp;

    private PushInstance p;
    private GameObject warning;

    private Lunge() { }

    public Lunge(LungeStats s) {
        stats = s;
    }



    // Check for if target is in range and ability is not on cooldown
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
                // if our ability stats has a warning visual, spawn it
                warning = GameObject.Instantiate(stats.warningPrefab,actor.position3D,actor.transform.rotation,actor.transform);
                
            }

            return true;
        }
        return false;
    }

    // Runs the ability execution
    public override bool RunAbilityUpdate(Actor actor, GameData data)
    {
       
        if (Time.time > timestamp) // Lunges after finished charging time, timestamp set on successful StartAbilityCheck
        {
            actor.transform.GetComponent<Animator>().SetTrigger("Attack");
            isLunging = true;
            timestamp = Time.time + stats.coolDown;
            actor.currMovement = Movement.WALKING;
            p = actor.StartNewPush(Utils.Vector3ToVector2XZ(actor.transform.forward),stats.lungeForce,stats.lungeFriction); // Using direction assigned at start of charge time
            // if we have a warning visual for our ability, de-parent so it no longer moves with the actor
            if (stats.warningPrefab != null) {
                warning.transform.parent = null;
            }
        }

        // stop tracking target movement if we reach lock on time
        if (timestamp - Time.time <= stats.targetLockTime && !isLunging && !lockedon) {

            lockedon = true;
        }

        if (!lockedon) {
            // rotate to try to face target
            float step = 0.85f * Time.deltaTime;
            Vector3 target = data.player.position3D - actor.position3D;
            Vector3 newDir = Vector3.RotateTowards(actor.transform.forward,target,step,0.0f);
            actor.transform.rotation = Quaternion.LookRotation(newDir);
        }

        if (isLunging) {
            // deal damage to our target if we get close enough while lunging
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
