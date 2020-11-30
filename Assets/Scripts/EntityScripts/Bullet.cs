using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class Bullet
{
    public NewCircleCollider collider;
    public Transform transform;

    public BulletStats stats;
    public BulletBehavior behavior;

    //public Actor target;

    public Team team;
    public Movement movement = Movement.FLYING;

    public Vector3 position3D { get { return transform.position; } set { transform.position = value; } }

    public Vector2 prevPos;

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
          
    public virtual void OnExpire(GameData data) {
        SoundManager.PlayOneClipAtLocation(AudioClips.singleton.bulletImpact,position2D,6f);
        //DestroyGameobject(transform.gameObject);
        ParticleEmitter.current.SpawnParticleEffect(GameManager.main.playerBulletHit,transform.position,Quaternion.identity);
        GameManager.main.DestroyBullet(this);
    }

    public Bullet(Transform t,BulletStats stats)
    {
        collider = new NewCircleCollider(stats.radius,t);
        transform = t;
        team = stats.targetTeam;
        this.stats = stats;
        behavior = stats.GetBehavior();
        //this.target = target;
    }

}
