using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple
{
    public Actor owner;

    EaseDelegate shoot = Ease.OutCubic;
    EaseDelegate retract = Ease.InOutQuart;

    const float stopDist = 0.6f;

    const float shootSpeedMax = 60f;
    public bool shootSpeedEase = false;
    const float shootSpeedMin = 40f;
    const float shootEaseTime = 1f;

    const float retractSpeedMin = 30f;
    //const bool retractSpeedEase;
    const float retractSpeedMax = 30f;
    //const float retractEaseTime;


    bool grappleLanded = false;
    public bool GrappleLanded { get { return grappleLanded; } } // This is a useful thing to have readable (but not editable) by outside components. - Kurt
    Vector2 dir;
    Vector2 pos;
    float speed;
    float timestamp;
    Vector2 playerPosAtGrapStart;
    float retractTime;
    bool grabbedActor;
    Actor actorGrabbed;

    List<Tuple<Vector2,Color>> grapCollisionPoints = new List<Tuple<Vector2,Color>>();

    public void StartGrapple(Vector2 dir) {
        owner.movement = Movement.IGNORE_CLIFFS;
        this.dir = dir;
        pos = owner.position2D;
        grappleLanded = false;
        owner.PlayAudioClip(AudioClips.singleton.grapShoot);
        speed = shootSpeedMax;
        timestamp = Time.time;
        GameManager.main.lineRenderer.positionCount = 2;
        //GrappleDebug(dir,pos);
    }

    void GrappleDebug(Vector2 dir, Vector2 pos) {
        
        Map map = GameManager.main.map;
        Vector2 pos2 = dir.normalized * 999 + pos;
        grapCollisionPoints.Clear();
        for (int i = 0; i < map.terrainEdges.Count; i++) {
            TerrainEdge e = map.terrainEdges[i];
            Vector2 point;
            if (Calc.LineIntersect(pos,pos2,e.vertA_pos2D,e.vertB_pos2D,out point)) {
                Layer l = e.layer;
                Color c = Color.magenta;

                if (l == Layer.CLIFFS) {
                    c = Color.blue;
                } else if (l == Layer.WALLS) {
                    c = Color.white;
                } else if (CollisionSystem.LayerCheck(l,Layer.NO_GRAPPLE)) {
                    c = Color.cyan;
                }
                Tuple<Vector2,Color> tuple = new Tuple<Vector2,Color>(point,c);
                grapCollisionPoints.Add(tuple);
            }
        }
        if (DebugControl.main.breakOnGrappleUpdate)
            Debug.Break();
    }

    void DrawCollisionPoints() {
        foreach(Tuple<Vector2,Color> t in grapCollisionPoints) {
            Vector3 pos = Utils.Vector2XZToVector3(t.Item1);
            Utils.debugStarPoint(pos,1f,t.Item2);
        }
    }

    public void EndGrapple() {
        owner.PlayAudioClip(AudioClips.singleton.grapEnd); 
        owner.movement = Movement.WALKING;
        //Vector3[] noPoints = new Vector3[0];
        GameManager.main.lineRenderer.positionCount = 0;
    }

    public void GrappleUpdate(List<EnemyActor> enemies,Map map) {
        //DrawCollisionPoints();
        if (grappleLanded) {

            float t = (Time.time - timestamp) / retractTime;
            owner.position2D = (pos - playerPosAtGrapStart) * retract(t) + playerPosAtGrapStart;

            if (Vector2.Distance(owner.position2D,pos) < stopDist) {
                EndGrapple();
            }

        } else {
            Vector2 grappleNextPos = pos + dir.normalized * speed * Time.deltaTime;

            // check for collision with enemies
            //for (int i = 0; i < enemies.Count; i++) {
            //    Actor a = enemies[i];
            //    Vector2 intersect = CollisionCalc.FindCircleLineCollision(a.collider,pos,owner.position2D);
            //    if (intersect != Vector2.negativeInfinity) {
            //        if (a.collider.Radius <= owner.collider.Radius) {

            //            actorGrabbed = a;
            //        } else {
            //            grabbedActor = false;
            //        }
            //    }
            //}

            // check for collisions with walls
            for (int i = 0; i < map.terrainEdges.Count; i++) {
                Vector2 intersect;
                TerrainEdge e = map.terrainEdges[i];
                

                if (e.layer == Layer.CLIFFS) {
                    continue;

                }

                if (Calc.LineIntersect(pos,grappleNextPos,e.vertA_pos2D,e.vertB_pos2D,out intersect)) {

                    if (CollisionSystem.LayerCheck(e.layer,Layer.NO_GRAPPLE)) {
                        EndGrapple();
                        return;
                    }

                    grappleLanded = true;
                    owner.PlayAudioClip(AudioClips.singleton.grapLoop,true);
                    pos = intersect;
                    playerPosAtGrapStart = owner.position2D;
                    retractTime = Vector2.Distance(pos,owner.position2D) / retractSpeedMin;
                    timestamp = Time.time;
                }



                //if (CollisionSystem.LinePolygonCollision(pos,grappleNextPos,polygons[i],out intersect)) {
                //    grappleLanded = true;
                //    owner.PlayAudioClip(AudioClips.singleton.grapLoop,true);
                //    pos = intersect;
                //    playerPosAtGrapStart = owner.position2D;
                //    retractTime = Vector2.Distance(pos,owner.position2D) / retractSpeedMin;
                //    timestamp = Time.time;
                //}
            }
            if (!grappleLanded) {
                pos = grappleNextPos;
                if (shootSpeedEase) {
                    float t = (Time.time - timestamp) / shootEaseTime;
                    if (t < 1) {
                        speed = shootSpeedMax - (shootSpeedMax - shootSpeedMin) * shoot(t);
                    } else {
                        speed = shootSpeedMin;
                    }
                }
            }
        }

        DrawGrapple(owner.position3D + Vector3.up,Utils.Vector2XZToVector3(pos)+Vector3.up);
        //GameManager.main.DrawDebugLine(owner.position3D,Utils.Vector2XZToVector3(pos),Color.white);
    }

    void DrawGrapple(Vector3 start,Vector3 end) {
        LineRenderer lr = GameManager.main.lineRenderer;
        Vector3[] points = { start,end };
        lr.SetPositions(points);
    }



}
