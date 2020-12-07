using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple singleton to refer to when calling SoundManager methods that use an AudioClip
/// </summary>
public class AudioClips : MonoBehaviour
{
    public static AudioClips singleton;
    public AudioClip grapShoot;
    public AudioClip grapImpact;
    public AudioClip grapLoop;
    public AudioClip grapEnd;
    public AudioClip enemyDie;
    public AudioClip gunShot;
    public AudioClip playerDie;
    public AudioClip bulletImpact;
    public AudioClip chargeShotCharge;
    public AudioClip chargeShotShot;
    public AudioClip HPCharge;
    public AudioClip HPIncrement;
    public AudioClip littleWalk;
    public AudioClip wingFlutter;
    public AudioClip ambientCave1;
    public AudioClip dash;
    public AudioClip burn;
    public AudioClip playerHurt;
    public AudioClip fireCast;
    public AudioClip fireLand;
    public AudioClip enemyShot;
    public AudioClip playerDmg;
    public AudioClip doorToggle;
    public AudioClip turretShot;

    



    // Start is called before the first frame update
    void Start()
    {
        singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
