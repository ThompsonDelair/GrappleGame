using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_EraseAllEnemies : ObjectiveSubscriber
{
    protected override void OnObjectiveCompletion() {
        GameManager.main.DestroyAllEnemies();
    }
}
