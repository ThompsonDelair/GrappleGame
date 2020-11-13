using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Movement { 
    NONE, 
    WALKING = Layer.BLOCK_WALK, 
    IGNORE_CLIFFS = Layer.BLOCK_FLY,
    PAUSED
}

public static class MovementSystem
{
    const float pathfindDelay = 1f;
    const float momentumDecay = 100f;


    public static void MovementUpdate(List<EnemyActor> enemies, List<Actor> allActors, List<Bullet> bullets, Map map) {
        EnemyPathfind(enemies, map);
        RememberStartingPostion(allActors);
        ActorMovementUpdate(allActors);
        BulletMovementUpdate(bullets);
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

            if (a.movement == Movement.PAUSED)
                continue;
            if (a.movement != Movement.WALKING)
                continue;


            if (a.momentumForce > 0f) {
                //Debug.Log("FORCE DETECTED");
                a.position2D = a.position2D + a.momentumDir.normalized * a.momentumForce * Time.deltaTime;
                a.momentumForce -= momentumDecay * Time.deltaTime;

            } else {
                float speed;
                if (a == GameManager.main.player) {
                    speed = GameManager.main.playerStats.GetStat(StatManager.StatType.Speed);
                } else {
                    speed = GameManager.main.enemySpeed;
                }

                if (a.followingPath) {
                    a.velocity = (a.pathfindWaypoint - a.position2D).normalized * GameManager.main.enemySpeed;
                }

                a.position2D = a.position2D + a.velocity * Time.deltaTime;
            }
        }
    }


    static void EnemyPathfind(List<EnemyActor> enemies, Map map) {
        for (int i = 0; i < enemies.Count; i++) {
            Actor e = enemies[i];
            if (e.pathfindTimer < Time.time || Vector2.Distance(e.position2D, e.pathfindWaypoint) < 0.5f) {

                //Debug.Log("pathfinding....");

                List<Vector2> path = Pathfinding.getPath(
                    e.position2D,
                    GameManager.main.player.position2D,
                    e.collider.Radius,
                    map);

                if (path != null) {
                    e.pathfindWaypoint = path[1];
                    e.followingPath = true;

                    //Debug.Log("found path!");                    
                } else {
                    e.followingPath = false;
                    e.velocity = Vector2.zero;
                }

                e.pathfindTimer = Time.time + pathfindDelay;
            }
        }
    }

    // Updates position of bullet
    // NEED TO SEGREGATE ENEMY AND PLAYER BULLET UPDATES IF WE WANT DIFF SPEEDS
    static void BulletMovementUpdate(List<Bullet> bullets) {
        for (int i = 0; i < bullets.Count; i++) {
            if (bullets[i].transform != null) {
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
