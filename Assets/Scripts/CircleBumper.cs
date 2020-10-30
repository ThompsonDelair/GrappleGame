using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBumper : Area
{
    public override void OnActorCollision(Actor a) {
        a.momentumDir = a.position2D - Utils.Vector3ToVector2XZ(transform.position);
        a.momentumForce = 30f;
    }
}
