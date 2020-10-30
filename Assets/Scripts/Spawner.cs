using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int totalSpawnedLimit = 4;
    [SerializeField] private int spawnedPerWave = 4;
    //[SerializeField] private int spawnLimit = 1;
    List<Transform> spawned = new List<Transform>();
    float timestamp;
    [SerializeField] private float timeBetweenSpawns = 0.5f;
    [SerializeField] private float waitForFirstSpawn;

    // Start is called before the first frame update
    void Start()
    {
        timestamp = waitForFirstSpawn;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = spawned.Count - 1; i >= 0; i--) {
            if (spawned[i] == null)
                spawned.RemoveAt(i);
        }
        if(spawned.Count < totalSpawnedLimit && Time.time > timestamp) {
            for (int i = 0; i < spawnedPerWave; i++) { 
                if(spawned.Count >= totalSpawnedLimit) {
                    break;
                }

                Vector2 miniRand = new Vector2(Random.Range(-0.1f,0.1f),Random.Range(-0.1f,0.1f));
                Transform t = GameManager.main.SpawnEnemy(Utils.Vector3ToVector2XZ(transform.position) + miniRand).transform;
                spawned.Add(t);
            }


            timestamp = Time.time + timeBetweenSpawns;
        }
    }
}
