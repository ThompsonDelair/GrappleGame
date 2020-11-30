using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MutateOnHealthThreshold : Behavior
{
    MutateOnHealthThresholdStats stats;

    public MutateOnHealthThreshold(MutateOnHealthThresholdStats s) {
        stats = s;
    }

    public override void Update(Actor a,GameData data) {
        if((a.health / a.stats.maxHP) <= stats.healthThreshold) {
            GameManager.main.SpawnActor(stats.actorToSpawn,a.position2D);
            GameManager.main.DestroyActor(a);
            AudioSource.PlayClipAtPoint(stats.mutateSound,a.position3D);
        }
    }
}
