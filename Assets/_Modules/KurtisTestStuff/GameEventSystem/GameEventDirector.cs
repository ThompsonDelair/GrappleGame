using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventDirector : MonoBehaviour
{

    public static GameEventDirector current;

    void Awake() {
        
        if (current == null) {
            // DontDestroyOnLoad(this);
            current = this;

        } else {
            Destroy(this.gameObject);
        }
    }

    // This action is what doors & stuff can subscribe to, and activate when it's dispatched.
    public event Action<int> onObjectiveCompletion;

    // This function is called by outside components, and tells the event system to dispatch an event.
    public void ObjectiveCompletion(int objectiveId) {
        // Check that the action is non-null before invoking.
        Debug.Log("An objective with id of " + objectiveId + " has been completed, dispatching.");
        if (onObjectiveCompletion != null) {
            onObjectiveCompletion(objectiveId);
        }
    }
}
