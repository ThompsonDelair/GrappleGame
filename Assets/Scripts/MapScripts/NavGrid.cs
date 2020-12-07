using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is essentially a square grid of cells that are on either a pit zone or floor zone
// It's divided up into "paritions" where each partition contains cells
//      this is so we can skip whole partitions that contain no valid cells for performance
public class NavGrid
{
    public int cellSize = 1;
    int cellsPerPartition = 8;

    const int sightCheckRange = 25;

    const float walkCost = 1f;
    const float flyCost = 3.2f;

    Vector2 offset;
    Cell[,][,] partitions;
    List<Zone>[,] zonesPerParitions;

    public void Init(Map map) {
        AABB_2D aabb = new AABB_2D(map.zones[0].aabb.bottomLeft,map.zones[0].aabb.topRight);

        for (int i = 1; i < map.zones.Count; i++) {
            aabb.Merge(map.zones[i].aabb);
        }
        int cellsX = (int)(aabb.Width / cellSize) + 1;
        int cellsY = (int)(aabb.Height / cellSize) + 1;

        int partitionsX = (cellsX / cellsPerPartition) + 1;
        int partitionsY = (cellsY / cellsPerPartition) + 1;

        Debug.LogFormat("partX: {0} partY: {1}",partitionsX,partitionsY);

        partitions = new Cell[partitionsX,partitionsY][,];
        zonesPerParitions = new List<Zone>[partitionsX,partitionsY];
        offset = new Vector2(aabb.X,aabb.Y);

        InitParitions(map);
        CellScan(map);
        TrimGrid();
    }

    public void InitParitions(Map map) {
        for (int px = 0; px < partitions.GetLength(0); px++) {
            for (int py = 0; py < partitions.GetLength(1); py++) {

                Vector2 p1 = PosFromPartition(px,py,0,0,false);
                Vector2 p2 = PosFromPartition(px,py,cellsPerPartition,cellsPerPartition,false);
                AABB_2D aabb = new AABB_2D(p1,p2);
                bool coversZone = false;
                for (int z = 0; z < map.zones.Count; z++) {

                    if (aabb.OverlapCheck(map.zones[z].aabb)) {
                        if (zonesPerParitions[px,py] == null) {
                            zonesPerParitions[px,py] = new List<Zone>();
                        }
                        zonesPerParitions[px,py].Add(map.zones[z]);
                        coversZone = true;
                    }
                }

                if (coversZone) {
                    partitions[px,py] = new Cell[cellsPerPartition,cellsPerPartition];
                }
            }
        }
    }

    void CellScan(Map map) {
        for (int px = 0; px < partitions.GetLength(0); px++) {
            for (int py = 0; py < partitions.GetLength(1); py++) {
                Cell[,] partition = partitions[px,py];
                if (partition != null) {
                    CellScanInParition(px,py);
                }
            }
        }
    }

    void CellScanInParition(int px,int py) {
        Cell[,] partition = partitions[px,py];
        for (int cx = 0; cx < partition.GetLength(0); cx++) {
            for (int cy = 0; cy < partition.GetLength(1); cy++) {
                Vector2 point = PosFromPartition(px,py,cx,cy);

                for (int z = 0; z < zonesPerParitions[px,py].Count; z++) {
                    Zone zone = zonesPerParitions[px,py][z];
                    if (zone.aabb.ContainsPoint(point) && NavUtils.TrisContainPoint(zone.tris,point)) {
                        partition[cx,cy].m = (int)zone.type;
                        break;
                    }
                }
            }
        }
    }

    public void Draw() {
        for (int px = 0; px < partitions.GetLength(0); px++) {
            for (int py = 0; py < partitions.GetLength(1); py++) {
                Cell[,] partition = partitions[px,py];

                if (partition != null) {
                    DrawParition(px,py);

                    Vector2 p1 = PosFromPartition(px,py,0,0,false);
                    Vector2 p2 = PosFromPartition(px,py,cellsPerPartition,cellsPerPartition,false);
                    AABB_2D aabb = new AABB_2D(p1,p2);
                    aabb.DrawBox(Color.white);
                }
            }
        }
    }

    public void DrawParition(int px,int py) {
        Cell[,] partition = partitions[px,py];

        for (int cx = 0; cx < partitions[px,py].GetLength(0); cx++) {
            for (int cy = 0; cy < partitions[px,py].GetLength(1); cy++) {
                Vector2 pos = PosFromPartition(px,py,cx,cy,false);
                if (partition[cx,cy].m == (int)Movement.WALKING) {
                    DrawSquareWithCross(pos,cellSize,Color.blue);
                } else if (partition[cx,cy].m == (int)Movement.FLYING) {
                    DrawSquareWithCross(pos,cellSize,CustomColors.darkRed);
                }
            }
        }
    }

