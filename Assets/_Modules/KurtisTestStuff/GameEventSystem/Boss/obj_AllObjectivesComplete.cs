using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_AllObjectivesComplete : ObjectiveTracker {
    [SerializeField] List<GameObject> objectiveList;

    protected int completedObjectives;
    protected override void ListenForObjectiveCompletion() {

        // If there were provided objective objects, track their progress.
        if (objectiveList.Count > 0) {
            completedObjectives = 0;
            foreach(GameObject objective in objectiveList) {

                // Check if objective has become null
                if (objective != null) {
                    ObjectiveTracker tracker = objective.GetComponent<ObjectiveTracker>();

                    // If the objective is not active and it's parameter is complete, increment completedObjectives.
                    if (tracker.objectiveActive == false && tracker.ObjectiveParametersComplete()) {
                        
                        completedObjectives++;
                    }
                }
                
            }

            // After all objectives have been counted, check if they are all completed
            if (ObjectiveParametersComplete()) {
                GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
                objectiveActive = false;
            }
        }
    }

    // If the count is equal to the size of the list, we have a winner.
    public override bool ObjectiveParametersComplete() {
        return objectiveList.Count > 0 && completedObjectives == objectiveList.Count;
    }
}
