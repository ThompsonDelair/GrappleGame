using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_EnableObjectiveTracking : ObjectiveSubscriber {
    
    [SerializeField] List<GameObject> objectiveList;

    protected override void OnObjectiveCompletion() {
        foreach(GameObject objective in objectiveList) {

            // Check if objective has become null
            if (objective != null) {
                ObjectiveTracker tracker = objective.GetComponent<ObjectiveTracker>();

                // Only re-enable objectives that have yet to meet their tracking parameter.
                if (tracker.objectiveActive == false && !tracker.ObjectiveParametersComplete()) {
                    tracker.objectiveActive = true;
                }
            }
            
        }
        
    }
}
