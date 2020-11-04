using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilitySystem
{
    // For now we pass playerPos under assumption this is enemy specific.
    public static void AbilityUpdate(List<EnemyActor> allActors, Vector2 playerPos)
    {
        for(int i=0; i < allActors.Count-1; i++)
        {
            if (!allActors[i].isCasting) // If player is not casting
            {
                if (allActors[i].ability.StartAbilityCheck(playerPos, allActors[i])) // if ability's pre-cast requirements are met
                {
                    allActors[i].isCasting = true;
                }
            }
            else // Player is currently casting.
            {
                // Runs update until RunAbilitUpdate() returns false, then the AbilitySystemUpdate 
                // will loop to check if ability can start again until another cast starts.
                allActors[i].isCasting = allActors[i].ability.RunAbilityUpdate(playerPos, allActors[i]);
            }  
        }
    }   
}
