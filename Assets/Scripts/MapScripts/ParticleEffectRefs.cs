using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectRefs : MonoBehaviour
{
    public static ParticleEffectRefs singleton;
    public GameObject enemyDeathEffect;

    // Start is called before the first frame update
    void Start()
    {
        singleton = this;
    }
}
