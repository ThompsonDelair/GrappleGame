using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonCollider : GenericCollider
{
    public Vector2[] Verts { get { return verts; } }


    Vector2[] verts;

    public PolygonCollider(Vector2[] v) {
        verts = v;
    }

    public override bool DetectCircleCollision(NewCircleCollider c) {
        return CollisionCalc.DetectCirclePolygonCollision(c,this);
    }

    public override Vector2 FindCircleCollisionPoint(NewCircleCollider c) {
        CollisionInfo nfo;
        return CollisionCalc.FindCirclePolygonCollision(c.Position2D,c.Radius,verts, out nfo);
    }
}
