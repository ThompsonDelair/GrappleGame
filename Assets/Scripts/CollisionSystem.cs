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
    WALLS = 0b1 << 4,
    CLIFFS = 0b1 << 5,
    NO_GRAPPLE = 0b1 << 6,
    NO_GRAPPLE_WALL = WALLS | NO_GRAPPLE,
    ALL_TERRAIN = WALLS | CLIFFS,

}


public static class CollisionSystem 
{

    public static bool LayerCheck(Layer a,Layer b) {
        return (b & a) != 0;
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
            if (a.movement == Movement.WALKING) {
                // player - terrain collision
                for (int j = 0; j < map.PChains.Length; j++) {
                    CollisionInfo info;
                    Vector2 newPos = CollisionCalc.ResolveCirclePChainCollision(a.position2D,a.collider.Radius,map.PChains[j],out info);
                    a.position2D = newPos;
                    if(info.points.Count > 0) {
                        actors[i].momentumForce = 0;
                    }
                    for (int k = 0; k < info.points.Count; k++) {
                        Debug.DrawLine(a.position3D,new Vector3(info.points[k].x,0,info.points[k].y));
                    }
                    //if (newPos != Vector2.zero) {
                    //    a.position2D = newPos;
                    //}
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
                if (LayerCheck(a.layer,b.layer) && a.collider.DetectCircleCollision(b.collider)) {
                    AudioSource.PlayClipAtPoint(AudioClips.singleton.bulletImpact, a.position3D,6f);

                    //Debug.Log("HIT DETECTED");

                    // Need to destroy in a better way so that references are deleted as well.
                    GameManager.main.DestroyGameobject(b.transform.gameObject);
                    bullets.RemoveAt(i);

                    // Now instead of destroying actors here we call dealDamage() in DamageSystem instead and all actors health is managed from there.
                    DamageSystem.DealDamage(a, b.damage);
                    //destroyedBullet = true;
                    goto end_of_loop;
                }
            }

            //if (destroyedBullet)
            //    continue;

            for(int j = 0; j < map.PChains.Length; j++) {
                Vector2[] pChain = map.PChains[j];
                for(int k = 0; k < pChain.Length -1; k++) {
                    if (LayerCheck((Layer)b.movement,map.VertTypes[j][k]) && CollisionCalc.DetectCircleLineCollision(b.collider,pChain[k],pChain[k+1])) {
                        AudioSource.PlayClipAtPoint(AudioClips.singleton.bulletImpact, b.position3D, 6f);
                        GameManager.main.DestroyGameobject(b.transform.gameObject);
                        bullets.RemoveAt(i);
                        //Debug.Log("bullet hit wall");
                        goto end_of_loop;
                    }
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
            if (CollisionCalc.CompareRadiusPlayerEnemy(enemies[i]))
            {
                if (enemies[i].attacking) // So that player doesnt get hurt by walking into non-attacking enemies.
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
            for(int j = 0; j < map.PChains.Length; j++) {
                Vector2[] pChain = map.PChains[j];
                for(int k = 0; k < pChain.Length -1; k++) {
                    Actor a = actors[i];
                    if (Calc.DoesLineIntersect(a.position2D,a.posAtFrameStart,pChain[k],pChain[k+1])) {
                        a.position2D = a.posAtFrameStart;
                    }
                }
            }
        }
    }
}

