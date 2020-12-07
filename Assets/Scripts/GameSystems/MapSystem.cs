using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// periodically maps player's line of sight to the navgrid
// periodically sets actor's current zone
public static class MapSystem
{
    const int playerSightRange = 30;
    static float timestamp;
    const float sightUpdateDelay = 0.3f;

    const float zoneUpdateDelay = 0.2f;
    static float zoneUpdateTimestamp;

    public static void Update(GameData gameData) {
        if(timestamp < Time.time) {
            UpdatePlayerSightLine(gameData);
            timestamp = Time.time + sightUpdateDelay;
        }
        if (zoneUpdateTimestamp < Time.time) {
            UpdateCurrZone(gameData.allActors,gameData.map);
            zoneUpdateTimestamp = Time.time + zoneUpdateDelay;
        }
    }

    static void UpdatePlayerSightLine(GameData gameData) {
        gameData.navGrid.ResetSight();
        gameData.navGrid.FindPlayerSight(gameData);
    }

    static void UpdateCurrZone(List<Actor> actors,Map map) {
        for (int i = 0; i < actors.Count; i++) {
            Actor a = actors[i];
            if (a.currMovement == Movement.NONE) {
                continue;
            }
            a.currZone = map.ZoneFromPoint(a.position2D);
        }
    }
}
