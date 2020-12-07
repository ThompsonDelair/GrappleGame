using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// area colliders are used to check collision with actors
//      (actors use circle colliders)
public abstract class AreaCollider : MonoBehaviour
{
    public abstract bool DetectCircleCollision(OurCircleCollider c);
}
