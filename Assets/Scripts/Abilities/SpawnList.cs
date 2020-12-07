using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// if placed on a gameobject with a spawn ability, 
// this will override that ability's normal spawn entity list
public class SpawnList : MonoBehaviour
{
    public List<GameObject> spawnableEntities;
}
