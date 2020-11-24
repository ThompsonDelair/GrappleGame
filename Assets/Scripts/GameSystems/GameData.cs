using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData 
{
    public Actor player;
    public List<Actor> allActors;
    public List<Actor> enemyActors;
    public List<Bullet> bullets;
    public List<Area> areas;
    public Map map;

    public GameData() {
        allActors = new List<Actor>();
        enemyActors = new List<Actor>();
        bullets = new List<Bullet>();
        areas = new List<Area>();
        map = new Map();
    }
}
