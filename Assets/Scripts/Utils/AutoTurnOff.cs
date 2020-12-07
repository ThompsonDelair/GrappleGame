using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTurnOff : MonoBehaviour
{
    private float timestamp;
    public float duration;
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        target.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(target.activeSelf && Time.time > timestamp) {
            target.SetActive(false);
        }
    }

    public void Activate() {
        target.SetActive(true);
        timestamp = Time.time + duration;
    }
}
