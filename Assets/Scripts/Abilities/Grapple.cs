using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this class runs the mechanics for the player's grapple ability
public class Grapple
{
    const float grapEndMargin = 0.5f;
    // how fast the grapple extends
    const float launchSpeed = 85f;
    // how fast the grapple retracts
    const float rewindSpeed = 38f;
    public const float grappleRange = 20f;
    const float minGrappleDist = 1.25f;
    //const float failedGrapModifier = 1.5f;

    Actor owner;
    EaseDelegate shoot = Ease.OutCubic;
    EaseDelegate retract = Ease.InOutCubic;

    float timestamp;
    float shootTime;   
    float retractTime;

    public bool GrappleLanded { get { return grappleAnimationLanded; } } // This is a useful thing to have readable (but not editable) by outside components. - Kurt
    bool grappleAnimationLanded = false;
    bool grapFailed = false;

    Vector2 grapplePos;
    Vector2 playerPosAtGrapStart;

    Vector2 grappleEndPoint;
    Vector2 grappleEndPointWithOffset;

    LineRenderer lr;

    List<Tuple<Vector2,Color>> grapCollisionPoints = new List<Tuple<Vector2,Color>>();
    //Material m;

    public Grapple(Actor a) {
        owner = a;
        lr = a.transform.GetComponent<LineRenderer>();
        //m = lr.material;
    }

    
    public void StartGrapple(Vector2 dir,Map map) {

        FindGrappleEndPoint(owner.position2D,dir,map);
        SetTravelSpeed(owner.position2D,grappleEndPoint);

        if (Vector2.Distance(grappleEndPoint,owner.position2D) > minGrappleDist) {
            owner.currMovement = Movement.FLYING;
            playerPosAtGrapStart = owner.position2D;
            grapplePos = owner.position2D;
            grapFailed = false;
            grappleAnimationLanded = false;
            SoundManager.StartClipOnActor(AudioClips.singleton.grapShoot, owner, 6f, false);
            timestamp = Time.time;
            lr.positionCount = 2;
        } else {
            SoundManager.StartClipOnActor(AudioClips.singleton.grapEnd, owner, 6f, false);
            grapFailed = true;
            Vector3 endPos = owner.position3D + Utils.Vector2XZToVector3(dir).normalized * grappleRange;
            owner.transform.GetComponent<GrappleFailedVisuals>().ShowVisual(endPos);
        }

    }

    // tries to find a valid point to grapple to
    void FindGrappleEndPoint(Vector2 start, Vector2 dir, Map map) {
        Vector2 end = dir.normalized * grappleRange + start;
        List<CollisionPoint> points = CollisionCalc.CollisionPointsOnLine(start,end,map.terrainEdges);
        Debug.LogFormat("{0} grapple points",points.Count);
        bool hitWall = false;

        if (points.Count == 0) {
            // our grapple has no collision with any terrain so it can automatically grapple its furthest distance
            grappleEndPoint = end;
        } else {

            // go through all terrain collision points to try to find a valid end point

            points.Sort(delegate (CollisionPoint a,CollisionPoint b) {
                return a.dist.CompareTo(b.dist);
            });

            // backpoint is used to check the area right behind a collision
            Vector2 backPoint = dir.normalized * -owner.collider.Radius;
            
            int i = 0;
            for(; i < points.Count; i++) {
                CollisionPoint c = points[i];

                // if our grapple hit a wall, it can't go any further

                if (CollisionSystem.LayerOrCheck(c.layer,Layer.BLOCK_FLY)){
                    hitWall = true;
                    break;
                }
            }

            if (!hitWall && map.IsPointWalkable(end + backPoint)) {

                grappleEndPoint = end + backPoint;
            } else {

                // if our grapple git a wall, loop backwards and try to find a valid walkable point to land on

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


    }

    
    void SetTravelSpeed(Vector2 start, Vector2 end) {
        float dist = Vector2.Distance(start,end);
        retractTime = dist / rewindSpeed;
        shootTime = dist / launchSpeed;
    }

    public void EndGrapple() {
        SoundManager.StartClipOnActor(AudioClips.singleton.grapEnd, owner, 6f, false);
        owner.currMovement = Movement.WALKING;
        lr.positionCount = 0;
    }

    public void GrappleUpdate(Map map) {

        if (grappleAnimationLanded) {

            // pull the player towards the grapple point

            float t = (Time.time - timestamp) / retractTime;

            if (!grapFailed) {
                owner.position2D = (grappleEndPointWithOffset - playerPosAtGrapStart) * retract(t) + playerPosAtGrapStart;
            }

            if (t > 1) {
                EndGrapple();
            }

        } else {

            // shoot the grapple animation out

            float t = (Time.time - timestamp) / shootTime;

            if (t > 1) {
                grappleAnimationLanded = true;
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
