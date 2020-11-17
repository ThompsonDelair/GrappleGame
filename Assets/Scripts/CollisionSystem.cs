using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Unity.Collections;
using UnityEngine;

public enum CollisionType { PUSH_A, PUSH_B, EQUAL }

public enum Layer {
    NONE = 0,
    PLAYER = 0b1,
    ENEMIES = 0b1 << 1,
    ALL_ACTORS = PLAYER | ENEMIES,
    BULLETS = 0b1 << 2,
    HAZARDS = 0b1 << 3,
    BLOCK_WALK = 0b1 << 4,
    BLOCK_FLY = 0b1 << 5,
    BLOCK_ALL = BLOCK_WALK | BLOCK_FLY
}


public static class CollisionSystem 
{

    public static bool LayerOrCheck(Layer a,Layer b) {
        return (a & b) != 0;
    }

    public static bool LayerExact(Layer a, Layer b) {
        return (a ^ b) == 0;
    }


    public static void CollisionUpdate(List<Actor> actors, List<EnemyActor> enemies, List<Bullet> bullets, Map map) {
        ResetCollisionBool(actors);
        ActorActorCollisionUpdate(actors);
        CheckPlayerEnemyCollisions(enemies);
        ActorTerrainCollision(actors,map);
        CheckBulletCollisions(actors,bullets,map);
        CheckBulletCollisions(map.objects,bullets,map);
        AreaActorCollisionUpdate(map.areas,actors);
        OutOfBoundsCheck(actors,map);
    }

    static void ResetCollisionBool(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {
            actors[i].collision = false;
        }
    }

    static void ActorActorCollisionUpdate(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {
            for (int j = 0; j < actors.Count; j++) {
                if (i == j)
                    continue;

                CollisionCalc.ResolveActorActorCollision(actors[i],actors[j]);
            }
        }
    }
         
    static void ActorTerrainCollision(List<Actor> actors,Map map) {

        for (int i = 0; i < actors.Count; i++) {
            Actor a = actors[i];
            bool momentumChange = false;
            if (a.movement == Movement.WALKING) {
                // player - terrain collision
                Vector2 newPos = a.position2D;
                CollisionInfo info = new CollisionInfo();
                a.aabb.UpdateActorPos(a);

                for (int j = 0; j < map.terrainEdges.Count; j++) {
                    TerrainEdge e = map.terrainEdges[j];
                    if (!LayerOrCheck((Layer)a.movement,e.layer)) {
                        continue;
                    }                                                        
                    
                    if (a.aabb.OverlapCheck(e.aabb)) {
                        Vector2 closestPoint = Calc.ClosestPointToLine(newPos,e.vertA_pos2D,e.vertB_pos2D);

                        float dist2 = Vector2.Distance(newPos,closestPoint);
                        if (dist2 < a.collider.Radius) {


                            //a.aabb.DrawBox(Color.blue);
                            //e.aabb.DrawBox(CustomColors.darkRed);

                            info.points.Add(closestPoint);
                            Vector2 away = (newPos - closestPoint).normalized * (a.collider.Radius - dist2);
                            newPos += away;

                            //if (!momentumChange) {
                            //    int c = Calc.ClockWiseCheck(closestPoint,a.position2D,a.position2D + a.momentumDir);

                            //    if (c > 0) {

                            //    } else if (c < 0) {

                            //    } else {

                            //    }
                            //    a.momentumForce /= 2;
                            //}
                        }
                    }


                    //if (newPos != Vector2.zero) {
                    //    a.position2D = newPos;
                    //}
                }

                a.position2D = newPos;
                //if (info.points.Count > 0) {
                //    actors[i].momentumForce = 0;
                //}
                for (int k = 0; k < info.points.Count; k++) {
                    Debug.DrawLine(a.position3D,new Vector3(info.points[k].x,0,info.points[k].y));
                }
            }
        }
    }

    // checks for collision between enemy actors and bullets in game
    static void CheckBulletCollisions(List<Actor> actors, List<Bullet> bullets, Map map) {

        for (int i = bullets.Count - 1; i >= 0; --i) {
            Bullet b = bullets[i];
            //bool destroyedBullet = false;
            for (int j = actors.Count - 1; j >= 0; --j) {
                Actor a = actors[j];

                // If they are overlapping
                if (LayerOrCheck(a.layer,b.layer) && a.collider.DetectCircleCollision(b.collider)) {
                    AudioSource.PlayClipAtPoint(AudioClips.singleton.bulletImpact, a.position3D,6f);
                    //Debug.Log("HIT DETECTED");

                    // Need to destroy in a better way so that references are deleted as well.
                    
                    GameManager.main.DestroyGameobject(b.transform.gameObject);
                    ParticleEmitter.current.SpawnParticleEffect(GameManager.main.playerBulletHit, b.transform.position);

                    bullets.RemoveAt(i);

                    // Now instead of destroying actors here we call dealDamage() in DamageSystem instead and all actors health is managed from there.
                    DamageSystem.DealDamage(a, b.damage);
                    //destroyedBullet = true;
                    goto end_of_loop;
                }
            }

            //if (destroyedBullet)
            //    continue;

            for(int j = 0; j < map.terrainEdges.Count; j++) {
                TerrainEdge e = map.terrainEdges[j];
                if (LayerOrCheck((Layer)b.movement,e.layer) && CollisionCalc.DetectCircleLineCollision(b.collider,e.vertA_pos2D,e.vertB_pos2D)) {
                    AudioSource.PlayClipAtPoint(AudioClips.singleton.bulletImpact,b.position3D,6f);
                    GameManager.main.DestroyGameobject(b.transform.gameObject);
                    bullets.RemoveAt(i);
                    //Debug.Log("bullet hit wall");
                    goto end_of_loop;
                }
            }

            end_of_loop:
            ;
        }
    }

    // Added to track whether enemies are in the player's collision area specifically 
    static void CheckPlayerEnemyCollisions(List<EnemyActor> enemies)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            //Debug.Log("is attacking?: " + enemies[i].ability.attacking);
            if (CollisionCalc.CompareRadiusPlayerEnemy(enemies[i]))
            {
                
                if (enemies[i].ability.attacking()) // So that player doesnt get hurt by walking into non-attacking enemies.
                {
                    DamageSystem.DealDamage(GameManager.main.player, 1f);
                }
            }
        }
    }

    static void AreaActorCollisionUpdate(List<Area> areas,List<Actor> actors) {
        for(int i = 0; i < areas.Count; i++) {
            Area area = areas[i];
            if (!area.active)
                continue;
            for(int j = 0; j < actors.Count; j++) {
                Actor actor = actors[j];
                if (area.DetectCircleCollision(actor.collider)) {
                    area.OnActorCollision(actor);
                }
            }
        }
    }

    static void OutOfBoundsCheck(List<Actor> actors, Map map) {
        for(int i = 0; i < actors.Count; i++) {
            for(int j = 0; j < map.terrainEdges.Count; j++) {
                TerrainEdge e = map.terrainEdges[j];
                Actor a = actors[i];
                if (!CollisionSystem.LayerOrCheck((Layer)a.movement,e.layer)) {
                    continue;
                }

                if (Calc.DoesLineIntersect(a.position2D,a.posAtFrameStart,e.vertA_pos2D,e.vertB_pos2D)) {
                    a.position2D = a.posAtFrameStart;
                }
            }
        }
    }


}

