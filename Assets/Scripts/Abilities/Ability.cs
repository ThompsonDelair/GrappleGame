using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For now abilities is only for EnemyActors, we may look into doing player actions with ability system as well after.
public abstract class Ability
{
    public abstract bool attacking();

    public Ability(){}

    public abstract bool StartAbilityCheck(Vector2 pos, EnemyActor a);
    public abstract bool RunAbilityUpdate(Vector2 pos, EnemyActor a);

}
