using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The SUBSCRIBER component is implemented to ACT on events.
//      It includes base setup for the event-listening flow.
//      The only aspect that must be implemented in inheriting members is the OnObjectiveCompletion() function.
public abstract class ObjectiveSubscriber : MonoBehaviour {
    protected GameManager gameManager;
    public int objectiveId;

    // The Start Function MUST subscribe to the GameEventDirector
    //      If the Inheriting member requires it's own Start() Function, ensure this code is copied into the new Start()
    void Start() {
        gameManager = FindObjectOfType<GameManager>();
        GameEventDirector.current.onObjectiveCompletion += CheckObjectiveId; // This is setting what function will be called on event dispatch.
    }

    // If the dispatched event's ID matches the SUBSCRIBER's objectiveId, we call the OnObjectiveCompletion function.
    protected void CheckObjectiveId(int id) {
        if (id == objectiveId) {
            OnObjectiveCompletion();
        }
    }

    // This function should be overriden by Inheriting members.
    //      It is called when an event matching the SUBSCRIBER's objectiveId is invoked.
    protected virtual void OnObjectiveCompletion() {
        // Do something.
    }

    // The Destroy Function MUST unsubscribe from the GameEvenDirector
    private void OnDestroy() {
        GameEventDirector.current.onObjectiveCompletion -= CheckObjectiveId;
    }
}
