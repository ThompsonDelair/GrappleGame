using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_FinalBossNeutralized : ObjectiveTracker
{
    [Header("Target Neutralization Configs")]
    [SerializeField] protected GameObject[] targetObjects;
    int numTargetsDestroyed;

    // This implementation of objective will check if all enemies have been cleared, and the specific given targets destroyed.
    protected override void ListenForObjectiveCompletion() {

        numTargetsDestroyed = 0;

        // If a GameObject in the list is null , it means it is Destroyed
        foreach (GameObject ob in targetObjects) {
            if (ob == null) {
                numTargetsDestroyed++;
            }
        }

        // If the number of null objects in the list is equal to it's size, all targets have been eliminated.
        if (ObjectiveParametersComplete()) {
            GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
            EffectController.main.CameraShake(5f, 0.7f);
            objectiveActive = false;
        }
        
    }

    public override bool ObjectiveParametersComplete() {
        return numTargetsDestroyed == targetObjects.Length;
    }
}
