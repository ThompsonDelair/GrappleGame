using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_AllEnemiesRouted : ObjectiveTracker
{
    // This implementation of objective will check if all enemies have been cleared, and the given targets destroyed.
    protected override void ListenForObjectiveCompletion() {
        // Check enemy count. If 0, check target objects.
        if (GameManager.main.gameData.enemyActors.Count == 0) {
            GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
            objectiveActive = false;
        }
        
    }
}
