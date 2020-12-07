using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class DamageSystem
{
    private static float playerDamageCooldown = 0.7f;
    private static float timeStamp;


    // Deducts amount of damage float from actor's health property
    // If health is below or equal to 0 set to 0, otherwise deduct amt from health
    public static void DealDamage(Actor a, float amt)
    {
        if(a.health > 0 && !a.invulnerable)
        {
            if (a.team == Team.PLAYER) // PLAYER CASE, DEDUCT BASED ON TIME INTERVALs
            {
                if (DebugControl.main.playerInvulnerable)
                    return;


                if (Time.time > timeStamp)
                {
                    Debug.Log("DAMAGIING");
                    a.health = a.health + amt <= 0 ? 0 : a.health -= amt;
                    timeStamp = Time.time + playerDamageCooldown;
                    SoundManager.PlayOneClipAtLocation(AudioClips.singleton.playerHurt, a.position2D, 6f);
                }

            }
            else // ENEMY CASE
            {
                a.health = a.health + amt <= 0 ? 0 : a.health -= amt;
            }
        }
        
    }

    // Adds amount of damage float to actor's health property
    // If health is greater than or equal to max, otherwise deduct amt from health
    public static void RestoreHealth(Actor a, float amt)
    {
        ActorStats stats = a.transform.GetComponent<ActorInfo>().stats;
        if(a.health < stats.maxHP)
        {
            if (a.team == Team.PLAYER) // PLAYER CASE, DEDUCT BASED ON TIME INTERVALs
            {
                a.health = a.health + amt > stats.maxHP ? stats.maxHP : a.health += amt;

            }
            else // ENEMY CASE
            {
                a.health = a.health + amt > stats.maxHP ? stats.maxHP : a.health += amt;
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
            SoundManager.PlayOneClipAtLocation(AudioClips.singleton.playerDie, a.position3D, 6f);
            GameManager.main.DestroyActor(a);
            ScoreDirector.main.AddDeath();
        }
        else
        {
            SoundManager.PlayOneClipAtLocation(AudioClips.singleton.enemyDie, a.position3D, 6f);
            ParticleEmitter.current.SpawnParticleEffect(ParticleEffectRefs.singleton.enemyDeathEffect, a.position3D + Vector3.up, Quaternion.identity);
            GameManager.main.DestroyActor(a);
            ScoreDirector.main.AddKill();
        }
    }


    // Adjust the slider UI element on the Actor's GameObject's Slider component by assiging it's value the percentage of health out of maxHealth
    private static void UpdateHealthSlider(Actor a)
    {
        if(a == GameManager.main.player) // In case of player we dont want to access canvas through gameObject
        {
            GameObject playerSliderObj = GameObject.FindGameObjectWithTag("PlayerSlider");

            try
            {
                playerSliderObj.GetComponent<Slider>().value = a.health / a.stats.maxHP;
            }
            catch 
            {
                //Debug.LogError(e.ToString());
            }
        }
        else // Otherwise update through the game object
        {
            try
            {
                a.transform.gameObject.GetComponentInChildren<Slider>().value = a.health / a.stats.maxHP;
            }
            catch
            {
                //Debug.LogError(e.ToString());
            }
        }
    }
}
