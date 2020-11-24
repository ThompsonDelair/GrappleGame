using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpawnerNew : Ability
{
    SpawnerStats stats;
    int spawnTracker;

    //[SerializeField] private int spawnLimit = 1;
    List<Transform> spawned = new List<Transform>();
    float timestamp;

    public SpawnerNew(SpawnerStats s) {
        stats = s;
    }

    public override bool RunAbilityUpdate(Actor a,GameData data) {
        int numToSpawn = 0;
        if (stats.spawnAllTogether) {
            numToSpawn = stats.totalSpawnedLimit;
        } else {
            numToSpawn = stats.totalSpawnedLimit - spawned.Count;
            numToSpawn = (numToSpawn > stats.spawnedPerWave) ? stats.spawnedPerWave : numToSpawn;
        }

        for(int i = 0; i < numToSpawn; i++) {
            SpawnEgg(stats.entitiesToSpawn[spawnTracker],a,data);
            spawnTracker = (spawnTracker + 1) % stats.entitiesToSpawn.Count;
        }

        timestamp = Time.time + stats.timeBetweenSpawns;
        return false;
    }

    public override bool StartAbilityCheck(Actor a,GameData data) {
        for (int i = spawned.Count - 1; i >= 0; i--) {
            if (spawned[i] == null)
                spawned.RemoveAt(i);
        }

        return Time.time > timestamp &&
            ((stats.spawnAllTogether && spawned.Count == 0) ||
            (spawned.Count < stats.totalSpawnedLimit));
    }

    void SpawnEgg(GameObject spawn, Actor a,GameData data) {
        Vector2 eggLocation = FindEggLocation(a, data);
        Actor egg = GameManager.main.SpawnActor(stats.egg.transform.gameObject,eggLocation);
        spawned.Add(egg.transform);
        egg.transform.localScale = Vector3.one * spawn.GetComponent<ActorInfo>().stats.radius * 2;
        egg.abilities.Add(new EggAbility(stats.eggGrowTime,spawn,spawned));
    }

    Vector2 FindEggLocation(Actor a,GameData data) {
        int counter = 0;
        bool foundSpawnLocation = false;;
        while (!foundSpawnLocation) {
            Vector2 rand = new Vector2();
            rand.x = Random.Range(-stats.spawnRange,stats.spawnRange) + a.position2D.x;
            rand.y = Random.Range(-stats.spawnRange,stats.spawnRange) + a.position2D.y;
            if (Vector2.Distance(a.position2D,rand) <= stats.spawnRange && data.map.IsPointWalkable(rand)) {
                List<Vector2> path = Pathfinding.getPath(a.position2D,rand,0f,Movement.WALKING,data.map);
                if (path == null)
                    continue;
                float total = 0f;
                for(int i = 0; i < path.Count - 1; i++) {
                    total += Vector2.Distance(path[i],path[i + 1]);                       
                }
                if (total <= stats.spawnRange) {
                    return rand;
                }
            }

            if (Utils.WhileCounterIncrementAndCheck(ref counter))
                break;
        }
        return a.position2D;
        ;
    }
}
