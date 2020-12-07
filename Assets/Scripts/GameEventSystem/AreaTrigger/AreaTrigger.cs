using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTrigger : Area {

    // Simply detect if a player has entered the region.
    public override void OnActorCollision(Actor a) {
        if (a == GameManager.main.player && GetComponent<obj_AreaEntered>().objectiveActive) {
            Debug.Log("Player has entered region.");
            active = false;
        }
    }
}
