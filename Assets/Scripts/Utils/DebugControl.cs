using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class DebugControl : MonoBehaviour
{
    public static DebugControl main;

    public bool playerInvulnerable;
    public bool breakOnGrappleUpdate;

    public bool drawTerrain;
    public bool drawNavMesh;
    public bool drawGrappleRange;
    public bool drawNavGrid;
    public bool drawZones;
    public bool drawPlayerSight;


    // Start is called before the first frame update
    void Start()
    {
        main = this;
    }

    void DrawTerrain(Map m) {
        for (int i = 0; i < m.terrainEdges.Count; i++) {
            //Layer[] types = map.VertTypes[i];
            TerrainEdge e = m.terrainEdges[i];
            Color c;

            if (e.layer == Layer.BLOCK_FLY) {
                c = CustomColors.darkRed;
            } else {
                c = Color.black;
            }
            Debug.DrawLine(e.vertA_pos3D,e.vertB_pos3D,c);
        }
    }

    public void OnDrawGizmos() {

        if (GameManager.main == null)
            return;

        GameData gameData = GameManager.main.GameData;

        if (drawNavGrid) {
            NavGrid navGrid = gameData.navGrid;

            navGrid.Draw();

            //Vector4 cell = navGrid.PartitionPosFromPos2D(Utils.Vector3ToVector2XZ(cursor.position));
            //int cellValue = navGrid.CellValue((int)cell.x,(int)cell.y,(int)cell.z,(int)cell.w);
            //Color c = Color.white;
            //if (cellValue == (int)Movement.FLYING) {
            //    c = Color.red;
            //} else if (cellValue == (int)Movement.WALKING) {
            //    c = Color.cyan;
            //}
            //navGrid.DrawSquareWithCross(navGrid.PosFromPartition((int)cell.x,(int)cell.y,(int)cell.z,(int)cell.w),navGrid.cellSize,c);
        }

        if (drawPlayerSight) {
            gameData.navGrid.DrawPlayerSight();
        }

        if (drawZones) {
            for (int i = 0; i < gameData.map.zones.Count; i++) {
                gameData.map.zones[i].aabb.DrawBox(Color.red);
            }
        }

        if (drawTerrain)
            gameData.map.DrawTerrainEdgesGizmos();

        if (drawGrappleRange && GameObject.Find("Player") != null) {
            Vector3 playerPos = GameObject.Find("Player").transform.position;
            Vector2[] circles = Calc.CircularPointsAroundPosition(Utils.Vector3ToVector2XZ(playerPos),20,Grapple.grappleRange);
            for (int i = 0; i < circles.Length; i++) {
                Debug.DrawLine(Utils.Vector2XZToVector3(circles[i]),Utils.Vector2XZToVector3(circles[(i + 1) % circles.Length]),CustomColors.green);
            }
            Debug.DrawLine(playerPos,playerPos + Vector3.forward * Grapple.grappleRange,CustomColors.green);
            Debug.DrawLine(playerPos,playerPos + Vector3.back * Grapple.grappleRange,CustomColors.green);
            Debug.DrawLine(playerPos,playerPos + Vector3.left * Grapple.grappleRange,CustomColors.green);
            Debug.DrawLine(playerPos,playerPos + Vector3.right * Grapple.grappleRange,CustomColors.green);
        }

        if (drawNavMesh) {
            NavUtils.DrawTrisXZ(gameData.map.navTris,Color.white,Color.black);
        }
    }
}
