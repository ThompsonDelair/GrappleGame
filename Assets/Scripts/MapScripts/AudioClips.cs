using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
