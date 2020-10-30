using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class Bullet
{
    public NewCircleCollider collider;
    public Transform transform;
    public float speed = 50f;
    public float damage;

    public Layer layer;
    public Movement movement = Movement.IGNORE_CLIFFS;

    public Vector3 position3D { get { return transform.position; } set { transform.position = value; } }
    

    public Vector2 position2D
    {
        get
        {
            return new Vector2(transform.position.x, transform.position.z);
        }
        set
        {
            transform.position = new Vector3(value.x,0,value.y);
        }
    }

    public Bullet(Transform t, float r, Layer l, float dmg)
    {
        collider = new NewCircleCollider(r,t);
        transform = t;
        layer = l;
        damage = dmg;
    }
}
