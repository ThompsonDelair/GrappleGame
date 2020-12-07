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

// this class is used for enemies and the player
// it acts as a wrapper between our code and unity game objects
// it can be used by both 2D and 3D systems
public class Actor
{
    public ActorStats stats;
    public OurCircleCollider collider;
    public AABB_2D aabb;
    public bool collision;

    public Transform transform;
    public Vector2 velocity;

    public Zone currZone;

    public Vector3 position3D { get { return transform.position; } set { transform.position = value; } }
    public Vector2 position2D {
        get {
            return new Vector2(transform.position.x,transform.position.z);
        }
        set {
            transform.position = new Vector3(value.x,0,value.y);
        }
    }
    public Vector2 posAtFrameStart;

    public List<PushInstance> pushForces = new List<PushInstance>();

    public Movement currMovement;

    public bool followingPath = false;
    public Vector2 pathfindWaypoint;
    public float pathfindTimer;
    public Vector2 pathfindWander;
    public float wanderTimer;

    public float health;
    public Team team;
    public bool invulnerable;

    public Animator animator;

    public List<Behavior> behaviors;
    public List<Ability> abilities;
    public Ability currAbility;

    public List<AbilityStats> abilityStats;
    public List<BehaviorStats> behaviorStats;

    private Actor() { }

    public Actor(Transform transform,ActorStats stats,Team team) {
        this.stats = stats;
        collider = new OurCircleCollider(stats.radius,transform);
        this.transform = transform;
        this.team = team;
        health = stats.maxHP;
        aabb = new AABB_2D(this);

        animator = transform.GetComponent<Animator>();
        currMovement = stats.movement;
        invulnerable = stats.invulnerable;

        behaviors = this.stats.GetBehaviorInstances();
        abilities = this.stats.GetAbilityInstances();        
    }

    public PushInstance StartNewPush(Vector2 direction,float force, float friction = 100f) {
        pushForces.Clear();
        PushInstance p = new PushInstance(direction,force,friction);
        pushForces.Add(p);
        return p;
    }

    public void AddPushForce(Vector2 direction, float force, float friction = 100f) {
        pushForces.Add(new PushInstance(direction,force,friction));

    }
}
