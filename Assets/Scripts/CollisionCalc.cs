using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionCalc 
{
    //  0 means point is ON the polygon's line
    //  1 means point is inside polygon
    //  2 means point is outside polygon
    public static byte PointInsidePolygon(Vector2 point,Vector2[] polygon) {
        int intersectCount = 0;
        Vector2 pointVeryFar = new Vector2(9999999,9999999);
        for (int i = 0; i < polygon.Length; i++) {
            Vector2 lineA = polygon[i];
            Vector2 lineB = polygon[(i + 1) % polygon.Length];
            if (Calc.DoesLineIntersect(point,pointVeryFar,lineA,lineB)) {
                intersectCount++;
            }
            if (Calc.ClockWiseCheck(point,lineA,lineB) == 0) {
                return 0;
            }
        }
        return (intersectCount % 2 != 0) ? (byte)1 : (byte)2;
    }

    public static Vector2 ResolveCircleEdgesCollision(Vector2 point,float radius,List<TerrainEdge> edges,out CollisionInfo info) {
        info = new CollisionInfo();
        Vector2 newPos = point;
        for(int i = 0; i < edges.Count; i++) {
            Vector2 lineA = edges[i].vertA_pos2D;
            Vector2 lineB = edges[i].vertB_pos2D;
            float dist = Calc.DistancePointToLine(newPos,lineA,lineB);
            if(dist < radius) {
                Vector2 closestPoint = Calc.ClosestPointToLine(newPos,lineA,lineB);
                info.points.Add(closestPoint);
                float dist2 = Vector2.Distance(newPos,closestPoint);
                if (dist2 < radius) {
                    Vector2 away = (newPos - closestPoint).normalized * (radius - dist2);
                    newPos += away;
                }
            }
        }
        return newPos;
    }

    //public static Vector2 ResolveCirclePChainCollision(Vector2 point,float radius,Vector2[] pchain,out CollisionInfo info) {
    //    info = new CollisionInfo();
    //    Vector2 newPos = point;
    //    for (int i = 0; i < pchain.Length - 1; i++) {
    //        Vector2 lineA = pchain[i];
    //        Vector2 lineB = pchain[i + 1];
    //        float dist = Calc.DistancePointToLine(newPos,lineA,lineB);
    //        if (dist < radius) {
    //            Vector2 closestPoint = Calc.ClosestPointToLine(newPos,lineA,lineB);
    //            info.points.Add(closestPoint);
    //            float dist2 = Vector2.Distance(newPos,closestPoint);
    //            if (dist2 < radius) {
    //                Vector2 away = (newPos - closestPoint).normalized * (radius - dist2);
    //                newPos += away;
    //            }
    //        }
    //    }
    //    return newPos;
    //}

    public static bool DetectCirclePChainCollision(Vector2 point,float radius,Vector2[] pchain) {
        for (int i = 0; i < pchain.Length - 1; i++) {
            Vector2 lineA = pchain[i];
            Vector2 lineB = pchain[i + 1];
            float dist = Calc.DistancePointToLine(point,lineA,lineB);
            if (dist < radius) {
                return true;
            }
        }
        return false;
    }

    public static bool DetectCirclePolygonCollision(Vector2 point,float radius,Vector2[] polygon) {
        for (int i = 0; i < polygon.Length; i++) {
            Vector2 lineA = polygon[i];
            Vector2 lineB = polygon[(i + 1)% polygon.Length];
            float dist = Calc.DistancePointToLine(point,lineA,lineB);
            if (dist < radius) {
                return true;
            }
        }
        return false;
    }

    public static bool DetectCirclePolygonCollision(NewCircleCollider circ, PolygonCollider poly) {
        Vector2[] verts = poly.Verts;

        for (int i = 0; i < verts.Length; i++) {
            Vector2 lineA = verts[i];
            Vector2 lineB = verts[(i + 1) % verts.Length];
            float dist = Calc.DistancePointToLine(circ.Position2D,lineA,lineB);
            if (dist < circ.Radius) {
                return true;
            }
        }

        return (CollisionCalc.PointInsidePolygon(circ.Position2D,poly.Verts) != 2);

    }

    

    public static Vector2 FindCirclePolygonCollision(Vector2 point,float radius,Vector2[] polygon,out CollisionInfo info) {
        info = new CollisionInfo();
        Vector2 newPos = point;
        for (int i = 0; i < polygon.Length; i++) {
            Vector2 lineA = polygon[i];
            Vector2 lineB = polygon[(i + 1) % polygon.Length];
            float dist = Calc.DistancePointToLine(newPos,lineA,lineB);
            if (dist < radius) {
                Vector2 closestPoint = Calc.ClosestPointToLine(newPos,lineA,lineB);
                info.points.Add(closestPoint);
                float dist2 = Vector2.Distance(newPos,closestPoint);
                if (dist2 < radius) {
                    Vector2 away = (newPos - closestPoint).normalized * (radius - dist2);
                    newPos += away;
                }
            }
        }
        return newPos;
    }

    public static Vector2 FindCircleLineCollision(NewCircleCollider c,Vector2 la,Vector2 lb) {
        float dist = Calc.DistancePointToLine(c.Position2D,la,lb);
        if (dist < c.Radius) {
            Vector2 closestPoint = Calc.ClosestPointToLine(c.Position2D,la,lb);
            float dist2 = Vector2.Distance(c.Position2D,closestPoint);
            if (dist2 < c.Radius) {
                return (c.Position2D - closestPoint).normalized * (c.Radius - dist2);
            }
        }
        return Vector2.negativeInfinity;
    }

    public static bool DetectCircleLineCollision(NewCircleCollider c,Vector2 la,Vector2 lb) {
        float dist = Calc.DistancePointToLine(c.Position2D,la,lb);
        if (dist < c.Radius) {
            Vector2 closestPoint = Calc.ClosestPointToLine(c.Position2D,la,lb);
            float dist2 = Vector2.Distance(c.Position2D,closestPoint);
            if (dist2 < c.Radius) {
                return true;
            }
        }
        return false;
    }

    //public static bool LinePolygonCollision(Vector2 lineStart,Vector2 lineEnd,Vector2[] polygon,out Vector2 intersect) {
    //    //Vector2 closest = Vector2.negativeInfinity;
    //    for (int i = 0; i < polygon.Length; i++) {
    //        Vector2 polyLineA = polygon[i];
    //        Vector2 polyLineB = polygon[(i + 1) % polygon.Length];
    //        if (Calc.LineIntersect(lineStart,lineEnd,polyLineA,polyLineB,out intersect)) {
    //            return true;
    //        }
    //    }
    //    intersect = Vector2.negativeInfinity;
    //    return false;
    //}

    public static bool DetectCircleCircleCollision(NewCircleCollider a, NewCircleCollider b) {
        Vector2 diff = b.Position2D - a.Position2D;
        return diff.sqrMagnitude < (a.Radius + b.Radius) * (a.Radius + b.Radius);
    }

    public static void ResolveActorActorCollision(Actor a,Actor b) {
        Vector2 diff = b.position2D - a.position2D;
        if (diff.sqrMagnitude < (a.collider.Radius + b.collider.Radius) * (a.collider.Radius + b.collider.Radius)) {
            Vector2 normal = diff.normalized;

            float halfPenetration;

            if (float.IsNaN(normal.x)) {
                // actors positions are identical
                halfPenetration = (a.collider.Radius + b.collider.Radius) / 2;
                normal = Vector2.right;
            } else {
                halfPenetration = (a.collider.Radius + b.collider.Radius - Vector2.Distance(a.position2D,b.position2D)) / 2;
            }

            if (Calc.DotProduct(normal,a.velocity) > 0) {
                a.velocity -= normal * halfPenetration;
            }

            if (Calc.DotProduct(normal,b.velocity) < 0) {
                b.velocity -= normal * halfPenetration;
            }

            if(a.movement == b.movement || a.movement != Movement.NONE) {
                a.position2D -= normal * halfPenetration;
            }
            
            if(a.movement == b.movement || b.movement != Movement.NONE) {
                b.position2D += normal * halfPenetration;
            }
        }
    }



    // Returns true if the radius of an enemy actor and the radius of bullet are overlapping 
    public static bool CompareRadiusBulletActor(Bullet b,Actor e) {
        if (Vector2.Distance(b.position2D,e.position2D) < b.collider.Radius + e.collider.Radius) {
            return true;
        }

        return false;
    }

    // Same as above but for player and enemy collisions, maybe we should just have generic compare radius
    public static bool CompareRadiusPlayerEnemy(Actor e)
    {
        if (Vector2.Distance(GameManager.main.player.position2D, e.position2D) < GameManager.main.player.collider.Radius + e.collider.Radius)
        {
            return true;
        }

        return false;
    }

    //public static Vector2 LineCirclesCollision(Vector2 lineA, Vector2 lineB, Actor[] actors) {
    //    Vector2 best = Vector2.negativeInfinity;
    //    for(int i = 0; i < actors.Length; i++) {
    //        Vector2 distance = Vector2.Distance(Utils.Vector3ToVector2XZ());
    //    }
    //    return best;
    //}

    public static List<CollisionPoint> CollisionPointsOnLine(Vector2 lineOrigin, Vector2 lineEnd, List<TerrainEdge> edges) {
        List<CollisionPoint> points = new List<CollisionPoint>();
        for(int i = 0; i < edges.Count; i++) {
            TerrainEdge e = edges[i];
            Vector2 intersect;
            //if (!CollisionSystem.LayerExact(e.layer,filter)) {
            //    continue;
            //}

            if (Calc.LineIntersect(lineOrigin,lineEnd,e.vertA_pos2D,e.vertB_pos2D,out intersect)) {
                points.Add(new CollisionPoint(intersect,e.layer,Vector2.Distance(lineOrigin,intersect)));
            }            
        }
        return points;
    }

    public static void SortPointListByDist(List<CollisionPoint> list) {

    }
}

public struct CollisionPoint {
    public Vector2 pos;
    public Layer layer;
    public float dist;
    public CollisionPoint(Vector2 p, Layer l, float d) {
        pos = p;
        layer = l;
        dist = d;
    }
}

