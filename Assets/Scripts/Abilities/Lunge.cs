using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lunge : Ability
{
    private bool isAttacking = false; // Read only to other classes
    private bool isLunging = false;
    public bool lunging { get { return isLunging; } }


    // THESE FIELDS MAY BE IN A SCRIPTABLE OBJECT AFTER, ABILITY STATS?
    // STATIC DATA SHOULD SCRIPTABLE OBJECT IDEALLY
    [SerializeField]
    private float range = 5f;
    private float chargeTime = 0.75f;
    private float lungeSpeed = 40f;

    // Timing and cooldown for attack
    private float coolDown = 1.5f;
    private float timestamp;

    private Vector2 dir;

    public Lunge() { }


    public override bool attacking() { return isAttacking; }

    // Check for if ability's pre-cast requirements have been met
    public override bool StartAbilityCheck(Vector2 pos, EnemyActor actor) // Position is of the thing they want to attack
    {
        
        if (timestamp < Time.time) // Charge setup
        {
            if (Time.time - timestamp >= actor.targetLockTime)
            {
                //Debug.Log("player position: " + pos + "\nEnemy position: " + actor.position2D);
                // Want the attack direction to be position at the specified targetLockTime.
                dir = (pos - actor.position2D);
            }

            if (Vector2.Distance(actor.position2D, pos) >= range) // Range check
            {
                return false; // OUT OF RANGE
            }

            timestamp = Time.time + chargeTime;
            // Disable movement
            actor.movement = Movement.NONE;
            actor.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

            return true;
        }
        return false;
    }

    // Updates the ability
    public override bool RunAbilityUpdate(Vector2 pos, EnemyActor actor)
    {
       // Debug.Log("LUNGE: " + actor.transform.gameObject.name + " attack status through actor: " + actor.ability.attacking + " attack status through local attacking: " + attacking);

        if (Time.time > timestamp) // Lunges after finished charging time
        {
            isAttacking = true;
            timestamp = Time.time + coolDown;
            actor.movement = Movement.WALKING;
            //Debug.Log("Direction when lunging: " + dir);
            actor.AddPushForce(dir, lungeSpeed); // Using direction assigned at start of charge time
        }
        if (actor.momentumForce < 1f && isAttacking) // Decreases momentum and return to non attack state
        {
            actor.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
            isAttacking = false;
            return false; // Finished the ability
        }

        return true; // Ability still casting
    }
}
