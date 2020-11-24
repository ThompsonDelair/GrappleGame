using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class MapProcessing{

    public static float terrain_threshold = 0.6f;
    public static float resolution = 4;

    public static void mapInitRandom(OldMap m) {
        //m.islands = findIslands(perlinTerrain(m.size));
        m.cells = perlinTerrain(m.size);
        mapInit(m);
    }

    public static void MapChunkedInit(MapChunked map) {
        if (map.size == 0) {
            Debug.LogError("Cant initialize a map with size 0");
        }

        //map.DrawChunkBorders();

        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        for (uint x = 0; x < 2; x++) {
            for(uint y = 0; y < 2; y++) {

                map.navMesh[x,y] = new MeshChunk(map);

                HashSet<EdgePair> implicits = new HashSet<EdgePair>();
                HashSet<Vector2> points = GetChunkPoints(map,x,y,implicits);
                List<NavTri> tris = Triangulation.BowyerWatson(points.ToList(),out map.navMesh[x,y].edgeMap);

                //foreach(NavTri t in tris) {
                //    Utilities.DrawLines(t,Color.green);
                //}

                
                List<EdgePair> orphans = ProcessMapImplicits(tris,implicits,map.vertEdgeMap);
                fixMapOrphans(tris,orphans,map.vertEdgeMap);
                
                map.navMesh[x,y].tris = tris;

            }
        }

        watch.Stop();
        Debug.LogFormat("bowyer watson chunks took {0} ms",watch.ElapsedMilliseconds);

        List<NavTri> trisA = map.navMesh[0,0].tris;
        List<NavTri> trisB = map.navMesh[1,0].tris;
        Dictionary<EdgePair,HalfEdge> borderA = map.navMesh[0,0].edgeMap;
        Dictionary<EdgePair,HalfEdge> borderB = map.navMesh[1,0].edgeMap;

        List<NavTri> trisC = map.navMesh[0,1].tris;
        List<NavTri> trisD = map.navMesh[1,1].tris;
        Dictionary<EdgePair,HalfEdge> borderC = map.navMesh[0,1].edgeMap;
        Dictionary<EdgePair,HalfEdge> borderD = map.navMesh[1,1].edgeMap;

        foreach (NavTri t in map.navMesh[0,0].tris) {
            NavUtils.DrawLines(t,Color.cyan);
        }

        foreach (NavTri t in map.navMesh[1,0].tris) {
            NavUtils.DrawLines(t,Color.blue);
        }

        foreach (NavTri t in map.navMesh[0,1].tris) {
            NavUtils.DrawLines(t,Color.green);
        }

        foreach (NavTri t in map.navMesh[1,1].tris) {
            NavUtils.DrawLines(t,Color.magenta);
        }
        
        //map.navMesh[0,0].tris = Triangulation.MergeChunks(trisA,borderA,trisB,borderB,false);

        //map.navMesh[1,1].tris = Triangulation.MergeChunks(trisC,borderC,trisD,borderD,false);


        //foreach (NavTri t in map.navMesh[0,0].tris) {
        //    if (Triangulation.DelaunayLineCheck(t,(int)MeshChunk.ChunkSize)) {
        //        Utilities.DrawLines(t,Color.red);
        //    }
        //}       

        //foreach (NavTri t in map.navMesh[1,0].tris) {
        //    if (Triangulation.DelaunayLineCheck(t,(int)MeshChunk.ChunkSize)) {
        //        Utilities.DrawLines(t,Color.red);
        //    }
        //}

        //StitchChunkList(map.navMesh);
    }

    public static void GenerateNavMesh(OldMap map) {

        if (map.size == 0) {
            Debug.LogError("Cant initialize a map with size 0");
        }

        //GetChunkPoints(map,0,0,GridControl.chunkTest);

        HashSet<EdgePair> implicits = new HashSet<EdgePair>();
        List<Island> islands = findIslands(map.cells);
        HashSet<Vector2> verts = findNavVertsAndEdges(islands,implicits);
        
        map.implicitEdges = implicits;
        addCornerVerts(map,verts);

        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        var watch2 = new System.Diagnostics.Stopwatch();
        watch2.Start();

        //int count = verts.Count / 20;
        

        watch2.Stop();
        Debug.LogFormat("KD tree setup took {0} ms",watch.ElapsedMilliseconds);

        List<List<NavTri>> triLists = new List<List<NavTri>>();

        if (verts.Count <= 100) {
            Dictionary<EdgePair, HalfEdge> dict = new Dictionary<EdgePair, HalfEdge>();
            triLists.Add(Triangulation.BowyerWatson(new List<Vector2>(verts), out dict));
        } else {
            int count = verts.Count / 20;
            int levels = (int)Mathf.Log(count, 2);
            List<List<Vector2>> pointLists = new List<List<Vector2>>();
            KDNode2 node = KDNode2.BalancedConstructor(new List<Vector2>(verts), levels, pointLists);

            
            List<Dictionary<EdgePair, HalfEdge>> dictList = new List<Dictionary<EdgePair, HalfEdge>>();

            for (int i = 0; i < pointLists.Count; i++)
            {
                Dictionary<EdgePair, HalfEdge> dict;
                List<NavTri> list = Triangulation.BowyerWatson(pointLists[i], out dict);
                triLists.Add(list);
                dictList.Add(dict);
            }

            bool xAxis = levels % 2 == 0;

            for (int i = 0; i < levels; i++)
            {
                for (int j = triLists.Count - 2; j >= 0; j -= 2)
                {
                    //;
                    Triangulation.MergeChunks(triLists[j], dictList[j], triLists[j + 1], dictList[j + 1], xAxis);

                    triLists.RemoveAt(j + 1);
                    dictList.RemoveAt(j + 1);
                }
                xAxis = !xAxis;
            }
        }


        

        watch.Stop();
        Debug.LogFormat("bowyer watson new took {0} ms to complete",watch.ElapsedMilliseconds);

        //List<NavTri> tris = Triangulation.BowyerWatsonGrid(verts.ToList(),(int)map.size);
        //watch.Stop();
        //Debug.LogFormat("bowyer watson grid took {0} ms",watch.ElapsedMilliseconds);

        //watch = new System.Diagnostics.Stopwatch();
        //watch.Start();
        //Dictionary<EdgePair,HalfEdge> edgeMap;
        //tris = Triangulation.BowyerWatson(verts.ToList(), out edgeMap);
        //watch.Stop();
        //Debug.LogFormat("bowyer watson took {0} ms",watch.ElapsedMilliseconds);

        addToVertEdgeMap(triLists[0],map.vertEdgeMap);

        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,0,true));
        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,map.size,true));

        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,0,false));
        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,map.size,false));

        List<EdgePair> orphans = ProcessMapImplicits(triLists[0],implicits,map.vertEdgeMap);

        fixMapOrphans(triLists[0],orphans,map.vertEdgeMap);

        orphans = ProcessMapImplicits(triLists[0],implicits,map.vertEdgeMap,true);

        if (orphans.Count != 0) {
            Debug.LogErrorFormat("still {0} orphans after running orphan fix",orphans.Count);
        }

        map.navtris = triLists[0];

        map.VertKDTree = new KDNode(map.vertEdgeMap.Keys.ToList());
    }

    public static void mapInit(OldMap map) {

        if(map.size == 0) {
            Debug.LogError("Cant initialize a map with size 0");
        }

        //GetChunkPoints(map,0,0,GridControl.chunkTest);
        
        HashSet<EdgePair> implicits = new HashSet<EdgePair>();
        List<Island> islands = findIslands(map.cells);
        HashSet<Vector2> verts = findNavVertsAndEdges(islands,implicits);

        map.implicitEdges = implicits;
        addCornerVerts(map,verts);

        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        List<NavTri> tris = Triangulation.BowyerWatsonGrid(verts.ToList(),(int)map.size);
        watch.Stop();
        Debug.LogFormat("bowyer watson grid took {0} ms",watch.ElapsedMilliseconds);

        watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        Dictionary<EdgePair,HalfEdge> edgeMap;
        tris = Triangulation.BowyerWatson(verts.ToList(),out edgeMap);
        watch.Stop();
        Debug.LogFormat("bowyer watson took {0} ms",watch.ElapsedMilliseconds);

        //Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2,HalfEdge>();
        addToVertEdgeMap(tris,map.vertEdgeMap);

        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,0,true));
        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,map.size,true));

        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,0,false));
        map.implicitEdges.UnionWith(implicitLinearScan(map,0,map.size,map.size,false));

        List<EdgePair> orphans = ProcessMapImplicits(tris,implicits,map.vertEdgeMap);

        fixMapOrphans(tris,orphans,map.vertEdgeMap);
        orphans = ProcessMapImplicits(tris,implicits,map.vertEdgeMap,true);
        if (orphans.Count != 0) {
            Debug.LogError("still orphans after running orphan fix");
        }
        map.navtris = tris;

        map.VertKDTree = new KDNode(map.vertEdgeMap.Keys.ToList());


    }
       
    public static HashSet<Vector2> perlinTerrain(uint size) {
        HashSet<Vector2> terrain = new HashSet<Vector2>();
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                if (Mathf.PerlinNoise(x / resolution,y / resolution) >= terrain_threshold) {
                    terrain.Add(new Vector2(x,y));
                }
            }
        }
        return terrain;
    }

    //public static HashSet<Vector2> cells

    public static List<Island> findIslands(HashSet<Vector2> terrain) {
        List<Island> islands = new List<Island>();
        HashSet<Vector2> unsorted = new HashSet<Vector2>(terrain);

        int counter = 0;
        while(unsorted.Count > 0) {
            Island isl = new Island();
            islands.Add(isl);
            //Vector2 v = unsorted.First();
            HashSet<Vector2> frontier = new HashSet<Vector2>();
            frontier.Add(unsorted.First());
            Vector2 topCorner = unsorted.First();
            Vector2 bottomCorner = unsorted.First();
            unsorted.Remove(unsorted.First());
            int counter2 = 0;

            while(frontier.Count > 0) {
                Vector2 v = frontier.First();
                Vector2[] cardinalNeighbors = Utils.cardinalNeighbors(v);
                for(int i = 0; i < 4; i++) {
                    Vector2 n = cardinalNeighbors[i];
                    if (unsorted.Contains(n)) {
                        frontier.Add(n);
                        unsorted.Remove(n);
                        
                        if(n.x > topCorner.x) {
                            topCorner.x = n.x;
                        } else if(n.x < bottomCorner.x) {
                            bottomCorner.x = n.x;
                        }

                        if(n.y > topCorner.y) {
                            topCorner.y = n.y;
                        } else if(n.y < bottomCorner.y) {
                            bottomCorner.y = n.y;
                        }
                    }
                }
                isl.cells.Add(v);
                frontier.Remove(v);


                if (Utils.WhileCounterIncrementAndCheck(ref counter2))
                    break;
            }

            //isl.position = bottomCorner;
            //isl.size = topCorner - bottomCorner;

            isl.topCorner = topCorner;
            isl.bottomCorner = bottomCorner;

            if (Utils.WhileCounterIncrementAndCheck(ref counter))
                break;
        }

        return islands;
    }

    //public class Island {
    //    public HashSet<Vector2> cells = new HashSet<Vector2>();
    //    //public Vector2 position;
    //    //public Vector2 size;

    //    public Vector2 topCorner;
    //    public Vector2 bottomCorner;

    //}

    static HashSet<Vector2> GetChunkPoints(MapChunked map, uint chunkX, uint chunkY ,HashSet<EdgePair> implicits = null) {
        implicits = implicits ?? new HashSet<EdgePair>();
        HashSet<Vector2> verts = new HashSet<Vector2>();

        for(uint x = 0; x <= MeshChunk.ChunkSize; x++) {
            
            //bool oLimit = x == 0;
            //bool iLimit = x == MeshChunk.ChunkSize;
            uint stop = (chunkY * MeshChunk.ChunkSize + MeshChunk.ChunkSize > map.size) ? map.size : chunkY * MeshChunk.ChunkSize + MeshChunk.ChunkSize;
            if (x + chunkX * MeshChunk.ChunkSize > map.size)
                break;
            HashSet <EdgePair> yAxisImplicits = implicitLinearScan(map.cells,map.size,chunkY * MeshChunk.ChunkSize,stop,x+chunkX * MeshChunk.ChunkSize,false);
            //Debug.Log("yAxisImplicits " + yAxisImplicits.Count);
            implicits.UnionWith(yAxisImplicits);
            foreach (EdgePair ep  in yAxisImplicits) {
                verts.Add(ep.a);
                verts.Add(ep.b);
            }
        }

        for (uint y = 0; y <= MeshChunk.ChunkSize; y++) {

            if (y + chunkY * MeshChunk.ChunkSize > map.size)
                break;

            //bool oLimit = y == 0;
            //bool iLimit = y == MeshChunk.ChunkSize;
            uint stop = (chunkX * MeshChunk.ChunkSize + MeshChunk.ChunkSize > map.size) ? map.size : chunkX * MeshChunk.ChunkSize + MeshChunk.ChunkSize;
            if (stop > map.size)
                break;
            HashSet<EdgePair> xAxisImplicits = implicitLinearScan(map.cells,map.size,chunkX * MeshChunk.ChunkSize,stop,y+chunkY*MeshChunk.ChunkSize,true);
            //Debug.Log("xAxisImplicits " + xAxisImplicits.Count);

            implicits.UnionWith(xAxisImplicits);
            foreach (EdgePair ep in xAxisImplicits) {

                if(!verts.Contains(ep.a))
                    verts.Add(ep.a);

                if (!verts.Contains(ep.b))
                    verts.Add(ep.b);
            }
        }

        //Debug.Log("implicits after get chunk points "+implicits.Count);
        //Vector2 chunkPos = new Vector2(chunkX * MeshChunk.ChunkSize,chunkY * MeshChunk.ChunkSize);
        //if (!verts.Contains(chunkPos))
        //    verts.Add(chunkPos);

        //if (!verts.Contains(chunkPos + new Vector2(MeshChunk.ChunkSize,0)))
        //    verts.Add(chunkPos + new Vector2(MeshChunk.ChunkSize,0));

        //if (!verts.Contains(chunkPos + new Vector2(0,MeshChunk.ChunkSize)))
        //    verts.Add(chunkPos + new Vector2(0,MeshChunk.ChunkSize));

        //if (!verts.Contains(chunkPos + new Vector2(MeshChunk.ChunkSize,MeshChunk.ChunkSize)))
        //    verts.Add(chunkPos + new Vector2(MeshChunk.ChunkSize,MeshChunk.ChunkSize));

        return verts;
    }

    static void StitchChunkList(MeshChunk[,] navChunks) {
        for(uint x = 0; x < navChunks.GetLength(0); x++) {
            for(uint y = 0; y < navChunks.GetLength(1); y++) {
                if (x + 1 < navChunks.GetLength(0))
                    StitchChunkPair(navChunks[x,y],navChunks[x + 1,y]);

                if(y + 1 < navChunks.GetLength(1))
                    StitchChunkPair(navChunks[x,y],navChunks[x,y + 1]);
            }
        }
    }

    static void StitchChunkPair(MeshChunk a,MeshChunk b) {
        foreach(KeyValuePair<EdgePair,HalfEdge> kvp in a.edgeMap) {
            if (b.edgeMap.ContainsKey(kvp.Key)) {
                a.edgeMap[kvp.Key].pair = b.edgeMap[kvp.Key];
                b.edgeMap[kvp.Key].pair = a.edgeMap[kvp.Key];
            }
        }
    }

    public static HashSet<Vector2> findNavVertsAndEdges(List<Island> islands,HashSet<EdgePair> implicits = null) {
        implicits = implicits ?? new HashSet<EdgePair>();
        HashSet<Vector2> verts = new HashSet<Vector2>();

        foreach(Island isl in islands) {        
            
            for(int y = (int)isl.bottomCorner.y; y <= isl.topCorner.y + 1; y++) {    // finds verts and horizontal implicit edges

                Vector2 startVert = new Vector2();
                bool tracing = false;
                bool innerMode = false;

                for(int x = (int)isl.bottomCorner.x; x <= isl.topCorner.x + 1; x++) { 
                    Vector2 p = new Vector2(x,y);
                    bool inner = isl.cells.Contains(p);
                    bool outer = isl.cells.Contains(p - new Vector2(0,1));

                    if(!tracing && inner != outer) {
                        tracing = true;
                        innerMode = inner;
                        verts.Add(p);
                        startVert = p;
                    } else if (tracing && (innerMode != inner || inner == outer)) {
                        tracing = false;
                        verts.Add(p);
                        EdgePair e = new EdgePair(startVert,p);
                        implicits.Add(e);                        
                        //isl.implicits.Add(e);
                        //isl.edges.Add(e);
                        
                        // I think we had an if statement here to for a seperate edge map that mapped edges specifically for tracing around the island in correct order                  

                    }
                }
            }

            for (int x = (int)isl.bottomCorner.x; x <= isl.topCorner.x + 1; x++) {   // finds vertical implicit edges (DONT ADD VERTS YOU DUMMY)

                Vector2 startVert = new Vector2();
                bool tracing = false;
                bool innerMode = false;

                for (int y = (int)isl.bottomCorner.y; y <= isl.topCorner.y + 1; y++) {
                    Vector2 p = new Vector2(x,y);
                    bool inner = isl.cells.Contains(p);
                    bool outer = isl.cells.Contains(p - new Vector2(1,0));

                    if (!tracing && inner != outer) {
                        tracing = true;
                        innerMode = inner;
                        startVert = p;
                    } else if (tracing && (innerMode != inner || inner == outer)) {
                        tracing = false;
                        //verts.Add(p);
                        EdgePair e = new EdgePair(startVert,p);
                        implicits.Add(e);
                        //isl.implicits.Add(e);
                        //isl.edges.Add(e);
                        // I think we had an if statement here to for a seperate edge map that mapped edges specifically for tracing around the island in correct order                  

                    }
                }
            }
        }

        return verts;
    }

    public static HashSet<EdgePair> implicitLinearScan(OldMap map,uint start,uint stop,uint level,bool xAxis = true) {
        return implicitLinearScan(map.cells,map.size,start,stop,level,xAxis);
    }

    public static HashSet<EdgePair> implicitLinearScan(HashSet<Vector2> cells,uint mapSize,uint start,uint stop,uint level,bool xAxis = true) {

        if(start < 0 || stop > mapSize) {
            Debug.LogError("implicit linear scan out of range");
        }

        HashSet<EdgePair> implicits = new HashSet<EdgePair>();
        Vector2 cursor = xAxis ?  new Vector2(start,level) : new Vector2(level,start);
        EdgePair newEdge = null;
        bool tracing = false;
        bool inner = false;
        bool outer = false;

        bool innerLimit = level == mapSize;
        bool outerLimit = level == 0;
               
        for(uint i = start; i <= stop && i <= mapSize; i++) {

            //if (tracing && ((xAxis && cursor.x == TestControl.main.current_map.size) || (!xAxis && cursor.y == TestControl.main.current_map.size))) {
            //    newEdge.b = cursor;
            //    implicits.Add(newEdge);
            //    break;
            //}

            if (tracing && (i == mapSize)) {
                newEdge.b = cursor;
                implicits.Add(newEdge);
                break;
            }

            Vector2 other = xAxis ? cursor - new Vector2(0,1) : cursor - new Vector2(1,0);
            bool cellCompare = (cells.Contains(cursor) == cells.Contains(other));
            if (!tracing && (!cellCompare || innerLimit || outerLimit)) {
                tracing = true;
                inner = cells.Contains(cursor) || innerLimit;
                outer = cells.Contains(other) || outerLimit;
                newEdge = new EdgePair();
                newEdge.a = cursor;
            } else if( tracing && (inner != (cells.Contains(cursor) || innerLimit) || outer != (cells.Contains(other) || outerLimit))) {

                newEdge.b = cursor;
                implicits.Add(newEdge);

                if (cellCompare && (!innerLimit && !outerLimit)) {
                    tracing = false;
                } else {
                    inner = cells.Contains(cursor) || innerLimit;
                    outer = cells.Contains(other) || outerLimit;
                    newEdge = new EdgePair();
                    newEdge.a = cursor;
                }

            }                       
            cursor += xAxis ? new Vector2(1,0) : new Vector2(0,1);
        }        

        return implicits;
    }

    public static HashSet<EdgePair> implicitLinearScanForChunk(MapChunked map,uint start,uint stop,uint level,bool xAxis = true) {

        if (start < 0 || stop > map.size) {
            Debug.LogError("implicit linear scan out of range");
        }

        HashSet<Vector2> cells = map.cells;
        HashSet<EdgePair> implicits = new HashSet<EdgePair>();
        Vector2 cursor = xAxis ? new Vector2(start,level) : new Vector2(level,start);
        EdgePair newEdge = null;
        bool tracing = false;
        bool outer = false;
        bool inner = false;        

        bool outerLimit = level == 0;
        bool innerLimit = level == map.size;
        
        for (uint i = start; i <= stop && i <= map.size; i++) {

            //if (tracing && ((xAxis && cursor.x == TestControl.main.current_map.size) || (!xAxis && cursor.y == TestControl.main.current_map.size))) {
            //    newEdge.b = cursor;
            //    implicits.Add(newEdge);
            //    break;
            //}

            if (tracing && ((i == map.size) || (i > 0 && i % MeshChunk.ChunkSize ==0 ))) {
                newEdge.b = cursor;
                implicits.Add(newEdge);
                break;
            }

            Vector2 other = xAxis ? cursor - new Vector2(0,1) : cursor - new Vector2(1,0);
            bool cellCompare = (cells.Contains(cursor) == cells.Contains(other));
            if (!tracing && (!cellCompare || innerLimit || outerLimit)) {
                tracing = true;
                inner = cells.Contains(cursor) || innerLimit;
                outer = cells.Contains(other) || outerLimit;
                newEdge = new EdgePair();
                newEdge.a = cursor;
            } else if (tracing && (inner != (cells.Contains(cursor) || innerLimit) || outer != (cells.Contains(other) || outerLimit))) {

                newEdge.b = cursor;
                implicits.Add(newEdge);

                if (cellCompare && (!innerLimit && !outerLimit)) {
                    tracing = false;
                } else {
                    inner = cells.Contains(cursor) || innerLimit;
                    outer = cells.Contains(other) || outerLimit;
                    newEdge = new EdgePair();
                    newEdge.a = cursor;
                }

            }
            cursor += xAxis ? new Vector2(1,0) : new Vector2(0,1);
        }

        return implicits;
    }

    public static HashSet<EdgePair> ChunkImplicitScan(uint x, uint y, uint level,uint mapSize, HashSet<Vector2> cells, bool xAxis) {

        HashSet<EdgePair> implicits = new HashSet<EdgePair>();
        Vector2 cursor = xAxis ? new Vector2(x * MeshChunk.ChunkSize,level) : new Vector2(level,y * MeshChunk.ChunkSize);

        throw new NotImplementedException();
    }

    public static void addCornerVerts(OldMap m,HashSet<Vector2> verts) {
        verts.Add(new Vector2(0,0));
        verts.Add(new Vector2(m.size,0));
        verts.Add(new Vector2(0,m.size));
        verts.Add(new Vector2(m.size,m.size));
    }
    
    public static void addToVertEdgeMap(IEnumerable<NavTri> tris,Dictionary<Vector2,HalfEdge> edgeMap) {

        foreach(NavTri t in tris) {
            HalfEdge curr = t.e1;
            for(int i = 0; i < 3; i++) {
                if (!edgeMap.ContainsKey(curr.start)) {
                    edgeMap.Add(curr.start,curr);
                } else {
                    edgeMap[curr.start] = curr;
                }
                curr = curr.next;
            }
        }
    }

    public static void removeFromVertEdgeMap(IEnumerable<NavTri> tris,Dictionary<Vector2,HalfEdge> edgeMap) {
        foreach(NavTri t in tris) {
            foreach (HalfEdge h in t.Edges()) {
                if (edgeMap.ContainsKey(h.start)) {
                    edgeMap.Remove(h.start);
                }
            }
        }
    }

    // Finds orphans AND sets edges
    // Note: both half edges of a terrain edge are marked as terrain edges
    public static List<EdgePair> ProcessMapImplicits(IEnumerable<NavTri> tris,IEnumerable<EdgePair> implicits,Dictionary<Vector2,HalfEdge> vertEdgeMap, bool errorOrphans = false) {
        List<EdgePair> orphans = new List<EdgePair>();

        foreach(EdgePair ep in implicits) {

            HalfEdge implicitEdge = null;

            try {
                implicitEdge = findImplicit(ep,vertEdgeMap[ep.a]);
            } catch (Exception e) {
                Debug.DrawLine(ep.a,ep.b,Color.red);
                Debug.LogErrorFormat("Process map implicits {0} | {1} not found in vert edge map",ep.a,ep.b);
                Debug.LogError(e.ToString());
            }
            
            if (implicitEdge == null) {

                if (errorOrphans) {
                    Debug.DrawLine(ep.a,ep.b,Color.red);

                    foreach (HalfEdge h in edgesAroundVert(ep.a,vertEdgeMap)) {
                        Debug.DrawLine(h.start,h.next.start,CustomColors.orange);
                    }

                    Debug.LogError("orphan connection not found");
                }

                orphans.Add(ep);
            } else {
                Layer moveBlock = Layer.BLOCK_ALL;
                if(ep.moveBlock != Layer.NONE) {
                    moveBlock = ep.moveBlock;
                }
                implicitEdge.moveBlock = moveBlock;
                if (implicitEdge.pair != null) {
                    implicitEdge.pair.moveBlock = moveBlock;
                }
            }
        }

        return orphans;
    }
    
    public static HalfEdge findImplicit(EdgePair implicitEdge,HalfEdge first) {

        if (first == null) {
            Debug.DrawLine(implicitEdge.a,implicitEdge.b,CustomColors.orange);
            Utils.debugStarPoint(implicitEdge.a,0.15f,Color.red);
            Debug.LogError("processImplicits: vert key not present in vertEdgeMap?");
        }

        foreach (HalfEdge h in edgesAroundVert(first)) {
            if (h.CompareEdgePair(implicitEdge)) {

                return h;
            }
        }

        return null;
    }

    //public static HalfEdge findImplicit

    public static bool orphanCheck(EdgePair implicitEdge, HalfEdge first,Dictionary<Vector2,HalfEdge> vertEdgeMap) {

        if (first == null) {
            Debug.DrawLine(implicitEdge.a,implicitEdge.b,CustomColors.orange);
            Utils.debugStarPoint(implicitEdge.a,0.15f,Color.red);
            Debug.LogError("processImplicits: vert key not present in vertEdgeMap?");
        }

        foreach (HalfEdge h in edgesAroundVert(first.start,vertEdgeMap)) {
            if (h.CompareEdgePair(implicitEdge)) {

                return false;
            }
        }

        return true;
    }
    
    public static void fixMapOrphans(List<NavTri> tris,List<EdgePair> orphans,Dictionary<Vector2,HalfEdge> vertEdgeMap) {
        foreach(EdgePair ep in orphans) {

            try {
                HalfEdge textOne = vertEdgeMap[ep.a];
                HalfEdge textTwo = vertEdgeMap[ep.b];
            } catch (Exception e) {
                Debug.LogError("fix map orphans, cannot find edge in vert edge map ");
                Debug.LogError(e.ToString());
                //Debug.DrawLine(ep.a,ep.b,CustomColors.purple);
            }                   

            List<NavTri> newTris = fixOrphan(tris,ep,vertEdgeMap[ep.a],vertEdgeMap[ep.b],vertEdgeMap);

            if(newTris != null) {
                removeFromVertEdgeMap(newTris,vertEdgeMap);
                addToVertEdgeMap(newTris,vertEdgeMap);
            }
        }
    }

    public static List<NavTri> fixOrphan(List<NavTri> tris, EdgePair orphan, HalfEdge aStartEdge, HalfEdge bStartEdge,Dictionary<Vector2,HalfEdge> vertEdgeMap) {
        
        if(findImplicit(orphan,aStartEdge) != null) {
            return null;
        }
        
        // find triangle a
        HalfEdge eA = null;
        
        foreach (HalfEdge e in edgesSurroundingVert(aStartEdge)) {
            if (Calc.DoLinesIntersect(orphan.a,orphan.b,e.start,e.next.start)) {
                eA = e;
                break;
            }
        }

        // find triangle b
        HalfEdge eB = null;

        foreach (HalfEdge e in edgesSurroundingVert(bStartEdge)) {
            if (Calc.DoLinesIntersect(orphan.a,orphan.b,e.start,e.next.start)) {
                eB = e;
                break;
            }
        }

        if(eA == null || eB == null) {
            Debug.DrawLine(orphan.a,orphan.b,Color.red);
            if (eA == null && eB == null) {                
                Debug.LogError("couldnt find orphan " + orphan.a + " " + orphan.b+" EA and EB are null");                
            } else if (eA == null) {
                Debug.LogError("couldnt find orphan " + orphan.a + " " + orphan.b + " EA is null");
            } else {
                Debug.LogError("couldnt find orphan " + orphan.a + " " + orphan.b + " EB is null");
            }
            return null;
        }

        HalfEdge connector = areNeighbors(eA,eB);
        // if orphans are neighbors
        if (connector != null) {

            //Debug.DrawLine(orphan.a,orphan.b,CustomColors.orange);
            //TestControl.simpleOrphans.Add(connector);

            HalfEdge[] a = { connector,connector.next,connector.next.next };
            HalfEdge[] b = { connector.pair,connector.pair.next,connector.pair.next.next };

            a[0].start = b[2].start;
            b[0].start = a[2].start;

            a[2].next = b[1];
            a[1].next = b[0];
            a[0].next = a[2];

            b[2].next = a[1];
            b[1].next = a[0];
            b[0].next = b[2];

            // set centroids
            // a centroid..
            // b centroid..

            a[0].face.e1 = a[0];
            b[0].face.e1 = b[0];

            a[1].face = b[0].face;
            b[1].face = a[0].face;

            NavTri[] faces = { a[0].face,b[0].face };
            removeFromVertEdgeMap(faces,vertEdgeMap);
            addToVertEdgeMap(faces,vertEdgeMap);

            return new List<NavTri>(faces);
        } else {
            // if orphan tris are not neighbors
            HashSet<Vector2> vertSetA = new HashSet<Vector2>();
            vertSetA.Add(eA.start);
            HashSet<Vector2> vertSetB = new HashSet<Vector2>();
            vertSetB.Add(eB.start);

            Dictionary<EdgePair,HalfEdge> edgeMap = new Dictionary<EdgePair,HalfEdge>();

            try {
                edgeMap.Add(eA.next.GetEdgePair(),eA.next.pair);
                edgeMap.Add(eA.next.next.GetEdgePair(),eA.next.next.pair);

                edgeMap.Add(eB.next.GetEdgePair(),eB.next.pair);
                edgeMap.Add(eB.next.next.GetEdgePair(),eB.next.next.pair);
            } catch (Exception e) {

                Debug.DrawLine(orphan.a,orphan.b,Color.yellow);
                Utils.debugStarPoint(orphan.a,0.08f,Color.red);
                Utils.DebugLinePercentage(aStartEdge.start,aStartEdge.next.start,0.4f,CustomColors.orange);
                foreach (HalfEdge h in edgesAroundVert(orphan.a,vertEdgeMap)) {
                    Utils.DebugLinePercentage(h.start,h.next.start,0.3f,Color.red);
                }
                Utils.debugStarPoint(orphan.b,0.08f,Color.magenta);
                Utils.DebugLinePercentage(aStartEdge.start,bStartEdge.next.start,0.4f,CustomColors.orange);
                foreach (HalfEdge h in edgesAroundVert(orphan.b,vertEdgeMap)) {
                    Utils.DebugLinePercentage(h.start,h.next.start,0.3f,Color.magenta);
                }

                Debug.LogError("fix orphans compelx: "+e);
                return null;
            }

            List<NavTri> triPath = new List<NavTri>();
            triPath.Add(eA.face);
            triPath.Add(eB.face);
            int tracker = 0;
            HalfEdge curr = eA.pair;
            while (curr != eB) {
                if (tracker > 1000) {
                    Debug.LogError("Limit reached");
                    break;
                }

                if (curr == null) {
                    

                    Debug.DrawLine(eB.start,eB.next.start,CustomColors.orange);
                    Debug.DrawLine(eA.start,eA.next.start,Color.yellow);

                    Utils.debugStarPoint(orphan.a,0.16f,Color.red);
                    Utils.debugStarPoint(orphan.b,0.16f,Color.red);

                    Debug.LogError("fix orphans complex: curr is null. Are neighbours? " + areNeighbors(eA,eB).ToString());
                }

                if (Calc.DoLinesIntersect(orphan.a,orphan.b,curr.nextStart(),curr.prevStart())) {
                    triPath.Add(curr.face);

                    tracker++;

                    vertSetA.Add(curr.nextStart());
                    vertSetB.Add(curr.start);

                    try {
                        edgeMap.Add(curr.prev().GetEdgePair(),curr.prev().pair);
                    } catch (ArgumentException e) {
                        Debug.DrawLine(orphan.a,orphan.b,Color.yellow);
                        Utils.debugStarPoint(orphan.a,0.08f,Color.red);
                        Utils.DebugLinePercentage(aStartEdge.start,aStartEdge.next.start,0.4f,CustomColors.orange);
                        foreach (HalfEdge h in edgesAroundVert(orphan.a,vertEdgeMap)) {
                            Utils.DebugLinePercentage(h.start,h.next.start,0.3f,Color.red);
                        }
                        Utils.debugStarPoint(orphan.b,0.08f,Color.magenta);
                        Utils.DebugLinePercentage(aStartEdge.start,aStartEdge.next.start,0.4f,CustomColors.orange);
                        foreach (HalfEdge h in edgesAroundVert(orphan.b,vertEdgeMap)) {
                            Utils.DebugLinePercentage(h.start,h.next.start,0.3f,Color.magenta);
                        }
                        Debug.LogError("complex orphan fixing: duplicate edges in edgemap");
                        Debug.LogError(e.ToString());
                        return null;
                    }


                    curr = curr.next.pair;
                    continue;
                }

                if (Calc.DoLinesIntersect(orphan.a,orphan.b,curr.prevStart(),curr.start)) {
                    triPath.Add(curr.face);

                    tracker++;

                    vertSetA.Add(curr.prevStart());
                    vertSetB.Add(curr.start);

                    edgeMap.Add(curr.next.GetEdgePair(),curr.next.pair);

                    curr = curr.prev().pair;
                    continue;
                }

                Debug.LogError("No intersect pair found");
                tracker++;
            }

            // get new contrained polys

            vertSetA.Add(eA.prevStart());
            vertSetA.Add(eB.prevStart());
            Dictionary<EdgePair,HalfEdge> pairMapA;
            List<NavTri> polyA = Triangulation.BowyerWatson(vertSetA.ToList(),out pairMapA);
            //tris.AddRange(polyA);

            vertSetB.Add(eA.prevStart());
            vertSetB.Add(eB.prevStart());
            Dictionary<EdgePair,HalfEdge> pairMapB;
            List<NavTri> polyB = Triangulation.BowyerWatson(vertSetB.ToList(),out pairMapB);
            //tris.AddRange(polyB);

            // find and stitch polyA-polyB pair edge
            bool found = false;
            foreach (NavTri t in polyA) {
                foreach (HalfEdge e in t.Edges()) {
                    if (e.CompareEdgePair(orphan)) {
                        foreach (NavTri t2 in polyB) {
                            foreach (HalfEdge e2 in t2.Edges()) {
                                if (e2.CompareEdgePair(orphan)) {
                                    e.pair = e2;
                                    e2.pair = e;
                                    found = true;

                                    goto stitchDone;
                                }
                            }
                        }
                    }
                }
            }
            stitchDone:
            if (!found) {
                Debug.LogError("polyA-polyB connecting edge not found for orphan border stiching");
            }

            foreach (NavTri t in polyA) {
                Utils.debugStarPoint(t.centroid(),0.16f,Color.blue);
            }

            foreach (NavTri t in polyB) {
                Utils.debugStarPoint(t.centroid(),0.16f,CustomColors.purple);
            }

            polyA.AddRange(polyB);

            Triangulation.trimWithBorder(polyA,edgeMap.Keys);
            
            //tris.UnionWith(polyA);
            //tris.UnionWith(polyB);

            // stitch results back together again (except polyA-polyB pair edge)
            foreach (KeyValuePair<EdgePair,HalfEdge> kvp in edgeMap) {

                bool pairFound = false;

                if (kvp.Value == null) {
                    Vector2 pos = (kvp.Key.a - kvp.Key.b) / 2 + kvp.Key.a;
                    Utils.debugStarPoint(pos,0.08f,CustomColors.orange);
                    continue;
                }

                foreach (NavTri t in polyA) {
                    foreach (HalfEdge e in t.Edges()) {
                        if (e.CompareEdgePair(kvp.Key)) {
                            e.pair = kvp.Value;
                            kvp.Value.pair = e;
                            e.moveBlock = kvp.Value.moveBlock;
                            pairFound = true;
                            goto next;
                        }
                    }
                }

                //foreach (NavTri t in polyB) {
                //    foreach (HalfEdge e in t.Edges()) {
                //        if (e.CompareEdgePair(kvp.Key)) {
                //            e.pair = kvp.Value;
                //            kvp.Value.pair = e;
                //            e.type = kvp.Value.type;
                //            pairFound = true;
                //            goto next;
                //        }
                //    }
                //}

                next:
                if (!pairFound) {
                    Debug.LogError("fix complex orphan, no pair found when stitching border");
                }
            }            

            // remove old tris from their collection
            foreach(NavTri t in triPath) {
                if (tris.Contains(t)) {
                    //tris.Remove(t);
                    List<NavTri> removedTris = new List<NavTri>();
                    int count = 0;
                    for(int i = 0; i < tris.Count; i++) {
                        if(tris[i].ID == t.ID) {
                            tris.RemoveAt(i);
                            count++;
                            removedTris.Add(t);
                            //break;
                        }
                    }
                    if(count >= 2) {
                        Debug.LogErrorFormat("removed {0} tris",count);
                        Vector2 a = removedTris[0][0].start;
                        Vector2 b = removedTris[0][1].start;
                        Vector2 c = removedTris[0][2].start;

                        for (int i = 1; i < removedTris.Count; i++) {
                            if (a != removedTris[i][0].start)
                                Debug.LogError("a does not match");

                            if (b != removedTris[i][1].start)
                                Debug.LogError("b does not match");

                            if (c != removedTris[i][2].start)
                                Debug.LogError("c does not match");
                        }
                    }
                }
                //else {
                //    Debug.LogWarning("list does not contain t?");
                //}

                //if (tris.Contains(t)) {
                //    Debug.LogWarning("list still contains t?");
                //}

                for (int i = 0; i < tris.Count; i++) {
                    if (tris[i].ID == t.ID) {
                        Debug.LogError("list still contains t? ID is "+t.ID);
                        break;
                    }
                }
            }

            // add new tris to the collection
            tris.AddRange(polyA);

            //for (int i2 = 0; i2 < triPath.Count; i2++) {
            //    if (!tris.Remove(triPath[i2])) {
            //        Debug.LogError("Failed to remove tri");
            //    }
            //}


            return polyA;
        }
    }
    
    public static HalfEdge areNeighbors(HalfEdge eA, HalfEdge eB) {
        NavTri t2 = eB.face;
        HalfEdge curr = eA;
        for(int i = 0;i < 3; i++) {
            if(curr.pair.face == t2) {
                // add curr to a
                return curr;
            }
        }
        return null;
    }

    public static IEnumerable edgesAroundVert(Vector2 v, Dictionary<Vector2,HalfEdge> vertEdgeMap) {
        HalfEdge h = null;
        try {
            h = vertEdgeMap[v];
        } catch (Exception e) {
            Debug.LogError("edges around vert: "+e.ToString());
            Utils.debugStarPoint(v,0.08f,Color.yellow);
        }

        return edgesAroundVert(h);
    }

    public static IEnumerable edgesAroundVert(HalfEdge e) {
        HalfEdge curr = e;
        bool reverse = false;
        int tracker = 0;
        do {
            yield return curr;
            if (curr.pair == null) {              
                reverse = true;
                break;
            }            
            curr = curr.pair.next;
            tracker++;
        } while (curr != e && tracker < Utils.smallLimit);

        // if we hit a null pair, loop from start again but backwards
        if (reverse) {



            curr = e;
            do {                
                yield return curr.next.next;
                curr = curr.next.next.pair;
                tracker++;
            } while (curr != null && tracker < Utils.smallLimit);

        }

        if(tracker >= Utils.smallLimit) {
            Debug.DrawLine(e.start,e.next.start,Color.yellow);
            Utils.debugStarPoint(e.start,0.15f,Color.yellow);
            Debug.LogError("edgesAroundVert: hit loop limit");
        }
    }

    public static IEnumerable edgesSurroundingVert(HalfEdge e) {
        HalfEdge curr = e;
        bool reverse = false;
        int tracker = 0;
        do {
            yield return curr.next;
            if (curr.pair == null) {
                reverse = true;
                break;
            }
            curr = curr.pair.next;
            tracker++;
        } while (curr != e && tracker < Utils.smallLimit);

        // if we hit a null pair, loop from start again but backwards
        if (reverse) {
            curr = e;
            while(curr.next.next.pair != null && tracker < Utils.smallLimit) {
                curr = curr.next.next.pair;
                yield return curr.next;
                tracker++;
            }

        }

        if (tracker >= Utils.smallLimit) {
            Debug.DrawLine(e.start,e.next.start,Color.yellow);
            Utils.debugStarPoint(e.start,0.15f,Color.yellow);
            Debug.LogError("edgesSurroundingVert: hit loop limit");
        }
    }

    public static IEnumerable TriEdges(HalfEdge e) {
        HalfEdge curr = e;
        for (int i = 0; i < 3; i++) {
            yield return curr;
            curr = curr.next;
        }
    }

    public static IEnumerable TriEdges(NavTri t) {
        HalfEdge curr = t.e1;
        for(int i = 0; i < 3; i++) {
            yield return curr;
            curr = curr.next;
        }
    }

    public static IEnumerable TriPoints(NavTri t) {
        foreach(HalfEdge e in TriEdges(t)) {
            yield return e.start;
        }
    }

    public static IEnumerable EdgesInList(IEnumerable<NavTri> list) {
        foreach(NavTri t in list) {
            foreach (HalfEdge e in TriEdges(t)) {
                yield return e;
            }
        }
    }
}

