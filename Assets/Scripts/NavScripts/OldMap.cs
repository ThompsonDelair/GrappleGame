using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OldMap {

    // nav points
    // public 
    public uint size;
    // terrainTiles
    public HashSet<Vector2> cells = new HashSet<Vector2>();
    //public HashSet<Vector2> terrain = new HashSet<Vector2>();
    public List<NavTri> navtris = new List<NavTri>();

    public MeshChunk[,] navMesh;

    public Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2,HalfEdge>();

    public HashSet<EdgePair> implicitEdges = new HashSet<EdgePair>();

    public KDNode VertKDTree;

    //public Cell[,] cellInfo;

    private OldMap() {

    }

    public MeshChunk this[byte x,byte y] {
        get { return navMesh[x,y]; }
        set { }
    }

    public OldMap(uint size) {
        this.size = size;
        uint chunkCount = (size / MeshChunk.ChunkSize) + (uint)((size % MeshChunk.ChunkSize == 0) ? 0 : 1);
        navMesh = new MeshChunk[chunkCount,chunkCount];
        //cellInfo = new Cell[size,size];
        //terrain.Add(new Vector2(1,1));
        //terrain.Add(new Vector2(2,2));
    }

    //public static void test() {
    //    //current
    //}

    //public bool containsTerrain(Vector2 v) {
    //    foreach(Island i in islands) {
    //        if (i.cells.Contains(v)) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public static OldMap randomMap(uint size) {
        OldMap m = new OldMap(size);
        MapProcessing.mapInitRandom(m);
        return m;
    }

    //public static void updateTilesGraphics(Map m) {
    //    tilemap.ClearAllTiles();

    //    for (int x = 0; x < m.size; x++) {
    //        for (int y = 0; y < m.size; y++) {
    //            Tile tile = ScriptableObject.CreateInstance<Tile>();
    //            if (m.cells.Contains(new Vector2(x,y))) {
    //                tile.sprite = TestControl.terrain.sprite;
    //                tilemap.SetTile(new Vector3Int(x,y,0),TestControl.terrain);
    //            } else {
    //                tile.sprite = TestControl.ground.sprite;
    //                tilemap.SetTile(new Vector3Int(x,y,0),TestControl.ground);
    //            }
    //        }
    //    }
    //}



    public bool ValidMapCoord(Vector2Int v) {
        return (
            v.x >= 0 &&
            v.x < size &&
            v.y >= 0 &&
            v.y < size
            );
    }

    public NavTri triFromPoint(Vector2 p) {

        Vector2 vert = VertKDTree.ClosestPoint(p);
        foreach (HalfEdge h in MapProcessing.edgesAroundVert(vert,vertEdgeMap)) {
            if (NavCalc.PointInTri(p,h.face)) {
                return h.face;
            }
        }

        //Debug.LogError("Cannot find tri from point");

        NavTri t = vertEdgeMap[vert].face;

        int counter = 0;
        while (counter < Utils.smallLimit) {
            foreach (HalfEdge h in t.Edges()) {
                if (h.pair != null && Calc.ClockWiseCheck(h.start,h.next.start,p) > 0) {
                    t = h.pair.face;
                    break;
                }
            }

            if (NavCalc.PointInOrOnTri(p,t))
                return t;
            counter++;
        }

        if (counter >= Utils.smallLimit) {
            Debug.LogError("loop limit reached, position: " + p);
            Utils.debugStarPoint(p, 1f, Color.red);
        }


        return null;

        //foreach(NavTri t in navtris) {
        //    if (Utilities.pointInTri(p,t)) {
        //        return t;
        //    }
        //}

        //return null;
    }

    //class KDTree {
    //    List<Vector2> points = new List<Vector2>();

    //    public KDTree(List<Vector2 points) {

    //    }
    //}

    

    public void DrawChunkBorders() {

        for (int x = 0; x < navMesh.GetLength(0); x++) {

            Vector2 point1x = new Vector2(x * MeshChunk.ChunkSize,0);
            Vector2 point2x = new Vector2(x * MeshChunk.ChunkSize,size);
            Debug.DrawLine(point1x,point2x,Color.yellow);

            for (int y = 0; y < navMesh.GetLength(1); y++) {
                Vector2 point1y = new Vector2(0,y * MeshChunk.ChunkSize);
                Vector2 point2y = new Vector2(size,y * MeshChunk.ChunkSize);
                Debug.DrawLine(point1y,point2y,Color.yellow);
            }
        }
    }

}

public class KDNode
{
    public bool xAxis;
    public Vector2 val;
    List<Vector2> list;
    public KDNode left;
    public KDNode right;

    public KDNode(List<Vector2> points,bool axis = true) {

        xAxis = axis;

        if (points.Count > 1) {

            if (axis) {
                points.Sort(CompareXAxis);
            } else {
                points.Sort(CompareYAxis);
            }

            if (points.Count > 2) {

                int index = points.Count / 2;

                val = points[index];

                left = new KDNode(points.GetRange(0,index),!axis);
                right = new KDNode(points.GetRange(index + 1,points.Count - index - 1),!axis);
            } else {
                val = points[0];
                right = new KDNode(points.GetRange(1,1),!axis);
            }
        } else {
            val = points[0];
        }
    }

