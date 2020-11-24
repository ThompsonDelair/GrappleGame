using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For now abilities is only for EnemyActors, we may look into doing player actions with ability system as well after.
public abstract class Ability
{
    //public abstract bool attacking();

    public Ability(){}

    public abstract bool StartAbilityCheck(Actor a, GameData data);
    public abstract bool RunAbilityUpdate(Actor a, GameData data);

}
