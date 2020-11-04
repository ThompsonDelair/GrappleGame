using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor
{
    public NewCircleCollider collider;
    public Transform transform;
    public Vector2 velocity;
    public float momentumForce;
    public Vector2 momentumDir;
    public Vector3 position3D { get { return transform.position; } set { transform.position = value; } }
    public Movement movement = Movement.WALKING;

    public bool followingPath = false;
    public Vector2 pathfindWaypoint;
    public float pathfindTimer;

    public float health;
    public float maxHealth;

    public Layer layer;

    public Vector2 posAtFrameStart;
    public bool collision;

    // Added for ability system.
    public bool isCasting;

    public Ability ability;


    public Vector2 position2D {
        get {
            return new Vector2(transform.position.x,transform.position.z);
        }
        set {   
            transform.position = new Vector3(value.x,0,value.y);
        }
    }

    public Actor(Transform t,float r,Layer l,float hp) {
        collider = new NewCircleCollider(r,t);
        transform = t;
        layer = l;
        health = hp;
        maxHealth = hp; // Initial HP is max that player can have.
    }

    public void StartNewPush(Vector2 direction,float force) {
        momentumDir = direction;
        momentumForce = force;
    }

    public void AddPushForce(Vector2 direction, float force) {
        Vector2 combined = momentumDir.normalized * momentumForce + direction.normalized * force;
        momentumDir = combined;
        momentumForce = combined.magnitude;
    }

    public void PlayAudioClip(AudioClip clip,bool loop = false) {
        AudioSource source = transform.GetComponent<AudioSource>();
        if (source == null) {
            source = transform.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        }
        source.clip = clip;
        source.loop = loop;
        source.Play();
    }

    public void StopAudioClip() {
        AudioSource source = transform.GetComponent<AudioSource>();
        if (source == null) {
            source = transform.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        }
        source.Stop();
    }
}
