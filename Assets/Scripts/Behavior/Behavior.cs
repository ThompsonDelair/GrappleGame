using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behavior
{
    public const float pathfindMaxRange = 45f;

    public abstract void Update(Actor a, GameData data);

    protected bool UnderMaxPathRange(Actor a, Actor player) {
        return Vector2.SqrMagnitude(a.position2D - player.position2D) < pathfindMaxRange * pathfindMaxRange;
    }
}
