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
    //          See ObjectiveSubscriber.cs for example implementation.
    public event Action<int> onObjectiveCompletion;

    // This function is called by TRACKERS, and tells the event system to dispatch an event.
    public void ObjectiveCompletion(List<int> objectiveIdList) {

        foreach (int id in objectiveIdList) {
            // Check that the action is non-null before invoking. All SUBSCRIBERS will be notified.
            if (id != 0) {
                Debug.Log("An objective with id of " + id + " has been completed, dispatching.");

                if (onObjectiveCompletion != null) {
                    onObjectiveCompletion(id);
                }
            }
        }
    }
}
