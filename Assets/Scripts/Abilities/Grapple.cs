using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple
{
    const float grapEndMargin = 0.5f;
    const float launchSpeed = 85f;
    const float rewindSpeed = 38f;
    public const float grappleRange = 20f;
    const float minGrappleDist = 1.25f;
    const float failedGrapModifier = 1.5f;

    public Actor owner;
    EaseDelegate shoot = Ease.OutCubic;
    EaseDelegate retract = Ease.InOutCubic;

    float timestamp;
    float shootTime;   
    float retractTime;

    public bool GrappleLanded { get { return grappleLanded; } } // This is a useful thing to have readable (but not editable) by outside components. - Kurt
    bool grappleLanded = false;
    bool grapFailed = false;

    Vector2 grapplePos;
    Vector2 playerPosAtGrapStart;

    Vector2 grappleEndPoint;
    Vector2 grappleEndPointWithOffset;

    LineRenderer lr;

    List<Tuple<Vector2,Color>> grapCollisionPoints = new List<Tuple<Vector2,Color>>();

    public Grapple(Actor a) {
        owner = a;
        lr = a.transform.GetComponent<LineRenderer>();
    }

    public void StartGrapple(Vector2 dir,Map map) {


        FindGrappleEndPoint(owner.position2D,dir,map);

        if(Vector2.Distance(grappleEndPoint,owner.position2D) > minGrappleDist) {
            owner.currMovement = Movement.FLYING;
            playerPosAtGrapStart = owner.position2D;
            grapplePos = owner.position2D;
            grapFailed = false;
            grappleLanded = false;
            owner.PlayAudioClip(AudioClips.singleton.grapShoot);
            timestamp = Time.time;
            lr.positionCount = 2;
        } else {
            owner.PlayAudioClip(AudioClips.singleton.grapEnd);
            grapFailed = true;
        }


        //GrappleDebug(dir,pos);
    }

    void FindGrappleEndPoint(Vector2 start, Vector2 dir, Map map) {
        Vector2 end = dir.normalized * grappleRange + start;
        List<CollisionPoint> points = CollisionCalc.CollisionPointsOnLine(start,end,map.terrainEdges);
        Debug.LogFormat("{0} grapple points",points.Count);
        bool hitWall = false;

        if (points.Count == 0) {
            grappleEndPoint = end;
        } else {
            //Vector2 closest = points[0].pos;
            //float bestDist = Vector2.SqrMagnitude(start - points[0].pos);
            //for(int i = 1; i < points.Count; i++) {
            //    CollisionPoint c = points[i];
            //    float currDist = Vector2.SqrMagnitude(start - c.pos);

            //    if (currDist < bestDist) {
            //        closest = c.pos;
            //        bestDist = currDist;
            //    }
            //}

            points.Sort(delegate (CollisionPoint a,CollisionPoint b) {
                return a.dist.CompareTo(b.dist);
            });
            Vector2 backPoint = dir.normalized * -owner.collider.Radius;
            
            int i = 0;
            //grappleEndPoint = closest;
            for(; i < points.Count; i++) {
                CollisionPoint c = points[i];

                //if (CollisionSystem.LayerOrCheck(c.layer,Layer.BLOCK_FLY)) {
                //    break;
                //}

                if (CollisionSystem.LayerOrCheck(c.layer,Layer.BLOCK_FLY)){
                    hitWall = true;
                    break;
                }
            }

            if (!hitWall && map.IsPointWalkable(end + backPoint)) {
                grappleEndPoint = end + backPoint;
                Debug.Log("end point is walkable");
            } else {
                if (i >= points.Count)
                    i = points.Count - 1;
                for (; i >= 0; i--) {
                    CollisionPoint c = points[i];
                    Vector2 point = c.pos + backPoint;
                    if (map.IsPointWalkable(point)) {
                        grappleEndPoint = point;
                        break;
                    }
                }
            }

           
        }

        if (hitWall) {
            grappleEndPointWithOffset = grappleEndPoint - dir.normalized * (owner.collider.Radius + grapEndMargin);
        } else {
            grappleEndPointWithOffset = grappleEndPoint;
        }

        


        float dist = Vector2.Distance(start,grappleEndPoint);
        retractTime = dist / rewindSpeed;
        shootTime = dist / launchSpeed;
    }

    //void GrappleDebug(Vector2 dir, Vector2 pos) {
        
    //    Map map = GameManager.main.map;
    //    Vector2 pos2 = dir.normalized * 999 + pos;
    //    grapCollisionPoints.Clear();
    //    for (int i = 0; i < map.terrainEdges.Count; i++) {
    //        TerrainEdge e = map.terrainEdges[i];
    //        Vector2 point;
    //        if (Calc.LineIntersect(pos,pos2,e.vertA_pos2D,e.vertB_pos2D,out point)) {
    //            Layer l = e.layer;
    //            Color c = Color.magenta;

    //            if (l == Layer.BLOCK_FLY) {
    //                c = Color.blue;
    //            } else if (l == Layer.BLOCK_WALK) {
    //                c = Color.white;
    //            }
    //            Tuple<Vector2,Color> tuple = new Tuple<Vector2,Color>(point,c);
    //            grapCollisionPoints.Add(tuple);
    //        }
    //    }
    //    if (DebugControl.main.breakOnGrappleUpdate)
    //        Debug.Break();
    //}

    void DrawCollisionPoints() {
        foreach(Tuple<Vector2,Color> t in grapCollisionPoints) {
            Vector3 pos = Utils.Vector2XZToVector3(t.Item1);
            Utils.debugStarPoint(pos,1f,t.Item2);
        }
    }

    public void EndGrapple() {
        owner.PlayAudioClip(AudioClips.singleton.grapEnd); 
        owner.currMovement = Movement.WALKING;
        //Vector3[] noPoints = new Vector3[0];
        lr.positionCount = 0;
    }

    public void GrappleUpdate(Map map) {
        //DrawCollisionPoints();
        if (grappleLanded) {

            float t = (Time.time - timestamp) / retractTime;


            if (!grapFailed) {
                owner.position2D = (grappleEndPointWithOffset - playerPosAtGrapStart) * retract(t) + playerPosAtGrapStart;
            }

            if (t > 1) {
                EndGrapple();
            }

            //if (Vector2.Distance(owner.position2D,grappleEndPointWithOffset) < grapEndMargin) {
                
            //}

        } else {
            float t = (Time.time - timestamp) / shootTime;

            if (t > 1) {
                grappleLanded = true;
                grapplePos = grappleEndPoint;
                timestamp = Time.time;
            }

            grapplePos = (grappleEndPoint - playerPosAtGrapStart) * shoot(t) + playerPosAtGrapStart;
        }
        DrawGrapple(owner.position3D + Vector3.up,Utils.Vector2XZToVector3(grapplePos)+Vector3.up);
    }

    void DrawGrapple(Vector3 start,Vector3 end) {
        Vector3[] points = { start,end };
        lr.SetPositions(points);
    }



}
