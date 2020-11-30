using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMutate",menuName = "Behaviors/Mutate",order = 5)]
public class MutateOnHealthThresholdStats : BehaviorStats
{
    public float healthThreshold;
    public GameObject actorToSpawn;
    public AudioClip mutateSound;

    public override Behavior GetBehaviorInstance() {
        return new MutateOnHealthThreshold(this);
    }
}
