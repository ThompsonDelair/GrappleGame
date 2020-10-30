using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProtoHazard : MonoBehaviour
{
    [SerializeField] private float cycleTimer;
    [SerializeField] private int cycles;
    [SerializeField] private int currCycle;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private bool startOff = false;
    private float timestamp;
    private float startStamp;
    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {

        //timestamp = Time.time;
        startStamp = Time.time;
        
        if (startOff) // to set it off on start
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponentInChildren<SpriteRenderer>().color = new Color32(220, 0, 0, 40);
        }
        else
        {
            OnCycleChange();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > startStamp + startDelay || startDelay == 0f)// Delay has passed, set to active
        {
            if(active == false)
                timestamp = Time.time;
            active = true;
        }

        if (active)
        {
            /*if (timestamp < Time.time)
            {
                timestamp += cycleTimer;
                currCycle = (currCycle + 1) % cycles;
                OnCycleChange();
            }*/
            if (Time.time > (timestamp + cycleTimer))
            {
                timestamp = Time.time;
                currCycle = (currCycle + 1) % cycles;
                OnCycleChange();
            }
        }
    }

    void OnCycleChange() {
        if(currCycle == 0) {
            GetComponent<BoxCollider>().enabled = true;
            GetComponentInChildren<SpriteRenderer>().color = new Color32(220,0,0,220);
        } else {
            GetComponent<BoxCollider>().enabled = false;
            GetComponentInChildren<SpriteRenderer>().color = new Color32(220,0,0,40);
        }
    }
}
