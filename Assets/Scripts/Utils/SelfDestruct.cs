using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// destroys self after a delay
public class SelfDestruct : MonoBehaviour
{
    public float selfDestructDelay;
    float timestamp;

    // Start is called before the first frame update
    void Start()
    {
        timestamp = selfDestructDelay + Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > timestamp)
            Destroy(gameObject);
    }
}
