using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_AreaEntered : ObjectiveTracker {
    protected AreaTrigger trigger;

    // Start is called before the first frame update
    void Awake() {
        trigger = this.GetComponent<AreaTrigger>();
    }

    // The AreaTrigger component is polled to determine when a trigger event is completed
    protected override void ListenForObjectiveCompletion() {
        if (!trigger.active) {
            GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
            objectiveActive = false;
        }
    }
}
