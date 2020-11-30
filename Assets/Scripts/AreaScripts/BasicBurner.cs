using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBurner : Area 
{
    Dictionary<Actor,float> actorMemory = new Dictionary<Actor,float>();
    [SerializeField] GameObject burnerParticleEffect;
    [SerializeField] float reburnTime = 1f;
    [SerializeField] float damage = 0.5f;

    float timeStamp = 0f;
    [SerializeField] float burnParticleFrequency = 0.1f;

    public override void OnActorCollision(Actor a) {
        if (actorMemory.ContainsKey(a)) {
            if (actorMemory[a] < Time.time) {
                DamageSystem.DealDamage(a,damage);
                if(a.transform.gameObject.tag == "Player")
                {
                    SoundManager.PlayOneClipAtLocation(AudioClips.singleton.playerHurt, a.position2D, 6f);
                }
                actorMemory[a] = Time.time + reburnTime;
            }
        } else {
            DamageSystem.DealDamage(a,damage);
            if (a.transform.gameObject.tag == "Player")
            {
                SoundManager.PlayOneClipAtLocation(AudioClips.singleton.playerHurt, a.position2D, 6f);
            }
            actorMemory.Add(a,Time.time + reburnTime);
        }
    }

    private void Update() {
        if (active) {
            SpawnParticleCheck();
        }
    }

    private void SpawnParticleCheck() {
        if (Time.time > timeStamp) {
            Vector2 newParticlePos = GetRandomPoint();


            
            
            ParticleEmitter.current.SpawnParticleEffect(burnerParticleEffect, Utils.Vector2XZToVector3(newParticlePos) + Vector3.up * 1, Quaternion.identity);
            timeStamp = Time.time + burnParticleFrequency;
        }
    }

    // Chooses a random point within a Polygonal Collider.
    public Vector2 GetRandomPoint() {

        AreaPolygonCollider pc = GetComponent<AreaPolygonCollider>();
        AreaCircleCollider cc = GetComponent<AreaCircleCollider>();
        AABB_2D aabb;
        if (pc != null) {
            // no area polygon collider
            // maybe there's a circle area collider?
            aabb = new AABB_2D(pc.PolygonCollider.Verts);
        } else {
            aabb = new AABB_2D(cc.CircleCollider);
        }
        Vector2 rand = Vector2.negativeInfinity;
        int counter = 0;
        bool foundValidRand = false;
        while (!foundValidRand) {
            rand.x = Random.Range(aabb.X,aabb.X + aabb.Width);
            rand.y = Random.Range(aabb.Y,aabb.Y + aabb.Height);
            
            if (pc != null &&CollisionCalc.PointInsidePolygon(rand,pc.PolygonCollider.Verts) != 2) {
                foundValidRand = true;
            }
            if(cc != null && Vector2.SqrMagnitude(rand-cc.CircleCollider.Position2D)<= cc.CircleCollider.Radius * cc.CircleCollider.Radius) {
                foundValidRand = true;
            }


            if (Utils.WhileCounterIncrementAndCheck(ref counter)) {
                break;
            }
        }
               
        return rand;
    }
}
