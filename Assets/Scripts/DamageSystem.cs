using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public static class DamageSystem
{
    private static float playerDamageCooldown = 1f;
    private static float timeStamp;

    // use later for pickups and healing
    /*    public static void healHealth(Actor a, float amt)
        {
            // If max health is exceeded set to max, otherwise add onto health
            if (a.health + amt >= a.maxHealth ? a.health = a.maxHealth : a.health += amt;)
            {
                a.health = 0;
            }
        }*/

    // Deducts amount of damage float from actor's health property
    // If health is below or equal to 0 set to 0, otherwise deduct amt from health
    public static void DealDamage(Actor a, float amt)
    {
        if(a.health > 0)
        {
            if (a.layer == Layer.PLAYER) // PLAYER CASE, DEDUCT BASED ON TIME INTERVALs
            {
                if (DebugControl.main.playerInvulnerable)
                    return;


                if (Time.time > timeStamp)
                {
                    Debug.Log("DAMAGIING");
                    a.health = a.health + amt <= 0 ? 0 : a.health -= amt;
                    timeStamp = Time.time + playerDamageCooldown;
                }

            }
            else // ENEMY CASE
            {
                a.health = a.health + amt <= 0 ? 0 : a.health -= amt;
            }
        }
        
    }
    
    // Iterates over all actors in game, kills dead actors (health <= 0)
    // also updates the healthbars to reflect current health of each actor
    public static void HealthUpdate(List<Actor> actors)
    {
        for(int i = 0; i < actors.Count; i++)
        {
            UpdateHealthSlider(actors[i]);
            if (actors[i].health <= 0f) // Dead actor, destroy it
            {
                OnDeath(actors[i]);
            }
        }
    }

    // Plays death sound at actors location then calls method to destroy actor
    private static void OnDeath(Actor a)
    {
        if(a.transform.gameObject.tag == "Player")
        {
            // If having audio isssues here with infinite play, it is likely caused by the chain reaction 
            // of destroying actor being interupted, it should be playSound->DestroyActor->DestroyObject could be stuck between playSound->DestroyActor
            AudioSource.PlayClipAtPoint(AudioClips.singleton.playerDie, a.position3D, 6f); 
            GameManager.main.DestroyActor(a);
            //Debug.Break(); // HACKY WAY TON STOP GAMEPLAY ONCE PLAYER DIES FOR NOW, NEED PROPER LOSS CONDITION
        }
        else
        {
            AudioSource.PlayClipAtPoint(AudioClips.singleton.enemyDie, a.position3D, 6f);
            GameManager.main.DestroyActor(a);
        }
    }


    // Adjust the slider UI element on the Actor's GameObject's Slider component by assiging it's value the percentage of health out of maxHealth
    private static void UpdateHealthSlider(Actor a)
    {
        try {
            a.transform.gameObject.GetComponentInChildren<Slider>().value = a.health / a.maxHealth;
        } catch(Exception e) {
            //Debug.LogError(e.ToString());
        }
    }
}
