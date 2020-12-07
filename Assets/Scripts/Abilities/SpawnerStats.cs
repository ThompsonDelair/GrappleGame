using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnerStats",menuName = "Abilities/Spawn",order = 4)]
public class SpawnerStats : AbilityStats
{
    public int totalSpawnedLimit = 6;
    public int spawnedPerWave = 2;
    public float timeBetweenSpawns = 4f;
    public float waitForFirstSpawn = 4f;
    public float eggGrowTime;       // how long it takes for eggs to hatch
    public bool spawnAllTogether;   // if the spawner should wait to spawn all entities together in one wave
    public float spawnRange;
    public float eggRadius;     // how fat should egg be?
    public GameObject egg;      // prefab to use as egg
    public int instaSpawnAmount;    // number of entities to spawn instantly without using egg
    public List<GameObject> entitySpawnList;    // what entities the spawner should spawn

    // gets an instance of the spawner ability
    public override Ability GetAbilityInstance() {
        return new SpawnerAbility(this);
    }
}
