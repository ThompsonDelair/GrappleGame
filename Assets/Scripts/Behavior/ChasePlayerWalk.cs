using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePlayerWalk : Behavior
{
    public override void Update(Actor a,GameData data) {
        //Debug.Log("UPDATING WALK");
        if(!UnderMaxPathRange(a,data.player)) {
            return;
        }

        if (a.pathfindTimer < Time.time || Vector2.Distance(a.position2D,a.pathfindWaypoint) < 0.5f) {

            //Debug.Log("pathfinding....");

            List<Vector2> path = Pathfinding.getPath(
                a.position2D,
                GameManager.main.player.position2D,
                a.collider.Radius,
                a.currMovement,
                data.map);

            if (path != null) {
                //Debug.Log("Walking!");
                a.pathfindWaypoint = path[1];
                a.followingPath = true;

                Vector2 dir = a.pathfindWaypoint - a.position2D;

                //float deg = Random.Range(-pathfindWanderAmount,pathfindWanderAmount);



                a.animator.SetBool("Walking",true);
                //SoundManager.StartClipOnActor(AudioClips.singleton.littleWalk, a, 6f, true); // start looping walk sound

                //Debug.Log("found path!");                    
            } else {
                a.followingPath = false;
                a.velocity = Vector2.zero;
                a.animator.SetBool("Walking",false);
                //SoundManager.StopClipOnActor(AudioClips.singleton.littleWalk, a); // start looping walk sound
            }

            a.pathfindTimer = Time.time + MovementSystem.pathfindDelay;
        }
    }
}
