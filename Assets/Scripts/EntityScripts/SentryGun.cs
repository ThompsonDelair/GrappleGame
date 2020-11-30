using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryGun : MonoBehaviour
{
    float shootingTimestamp;
    public float timeBetweenShots;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ShootUpdate();
    }

    void ShootUpdate() {
        if (CanSeePlayer()) {
            // rotate to look at player
            transform.LookAt(GameManager.main.player.position3D);

            if(shootingTimestamp < Time.time) {
                shootingTimestamp = Time.time + timeBetweenShots;
                ShootAtPlayer();
            }
        }
    }

    void ShootAtPlayer() {
        //GameObject gameObjBullet = (GameObject)Instantiate(Resources.Load("Prefabs/EvilBullet"));
        //Bullet bullet = new Bullet(gameObjBullet.transform,0.5f,Team.PLAYER,1f);
        //bullet.speed = 20f;
        //bullet.position3D = transform.position;


        Bullet b = GameManager.main.SpawnBullet((GameObject)Resources.Load("Prefabs/EvilBullet"),Utils.Vector3ToVector2XZ(transform.position));
        b.transform.rotation = transform.rotation;
        //GameManager.main.gameData.bullets.Add(bullet);

        //GetComponent<Actor>().PlayAudioClip(AudioClips.singleton.gunShot);
        // Set to shoot in that direction

    }

    bool CanSeePlayer() {
        Vector2 playerPos2D = GameManager.main.player.position2D;
        Vector2 sentryPos2D = Utils.Vector3ToVector2XZ(transform.position);
        Map map = GameManager.main.gameData.map;
        for(int i = 0; i < map.terrainEdges.Count; i++) {
            TerrainEdge e = map.terrainEdges[i];
            if (CollisionSystem.LayerOrCheck(e.layer,Layer.BLOCK_WALK) && Calc.DoLinesIntersect(playerPos2D,sentryPos2D,e.vertA_pos2D,e.vertB_pos2D)) {
                TurnOffLineRenderer();
                return false;
            }
        }
        //Debug.DrawLine(GameManager.main.player.position3D,transform.position,Color.red);
        UpdateLineRenderer();
        return true;
    }

    void UpdateLineRenderer() {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr.positionCount != 2)
            lr.positionCount = 2;

        Vector3[] points = { transform.position,GameManager.main.player.position3D };
        lr.SetPositions(points);
    }

    void TurnOffLineRenderer() {
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr.positionCount != 0)
            lr.positionCount = 0;
    }
}
