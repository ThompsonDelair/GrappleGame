using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGridPathfindTest : MonoBehaviour
{
    float timestamp;
    float delay = 0.3f;

    List<Vector2> path;

    Transform pointA;
    Transform pointB;

    NavGrid navGrid;

    // Start is called before the first frame update
    void Start()
    {
        pointA = transform.GetChild(0);
        pointB = transform.GetChild(1);
        navGrid = GameManager.main.GameData.navGrid;
        GetPath();
    }

    // Update is called once per frame
    void Update()
    {
        DrawPath();

        if(timestamp < Time.time) {
            GetPath();
            timestamp = Time.time + delay;
        }
    }

    void GetPath() {
        path = NavGridPathfind.Pathfind(
            navGrid.CellPosFromPos2D(Utils.Vector3ToVector2XZ(pointA.position)),
            navGrid.CellPosFromPos2D(Utils.Vector3ToVector2XZ(pointB.position)),
            Movement.FLYING,
            navGrid);
    }

    void DrawPath() {
        if(path == null) {
            return;
        }

        Vector2Int cellA = navGrid.CellPosFromPos2D(Utils.Vector3ToVector2XZ(pointA.position));
        Vector2 cellPosA = navGrid.PosFromCell(cellA.x,cellA.y);
        Utils.debugStarPoint(Utils.Vector2XZToVector3(cellPosA),1f,Color.green);

        Vector2Int cellB = navGrid.CellPosFromPos2D(Utils.Vector3ToVector2XZ(pointB.position));
        Vector2 cellPosB = navGrid.PosFromCell(cellB.x,cellB.y);
        Utils.debugStarPoint(Utils.Vector2XZToVector3(cellPosB),1f,Color.red);

        for (int i = 0; i < path.Count - 1; i++) {
            Vector2 posA = path[i];
            Vector2 posB = path[i+1];

            Debug.DrawLine(Utils.Vector2XZToVector3(posA),Utils.Vector2XZToVector3(posB),Color.white);
            Utils.debugStarPoint(Utils.Vector2XZToVector3(posA),0.8f,Color.white);
        }
    }
}
