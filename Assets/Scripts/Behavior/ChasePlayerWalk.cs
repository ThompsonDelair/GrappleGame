using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This behavior makes the actor constantly walk towards the player 
//  if the player is in the same zone as them
public class ChasePlayerWalk : Behavior
{
    public override void Update(Actor a,GameData data) {

        if(!UnderMaxPathRange(a,data.player)) {
            return;
        }

        if (a.pathfindTimer < Time.time || Vector2.Distance(a.position2D,a.pathfindWaypoint) < 0.5f) {

            List<Vector2> path = Pathfinding.getPath(
                a.position2D,
                GameManager.main.player.position2D,
                a.collider.Radius,
                a.currMovement,
                data.map);

            if (path != null) {
                a.pathfindWaypoint = path[1];
                a.followingPath = true;

                Vector2 dir = a.pathfindWaypoint - a.position2D;
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
