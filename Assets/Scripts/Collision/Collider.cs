using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericCollider 
{
    public abstract bool DetectCircleCollision(NewCircleCollider c);
    public abstract Vector2 FindCircleCollisionPoint(NewCircleCollider c);
}