    public KDNode(List<Vector2> points,int k,bool axis = true) {
        xAxis = axis;
        if (points.Count <= 10) {
            list = points;
            return;
        }

        int medianIndex = Calc.MyMedianFind(points,0,points.Count - 1,Compare);
        val = points[medianIndex];
        left = new KDNode(points.GetRange(0,medianIndex),1,!axis);
        right = new KDNode(points.GetRange(medianIndex + 1,points.Count - medianIndex - 1),1,!axis);
    }

    class SearchInfo
    {
        public Vector2 bestPos;
        public float bestDist;
    }

    public Vector2 ClosestPoint(Vector2 pos) {
        SearchInfo info = new SearchInfo();
        info.bestDist = Mathf.Infinity;
        ClosestPointSearch(this,pos,ref info);
        return info.bestPos;
    }

    static void ClosestPointSearch(KDNode root,Vector2 pos,ref SearchInfo info) {

        if (root == null) {
            return;
        }

        float dist = Vector2.Distance(pos,root.val);
        if (dist <= info.bestDist) {
            info.bestDist = dist;
            info.bestPos = root.val;
        }

        int compare = root.Compare(pos,root.val);

        if (compare == 0)
            return;

        ClosestPointSearch((compare < 0) ? root.left : root.right,pos,ref info);
        if (root.ThresholdCheck(info.bestDist,pos))
            return;
        ClosestPointSearch((compare > 0) ? root.left : root.right,pos,ref info);
    }

    public bool ThresholdCheck(float bestDist,Vector2 pos) {
        if (xAxis) {
            return Mathf.Abs(pos.x - val.x) >= bestDist;
        } else {
            return Mathf.Abs(pos.y - val.y) >= bestDist;
        }
    }

    public int Compare(Vector2 a,Vector2 b) {
        if (xAxis)
            return CompareXAxis(a,b);
        else
            return CompareYAxis(a,b);
    }

    public int CompareXAxis(Vector2 a,Vector2 b) {
        if (a.x != b.x) {
            return a.x.CompareTo(b.x);
        }
        return a.y.CompareTo(b.y);

    }

    public int CompareYAxis(Vector2 a,Vector2 b) {
        if (a.y != b.y) {
            return a.y.CompareTo(b.y);
        }
        return a.x.CompareTo(b.x);
    }

    public int Pivot(List<Vector2> list,int left,int right) {
        if (right - left < 5) {
            return Partition5(list,left,right);
        }
        for (int i = left; i < right; i += 5) {
            int subRight = i + 4;
            if (subRight > right) {
                subRight = right;
            }
            int median5 = Partition5(list,i,subRight);
            Utils.ListSwap(list,median5,left + ((i - left) / 5));
            //Vector2 temp = list[median5];
            //list[median5] = list[left + ((i - left) / 5)];
        }
        int mid = (right - left) / 10 + left;
        //int pivotIndex = Pivot(list,left,right);
        //pivotIndex = Partition(list,left,right,pivotIndex,n);
        return Select(list,left,left + ((right - left) / 5),mid);
    }

    public int Partition5(List<Vector2> list,int left,int right) {
        int i = left + 1;
        int counter = 0;
        while (i <= right) {
            int j = i;
            int counter2 = 0;
            while (j > left && CompareVector2(list[j - 1],list[j])) {
                Vector2 temp = list[j - 1];
                list[j - 1] = list[j];
                list[j] = temp;
                j = j - 1;
                if (Utils.WhileCounterIncrementAndCheck(ref counter2))
                    break;
            }
            i = i + 1;
            if (Utils.WhileCounterIncrementAndCheck(ref counter))
                break;

        }
        return (left + right) / 2;
    }

    public bool CompareVector2(Vector2 a,Vector2 b) {
        if (a.x != b.x) {
            return a.x > b.x;
        } else if (a.y != b.y) {
            return a.y > b.y;
        }
        return false;
    }

    int Select(List<Vector2> list,int left,int right,int n) {
        if (left == right)
            return left;
        int pivotIndex = Pivot(list,left,right);
        pivotIndex = Partition(list,left,right,pivotIndex,n);
        if (n == pivotIndex) {
            return n;
        } else if (n < pivotIndex) {
            right = pivotIndex - 1;
        } else {
            left = pivotIndex + 1;
        }
        return Select(list,left,right,n);
    }

    int Partition(List<Vector2> list,int left,int right,int pivotIndex,int n) {
        Vector2 pivotValue = list[pivotIndex];
        Utils.ListSwap(list,pivotIndex,right); // move the pivot to the end of the list
        int storeIndex = left;

        // move all elements smaller than the pivot to the left of the pivot
        for (int i = left; i < right; i++) {
            if (Compare(list[i],pivotValue) < 0) {
                Utils.ListSwap(list,storeIndex,i);
                storeIndex++;
            }
        }

        // move all elements equal to the pivot right after the smaller elements
        // shouldn't really need to do this??
        //int storeIndexEq = storeIndex;
        //for(int i = storeIndex; i < right; i++) {
        //    if (Compare(list[i],pivotValue) == 0) {
        //        Utilities.ListSwap(list,storeIndexEq,i);
        //        storeIndexEq++;
        //    }
        //}

        Utils.ListSwap(list,right,storeIndex);    // move the pivot back to its spot
        return storeIndex;
    }


}
