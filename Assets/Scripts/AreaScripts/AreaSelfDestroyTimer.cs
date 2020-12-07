using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Area))]
public class AreaSelfDestroyTimer : MonoBehaviour
{
    float timestamp;
    public float selfDestructDelay;

    // Start is called before the first frame update
    void Start()
    {
        timestamp = Time.time + selfDestructDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > timestamp) {
            GameManager.main.DestroyArea(GetComponent<Area>());
        }
    }
}
