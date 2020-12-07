using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLobBulletStats",menuName = "Stats/LobBulletStats",order = 2)]
public class LobBulletStats : BulletStats
{
    public float lobHeight;
    public float travelTimeMin;
    public float travelTimeMax;
    public float travelTimeMaxDist;
    public GameObject burnArea;
    public GameObject warningPrefab;


    public override BulletBehavior GetBehavior() {
        return new LobBulletBehavior(this);
    }
}
