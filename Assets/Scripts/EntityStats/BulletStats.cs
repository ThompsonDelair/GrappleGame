using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletStats",menuName = "ScriptableObjects/BulletStats",order = 2)]
public class BulletStats : ScriptableObject
{
    public float speed;
    public float damage;
    public float radius;
}
