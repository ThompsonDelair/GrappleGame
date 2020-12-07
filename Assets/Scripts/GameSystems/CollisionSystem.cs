using System.Collections.Generic;
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

// Detects and resolves collisions between actors, bullets and terrain
public static class CollisionSystem 
{

    public static bool LayerOrCheck(Layer a,Layer b) {
        return (a & b) != 0;
    }

    public static void CollisionUpdate(GameData data) {
        ResetCollisionBool(data.allActors);
        ActorActorCollisionUpdate(data.allActors);
        ActorTerrainCollision(data.allActors,data.map);
        AreaActorCollisionUpdate(data.areas,data.allActors);
        CheckBulletCollisions(data.allActors,data.bullets,data);
        CheckBulletTerrainCollision(data.bullets,data.map);
        OutOfBoundsCheck(data.allActors,data.map);        
    }

    // sets the "has collided" bool for each actor to false
    static void ResetCollisionBool(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {
            actors[i].collision = false;
        }
    }

    // detects and resolves actor-actor collisions
    static void ActorActorCollisionUpdate(List<Actor> actors) {
        for (int i = 0; i < actors.Count; i++) {
            for (int j = i + 1; j < actors.Count; j++) {
                if (i == j)
                    continue;

                if (actors[i].currMovement == Movement.NONE && actors[j].currMovement == Movement.NONE)
                    continue;

                CollisionCalc.ResolveActorActorCollision(actors[i],actors[j]);
            }
        }
    }
         
    // detects and resolves actor-terrain collisions
    static void ActorTerrainCollision(List<Actor> actors,Map map) {

        for (int i = 0; i < actors.Count; i++) {
            Actor a = actors[i];
            if (a.currMovement == Movement.WALKING) {
                Vector2 newPos = a.position2D;
                CollisionInfo info = new CollisionInfo();
                a.aabb.UpdateActorPos(a);

                for (int j = 0; j < map.terrainEdges.Count; j++) {
                    TerrainEdge e = map.terrainEdges[j];
                    if (!LayerOrCheck((Layer)a.currMovement,e.layer)) {
                        continue;
                    }                                                        
                    
                    if (a.aabb.OverlapCheck(e.aabb)) {
                        Vector2 closestPoint = Calc.ClosestPointToLine(newPos,e.vertA_pos2D,e.vertB_pos2D);

                        float dist2 = Vector2.Distance(newPos,closestPoint);
                        if (dist2 < a.collider.Radius) {

                            info.points.Add(closestPoint);
                            Vector2 away = (newPos - closestPoint).normalized * (a.collider.Radius - dist2);
                        }
                    }
                }

                a.position2D = newPos;

                for (int k = 0; k < info.points.Count; k++) {
                    Debug.DrawLine(a.position3D,new Vector3(info.points[k].x,0,info.points[k].y));
                }
            }
        }
    }

    // checks for collision between enemy actors and bullets in game
    static void CheckBulletCollisions(List<Actor> actors, List<Bullet> bullets,GameData data) {

        for (int i = bullets.Count - 1; i >= 0; --i) {
            Bullet b = bullets[i];
            for (int j = actors.Count - 1; j >= 0; --j) {
                Actor a = actors[j];

                // If they are overlapping
                if (Utils.BitwiseOverlapCheck((int)a.team,(int)b.team) && a.collider.DetectCircleCollision(b.collider)) {

                    bullets[i].behavior.OnActorCollision(bullets[i],a,data);

                    break;
                }
            }
        }
    }

    // detects and resovles bullet-terrain collision
    static void CheckBulletTerrainCollision(List<Bullet> bullets,Map map) {
        for(int i = bullets.Count -1; i >=0; i--) {
            Bullet b = bullets[i];
            AABB_2D aabb = new AABB_2D(b.position2D,b.prevPos);
            for(int j = 0; j < map.terrainEdges.Count; j++) {
                TerrainEdge e = map.terrainEdges[j];
                if (LayerOrCheck((Layer)b.movement, e.layer) && 
                    aabb.OverlapCheck(e.aabb) && 
                    Calc.DoLinesIntersect(b.position2D,b.prevPos,e.vertA_pos2D,e.vertB_pos2D)) {

                    // If we want to have different impact sounds for different bullets must add check for bullet type here.
                    SoundManager.PlayOneClipAtLocation(AudioClips.singleton.bulletImpact, b.position2D, 6f);

                    GameManager.main.DestroyGameobject(b.transform.gameObject);
                    ParticleEmitter.current.SpawnParticleEffect(GameManager.main.playerBulletHit,b.transform.position, Quaternion.identity);
                    bullets.RemoveAt(i);
                    break;
                }
            }
        }
    }

    // detects and resolves actor-area collisions
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

    // checks if any actors managed to erroneously clip through a terrain edge they're not supposed to
    // if so, it resets their position to the position they had at the beginning of the frame
    static void OutOfBoundsCheck(List<Actor> actors, Map map) {
        for(int i = 0; i < actors.Count; i++) {
            for(int j = 0; j < map.terrainEdges.Count; j++) {
                TerrainEdge e = map.terrainEdges[j];
                Actor a = actors[i];
                if (!CollisionSystem.LayerOrCheck((Layer)a.currMovement,e.layer)) {
                    continue;
                }

                if (Calc.DoesLineIntersect(a.position2D,a.posAtFrameStart,e.vertA_pos2D,e.vertB_pos2D)) {
                    a.position2D = a.posAtFrameStart;
                }
            }
        }
    }
}

