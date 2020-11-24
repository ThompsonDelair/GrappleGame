using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AreaCollider : MonoBehaviour
{
    public abstract bool DetectCircleCollision(NewCircleCollider c);
}
