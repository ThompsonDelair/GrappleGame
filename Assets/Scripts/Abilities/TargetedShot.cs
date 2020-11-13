using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetedShot : Ability
{

    float shootingTimestamp;
    public float timeBetweenShots = 2f;
    private bool isAttacking = false;
    private EnemyActor actor = null;

    public TargetedShot() { }

    public override bool attacking() { return isAttacking; }

    public override bool RunAbilityUpdate(Vector2 pos, EnemyActor a)
    {
        if(actor == null)
        {
            actor = a;
        }
        if (a.transform == null) // HACKY WAY TO STOP ABILITY, workaround so that I dont have to refactor some code for now
        {
            return false;
        }

        return ShootUpdate();
    }

    public override bool StartAbilityCheck(Vector2 pos, EnemyActor a)
    {
        
        return true; // hardcoded to return true as pre cast check not necessary with sentry logic. Should redo abstract class as abstract methods aren't implemented in all children.
    }


    // Modified to return boolean based on whether casting should cease or not
    private bool ShootUpdate()
    {
        if (CanSeePlayer())
        {
            // rotate to look at player
            actor.transform.LookAt(GameManager.main.player.position3D);

            if (shootingTimestamp < Time.time)
            {
                shootingTimestamp = Time.time + timeBetweenShots;
                ShootAtPlayer();
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    void ShootAtPlayer()
    {
        GameObject gameObjBullet = GameManager.main.GetNewSentryBullet(); // Since ability does not derive from MonoBehaviour we have to get another mono class to do the work of instantiation for us.
        Bullet bullet = new Bullet(gameObjBullet.transform, 0.5f, Layer.PLAYER, 1f);
        bullet.speed = 20f;
        bullet.position3D = actor.transform.position;
        GameManager.main.bullets.Add(bullet);

        //GetComponent<Actor>().PlayAudioClip(AudioClips.singleton.gunShot);
        // Set to shoot in that direction
        bullet.transform.rotation = actor.transform.rotation;
    }

    bool CanSeePlayer()
    {
        Vector2 playerPos2D = GameManager.main.player.position2D;
        Vector2 sentryPos2D = Utils.Vector3ToVector2XZ(actor.transform.position);
        Map map = GameManager.main.map;
        for (int i = 0; i < map.terrainEdges.Count; i++)
        {
            TerrainEdge e = map.terrainEdges[i];
            if (CollisionSystem.LayerOrCheck(e.layer, Layer.BLOCK_FLY) && Calc.DoLinesIntersect(playerPos2D, sentryPos2D, e.vertA_pos2D, e.vertB_pos2D))
            {
                TurnOffLineRenderer();
                return false;
            }
        }
        //Debug.DrawLine(GameManager.main.player.position3D,transform.position,Color.red);
        UpdateLineRenderer();
        return true;
    }

    void UpdateLineRenderer()
    {
        LineRenderer lr = actor.transform.gameObject.GetComponent<LineRenderer>();
        if (lr.positionCount != 2)
            lr.positionCount = 2;

        Vector3[] points = { actor.transform.position, GameManager.main.player.position3D };
        lr.SetPositions(points);
    }

    void TurnOffLineRenderer()
    {
        LineRenderer lr = actor.transform.gameObject.GetComponent<LineRenderer>();
        if (lr.positionCount != 0)
            lr.positionCount = 0;
    }
}
