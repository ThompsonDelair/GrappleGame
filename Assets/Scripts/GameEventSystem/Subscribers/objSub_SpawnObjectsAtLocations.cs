using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSub_SpawnObjectsAtLocations : ObjectiveSubscriber
{  
    // This will spawn an object on each of it's transforms if no object exists.
    [SerializeField] GameObject objectToSpawn;
    List<GameObject> currentObjects;
    List<Transform> spawnLocations;
    private bool validParameters = true;

    // On Awake
    void Awake() {
        // create a null entry in the currentObjects List for each objectLocation
        currentObjects = new List<GameObject>();
        spawnLocations = new List<Transform>();

        foreach(Transform location in this.transform) {
            spawnLocations.Add(location);
            currentObjects.Add(null);
        }

        if (spawnLocations.Count <= 0) {
            Debug.LogWarning("No object locations assigned to " + this.name);
            validParameters = false;
        }

        if (objectToSpawn == null) {
            Debug.LogWarning("No object to spawn given to " + this.name);
            validParameters = false;
        }
    }
    protected override void OnObjectiveCompletion() {
        if (validParameters) {
            for (int i = 0; i < currentObjects.Count; ++i) {
                if (currentObjects[i] == null) {
                    GameObject newObject = GameManager.main.SpawnActor ( objectToSpawn, Utils.Vector3ToVector2XZ(spawnLocations[i].position) ).transform.gameObject;
                    currentObjects[i] = newObject;
                }
            }
        }
    }
}
