using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    // layer?
    GenericCollider collider;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool DetectBulletCollision(Bullet b) {
        return collider.DetectCircleCollision(b.collider);
    }
}
