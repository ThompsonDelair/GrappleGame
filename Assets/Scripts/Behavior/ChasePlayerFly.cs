using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePlayerFly : Behavior
{
    ChasePlayerFlyStats stats;
    public float pathfindTimer;
    public bool flying;
    public float flyingCheckDelay = 0.25f;
    public float flyingCheckTimetamp;

    public override void Update(Actor a,GameData data) {

        if (flyingCheckTimetamp < Time.time) {
            if (!flying && !data.map.IsPointWalkable(a.position2D)) {
                Debug.Log("Im flying!");
                a.animator.SetBool("Airborne",true);
                flying = true;
            } else if (flying && data.map.IsPointWalkable(a.position2D)) {
                a.animator.SetBool("Airborne",false);
                flying = false;
            }
            flyingCheckTimetamp = Time.time + flyingCheckDelay;
        }

        if (a.pathfindTimer < Time.time || Vector2.Distance(a.position2D,a.pathfindWaypoint) < 0.5f) {

            //Debug.Log("pathfinding....");
            if(a.currZone == data.player.currZone && a.currZone.type != Movement.FLYING) {
                a.currMovement = Movement.WALKING;
            } else {
                a.currMovement = Movement.FLYING;
            }

            List<Vector2> path = Pathfinding.getPath(
                a.position2D,
                data.player.position2D,
                a.collider.Radius,
                a.currMovement,
                data.map
                );

            if (path != null) {
                a.pathfindWaypoint = path[1];
                a.followingPath = true;

                Vector2 dir = a.pathfindWaypoint - a.position2D;

                //float deg = Random.Range(-pathfindWanderAmount,pathfindWanderAmount);



                a.animator.SetBool("Walking",true);

                //Debug.Log("found path!");                    
            } else {
                a.followingPath = false;
                a.velocity = Vector2.zero;
                a.animator.SetBool("Walking",false);
            }

            a.pathfindTimer = Time.time + MovementSystem.pathfindDelay;
        }


    }
}
