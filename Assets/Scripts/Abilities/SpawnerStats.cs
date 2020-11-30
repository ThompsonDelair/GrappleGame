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
    public float eggGrowTime;
    public bool spawnAllTogether;
    public float spawnRange;
    public float eggRadius;
    public GameObject egg;
    public int instaSpawnAmount;
    //public List<GameObject> spawnGuards;
    public List<GameObject> eggSpawn;

    public override Ability GetAbilityInstance() {
        return new SpawnerAbility(this);
    }
}
