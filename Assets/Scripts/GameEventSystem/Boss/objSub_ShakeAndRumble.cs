using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_ShakeAndRumble : ObjectiveSubscriber {
    // [SerializeField] List<GameObject> objectiveList;

    protected override void OnObjectiveCompletion() {
        EffectController.main.CameraShake(2000f, 0.1f);
        
    }
}
