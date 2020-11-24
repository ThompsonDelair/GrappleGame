using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_DestroyListedActors : ObjectiveSubscriber
{
    [SerializeField] private GameObject[] actorsToDestroy;
    protected override void OnObjectiveCompletion() {

        for(int i = 0; i < actorsToDestroy.Length; ++i) {
            // Actor a = actorsToDestroy[i].GetComponent<Actor>();
            // GameManager.main.DestroyActor(a);
        }
        
    }
}
