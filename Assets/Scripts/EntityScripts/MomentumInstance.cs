using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushInstance
{
    public Vector2 dir;
    public float force;
    public float friction;

    public PushInstance(Vector2 dir, float force, float friction) {
        this.dir = dir;
        this.force = force;
        this.friction = friction;
    }
}
