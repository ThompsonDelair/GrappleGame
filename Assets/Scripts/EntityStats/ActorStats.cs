using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityStats",menuName = "Stats/ActorStats",order = 1)]
public class ActorStats : ScriptableObject
{
    public float maxHP;
    public float radius;
    public float speed;
    //public float targetLockTime; // Used in lunge
    //public string mainAttack; // Take the string and use to make class later.
    //public float timeBetweenAttack;
    //public bool moveable;
    public Movement movement;
    public bool invulnerable;
    public bool burnImmune;

    public List<AbilityStats> abilityStats;
    public List<BehaviorStats> behaviorStats;

    public List<Ability> GetAbilityInstances() {
        List<Ability> abilities = new List<Ability>();
        for(int i = 0; i < abilityStats.Count; i++) {
            abilities.Add(abilityStats[i].GetAbilityInstance());
        }
        return abilities;
    }

    public List<Behavior> GetBehaviorInstances() {
        List<Behavior> behaviors = new List<Behavior>();
        for(int i = 0; i < behaviorStats.Count; i++) {
            behaviors.Add(behaviorStats[i].GetBehaviorInstance());
        }
        return behaviors;
    }
}
