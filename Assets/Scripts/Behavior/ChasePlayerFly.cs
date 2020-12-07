using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This behavior makes the actor chase the player and fly over pits
//      only chases if the player is within a stats-defined chase range
public class ChasePlayerFly : Behavior
{
    ChasePlayerFlyStats stats;
    public float pathfindTimer;
    public bool flying;
    public float flyingCheckDelay = 0.25f;
    public float flyingCheckTimetamp;

    public ChasePlayerFly(ChasePlayerFlyStats s) {
        stats = s;
    }

    public override void Update(Actor a,GameData data) {

        if (!UnderMaxPathRange(a,data.player)) {
            return;
        }

        // if the actor is above a pit, start flying animation
        if (flyingCheckTimetamp < Time.time) {
            if (!flying && !data.map.IsPointWalkable(a.position2D)) {
                a.animator.SetBool("Airborne",true);
                flying = true;
            } else if (flying && data.map.IsPointWalkable(a.position2D)) {
                a.animator.SetBool("Airborne",false);
                flying = false;
            }
            flyingCheckTimetamp = Time.time + flyingCheckDelay;
        }

        if (a.pathfindTimer < Time.time || Vector2.Distance(a.position2D,a.pathfindWaypoint) < 0.5f) {

            if (a.currZone == data.player.currZone && a.currZone.type != Movement.FLYING) {
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

                if(a.currMovement == Movement.FLYING && Calc.TotalDistanceTravelled(path) > stats.AggroDistance) {
                    return;
                }
                a.pathfindWaypoint = path[1];
                a.followingPath = true;
                a.animator.SetBool("Walking",true);
         
            } else {
                a.followingPath = false;
                a.velocity = Vector2.zero;
                a.animator.SetBool("Walking",false);
            }
            a.pathfindTimer = Time.time + MovementSystem.pathfindDelay;
        }
    }
}
