using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBurner : Area
{
    Dictionary<Actor,float> actorMemory = new Dictionary<Actor,float>();
    [SerializeField] float reburnTime = 1f;
    [SerializeField] float damage = 0.5f;

    public override void OnActorCollision(Actor a) {
        if (actorMemory.ContainsKey(a)) {
            if (actorMemory[a] < Time.time) {
                DamageSystem.DealDamage(a,damage);
                actorMemory[a] = Time.time + reburnTime;
            }
        } else {
            DamageSystem.DealDamage(a,damage);
            actorMemory.Add(a,Time.time + reburnTime);
        }
    }
}
