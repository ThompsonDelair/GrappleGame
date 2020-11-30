using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaCircleCollider : AreaCollider
{
    public NewCircleCollider CircleCollider { get { return circleCollider; } }
    NewCircleCollider circleCollider;
    [SerializeField] private float radius;

    public override bool DetectCircleCollision(NewCircleCollider c) {
        return circleCollider.DetectCircleCollision(c);
    }

    private void Awake() {
        circleCollider = new NewCircleCollider(radius,transform);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position,radius);
    }
}
