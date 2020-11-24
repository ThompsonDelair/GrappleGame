using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Movement { 
    NONE, 
    WALKING = Layer.BLOCK_WALK, 
    FLYING = Layer.BLOCK_FLY,
    //PAUSED
}

public static class MovementSystem
{
    public const float pathfindDelay = 1f;
    const float pathfindWanderAmount = 20f;
    const float pathfindWanderDelay = 3f;
    const float zoneUpdateDelay = 0.3f;
    static float zoneUpdateTimestamp;

    public static void MovementUpdate(GameData data) {
        //EnemyPathfind(data.enemyActors, data.map);
        RememberStartingPostion(data.allActors);
        ActorMovementUpdate(data.allActors);
        BulletMovementUpdate(data.bullets);

        if(zoneUpdateTimestamp < Time.time) {
            UpdateCurrZone(data.allActors,data.map);
            zoneUpdateTimestamp = Time.time + zoneUpdateTimestamp;
        }
        //EnemyScanForPlayer(enemies);
    }

    //public static void EnemyFollowPath(List<Actor> actors) {
    //    for (int i = 0; i < actors.Count; i++) {
    //        Actor e = actors[i];
    //        if (e.followingPath) {
    //            Vector2 dir = e.pathfindWaypoint - e.position2D;
    //            e.velocity += dir.normalized * GameManager.main.enemySpeed;
    //        }
    //    }
    //}

    static void RememberStartingPostion(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {
            actors[i].posAtFrameStart = actors[i].position2D;
        }
    }

    static void ActorMovementUpdate(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {
                       

            Actor a = actors[i];

            if (a.currMovement == Movement.NONE)
                continue;

            //if (a.momentumForce > 0f) {
            //    //Debug.Log("FORCE DETECTED");
            //    a.position2D = a.position2D + a.momentumDir.normalized * a.momentumForce * Time.deltaTime;
            //    a.momentumForce -= momentumDecay * Time.deltaTime;

            //}
                       
            if (a.pushForces.Count > 0) {
                for(int j = a.pushForces.Count - 1; j >= 0; j--) {
                    PushInstance p;
                    try {
                        p = a.pushForces[j];
                    } catch{
                        ;
                        p = new PushInstance(new Vector2(),0,0);
                    }

                    a.position2D = a.position2D + p.dir.normalized * p.force * Time.deltaTime;
                    p.force -= p.friction * Time.deltaTime;
                    if(p.force <= 0) {
                        a.pushForces.RemoveAt(j);
                    }
                }
            } else {
                //float speed;
                //if (a == GameManager.main.player) {
                //    speed = GameManager.main.playerStats.GetStat(StatManager.StatType.Speed);
                //} else {
                //    speed = GameManager.main.enemySpeed;
                //}

                if (a.followingPath) {
                    a.velocity = (a.pathfindWaypoint - a.position2D).normalized * a.stats.speed;
                    a.transform.LookAt(Utils.Vector2XZToVector3(a.pathfindWaypoint));
                }

                a.position2D = a.position2D + a.velocity * Time.deltaTime;
            }
        }
    }


    static void EnemyPathfind(List<Actor> enemies, Map map) {
        for (int i = 0; i < enemies.Count; i++) {

            Actor e = enemies[i];

            if(e.stats.movement == Movement.NONE) {
                continue;
            }

            if (e.pathfindTimer < Time.time || Vector2.Distance(e.position2D, e.pathfindWaypoint) < 0.5f) {

                //Debug.Log("pathfinding....");

                List<Vector2> path = Pathfinding.getPath(
                    e.position2D,
                    GameManager.main.player.position2D,
                    e.collider.Radius,
                    e.currMovement,
                    map);

                if (path != null) {
                    e.pathfindWaypoint = path[1];
                    e.followingPath = true;

                    Vector2 dir = e.pathfindWaypoint - e.position2D;

                    float deg = Random.Range(-pathfindWanderAmount,pathfindWanderAmount);



                    e.animator.SetBool("Walking", true);

                    //Debug.Log("found path!");                    
                } else {
                    e.followingPath = false;
                    e.velocity = Vector2.zero;
                    e.animator.SetBool("Walking", false);
                }

                e.pathfindTimer = Time.time + pathfindDelay;
            }

            
        }
    }

    static void UpdateCurrZone(List<Actor> actors, Map map) {
        for(int i = 0; i < actors.Count; i++) {
            Actor a = actors[i];
            if(a.currMovement == Movement.NONE) {
                continue;
            }
            a.currZone = map.ZoneFromPoint(a.position2D);

        }
    }

    // Updates position of bullet
    // NEED TO SEGREGATE ENEMY AND PLAYER BULLET UPDATES IF WE WANT DIFF SPEEDS
    static void BulletMovementUpdate(List<Bullet> bullets) {
        for (int i = 0; i < bullets.Count; i++) {
            if (bullets[i].transform != null) {
                bullets[i].prevPos = bullets[i].position2D;
                bullets[i].transform.position += bullets[i].transform.rotation * new Vector3(0, 0, 1) * Time.deltaTime * bullets[i].speed;
            }
        }
    }


    // Calls each enemy actors scan function to see if iin range with player
    // This should go else where after alpha
/*    static void EnemyScanForPlayer(List<EnemyActor> enemies){
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].scanForPlayer();
        }
    }*/
        
}
