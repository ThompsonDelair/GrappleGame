using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AreaCollider))]
public abstract class Area : MonoBehaviour
{
    public bool active = true;
    AreaCollider areaCollider;
    //public ColliderType type;

    private void Awake() {
        areaCollider = GetComponent<AreaCollider>();
        if(areaCollider == null) {
            Debug.LogError("Couldn't find collider for area");
        }
    }

    public bool DetectCircleCollision(NewCircleCollider c) {
        return areaCollider.DetectCircleCollision(c);
    }

    public abstract void OnActorCollision(Actor a);
}
