using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityStats : ScriptableObject
{
    public abstract Ability GetAbilityInstance();
}
