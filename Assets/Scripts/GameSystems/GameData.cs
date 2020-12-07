using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// holds the primary game data
public class GameData 
{
    public static GameData main;

    public Actor player;
    public List<Actor> allActors;
    public List<Actor> enemyActors;
    public List<Bullet> bullets;
    public List<Area> areas;
    public Map map;
    public NavGrid navGrid;

    public GameData() {
        allActors = new List<Actor>();
        enemyActors = new List<Actor>();
        bullets = new List<Bullet>();
        areas = new List<Area>();
        map = new Map();
        main = this;
    }
}
