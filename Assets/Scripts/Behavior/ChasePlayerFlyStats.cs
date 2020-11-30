using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChasePlayerFly",menuName = "Behaviors/ChasePlayerFly",order = 2)]
public class ChasePlayerFlyStats : BehaviorStats
{
    //public float wanderDegrees;

    public float AggroDistance;

    public override Behavior GetBehaviorInstance() {
        return new ChasePlayerFly(this);
    }
}
