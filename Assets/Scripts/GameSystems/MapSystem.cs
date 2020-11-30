using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapSystem
{
    const int playerSightRange = 30;
    static float timestamp;
    const float sightUpdateDelay = 0.3f;

    public static void Update(GameData gameData) {
        if(timestamp < Time.time) {
            UpdatePlayerSightLine(gameData);
            timestamp = Time.time + sightUpdateDelay;
        }
    }

    static void UpdatePlayerSightLine(GameData gameData) {
        gameData.navGrid.ResetSight();
        gameData.navGrid.FindPlayerSight(gameData);
    }
}
