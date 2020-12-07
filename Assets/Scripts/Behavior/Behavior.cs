using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Behaviors update every frame, similar to Update() in monobehaviors
// Actors can have multiple behaviors
public abstract class Behavior
{
    public const float pathfindMaxRange = 45f;

    public abstract void Update(Actor a, GameData data);

    protected bool UnderMaxPathRange(Actor a, Actor player) {
        return Vector2.SqrMagnitude(a.position2D - player.position2D) < pathfindMaxRange * pathfindMaxRange;
    }
}
