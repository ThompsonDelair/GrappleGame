using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_AllObjectsDestroyed : ObjectiveListener {

    [Header("Target Neutralization Configs")]
    [SerializeField] protected GameObject[] targetObjects;

    // This implementation of objective will check if all enemies have been cleared, and the given targets destroyed.
    protected override void ListenForObjectiveCompletion() {
        Debug.Log("The objective is to destroy all objects in the game list.");

        // Check enemy count. If 0, check target objects.
        if (gameManager.EnemyList.Count <= 0) {
            int numTargetsDestroyed = 0;

            // If a GameObject in the list is null , it means it is Destroyed
            foreach (GameObject ob in targetObjects) {
                if (ob == null) {
                    numTargetsDestroyed++;
                }
            }

            // If the number of null objects in the list is equal to it's size, all targets have been eliminated.
            if (numTargetsDestroyed == targetObjects.Length) {
                sceneController.ShowMessageThenNextLevel(victoryMessage);
            }
        }
        
    }
}
