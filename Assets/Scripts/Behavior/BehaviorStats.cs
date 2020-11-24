using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviorStats : ScriptableObject
{
    public abstract Behavior GetBehaviorInstance();
}
