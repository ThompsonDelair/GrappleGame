using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_TransitionToNextLevel : ObjectiveSubscriber {
    protected SceneControl sceneController;
    [SerializeField] protected float transitionDuration = 0f;

    protected override void OnObjectiveCompletion() {
        sceneController = gameManager.GetComponent<SceneControl>();
        sceneController.ShowMessageThenNextLevel("Objective complete!", transitionDuration);
    }
}
