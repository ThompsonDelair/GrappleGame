using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_DisableObjectiveTracking : ObjectiveSubscriber {
    
    [SerializeField] List<GameObject> objectiveList;

    protected override void OnObjectiveCompletion() {
        foreach(GameObject objective in objectiveList) {

            // Check if objective has become null
            if (objective != null) {
                ObjectiveTracker tracker = objective.GetComponent<ObjectiveTracker>();

                if (tracker.objectiveActive == true) {
                    tracker.objectiveActive = false;
                }
            }
            
        }
        
    }
}
