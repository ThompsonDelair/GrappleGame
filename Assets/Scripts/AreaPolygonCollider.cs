using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AreaPolygonCollider : AreaCollider
{
    public PolygonCollider PolygonCollider { get { return polygonCollider; } }
    PolygonCollider polygonCollider;

    public override bool DetectCircleCollision(NewCircleCollider c) {
        return polygonCollider.DetectCircleCollision(c);
    }

    private void Awake() {
        
        Transform[] children = Utils.GetChildren(transform);
        polygonCollider = new PolygonCollider(Utils.Vector2XZsFromTransforms(children));
    }

    private void OnDrawGizmos() {
        Transform[] children = Utils.GetChildren(transform);
        Utils.DrawTransformsAsPolygon(children,Color.cyan);
    }
}
