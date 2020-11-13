using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
//using UnityEditorInternal;
using UnityEngine;

public class EnemyActor : Actor
{
/*    private bool isAttacking = false; // Read only to other classes
    private bool isLunging = false;
    public bool attacking { get { return isAttacking; } }
    public bool lunging { get { return isLunging; } }



    [SerializeField]
    private float range = 5f;
    private float chargeTime = 0.75f;
    private float lungeSpeed = 40f;
    // Timing and cooldown for attack
    private float coolDown = 1.5f;
    private float lastAttackStamp;*/

    public float targetLockTime;


    private GameObject player = GameManager.main.player.transform.gameObject;

    public Vector2 position2D {
        get {
            return new Vector2(transform.position.x,transform.position.z);
        }
        set {   
            transform.position = new Vector3(value.x,0,value.y);
        }
    }

    public EnemyActor(Transform t, float r, Layer l, float hp, float targetLockTime, Ability ab) : base(t, r, l, hp, ab) 
    {
        this.targetLockTime = targetLockTime;
    }


    /*-------------------THIS IS THE OLD MELEE CODE, SHOULD BE DONE WITH ABILITY SYS NOW---------------------------------*/

    // Checks to see if player is within range, if so, calls meleeAttack
/*    public void scanForPlayer()
    {
        meleeAttackUpdate(player.transform);
    }

    // Stops movment and charges enemy, after charge lunges to specified position
    // Code to deal damage is still handled by collision sys, we only push enemies in hopes of making a collision here.
    public void meleeAttackUpdate(Transform t) // Tried to make kinda generic params so we can use to damage objects later if we need.
    {


        if (!isAttacking && lastAttackStamp < Time.time) // Stops and sets charge
        {
            if (Vector3.Distance(this.transform.position, player.transform.position) >= range)
            {
                return;
            }
            isAttacking = true;
            lastAttackStamp = Time.time + chargeTime;
            // Disable movement
            this.movement = Movement.NONE;
            this.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        }

        if (isAttacking && Time.time > lastAttackStamp)
        {
            Debug.Log("INSIDE IF");
            lastAttackStamp = Time.time + coolDown;
            this.movement = Movement.WALKING;
            Vector3 dir = (t.position - this.transform.position).normalized;
            AddPushForce(Utils.Vector3ToVector2XZ(dir), lungeSpeed);
            isLunging = true;
        }


        if ((momentumForce < 1f && isAttacking) && isLunging) // Attack is coming to end
        {
            isAttacking = false;
            this.transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
            isLunging = false;
        }
    }*/
}