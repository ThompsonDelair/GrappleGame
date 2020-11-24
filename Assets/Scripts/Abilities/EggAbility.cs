using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggAbility : Ability
{
    public float timestamp;
    GameObject spawn;
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
