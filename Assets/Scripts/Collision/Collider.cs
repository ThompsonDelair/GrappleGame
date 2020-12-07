using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericCollider 
{
    public abstract bool DetectCircleCollision(OurCircleCollider c);
    public abstract Vector2 FindCircleCollisionPoint(OurCircleCollider c);
}
