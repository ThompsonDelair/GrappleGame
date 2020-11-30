using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChasePlayerWalkRanged",menuName = "Behaviors/ChasePlayerWalkRanged",order = 1)]
public class ChasePlayerWalkRangedStats : BehaviorStats
{
    public int range;
    public override Behavior GetBehaviorInstance() {
        return new ChasePlayerWalkRanged(this);
    }
}
