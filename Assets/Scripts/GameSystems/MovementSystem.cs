using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Movement { 
    NONE, 
    WALKING = Layer.BLOCK_WALK, 
    FLYING = Layer.BLOCK_FLY,
}

// Movement system is responsible for moving actors and bullets
public static class MovementSystem
{
    public const float pathfindDelay = 1f;

    public static void MovementUpdate(GameData data) {
        RememberStartingPostion(data.allActors);
        ActorMovementUpdate(data.allActors);
        BulletMovementUpdate(data.bullets);
    }

    // records the position for each actor at the start of the frame
    // incase their movement or collision is invalid and their position needs to be reset
    static void RememberStartingPostion(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {
            actors[i].posAtFrameStart = actors[i].position2D;
        }
    }

    // moves actors towards their pathfinding waypoint if they have one
    // applies and updates actors push forces
    static void ActorMovementUpdate(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {                      

            Actor a = actors[i];

            if (a.currMovement == Movement.NONE)
                continue;
            
            if (a.pushForces.Count > 0) {
                for(int j = a.pushForces.Count - 1; j >= 0; j--) {
                    PushInstance p;
                    try {
                        p = a.pushForces[j];
                    } catch{                        
                        p = new PushInstance(new Vector2(),0,0);
                    }

                    a.position2D = a.position2D + p.dir.normalized * p.force * Time.deltaTime;
                    p.force -= p.friction * Time.deltaTime;
                    if(p.force <= 0) {
                        a.pushForces.RemoveAt(j);
                    }
                }
            } else {

                if (a.followingPath) {
                    a.velocity = (a.pathfindWaypoint - a.position2D).normalized * a.stats.speed;
                    a.transform.LookAt(Utils.Vector2XZToVector3(a.pathfindWaypoint));
                }

                a.position2D = a.position2D + a.velocity * Time.deltaTime;
            }
        }
    }

    // Updates position of bullets
    static void BulletMovementUpdate(List<Bullet> bullets) {
        for (int i = 0; i < bullets.Count; i++) {
            if (bullets[i].transform != null) {
                bullets[i].prevPos = bullets[i].position2D;
                bullets[i].transform.position += bullets[i].transform.rotation * new Vector3(0, 0, 1) * Time.deltaTime * bullets[i].stats.speed;
            }
        }
    }
}
