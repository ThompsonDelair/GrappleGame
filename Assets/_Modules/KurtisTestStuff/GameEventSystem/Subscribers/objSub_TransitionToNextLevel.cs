using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_TransitionToNextLevel : ObjectiveSubscriber {
    protected SceneControl sceneController;

    protected override void OnObjectiveCompletion() {
        sceneController = gameManager.GetComponent<SceneControl>();
        sceneController.ShowMessageThenNextLevel("Objective complete!");
    }
}
