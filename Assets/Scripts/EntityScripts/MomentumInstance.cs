using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An instance of a push force active on an actor
// An actor has a list of push forces currently active on it
// PushInstances will be destroyed once their force decays to <= 0
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