    public void DrawPlayerSight() {
        for (int px = 0; px < partitions.GetLength(0); px++) {
            for (int py = 0; py < partitions.GetLength(1); py++) {
                Cell[,] partition = partitions[px,py];

                if (partition != null) {
                    for (int cx = 0; cx < partitions[px,py].GetLength(0); cx++) {
                        for (int cy = 0; cy < partitions[px,py].GetLength(1); cy++) {
                            if (partition[cx,cy].canSeePlayer) {
                                Vector2 pos = PosFromPartition(px,py,cx,cy,false);
                                DrawSquareWithCross(pos,cellSize,CustomColors.green);
                            }
                        }
                    }
                }
            }
        }
    }

    public void DrawSquareWithCross(Vector2 a,float size,Color color) {
        Vector2 b = a + Vector2.right * size;
        Vector2 c = a + Vector2.one * size;
        Vector2 d = a + Vector2.up * size;

        Vector3 a3D = Utils.Vector2XZToVector3(a);
        Vector3 b3D = Utils.Vector2XZToVector3(b);
        Vector3 c3D = Utils.Vector2XZToVector3(c);
        Vector3 d3D = Utils.Vector2XZToVector3(d);

        Debug.DrawLine(a3D,b3D,color);
        Debug.DrawLine(b3D,c3D,color);
        Debug.DrawLine(c3D,d3D,color);
        Debug.DrawLine(d3D,a3D,color);

        Debug.DrawLine(a3D,c3D,color);
        Debug.DrawLine(b3D,d3D,color);
    }

    public Vector2 PosFromPartition(int partX,int partY,int cellX,int cellY,bool centered = true) {
        Vector2 pos = new Vector2();
        pos.x = (cellX + partX * cellsPerPartition) * cellSize;
        pos.y = (cellY + partY * cellsPerPartition) * cellSize;
        pos += offset;
        if (centered) {
            pos += Vector2.one * cellSize / 2;
        }
        return pos;
    }

    public Vector2 PosFromCell(int cellX, int cellY,bool centered = true) {
        Vector2 v = new Vector2(cellX,cellY);
        v *= cellSize;
        v += offset;
        if (centered) {
            v += Vector2.one * cellSize / 2;
        }        
        return v;
    }

    public int CellValue(int px,int py,int cx,int cy) {
        if (px >= partitions.GetLength(0) || py >= partitions.GetLength(1) || cx >= cellsPerPartition || cy >= cellsPerPartition) {
            return 0;
        }

        if (px < 0 || py < 0 || cx < 0 || cy < 0 || partitions[px,py] == null) {
            return 0;
        }

        return partitions[px,py][cx,cy].m;
    }

    public PartitionPos PartitionPosFromCellPos(int cx,int cy) {

        if (cx < 0 || cy < 0) {
            return PartitionPos.BadPos;
        }

        int px = cx / cellsPerPartition;
        int py = cy / cellsPerPartition;
        cx %= cellsPerPartition;
        cy %= cellsPerPartition;

        if (px >= partitions.GetLength(0) || py >= partitions.GetLength(1) || px < 0 || py < 0) {
            return PartitionPos.BadPos;
        }

        return new PartitionPos(px,py,cx,cy);
    }

    public Vector4 PartitionPosFromPos2D(Vector2 pos) {
        pos -= offset;
        float x = pos.x;
        float y = pos.y;
        int cx = (int)(x / cellSize);
        int cy = (int)(y / cellSize);
        int px = cx / cellsPerPartition;
        int py = cy / cellsPerPartition;
        cx %= cellsPerPartition;
        cy %= cellsPerPartition;

        if (px >= partitions.GetLength(0) || py >= partitions.GetLength(1)) {
            return Vector4.negativeInfinity;
        }

        return new Vector4(px,py,cx,cy);
    }

    public Vector2Int CellPosFromPos2D(Vector2 pos) {
        pos -= offset;
        return new Vector2Int((int)(pos.x / cellSize),(int)(pos.y / cellSize));
    }

    public Cell CellFromPos2D(Vector2 pos) {
        pos -= offset;
        int cx = (int)(pos.x / cellSize);
        int cy = (int)(pos.y / cellSize);
        return CellFromCellPos(cx,cy);
    }

