using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCircleCollider : GenericCollider
{
    public Vector2 Position2D { get { return Utils.Vector3ToVector2XZ(transform.position); } }
    public float Radius { get { return radius; } }


    Transform transform;
    float radius;

    public NewCircleCollider(float r, Transform t) {
        radius = r;
        transform = t;
    }

    public override bool DetectCircleCollision(NewCircleCollider c) {
        return CollisionCalc.DetectCircleCircleCollision(this,c);
    }

    public override Vector2 FindCircleCollisionPoint(NewCircleCollider c) {
        return (c.Position2D - Position2D).normalized * radius + Position2D;
    }
}
