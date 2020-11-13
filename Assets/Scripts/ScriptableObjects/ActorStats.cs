using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActorStats",menuName = "ScriptableObjects/ActorStats",order = 1)]
public class ActorStats : ScriptableObject
{
    public float maxHP;
    public float radius;
    public float targetLockTime; // Used in lunge
    public string mainAttack; // Take the string and use to make class later.
}