    public int CellValueFromCellPos(int cx,int cy) {
        int px = cx / cellsPerPartition;
        int py = cy / cellsPerPartition;
        cx %= cellsPerPartition;
        cy %= cellsPerPartition;

        if (px >= partitions.GetLength(0) || py >= partitions.GetLength(1) || partitions[px,py] == null) {
            return 0;
        }

        if (px < 0 || py < 0 || cx < 0 || cy < 0)
            return 0;

        return partitions[px,py][cx,cy].m;
    }

    public Cell CellFromCellPos(int cx,int cy) {
        int px = cx / cellsPerPartition;
        int py = cy / cellsPerPartition;
        cx %= cellsPerPartition;
        cy %= cellsPerPartition;

        if (px >= partitions.GetLength(0) || py >= partitions.GetLength(1) || partitions[px,py] == null) {
            return Cell.BadCell;
        }

        if (px < 0 || py < 0 || cx < 0 || cy < 0)
            return Cell.BadCell;

        return partitions[px,py][cx,cy];
    }

    public float WalkCostFromCellPos(int cx,int cy) {
        int px = cx / cellsPerPartition;
        int py = cy / cellsPerPartition;
        cx %= cellsPerPartition;
        cy %= cellsPerPartition;

        if (px >= partitions.GetLength(0) || py >= partitions.GetLength(1) || partitions[px,py] == null) {
            return 0;
        }

        int m = partitions[px,py][cx,cy].m;

        if(m == (int)Movement.FLYING) {
            return flyCost;
        } else if(m == (int)Movement.WALKING) {
            return walkCost;
        }

        return 0;
    }

    public void ResetSight() {
        for(int px = 0; px < partitions.GetLength(0); px++) {
            for(int py = 0; py < partitions.GetLength(1); py++) {
                Cell[,] partition = partitions[px,py];
                if(partition == null) {
                    continue;
                }
                for(int cx = 0; cx < partition.GetLength(0); cx++) {
                    for(int cy = 0; cy < partition.GetLength(1); cy++) {
                        partition[cx,cy].canSeePlayer = false;
                    }
                }
            }
        }
    }

    public void FindPlayerSight(GameData gameData) {
        Actor player = gameData.player;
        int cellSightRange = (sightCheckRange / cellSize) + 1;
        Vector2Int pCell = CellPosFromPos2D(player.position2D);
        int startX = pCell.x - cellSightRange;
        int startY = pCell.y - cellSightRange;

        for(int cx = startX; cx < pCell.x + cellSightRange; cx++) {
            for(int cy = startY; cy < pCell.y + cellSightRange; cy++) {
                Vector2 pos = PosFromCell(cx,cy);
                //pos += (Vector2.one * cellSize) / 2f;
                if (gameData.map.ClearSightLine(player.position2D,pos)) {
                    PartitionPos p = PartitionPosFromCellPos(cx,cy);
                    if(p.px == -1) {
                        continue;
                    }
                    partitions[p.px,p.py][p.cx,p.cy].canSeePlayer = true;
                }
            }
        }
    }

    public void TrimGrid() {
        List<PartitionPos> cellsToSet = new List<PartitionPos>(100);
        Vector2Int[] neighbors = { Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right };

        for (int px = 0; px < partitions.GetLength(0); px++) {
            for (int py = 0; py < partitions.GetLength(1); py++) {
                Cell[,] partition = partitions[px,py];

                if (partition != null) {
                    for (int cx = 0; cx < partitions[px,py].GetLength(0); cx++) {
                        for (int cy = 0; cy < partitions[px,py].GetLength(1); cy++) {
                            int cxPos = cx + px * cellsPerPartition;
                            int cyPos = cy + py * cellsPerPartition;
                            for(int n = 0; n < neighbors.Length; n++) {
                                if(CellValueFromCellPos(cxPos + neighbors[n].x,cyPos + neighbors[n].y) == 0) {
                                    cellsToSet.Add(new PartitionPos(px,py,cx,cy));
                                    break;
                                }
                            }                            
                        }
                    }
                }
            }
        }

        for (int i = 0; i < cellsToSet.Count; i++) {
            PartitionPos p = cellsToSet[i];
            partitions[p.px,p.py][p.cx,p.cy].m = 0;
        }
    }
}

public struct Cell {
    public int m;
    public bool canSeePlayer;

    public static Cell BadCell = new Cell(-1);

    public Cell(int m) {
        this.m = m;
        canSeePlayer = false;
    }
}

public struct PartitionPos{
    public int px;
    public int py;
    public int cx;
    public int cy;
    public PartitionPos(int px,int py,int cx,int cy) {
        this.px = px;
        this.py = py;
        this.cx = cx;
        this.cy = cy;
    }

    public static PartitionPos BadPos { get { return new PartitionPos(-1,-1,-1,-1); } }
}

