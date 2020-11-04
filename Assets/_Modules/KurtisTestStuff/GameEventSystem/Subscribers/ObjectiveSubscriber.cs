using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectiveSubscriber : MonoBehaviour {
    protected GameManager gameManager;
    public int objectiveId;

    void Start() {
        gameManager = FindObjectOfType<GameManager>();
        GameEventDirector.current.onObjectiveCompletion += CheckObjectiveId;
    }

    protected void CheckObjectiveId(int id) {
        if (id == objectiveId) {
            OnObjectiveCompletion();
        }
    }

    protected virtual void OnObjectiveCompletion() {
        // Do something.
    }

    private void OnDestroy() {
        GameEventDirector.current.onObjectiveCompletion -= CheckObjectiveId;
    }
}
