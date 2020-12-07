using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// spawns waves of enemies around actor
public class SpawnerAbility : Ability
{
    SpawnerStats stats;
    int spawnTracker;   // used to step through stats list of spawnable entities in sequence

    List<Transform> spawned = new List<Transform>();
    float timestamp;

    // spawners do not activate until the player gets close enough
    bool activated = false;     
    float activateCheckDelay = 1f;
    const float activateRange = 50f;

    public SpawnerAbility(SpawnerStats s) {
        stats = s;
    }

    public override bool RunAbilityUpdate(Actor a,GameData data) {

        List<GameObject> eggSpawn;  // the actor we're going to spawn

        SpawnList spawnlist = a.transform.GetComponent<SpawnList>();
        if (spawnlist != null) {
            eggSpawn = spawnlist.spawnableEntities;
        } else {
            eggSpawn = stats.entitySpawnList;
        }

        int numToSpawn = 0;
        if (stats.spawnAllTogether) {
            numToSpawn = stats.totalSpawnedLimit;
        } else {
            numToSpawn = stats.totalSpawnedLimit - spawned.Count;
            numToSpawn = (numToSpawn > stats.spawnedPerWave) ? stats.spawnedPerWave : numToSpawn;
        }

        int spawnedThisWave = 0;

        // stats let us instantly spawn a certain number of enemies
        for(int i = 0; i < numToSpawn && i < stats.instaSpawnAmount; i++, spawnedThisWave++) {
            SpawnEgg(eggSpawn[spawnTracker],a,data);
            spawnTracker = (spawnTracker + 1) % eggSpawn.Count;
        }

        // spawn the rest for this wave using eggs
        for(; spawnedThisWave < numToSpawn; spawnedThisWave++) {
            SpawnEgg(eggSpawn[spawnTracker],a,data);
            spawnTracker = (spawnTracker + 1) % eggSpawn.Count;
        }

        timestamp = Time.time + stats.timeBetweenSpawns;
        return false;
    }

    // checks if our ability is on cooldown, or if we've already spawned the max number of entities 
    public override bool StartAbilityCheck(Actor a,GameData data) {

        if (!activated && timestamp < Time.time && Vector2.Distance(a.position2D,data.player.position2D) < activateRange) {
            PathfindVars vars = new PathfindVars() { rangeLimit = activateRange };

            List<Vector2> p = Pathfinding.getPath(a.position2D,data.player.position2D,0.1f,Movement.FLYING,data.map,vars);
            if(p != null && Calc.TotalDistanceTravelled(p) < activateRange) {
                activated = true;
            } else {
                timestamp = Time.time + activateCheckDelay;
                return false;
            }
        }

        if (!activated) {
            // spawner is only activated if player gets close enough
            return false;
        }

        // removes any destroyed entities from our spawn list
        for (int i = spawned.Count - 1; i >= 0; i--) {
            if (spawned[i] == null)
                spawned.RemoveAt(i);
        }

        return Time.time > timestamp &&
            ((stats.spawnAllTogether && spawned.Count == 0) ||
            (spawned.Count < stats.totalSpawnedLimit));
    }

    // finds a location for an egg, instantiates an egg there
    void SpawnEgg(GameObject spawn,Actor a,GameData data) {
        Vector2 eggLocation = FindEggLocation(a,data);
        Actor egg = GameManager.main.SpawnActor(stats.egg.transform.gameObject,eggLocation);
        spawned.Add(egg.transform);
        egg.transform.localScale = Vector3.one * stats.eggRadius;
        egg.abilities.Add(new EggAbility(stats.eggGrowTime,spawn,spawned));
    }

    // searches for a walkable location within a stats-defined distance
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
