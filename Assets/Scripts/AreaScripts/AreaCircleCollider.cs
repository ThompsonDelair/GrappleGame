using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaCircleCollider : AreaCollider
{
    public OurCircleCollider CircleCollider { get { return circleCollider; } }
    OurCircleCollider circleCollider;
    [SerializeField] private float radius;

    public override bool DetectCircleCollision(OurCircleCollider c) {
        return circleCollider.DetectCircleCollision(c);
    }

    private void Awake() {
        circleCollider = new OurCircleCollider(radius,transform);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position,radius);
    }
}
