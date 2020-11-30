using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasePlayerWalkRanged : Behavior
{
    ChasePlayerWalkRangedStats stats;

    public ChasePlayerWalkRanged(ChasePlayerWalkRangedStats s) {
        stats = s;
    }

    public override void Update(Actor a,GameData gameData) {
        //Debug.Log("UPDATING WALK");

        bool canSeePlayer = gameData.map.ClearSightLine(a.position2D,gameData.player.position2D);
        bool inRange = Vector2.SqrMagnitude(a.position2D - gameData.player.position2D) <= stats.range * stats.range;

        if (a.followingPath == true && canSeePlayer && inRange) {
            a.followingPath = false;
            a.velocity = Vector2.zero;
            a.animator.SetBool("Walking",false);
            return;
        }


        if ((a.pathfindTimer < Time.time && (!canSeePlayer || !inRange)) || Vector2.Distance(a.position2D,a.pathfindWaypoint) < 0.2f) {

            //Debug.Log("pathfinding....");

            List<Vector2> path = NavGridPathfind.Pathfind(a.position2D,gameData.player.position2D,Movement.WALKING,gameData.navGrid,stats.range);

            if (path != null) {
                
                path = Pathfinding.getPath(
                a.position2D,
                path[path.Count-1],
                a.collider.Radius,
                a.currMovement,
                gameData.map);

                if (path == null)
                    return;

                a.pathfindWaypoint = path[1];
                a.followingPath = true;

                //Vector2 dir = a.pathfindWaypoint - a.position2D;

                //float deg = Random.Range(-pathfindWanderAmount,pathfindWanderAmount);

                //Debug.Break();
                DebugDraw.DrawPath3D(path,Color.white);
                Utils.debugStarPoint(Utils.Vector2XZToVector3(path[1]),1f,Color.white);

                a.animator.SetBool("Walking",true);
                //SoundManager.StartClipOnActor(AudioClips.singleton.littleWalk, a, 6f, true); // start looping walk sound

                //Debug.Log("found path!");                    
            } else {

                // pathfind with navgrid




                a.followingPath = false;
                a.velocity = Vector2.zero;
                a.animator.SetBool("Walking",false);
                //SoundManager.StopClipOnActor(AudioClips.singleton.littleWalk, a); // start looping walk sound
            }

            a.pathfindTimer = Time.time + MovementSystem.pathfindDelay;
        }
    }
}
