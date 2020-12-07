using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BehaviorAbilitySystem
{
    public static void Update(GameData d) {
        BehaviorUpdate(d.allActors,d);
        AbilityUpdate(d.allActors,d);
        BulletBehaviorUpdate(d.bullets,d);

    }
    
    public static void BehaviorUpdate(List<Actor> actors, GameData data) {
        for (int i = 0; i < actors.Count; i++) {
            Actor a = actors[i];
            for(int j = 0; j < a.behaviors.Count; j++) {
                a.behaviors[j].Update(a,data);
            }
        }
    }

    // if the actor has a currently executing ability, update it
    // else if the actor has an ability ready to cast, cast it
    public static void AbilityUpdate(List<Actor> actors, GameData data) {
        for(int i = 0; i < actors.Count; i++) {
            Actor a = actors[i];
            if(a.currAbility != null) {
                bool stillRunning = a.currAbility.RunAbilityUpdate(a,data);
                if (!stillRunning) {
                    a.currAbility = null;
                }
            } else {
                for(int j = 0; j < a.abilities.Count; j++) {
                    Ability ab = a.abilities[j];
                    if (ab.StartAbilityCheck(a,data)) {
                        a.currAbility = ab;
                        break;
                    }
                }
            }
        }
    }

    public static void BulletBehaviorUpdate(List<Bullet> bullets, GameData data) {
        for(int i = 0; i < bullets.Count; i++) {
            Bullet b = bullets[i];
            b.behavior.BulletBehaviorUpdate(b,data);
        }
    }
}
