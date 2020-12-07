using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for ability stats scriptable object
public abstract class AbilityStats : ScriptableObject
{
    // get an instance on the ability these stats are for
    public abstract Ability GetAbilityInstance();
}
