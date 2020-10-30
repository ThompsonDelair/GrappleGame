using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data",menuName = "ScriptableObjects/ActorStats",order = 1)]
public class ActorStats : ScriptableObject
{
    public float maxHP;
    public float radius;
}
