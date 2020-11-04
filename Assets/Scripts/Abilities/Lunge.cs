using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lunge : Ability
{
    private bool isAttacking = false; // Read only to other classes
    private bool isLunging = false;
    public bool attacking { get { return isAttacking; } }
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


    // Check for if ability's pre-cast requirements have been met
    public override bool StartAbilityCheck(Vector2 pos, EnemyActor actor) // Position is of the thing they want to attack
    {
        

        if (timestamp < Time.time) // Charge setup
        {
            Debug.Log("TIME: " + actor.targetLockTime);
            if (Time.time - timestamp >= actor.targetLockTime)
            {
                // Want the attack direction to be position at the specified targetLockTime.
                dir = (pos - actor.position2D).normalized;
            }

            if (Vector3.Distance(actor.position2D, pos) >= range) // Range check
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
        
        if (Time.time > timestamp) // Lunges after finished charging time
        {
            timestamp = Time.time + coolDown;
            actor.movement = Movement.WALKING;
            actor.AddPushForce(dir, lungeSpeed); // Using direction assigned at start of charge time
            isAttacking = true;
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
