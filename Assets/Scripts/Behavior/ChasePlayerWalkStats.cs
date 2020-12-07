using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChasePlayerWalk",menuName = "Behaviors/ChasePlayerWalk",order = 1)]
public class ChasePlayerWalkStats : BehaviorStats
{    
    public override Behavior GetBehaviorInstance() {
        return new ChasePlayerWalk();
    }
}
