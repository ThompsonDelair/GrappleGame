using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetedShot : Ability
{
    ShootStats stats;

    float shootingTimestamp;
    private bool isAttacking = false;

    private TargetedShot() { }

    //public override bool attacking() { return isAttacking; }

    public TargetedShot(ShootStats s) {
        stats = s;
    }

    public override bool RunAbilityUpdate(Actor a, GameData data)
    {
        //if(actor == null)
        //{
        //    actor = a;
        //    timeBetweenShots = a.transform.gameObject.GetComponent<ActorInfo>().stats.timeBetweenAttack;
        //}
        //if (a.transform == null) // HACKY WAY TO STOP ABILITY, workaround so that I dont have to refactor some code for now
        //{
        //    return false;
        //}

        return ShootUpdate(a);
    }

    public override bool StartAbilityCheck(Actor a,GameData data)
    {
        
        return true; // hardcoded to return true as pre cast check not necessary with sentry logic. Should redo abstract class as abstract methods aren't implemented in all children.
    }


    // Modified to return boolean based on whether casting should cease or not
    private bool ShootUpdate(Actor a)
    {
        if (CanSeePlayer(a))
        {
            // rotate to look at player
            a.transform.LookAt(GameManager.main.player.position3D);

            if (shootingTimestamp < Time.time)
            {
                shootingTimestamp = Time.time + stats.timeBetweenShots;
                ShootAtPlayer(a);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    void ShootAtPlayer(Actor a)
    {
        GameObject gameObjBullet = GameManager.main.GetNewSentryBullet(); // Since ability does not derive from MonoBehaviour we have to get another mono class to do the work of instantiation for us.
        Bullet bullet = new Bullet(gameObjBullet.transform, stats.bulletStats.radius, Team.PLAYER, stats.damage);
        bullet.speed = stats.bulletStats.speed;
        bullet.position3D = a.transform.position;
        GameManager.main.gameData.bullets.Add(bullet);

        //GetComponent<Actor>().PlayAudioClip(AudioClips.singleton.gunShot);
        // Set to shoot in that direction
        bullet.transform.rotation = a.transform.rotation;
    }

    bool CanSeePlayer(Actor a)
    {
        Vector2 playerPos2D = GameManager.main.player.position2D;
        Vector2 sentryPos2D = Utils.Vector3ToVector2XZ(a.transform.position);
        Map map = GameManager.main.gameData.map;
        for (int i = 0; i < map.terrainEdges.Count; i++)
        {
            TerrainEdge e = map.terrainEdges[i];
            if (CollisionSystem.LayerOrCheck(e.layer, Layer.BLOCK_FLY) && Calc.DoLinesIntersect(playerPos2D, sentryPos2D, e.vertA_pos2D, e.vertB_pos2D))
            {
                TurnOffLineRenderer(a);
                return false;
            }
        }
        //Debug.DrawLine(GameManager.main.player.position3D,transform.position,Color.red);
        UpdateLineRenderer(a);
        return true;
    }

    void UpdateLineRenderer(Actor a)
    {
        LineRenderer lr = a.transform.gameObject.GetComponent<LineRenderer>();
        if (lr.positionCount != 2)
            lr.positionCount = 2;

        Vector3[] points = { a.transform.position, GameManager.main.player.position3D };
        lr.SetPositions(points);
    }

    void TurnOffLineRenderer(Actor a)
    {
        LineRenderer lr = a.transform.gameObject.GetComponent<LineRenderer>();
        if (lr.positionCount != 0)
            lr.positionCount = 0;
    }
}
