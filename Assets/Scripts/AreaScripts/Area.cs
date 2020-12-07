using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an area is essentially an area collider with an OnCollision behavior
[RequireComponent(typeof(AreaCollider))]
public abstract class Area : MonoBehaviour
{
    public bool active = true;
    AreaCollider areaCollider;

    private void Awake() {
        areaCollider = GetComponent<AreaCollider>();
        if(areaCollider == null) {
            Debug.LogError("Couldn't find collider for area");
        }
    }

    public bool DetectCircleCollision(OurCircleCollider c) {
        return areaCollider.DetectCircleCollision(c);
    }

    public abstract void OnActorCollision(Actor a);
}
