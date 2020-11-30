using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BehaviorSystem
{
    public static void Update(GameData d) {
        BehaviorUpdate(d.allActors,d);
        AbilityUpdate(d.allActors,d);
        BulletBehaviorUpdate(d.bullets,d);

    }


    // For now we pass playerPos under assumption this is enemy specific.
    //public static void EnemyAbilityUpdate(List<EnemyActor> enemyDudes, Vector2 playerPos, List<EnemyActor> enemyObjects)
    //{

    //    // For enemy actors
    //    for(int i=0; i < enemyDudes.Count; i++)
    //    {
    //        if (!enemyDudes[i].isCasting) // If player is not casting
    //        {
    //            if (enemyDudes[i].ability.StartAbilityCheck(playerPos, enemyDudes[i])) // if ability's pre-cast requirements are met
    //            {
    //                enemyDudes[i].isCasting = true;
    //            }
    //        }
    //        else // Player is currently casting.
    //        {
    //            // Runs update until RunAbilitUpdate() returns false, then the AbilitySystemUpdate 
    //            // will loop to check if ability can start again until another cast starts.
    //            enemyDudes[i].isCasting = enemyDudes[i].ability.RunAbilityUpdate(playerPos, enemyDudes[i]);
    //        }  
    //    }

    //    // For objects
    //    for (int i = 0; i < enemyObjects.Count; i++)
    //    {
    //        /*Debug.Log("ENTERING LOOP");
    //        if(!(enemyObjects[i].health <= 0))
    //        {
    //            enemyObjects[i].ability.RunAbilityUpdate(playerPos, enemyObjects[i]); // Not utilizing isCasting right now as no need, should be easy to add later if need be
    //        }*/

    //        if (!enemyObjects[i].isCasting) // If player is not casting
    //        {
    //            if (enemyObjects[i].ability.StartAbilityCheck(playerPos, enemyObjects[i])) // if ability's pre-cast requirements are met
    //            {
    //                enemyObjects[i].isCasting = true;
    //            }
    //        }
    //        else // Player is currently casting.
    //        {
    //            // Runs update until RunAbilitUpdate() returns false, then the AbilitySystemUpdate 
    //            // will loop to check if ability can start again until another cast starts.
    //            enemyObjects[i].isCasting = enemyObjects[i].ability.RunAbilityUpdate(playerPos, enemyObjects[i]);
    //        }

    //    }
    //} 
    
    public static void BehaviorUpdate(List<Actor> actors, GameData data) {
        for (int i = 0; i < actors.Count; i++) {
            Actor a = actors[i];
            for(int j = 0; j < a.behaviors.Count; j++) {
                a.behaviors[j].Update(a,data);
            }
        }
    }

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
