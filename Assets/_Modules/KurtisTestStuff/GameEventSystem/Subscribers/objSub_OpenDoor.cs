using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_OpenDoor : ObjectiveSubscriber {
    protected override void OnObjectiveCompletion() {
        Debug.Log(this.name + " is notified of objective ID " + objectiveId + " dispatch, opening door.");
    }
}
