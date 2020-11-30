using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaPulser : MonoBehaviour
{
    MeshRenderer meshRenderer;
    Area area;

    public int currCycle;
    public int cycles;
    public int activeCycles;
    public float cycleDuration;
    public float warmUpTime;
    float timestamp;
    // Start is called before the first frame update
    void Start()
    {
        area = GetComponent<Area>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if (Time.time > timestamp) {
            NextCycle();
        } else if (currCycle == cycles - 1 && activeCycles != cycles) {
            WarmUpUpdate();
        }
    }

    void NextCycle() {
        currCycle = (currCycle + 1) % cycles;
        timestamp = Time.time + cycleDuration;
        if(currCycle < activeCycles) {
            AreaOn();
        } else {
            AreaOff();
        }
    }

    void AreaOn() {
        area.active = true;
        
        SetMeshAlpha(255);
    }

    void AreaOff() {
        area.active = false;
        
        SetMeshAlpha(30);
    }

    void WarmUpUpdate() {
        float phaseTime = warmUpTime - (timestamp - Time.time);
        float phase = phaseTime * 3 / warmUpTime;
        byte alpha = 32;
        if(phase > 2f) {
            alpha = 150;
        } else if(phase > 1f) {
            alpha = 100;
        } else if(phase > 0f) {
            alpha = 60;
        }
        SetMeshAlpha(alpha);
    }

    void SetMeshAlpha(byte alpha) {
        Color32 color = meshRenderer.material.color;
        color.a = alpha;
        meshRenderer.material.color = color;
    }
}
