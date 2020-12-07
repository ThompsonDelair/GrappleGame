using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This behavior makes an actor try to get within range and withing line-of-sight to the player
public class ChasePlayerWalkRanged : Behavior
{
    ChasePlayerWalkRangedStats stats;

    public ChasePlayerWalkRanged(ChasePlayerWalkRangedStats s) {
        stats = s;
    }

    public override void Update(Actor a,GameData gameData) {

        bool canSeePlayer = gameData.map.ClearSightLine(a.position2D,gameData.player.position2D);
        bool inRange = Vector2.SqrMagnitude(a.position2D - gameData.player.position2D) <= stats.range * stats.range;

        if (a.followingPath == true && canSeePlayer && inRange) {
            a.followingPath = false;
            a.velocity = Vector2.zero;
            a.animator.SetBool("Walking",false);
            return;
        }

        // if we can't see the player or if we arent in range of the player
        // OR if we are at our pathfind checkpoint
        // then try to pathfind
        if ((a.pathfindTimer < Time.time && (!canSeePlayer || !inRange)) || Vector2.Distance(a.position2D,a.pathfindWaypoint) < 0.2f) {

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
