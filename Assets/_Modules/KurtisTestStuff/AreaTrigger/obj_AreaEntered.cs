using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj_AreaEntered : ObjectiveTracker {
    protected AreaTrigger trigger;

    // Start is called before the first frame update
    void Awake() {
        trigger = this.GetComponent<AreaTrigger>();
    }

    protected override void ListenForObjectiveCompletion() {
        if (!trigger.active) {
            GameEventDirector.current.ObjectiveCompletion(objectiveIdList);
            objectiveActive = false;
        }
    }
}
