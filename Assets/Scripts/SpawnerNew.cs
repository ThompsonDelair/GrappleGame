using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpawnerNew : MonoBehaviour
{
    [SerializeField] private int totalSpawnedLimit = 6;
    [SerializeField] private int spawnedPerWave = 2;
    [SerializeField] private float timeBetweenSpawns = 4f;
    [SerializeField] private float waitForFirstSpawn = 4f;
    [SerializeField] private float gracePeriod = 5f;

    //[SerializeField] private int spawnLimit = 1;
    List<Transform> spawned = new List<Transform>();
    float timestamp;
          
    // Start is called before the first frame update
    void Start()
    {
        timestamp = waitForFirstSpawn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
