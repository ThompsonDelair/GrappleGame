using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTrigger : Area {
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnActorCollision(Actor a) {
        if (a == GameManager.main.player) {
            Debug.Log("Player has entered region.");
            active = false;
        }
    }
}
