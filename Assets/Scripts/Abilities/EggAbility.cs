using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used by an egg
// destroys itself and spawns a new actor after a delay
public class EggAbility : Ability
{
    float timestamp;
    GameObject spawn;

    // reference to the list of thing that spawned the egg
    // so the egg can add its spawned actor to its mothers list of spawned entities
    List<Transform> motherSpawnerList;

    public EggAbility(float growTime, GameObject spawn, List<Transform> list = null) {
        timestamp = growTime + Time.time;
        this.spawn = spawn;
        motherSpawnerList = list;
    }


    public override bool RunAbilityUpdate(Actor a,GameData data) {
        
        Actor babyBug = GameManager.main.SpawnActor(spawn,a.position2D);
        if (motherSpawnerList != null) {
            motherSpawnerList.Add(babyBug.transform);
        }
        GameManager.main.DestroyActor(a);
        return true;
    }

    
    public override bool StartAbilityCheck(Actor a,GameData data) {
        return Time.time > timestamp;
    }
}
