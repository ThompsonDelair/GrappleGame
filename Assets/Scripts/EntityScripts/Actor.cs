using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    NONE = 0,
    PLAYER = 0b1,
    ENEMIES = 0b1 << 1,
    ALL = PLAYER | ENEMIES,
}

public class Actor
{
    public ActorStats stats;
    public NewCircleCollider collider;
    public AABB_2D aabb;
    public Transform transform;
    public Vector2 velocity;
    public List<PushInstance> pushForces = new List<PushInstance>();
    public Vector3 position3D { get { return transform.position; } set { transform.position = value; } }
    public Movement currMovement;

    public bool followingPath = false;
    public Vector2 pathfindWaypoint;
    public float pathfindTimer;
    public Vector2 pathfindWander;
    public float wanderTimer;

    public Zone currZone;

    public float health;

    public Team team;

    public Vector2 posAtFrameStart;
    public bool collision;

    // Added for ability system.
    //public bool isCasting;
    public bool invulnerable;

    //public Ability ability;

    public Animator animator;

    public List<Behavior> behaviors;
    public List<Ability> abilities;
    public Ability currAbility;

    public List<AbilityStats> abilityStats;
    public List<BehaviorStats> behaviorStats;

    public Vector2 position2D {
        get {
            return new Vector2(transform.position.x,transform.position.z);
        }
        set {   
            transform.position = new Vector3(value.x,0,value.y);
        }
    }

    private Actor() { }

    public Actor(Transform t,ActorStats s,Team l) {
        stats = s;
        collider = new NewCircleCollider(s.radius,t);
        transform = t;
        team = l;
        health = s.maxHP;
        aabb = new AABB_2D(this);

        animator = t.GetComponent<Animator>();
        currMovement = s.movement;
        invulnerable = s.invulnerable;

        behaviors = stats.GetBehaviorInstances();
        abilities = stats.GetAbilityInstances();        
    }

    public PushInstance StartNewPush(Vector2 direction,float force, float friction = 100f) {
        //momentumDir = direction;
        //momentumForce = force;
        pushForces.Clear();
        PushInstance p = new PushInstance(direction,force,friction);
        pushForces.Add(p);
        return p;
    }

    public void AddPushForce(Vector2 direction, float force, float friction = 100f) {
        //Vector2 combined = momentumDir.normalized * momentumForce + direction.normalized * force;
        //momentumDir = combined;
        //momentumForce = combined.magnitude;
        pushForces.Add(new PushInstance(direction,force,friction));

    }

    // Using SoundManager now
    /*public void PlayAudioClip(AudioClip clip,bool loop = false) {
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
    }*/
}
