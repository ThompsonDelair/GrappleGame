using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for abilities
public abstract class Ability
{
    public Ability(){}

    // this function returns true when an ability is ready to cast
    // returns false when ability isn't ready (out of range, on cooldown, etc)
    public abstract bool StartAbilityCheck(Actor a, GameData data);

    // runs when an ability is being executed
    // returns false when ability is finished executing
    public abstract bool RunAbilityUpdate(Actor a, GameData data);

}
