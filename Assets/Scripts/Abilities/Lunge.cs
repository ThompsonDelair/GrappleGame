using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lunge : Ability
{
    LungeStats stats;


    private bool isLunging = false; // Read only to other classes
    //private bool isLunging = false;
    bool lockedon;
    //public bool lunging { get { return isLunging; } }


    // THESE FIELDS MAY BE IN A SCRIPTABLE OBJECT AFTER, ABILITY STATS?
    // STATIC DATA SHOULD SCRIPTABLE OBJECT IDEALLY
    //[SerializeField]
    //private float range = 5f;
    //private float chargeTime = 0.75f;
    //private float lungeSpeed = 40f;

    //// Timing and cooldown for attack
    //private float coolDown = 1.5f;
    
    
    private float timestamp;

    private Vector2 dir;

    private PushInstance p;

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
            p = actor.StartNewPush(dir,stats.lungeForce,stats.lungeFriction); // Using direction assigned at start of charge time
        }

        if (timestamp - Time.time <= stats.targetLockTime && !isLunging && !lockedon) {
            dir = (data.player.position2D - actor.position2D);
            lockedon = true;
        }

        if (!lockedon) {
            actor.transform.LookAt(data.player.position3D);
        }

        if (isLunging) {
            float dist = Vector2.Distance(actor.position2D,data.player.position2D);
            float threshold = actor.collider.Radius + data.player.collider.Radius + 0.2f;
            if(dist < threshold) {
                DamageSystem.DealDamage(data.player,stats.damage);
            }
        }
        if (!actor.pushForces.Contains(p) && isLunging) // Decreases momentum and return to non attack state
        {
            actor.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
            isLunging = false;
            lockedon = false;
            return false; // Finished the ability
        }

        return true; // Ability still casting
    }
}
