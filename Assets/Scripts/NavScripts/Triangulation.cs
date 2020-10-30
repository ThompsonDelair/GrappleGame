using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//using EdgeID = System.Tuple<byte,byte,short>;

public static class Triangulation
{
    public static void trimWithBorder(List<NavTri> tris,IEnumerable<EdgePair> border) {

        Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2,HalfEdge>();
        MapProcessing.addToVertEdgeMap(tris,vertEdgeMap);

        foreach(EdgePair ep in border) {

            if (!vertEdgeMap.ContainsKey(ep.a) || !vertEdgeMap.ContainsKey(ep.b)) {
                foreach(NavTri t in tris) {
                    NavUtils.DrawLines(t,Color.yellow);
                }
                foreach (EdgePair h2 in border) {
                    Debug.DrawLine(h2.a,h2.b,CustomColors.orange);
                }

                foreach (Vector2 v in vertEdgeMap.Keys) {
                    Utils.debugStarPoint(v,0.16f,CustomColors.purple);
                }
                Utils.debugStarPoint(ep.a,0.08f,Color.red);
                Debug.LogError("trim with border: missing point from vertEdgeMap");                
            }
            
            if (MapProcessing.orphanCheck(ep,vertEdgeMap[ep.a],vertEdgeMap)) {
                MapProcessing.fixOrphan(tris,ep,vertEdgeMap[ep.a],vertEdgeMap[ep.b],vertEdgeMap);
            }
        }

        RemoveOuterTris(tris,border);
    }
    
    static void RemoveOuterTris(List<NavTri> tris,IEnumerable<EdgePair> border) {
        Vector2 farpoint = new Vector2(999999,999999);
        tris.RemoveAll(delegate (NavTri t) {
            int intersects = 0;
            foreach (EdgePair e in border) {

                if (e == null) {
                    Debug.LogError("border trim, e is null");
                    foreach (EdgePair e2 in border) {
                        if (e2 != null) {
                            Debug.DrawLine(e2.a,e2.b,Color.red);
                        }
                    }
                }
                if (t == null) {
                    Debug.LogError("border trim, t is null");
                }

                if (Calc.ClockWiseCheck(e.a,t.centroid(),farpoint) == 0 || Calc.ClockWiseCheck(e.b,t.centroid(),farpoint) == 0) {
                    farpoint.x++;
                }

                if (Calc.DoesLineIntersect(farpoint,t.centroid(),e.a,e.b)) {
                    intersects++;
                }
            }

            //if (intersects % 2 == 0) {
            //    Utilities.debugStarPoint(t.centroid(),0.12f,Color.yellow);
            //} else {
            //    Utilities.debugStarPoint(t.centroid(),0.08f,Color.green);
            //}

            return intersects % 2 == 0;
        });
    }

    public static List<NavTri> BowyerWatson(List<Vector2> verts, out Dictionary<EdgePair,HalfEdge> edgeMap) {

        edgeMap = new Dictionary<EdgePair,HalfEdge>();

        if (verts.Count < 3) {
            Debug.LogError("cant triangulate with less than 3 points");
            return null;
        }
        
        short superNum = 3000;
        List<NavTri> tris = new List<NavTri>();
               
        Vector2[] superPoints = {
            new Vector2(superNum,superNum),
            new Vector2(-superNum * 3,superNum),
            new Vector2(superNum,-superNum * 3)
        };

        HalfEdge e1 = new HalfEdge();
        HalfEdge e2 = new HalfEdge();
        HalfEdge e3 = new HalfEdge();

        NavTri face = new NavTri();
        face.e1 = e1;

        e1.start = superPoints[0];
        e1.next = e2;
        e1.face = face;
        e2.start = superPoints[1];
        e2.next = e3;
        e2.face = face;
        e3.start = superPoints[2];
        e3.next = e1;
        e3.face = face;

        tris.Add(face);

        int tracker = 0;

        foreach (Vector2 v in verts) {

            // find bad triangles, add them to an array, then remove them from triangles
            // loop backwards so we can removeAt with impunity
            List<NavTri> badTris = new List<NavTri>();

            tris.RemoveAll(delegate (NavTri t) {

                if (DelaunayInvalid(t,v)) {
                    badTris.Add(t);
                    return true;
                }

                return false;
            });

            if (badTris.Count == 0) {
                Debug.LogError("bad tris empty");
                return tris;
            }

            //HashSet<EdgePair> oldEdges = new HashSet<EdgePair>();
            //HashSet<EdgePair> oldEdges = new HashSet<EdgePair>();
            Dictionary<EdgePair,HalfEdge> oldEdges = new Dictionary<EdgePair,HalfEdge>();

            // find the boundary of the polygonal hole  
            for (int i = 0; i < badTris.Count(); i++) {
                HalfEdge curr = badTris[i].e1;
                for (int n = 0; n < 3; n++) {
                    EdgePair e = new EdgePair(curr.start,curr.next.start);
                    if (oldEdges.ContainsKey(e)) {
                        oldEdges[e] = null;
                    } else {
                        oldEdges.Add(e,curr);
                    }
                    curr = curr.next;
                }
            }

            Dictionary<Vector2,HalfEdge> polygon = new Dictionary<Vector2,HalfEdge>();
            // get rid of edges with duplicates
            // create link map
            // create new triangles
            foreach (KeyValuePair<EdgePair,HalfEdge> edge in oldEdges) {
                // if edge is unique
                if (edge.Value != null) {
                    // make new triangle (with unset pairs for eB and eC)
                    // eC is the edge that goes from the new point back to the unqiue edges
                    HalfEdge eB = new HalfEdge();
                    HalfEdge eC = new HalfEdge();
                    // 01/01/2019
                    NavTri newFace = new NavTri();

                    HalfEdge.set(eB,edge.Value.next.start,eC,null,newFace);
                    HalfEdge.set(eC,v,edge.Value,null,newFace);

                    newFace.e1 = edge.Value;
                    edge.Value.face = newFace;
                    edge.Value.next = eB;

                    polygon.Add(edge.Value.start,edge.Value);
                    tris.Add(newFace);
                    // need to manage adding new tris to tris
                    // we already removed the original polygon hole triangles when we checked for bad triangles
                    // so all we need to do is add new ones!
                    // we should make a new navTri yadda yadda
                }
            }

            if (polygon.Count == 0) {
                Debug.LogError("empty polygon");
            }

            //if (polygon.Count <= badTris.Count)
            //    GridControl.badTriCountTest = true;

            // link pairs
            //var first = polygon.First();
            HalfEdge currEdge = polygon.FirstOrDefault().Value;
            //Debug.Log(currEdge);
            //Debug.Log(currEdge.next);
            //Debug.Log(currEdge.next.start);
            HalfEdge nextEdge = polygon[currEdge.next.start];
            for (int i = 0; i < polygon.Count; i++) {
                currEdge.next.pair = nextEdge.next.next;
                nextEdge.next.next.pair = currEdge.next;
                currEdge = nextEdge;
                nextEdge = polygon[currEdge.next.start];
            }

            tracker++;
        }

        // done inserting points, now clean up
        // remove every triangle that has a vertex of the original super triangle
        // also record a map of the outer edges of the final polygon with their pair edges

        //Dictionary<EdgePair,HalfEdge> pairMap = new Dictionary<EdgePair,HalfEdge>();
        //HashSet<NavTri> toRemove = new HashSet<NavTri>();

        for (int i = tris.Count - 1; i >= 0; i--){
            HalfEdge curr = tris[i].e1;
            for (int n = 0; n < 3; n++) {
                if (curr.start == superPoints[0] || curr.start == superPoints[1] || curr.start == superPoints[2]) {
                    if (curr.next.start == superPoints[0] || curr.next.start == superPoints[1] || curr.next.start == superPoints[2]) {
                        tris.RemoveAt(i);
                        break;
                    }
                    if (curr.prev().start == superPoints[0] || curr.prev().start == superPoints[1] || curr.prev().start == superPoints[2]) {
                        tris.RemoveAt(i);
                        break;
                    }

                    try {
                        edgeMap.Add(new EdgePair(curr.next.start,curr.next.next.start),curr.next.pair);
                    } catch (Exception e) {
                        Debug.DrawLine(curr.next.start,curr.next.next.start,Color.red);
                        Debug.LogError(e.ToString());
                    }

                    curr.next.pair.pair = null;
                    tris.RemoveAt(i);
                    break;
                }
                curr = curr.next;
            }
        }

        //tris.RemoveAll(delegate(NavTri t) {
        //    HalfEdge curr = t.e1;
        //    for (int n = 0; n < 3; n++) {
        //        if (curr.start == superPoints[0] || curr.start == superPoints[1] || curr.start == superPoints[2]) {
        //            if (curr.next.start == superPoints[0] || curr.next.start == superPoints[1] || curr.next.start == superPoints[2]) {
        //                return true;
        //            }
        //            if (curr.prev().start == superPoints[0] || curr.prev().start == superPoints[1] || curr.prev().start == superPoints[2]) {
        //                return true;
        //            }

        //            try {
        //                edgeMap.Add(new EdgePair(curr.next.start,curr.next.next.start),curr.next.pair);
        //            } catch (Exception e) {
        //                Debug.DrawLine(curr.next.start,curr.next.next.start,Color.red);
        //                Debug.LogError(e.ToString());
        //            }


        //            curr.next.pair.pair = null;
        //            return true;
        //        }
        //        curr = curr.next;
        //    }
        //    return false;
        //});

        //for (int i = tris.Count - 1; i >= 0; i--) {
        //    NavTri t = tris.ElementAt(i);
            
        //}
        return tris;
    }
       
    public static List<NavTri> BowyerWatsonGrid(List<Vector2> verts,int mapSize, Dictionary<EdgePair,HalfEdge> edgeMap = null) {

        const int sectorSize = 32;

        if (verts.Count < 3) {
            Debug.LogError("cant triangulate with less than 3 points");
            return null;
        }
        //DrawSectorBorders();
        //void DrawSectorBorders() {

        //    for (int x = 0; x < mapSize / sectorSize + 1; x++) {

        //        Vector2 point1x = new Vector2(xectorSize,0);
        //        Vector2 point2x = new Vector2(x * sectorSize,mapSize);
        //        Debug.DrawLine(point1x,point2x,Color.yellow);

        //        for (int y = 0; y < mapSize / sectorSize + 1; y++) {
        //            Vector2 point1y = new Vector2(0,y * sectorSize);
        //            Vector2 point2y = new Vector2(mapSize,y * sectorSize);
        //            Debug.DrawLine(point1y,point2y,Color.yellow);
        //        }
        //    }
        //}

        HashSet<NavTri> explored = new HashSet<NavTri>();
        List<NavTri> badTris = new List<NavTri>();
        Queue<NavTri> queue = new Queue<NavTri>();

        HashSet<NavTri>[,] sectorMap = new HashSet<NavTri>[mapSize/sectorSize+1,mapSize / sectorSize+1];
        for(int x = 0; x <= mapSize / sectorSize; x++) {
            for(int y = 0; y <= mapSize / sectorSize; y++) {
                sectorMap[x,y] = new HashSet<NavTri>();
            }
        }

        edgeMap = edgeMap ?? new Dictionary<EdgePair,HalfEdge>();
        short superNum = 10000;
        HashSet<NavTri> tris = new HashSet<NavTri>();

        Vector2[] superPoints = {
            new Vector2(superNum,superNum),
            new Vector2(-superNum * 3,superNum),
            new Vector2(superNum,-superNum * 3)
        };

        HalfEdge e1 = new HalfEdge();
        HalfEdge e2 = new HalfEdge();
        HalfEdge e3 = new HalfEdge();

        NavTri face = new NavTri();
        face.e1 = e1;

        e1.start = superPoints[0];
        e1.next = e2;
        e1.face = face;
        e2.start = superPoints[1];
        e2.next = e3;
        e2.face = face;
        e3.start = superPoints[2];
        e3.next = e1;
        e3.face = face;

        tris.Add(face);

        AddRemoveFromTriSectors(face,true);

        int tracker = 0;

        for (int i = 0; i < verts.Count;i++) {

            Vector2 currPoint = verts[i];

            // find bad triangles, add them to an array, then remove them from triangles
            // loop backwards so we can removeAt with impunity

            NavTri firstBadTri = GetBadTriFromGrid(currPoint);

            GetBadTris(firstBadTri,currPoint);            

            for(int j = 0; j < badTris.Count; j++) {
                AddRemoveFromTriSectors(badTris[j],false);
                tris.Remove(badTris[j]);
            }

            if (badTris.Count == 0) {
                Debug.LogErrorFormat("bad tris empty, loop: {0}",tracker);
                return tris.ToList();
            }

            //HashSet<EdgePair> oldEdges = new HashSet<EdgePair>();
            //HashSet<EdgePair> oldEdges = new HashSet<EdgePair>();
            Dictionary<EdgePair,HalfEdge> oldEdges = new Dictionary<EdgePair,HalfEdge>();

            // find the boundary of the polygonal hole  
            for (int j = 0; j < badTris.Count(); j++) {
                HalfEdge curr = badTris[j].e1;
                for (int n = 0; n < 3; n++) {
                    EdgePair e = new EdgePair(curr.start,curr.next.start);
                    if (oldEdges.ContainsKey(e)) {
                        oldEdges[e] = null;
                    } else {
                        oldEdges.Add(e,curr);
                    }
                    curr = curr.next;
                }
            }

            Dictionary<Vector2,HalfEdge> polygon = new Dictionary<Vector2,HalfEdge>();
            //int reuseCounter = 0;
            // get rid of edges with duplicates
            // create link map
            // create new triangles
            foreach (KeyValuePair<EdgePair,HalfEdge> edge in oldEdges) {
                // if edge is unique
                if (edge.Value != null) {

                    // make new triangle (with unset pairs for eB and eC)
                    // eC is the edge that goes from the new point back to the unqiue edges
                    HalfEdge eB = new HalfEdge();
                    HalfEdge eC = new HalfEdge();
                    // 01/01/2019
                    NavTri newFace = new NavTri();

                    HalfEdge.set(eB,edge.Value.next.start,eC,null,newFace);
                    HalfEdge.set(eC,currPoint,edge.Value,null,newFace);

                    newFace.e1 = edge.Value;
                    edge.Value.face = newFace;
                    edge.Value.next = eB;

                    polygon.Add(edge.Value.start,edge.Value);
                    tris.Add(newFace);
                    AddRemoveFromTriSectors(newFace,true);

                    // need to manage adding new tris to tris
                    // we already removed the original polygon hole triangles when we checked for bad triangles
                    // so all we need to do is add new ones!
                    // we should make a new navTri yadda yadda

                    // -------------

                    //if(reuseCounter < badTris.Count) {

                    //    // ! Problem ! 
                    //    // edge.value might have been modified in one of the previous loops!

                    //    NavTri b = badTris[reuseCounter];
                    //    b.e1.pair = edge.Value.pair;
                    //    b.e1.pair.pair = b.e1;
                    //    b.e1.start = edge.Value.start;
                    //    b.e1.next.start = edge.Value.next.start;
                    //    b.e1.next.next.start = v;
                    //    reuseCounter++;
                    //    polygon.Add(edge.Value.start,b.e1);
                    //} else {

                    //}
                }
            }

            if (polygon.Count == 0) {
                Debug.LogError("empty polygon");
            }

            // link pairs
            //var first = polygon.First();
            HalfEdge currEdge = polygon.FirstOrDefault().Value;
            //Debug.Log(currEdge);
            //Debug.Log(currEdge.next);
            //Debug.Log(currEdge.next.start);
            HalfEdge nextEdge = polygon[currEdge.next.start];
            for (int j = 0; j < polygon.Count; j++) {
                currEdge.next.pair = nextEdge.next.next;
                nextEdge.next.next.pair = currEdge.next;
                currEdge = nextEdge;
                nextEdge = polygon[currEdge.next.start];
            }

            tracker++;
        }

        // done inserting points, now clean up
        // remove every triangle that has a vertex of the original super triangle
        // also record a map of the outer edges of the final polygon with their pair edges

        //Dictionary<EdgePair,HalfEdge> pairMap = new Dictionary<EdgePair,HalfEdge>();
        //HashSet<NavTri> toRemove = new HashSet<NavTri>();

        tris.RemoveWhere(delegate (NavTri t) {
            HalfEdge curr = t.e1;
            for (int n = 0; n < 3; n++) {
                if (curr.start == superPoints[0] || curr.start == superPoints[1] || curr.start == superPoints[2]) {
                    if (curr.next.start == superPoints[0] || curr.next.start == superPoints[1] || curr.next.start == superPoints[2]) {
                        return true;
                    }
                    if (curr.prev().start == superPoints[0] || curr.prev().start == superPoints[1] || curr.prev().start == superPoints[2]) {
                        return true;
                    }

                    try {
                        edgeMap.Add(new EdgePair(curr.next.start,curr.next.next.start),curr.next.pair);
                    } catch (Exception e) {
                        Debug.DrawLine(curr.next.start,curr.next.next.start,Color.red);
                        Debug.LogError(e.ToString());
                    }


                    curr.next.pair.pair = null;
                    return true;
                }
                curr = curr.next;
            }
            return false;
        });
               
        for (int i = tris.Count - 1; i >= 0; i--) {
            NavTri t = tris.ElementAt(i);

        }
        return tris.ToList();

        void AddRemoveFromTriSectors(NavTri t,bool add) {
            Tuple<Vector2,Vector2> minMax = Utils.GetBoundingBox(t.Points());
            Vector2 min = minMax.Item1;
            Vector2 max = minMax.Item2;

            if (min.x < 0)
                min.x = 0;
            if (min.y < 0)
                min.y = 0;

            int xStart = (min.x < 0) ? (int)min.x / sectorSize : 0;
            int x0 = mapSize / sectorSize;
            int x1 = (int)max.x / sectorSize;
            int xEnd = (x1 > x0)  ? x0 : x1 ;

            int yStart = (min.y < 0) ? (int)min.y / sectorSize : 0;
            x1 = (int)max.y / sectorSize;
            int yEnd = (x1 > x0) ? x0 : x1;

            for(int x = xStart; x <= xEnd; x++) {
                for(int y = yStart; y <= yEnd; y++) {
                    if (add)
                        sectorMap[x,y].Add(t);
                    else
                        sectorMap[x,y].Remove(t);
                }
            }
        }

        void GetBadTris(NavTri t,Vector2 p) {
            explored.Clear();
            badTris.Clear();
            queue.Clear();
            queue.Enqueue(t);

            int counter = 0;
            while(queue.Count != 0) {
                NavTri t2 = queue.Dequeue();

                if (DelaunayInvalid(t2,p)) {
                    badTris.Add(t2);

                    foreach (HalfEdge h in t2.Edges()) {
                        if (h.pair == null || explored.Contains(h.pair.face))
                            continue;

                        queue.Enqueue(h.pair.face);
                    }
                }

                explored.Add(t2);
                if (Utils.WhileCounterIncrementAndCheck(ref counter))
                    break;
            }
        }

        NavTri GetBadTriFromGrid(Vector2 v) {
            int x = (int)v.x / sectorSize;
            int y = (int)v.y / sectorSize;
            foreach(NavTri t in sectorMap[x,y]) {
                if (DelaunayInvalid(t,v))
                    return t;
            }
            Debug.LogErrorFormat("couldn't find tri from point in BW triangulation, loop {0}",tracker);
            foreach (NavTri t in tris)
                NavUtils.DrawLines(t,Color.grey);
            Utils.debugStarPoint(v,1f,Color.red);
            return null;
        }
    }

    public static void BowyerWatsonMultithread(Queue<Vector2> points,NavMeshData data) {

        if (points.Count < 3) {
            Debug.LogError("cant triangulate with less than 3 points");
            return;
        }

        short superNum = 1000;
        Vector2[] superPoints = {
            new Vector2(superNum,superNum),
            new Vector2(-superNum * 3,superNum),
            new Vector2(superNum,-superNum * 3)
        };

        data.AddTri(superPoints,new uint?[] { null,null,null },new short[3]);

        uint threadCount = 6;

        for (uint i = 0; i < threadCount; i++) {
            if (points.Count == 0)
                break;
        }

    }
    
    public static List<NavTri> BowyerWatsonOld(HashSet<Vector2> verts,Dictionary<EdgePair,HalfEdge> pairMap = null) {

        if(verts.Count < 3) {
            Debug.LogError("cant triangulate with less than 3 points");
            return null;
        }

        pairMap = pairMap ?? new Dictionary<EdgePair,HalfEdge>();
        short superNum = 1000;
        List<NavTri> tris = new List<NavTri>();

        Vector2[] superPoints = {
            new Vector2(superNum,superNum),
            new Vector2(-superNum * 3,superNum),
            new Vector2(superNum,-superNum * 3)
        };

        HalfEdge e1 = new HalfEdge();
        HalfEdge e2 = new HalfEdge();
        HalfEdge e3 = new HalfEdge();

        NavTri face = new NavTri();
        face.e1 = e1;

        e1.start = superPoints[0];
        e1.next = e2;
        e1.face = face;
        e2.start = superPoints[1];
        e2.next = e3;
        e2.face = face;
        e3.start = superPoints[2];
        e3.next = e1;
        e3.face = face;

        tris.Add(face);

        int tracker = 0;

        foreach (Vector2 v in verts) {

            // find bad triangles, add them to an array, then remove them from triangles
            // loop backwards so we can removeAt with impunity
            List<NavTri> badTris = new List<NavTri>();
            for (int i = tris.Count - 1; i >= 0; i--) {
                //NavTri test = tris[i];
                //if (clockwiseCheck(tris[i])) {
                //    Debug.LogError("found clockwise tri");
                //}

                if (DelaunayInvalid(tris[i],v)) {
                    badTris.Add(tris[i]);
                    tris.RemoveAt(i);
                }
            }

            if (badTris.Count == 0) {
                Debug.LogError("bad tris empty");
                return tris;
            }

            //HashSet<EdgePair> oldEdges = new HashSet<EdgePair>();
            //HashSet<EdgePair> oldEdges = new HashSet<EdgePair>();
            Dictionary<EdgePair,HalfEdge> oldEdges = new Dictionary<EdgePair,HalfEdge>();

            // find the boundary of the polygonal hole  
            for (int i = 0; i < badTris.Count(); i++) {
                HalfEdge curr = badTris[i].e1;
                for (int n = 0; n < 3; n++) {
                    EdgePair e = new EdgePair(curr.start,curr.next.start);
                    if (oldEdges.ContainsKey(e)) {
                        oldEdges[e] = null;
                    } else {
                        oldEdges.Add(e,curr);
                    }
                    curr = curr.next;
                }
            }

            Dictionary<Vector2,HalfEdge> polygon = new Dictionary<Vector2,HalfEdge>();
            // get rid of edges with duplicates
            // create link map
            // create new triangles
            foreach (KeyValuePair<EdgePair,HalfEdge> edge in oldEdges) {
                // if edge is unique
                if (edge.Value != null) {
                    // make new triangle (with unset pairs for eB and eC)
                    // eC is the edge that goes from the new point back to the unqiue edges
                    HalfEdge eB = new HalfEdge();
                    HalfEdge eC = new HalfEdge();
                    // 01/01/2019
                    NavTri newFace = new NavTri();

                    HalfEdge.set(eB,edge.Value.next.start,eC,null,newFace);
                    HalfEdge.set(eC,v,edge.Value,null,newFace);

                    newFace.e1 = edge.Value;
                    edge.Value.face = newFace;
                    edge.Value.next = eB;

                    polygon.Add(edge.Value.start,edge.Value);
                    tris.Add(newFace);
                    // need to manage adding new tris to tris
                    // we already removed the original polygon hole triangles when we checked for bad triangles
                    // so all we need to do is add new ones!
                    // we should make a new navTri yadda yadda
                }
            }

            if (polygon.Count == 0) {
                Debug.LogError("empty polygon");
            }

            // link pairs
            //var first = polygon.First();
            HalfEdge currEdge = polygon.FirstOrDefault().Value;
            //Debug.Log(currEdge);
            //Debug.Log(currEdge.next);
            //Debug.Log(currEdge.next.start);
            HalfEdge nextEdge = polygon[currEdge.next.start];
            for (int i = 0; i < polygon.Count; i++) {
                currEdge.next.pair = nextEdge.next.next;
                nextEdge.next.next.pair = currEdge.next;
                currEdge = nextEdge;
                nextEdge = polygon[currEdge.next.start];
            }

            tracker++;
        }

        // done inserting points, now clean up
        // remove every triangle that has a vertex of the original super triangle
        // also record a map of the outer edges of the final polygon with their pair edges

        //Dictionary<EdgePair,HalfEdge> pairMap = new Dictionary<EdgePair,HalfEdge>();
        for (int i = tris.Count - 1; i >= 0; i--) {
            HalfEdge curr = tris.ElementAt(i).e1;
            for (int n = 0; n < 3; n++) {
                if (curr.start == superPoints[0] || curr.start == superPoints[1] || curr.start == superPoints[2]) {
                    if (curr.next.start == superPoints[0] || curr.next.start == superPoints[1] || curr.next.start == superPoints[2]) {
                        tris.RemoveAt(i);
                        break;
                    }

                    try {
                        pairMap.Add(new EdgePair(curr.next.start,curr.next.next.start),curr.next.pair);
                    } catch (Exception e) {
                        Debug.DrawLine(curr.next.start,curr.next.next.start,Color.red);
                        Debug.LogError(e.ToString());
                    }

                    
                    curr.next.pair.pair = null;
                    tris.RemoveAt(i);
                    break;
                }
                curr = curr.next;
            }
        }
        return tris;
    }

    static bool DelaunayInvalid(NavTri t,Vector2 p) {

        // if the determinant is greater than zero
        // the point is inside the circumcircle
        return (PointInsideCircumcircle(t[0].start,t[1].start,t[2].start,p));
    }

    public static bool PointInsideCircumcircle(Vector2 e1,Vector2 e2,Vector2 e3,Vector2 p) {

        // e1, e2, e2 must be in counter clockwise order

        //if (Calc.clockwiseCheck(e1,e2,e3) >= 0)
        //    Debug.LogError("non counter clockwise points in delaunay check");


        Vector2 s = e1;

        float a = s.x - p.x;
        float b = s.y - p.y;
        float c = a * a + b * b;

        s = e2;

        float d = s.x - p.x;
        float e = s.y - p.y;
        float f = d * d + e * e;

        s = e3;

        float g = s.x - p.x;
        float h = s.y - p.y;
        float i = g * g + h * h;

        float determinant = a * e * i + b * f * g + c * d * h - c * e * g - b * d * i - a * f * h;

        // if the determinant is greater than zero
        // the point is inside the circumcircle
        return (determinant > 0);
    }

    public static bool DelaunayLineCheck(NavTri t,int l) {
        Vector2 a = t[0].start;
        Vector2 b = t[1].start;
        Vector2 c = t[2].start;
        float A = new Matrix4x4(
            new Vector4(a.y, a.x * a.x + a.y * a.y, 1, 0),
            new Vector4(b.y, b.x * b.x + b.y * b.y, 1, 0),
            new Vector4(c.y, c.x * c.x + c.y * c.y, 1, 0),
            new Vector4(0  , 0                    , 0, 1)).determinant;

        float G = new Matrix4x4(
            new Vector4(a.x, a.y, 1, 0),
            new Vector4(b.x, b.y, 1, 0),
            new Vector4(c.x, c.y, 1, 0),
            new Vector4(0  , 0  , 0, 1)).determinant;

        float B = new Matrix4x4(
            new Vector4(a.x,a.x * a.x + a.y * a.y,1,0),
            new Vector4(b.x,b.x * b.x + b.y * b.y,1,0),
            new Vector4(c.x,c.x * c.x + c.y * c.y,1,0),
            new Vector4(0,0,0,1)).determinant;

        float D = new Matrix4x4(
            new Vector4(a.x,a.y,a.x * a.x + a.y * a.y,0),
            new Vector4(b.x,b.y,b.x * b.x + b.y * b.y,0),
            new Vector4(c.x,c.y,c.x * c.x + c.y * c.y,0),
            new Vector4(0,0,0,1)).determinant;

        return B * B - 4 * G * (G * l * l + A * l - D) > 0;

        //if (xAxis) {
        //    return B * B - 4 * G * (G * l * l + A * l - D) > 0;
        //} else {

        //}

        //throw new NotImplementedException();
    }

    public static List<NavTri> DeWall() {

        throw new NotImplementedException();
    }
    
    static List<NavTri> DeWall(HashSet<Vector2> points, List<EdgePair> edges, bool xAxis) {

        Queue<EdgePair> edgesA = new Queue<EdgePair>();
        List<EdgePair> edges1 = new List<EdgePair>();
        List<EdgePair> edges2 = new List<EdgePair>();

        float a = 0;

        HashSet<Vector2> points1 = new HashSet<Vector2>();
        HashSet<Vector2> points2 = new HashSet<Vector2>();

        foreach(EdgePair e in edges) {
            if (Intersects(e,a,xAxis))
                edgesA.Enqueue(e);
            if (points1.Contains(e.a) || points1.Contains(e.b))
                edges1.Add(e);
            if (points2.Contains(e.a) || points1.Contains(e.b))
                edges2.Add(e);
        }

        List<NavTri> tris = new List<NavTri>();
                
        EdgePair ep;
        NavTri t;

        int counter = 0;
        while (edgesA.Count != 0) {

            ep = edgesA.Dequeue();
            t = MakeTri(ep,points);

            if(t != null) {
                tris.Add(t);

                for(int i = 0; i < 3; i++) {

                }
            }

            if (Utils.WhileCounterIncrementAndCheck(ref counter))
                break;
        }

        if(edges1.Count != 0) {
            tris.Union(DeWall(points1,edges1,!xAxis));
        }
        if (edges2.Count != 0) {
            tris.Union(DeWall(points2,edges2,!xAxis));
        }

        return tris;
    }

    public static void MergeChunks(List<NavTri> trisA,Dictionary<EdgePair,HalfEdge> pairMapA,List<NavTri> trisB,Dictionary<EdgePair,HalfEdge> pairMapB,bool xAxis,bool draw = false) {

        //List<NavTri> unionTris = new List<NavTri>();
        if (draw) {
            foreach (NavTri t in trisA)
                NavUtils.DrawLines(t,Color.blue);

            foreach (NavTri t in trisB)
                NavUtils.DrawLines(t,Color.blue);
        }

        Queue<NavTri> unlinkedA = new Queue<NavTri>();
        Queue<NavTri> unlinkedB = new Queue<NavTri>();

        HalfEdge edgeA;
        HalfEdge edgeB;

        // only used if a tri is deleted and breaks the ability to get next null pair forwards/backwards
        //HalfEdge nextEdgeA = null;
        //HalfEdge nextEdgeB = null;
        Stack<HalfEdge> nextEdgesA = new Stack<HalfEdge>();
        Stack<HalfEdge> nextEdgesB = new Stack<HalfEdge>();

        if (xAxis) {
            FindFirstEdgesXAxis();
        } else {
            FindFirstEdgesYAxis();
        }
        
        HalfEdge prev = null;
        HalfEdge candidateA = GetCandidateA();
        HalfEdge candidateB = GetCandidateB();

        NavTri newTri;

        //yield return null;

        int tracker = 0;
        while (candidateA != null || candidateB != null) {

            if (draw) {
                foreach (NavTri t in trisA)
                    NavUtils.DrawLines(t,Color.blue);

                foreach (NavTri t in trisB)
                    NavUtils.DrawLines(t,Color.blue);
            }

            if (draw) {
                Debug.DrawLine(edgeA.start,edgeA.next.start,Color.red);
                Debug.DrawLine(edgeB.start,edgeB.next.next.start,CustomColors.orange);
            }

            newTri = GetNewTri();

            if (candidateA != null && candidateB != null) {
                if (!PointInsideCircumcircle(candidateA.start,edgeA.start,edgeB.start,candidateB.start)) {
                    NewTriFromA();        
                } else {
                    NewTriFromB();
                }
            } else {
                if (candidateA != null) {
                    NewTriFromA();
                } else {
                    NewTriFromB();
                }
            }

            candidateA = GetCandidateA();
            candidateB = GetCandidateB();

            if (Utils.WhileCounterIncrementAndCheck(ref tracker,5000)) {
                Debug.DrawLine(edgeA.start,edgeB.start,Color.yellow);
                Debug.DrawLine(edgeA.start,edgeA.next.start,Color.red);
                Debug.DrawLine(edgeB.start,edgeB.next.next.start,CustomColors.orange);
                break;
                ;
            }

            //yield return null;
        };

        prev.pair = null;
        pairMapA.Add(prev.GetEdgePair(),prev);

        RemoveExtraUnlinkedTris();

        //trisA.Union(unionTris);
        trisA.AddRange(trisB);

        foreach (KeyValuePair<EdgePair,HalfEdge> kvp in pairMapB) {
            pairMapA.Add(kvp.Key,kvp.Value);
        }

        //TriangulationTestManager.listReturn = trisA;
        //TriangulationTestManager.mapReturn = pairMapA;


        //if (draw) {
        //    foreach (NavTri t in trisA)
        //        Utilities.DrawLines(t,Color.blue);

        //    foreach (NavTri t in trisB)
        //        Utilities.DrawLines(t,Color.blue);
        //}

        //yield return null;

        //yield return null;

        ///

        void FindFirstEdgesXAxis() {
            FindLowestEdgesXAxis();
            EnsureConvex();

            edgeB = edgeB.next;
        }

        ///

        void FindFirstEdgesYAxis() {

            FindLowestEdgesYAxis();
            EnsureConvex();

            edgeB = edgeB.next;
        }

        ///

        void FindLowestEdgesXAxis() {
            edgeA = pairMapA.FirstOrDefault().Value;
            Vector2 curr = edgeA.start;

            foreach (KeyValuePair<EdgePair,HalfEdge> kvp in pairMapA) {
                if (kvp.Value.start.x > curr.x) {
                    edgeA = kvp.Value;
                    curr = kvp.Value.start;
                } else if (kvp.Value.start.x == curr.x && kvp.Value.start.y < curr.y) {
                    edgeA = kvp.Value;
                    curr = kvp.Value.start;
                }
                //Debug.DrawLine(kvp.Key.a,kvp.Key.b,Color.white);
            }

            edgeB = pairMapB.FirstOrDefault().Value;
            curr = edgeB.next.start;

            foreach (KeyValuePair<EdgePair,HalfEdge> kvp in pairMapB) {
                if (kvp.Value.next.start.x > curr.x) {
                    edgeB = kvp.Value;
                    curr = kvp.Value.next.start;
                } else if (kvp.Value.next.start.x == curr.x && kvp.Value.next.start.y > curr.y) {
                    edgeB = kvp.Value;
                    curr = kvp.Value.next.start;
                }
                //Debug.DrawLine(kvp.Key.a,kvp.Key.b,Color.white);
            }
        }

        ///

        void FindLowestEdgesYAxis() {
            edgeA = pairMapA.FirstOrDefault().Value;
            Vector2 curr = edgeA.start;

            foreach (KeyValuePair<EdgePair,HalfEdge> kvp in pairMapA) {
                if (kvp.Value.start.y < curr.y) {
                    edgeA = kvp.Value;
                    curr = kvp.Value.start;
                } else if (kvp.Value.start.y == curr.y && kvp.Value.start.x < curr.x) {
                    edgeA = kvp.Value;
                    curr = kvp.Value.start;
                }
                //Debug.DrawLine(kvp.Key.a,kvp.Key.b,Color.white);
            }

            edgeB = pairMapB.FirstOrDefault().Value;
            curr = edgeB.next.start;

            foreach (KeyValuePair<EdgePair,HalfEdge> kvp in pairMapB) {
                if (kvp.Value.next.start.y < curr.y) {
                    edgeB = kvp.Value;
                    curr = kvp.Value.next.start;
                } else if (kvp.Value.next.start.y == curr.y && kvp.Value.next.start.x > curr.x) {
                    edgeB = kvp.Value;
                    curr = kvp.Value.next.start;
                }
                //Debug.DrawLine(kvp.Key.a,kvp.Key.b,Color.white);
            }
        }

        ///

        void EnsureConvex() {
            bool equal = (xAxis) ? edgeA.start.x == edgeB.next.start.x : edgeA.start.y == edgeB.next.start.y;
            bool aLower = (xAxis) ? edgeA.start.x > edgeB.next.start.x : edgeA.start.y < edgeB.next.start.y;

            if (equal) {

                // advance together
                while (Calc.ClockWiseCheck(edgeA.start,edgeA.next.start,edgeB.next.start) <= 0) {
                    if (edgeA.next.pair != null) {
                        edgeA = HalfEdge.NextNullPairForwards(edgeA);
                    } else {
                        edgeA = edgeA.next;
                    }
                }

                while (Calc.ClockWiseCheck(edgeA.start,edgeB.start,edgeB.next.start) <= 0) {
                    if (edgeB.next.next.pair != null) {
                        edgeB = HalfEdge.NextNullPairBackwards(edgeB);
                    } else {
                        edgeB = edgeB.next.next;
                    }
                }

            } else if (aLower) {

                // edgeA-edgeB connection goes through polyA
                // advance forwards until fixed
                while (Calc.ClockWiseCheck(edgeA.start,edgeA.next.start,edgeB.next.start) <= 0) {
                    if (edgeA.next.pair != null) {
                        edgeA = HalfEdge.NextNullPairForwards(edgeA);
                    } else {
                        edgeA = edgeA.next;
                    }
                }

                // try to go backwards on upper poly edge as far as possible
                HalfEdge prevB;
                if (edgeB.next.pair != null) {
                    prevB = HalfEdge.NextNullPairForwards(edgeB);
                } else {
                    prevB = edgeB.next;
                }

                while (Calc.ClockWiseCheck(edgeA.start,prevB.start,prevB.next.start) > 0) {
                    edgeB = prevB;

                    if (edgeB.next.pair != null) {
                        prevB = HalfEdge.NextNullPairForwards(edgeB);
                    } else {
                        prevB = edgeB.next;
                    }
                }

            } else {
                // edgeA.y > edgeB.y
                //Debug.Log("ensure convex, B is lower");
                // edgeA-edgeB connection goes through polyA
                while (Calc.ClockWiseCheck(edgeA.start,edgeB.start,edgeB.next.start) <= 0) {
                    if (edgeB.next.next.pair != null) {
                        edgeB = HalfEdge.NextNullPairBackwards(edgeB);
                    } else {
                        edgeB = edgeB.next.next;
                    }
                }

                HalfEdge prevA;
                if (edgeA.next.next.pair != null) {
                    prevA = HalfEdge.NextNullPairBackwards(edgeA);
                } else {
                    prevA = edgeA.next.next;
                }

                while (Calc.ClockWiseCheck(prevA.start,prevA.next.start,edgeB.next.start) > 0) {
                    edgeA = prevA;

                    if (edgeA.next.next.pair != null) {
                        prevA = HalfEdge.NextNullPairBackwards(edgeA);
                    } else {
                        prevA = edgeA.next.next;
                    }
                }
            }
        }

        ///

        HalfEdge GetCandidateA() {

            if(edgeA == null) {
                Debug.LogError("edgeA is null");
                return null;
            }

            HalfEdge candidate;
            HalfEdge nextCandidate = null;
            bool b;
            int counter = 0;
            do {
                candidate = edgeA.next;

                if (Calc.ClockWiseCheck(candidate.start,edgeA.start,edgeB.start) >= 0) {

                    //Utilities.debugStarPoint(edgeA.start,0.2f,Color.red);
                    return null;
                }

                // this case is if so many tris have been removed, that EdgeA is close to looping back onto the previous edgeA
                // meaning we have "ran out" of nextCandidate tris

                if (candidate.next.pair != null) {
                    nextCandidate = candidate.next.pair.next;
                } else {
                    return candidate;
                }

                //nextCandidate = candidate.next.pair.next;

                //if (candidate.next.pair != null) {
                //    nextCandidate = candidate.next.pair.next;
                //} else {
                //    Debug.DrawLine(edgeA.start,edgeA.next.start,Color.red);
                //    Debug.DrawLine(candidate.next.start,candidate.next.next.start,Color.cyan);
                //    return null;
                //}

                // remove tri if next candidate lies inside circumcircle A-B-candidate
                if (b = PointInsideCircumcircle(edgeA.start,edgeB.start,candidate.start,nextCandidate.start)) {

                    if (edgeA.next.pair == null) {
                        Utils.debugStarPoint((edgeA.next.start + edgeA.next.next.start)/2,1f,Color.yellow);
                        return candidate;
                    }

                    TryRemoveAFromMap();
                    nextEdgesA.Push(edgeA.next.pair);
                    //nextEdgeA = edgeA.next.pair;
                    edgeA = edgeA.next.next.pair;                    
                    UnlinkTri(edgeA.pair.face,true);
                }

                if (Utils.WhileCounterIncrementAndCheck(ref counter))
                    break;
            } while (b);

            return candidate;            
        }

        HalfEdge GetCandidateB() {

            if (edgeB == null) {
                Debug.LogError("edgeA is null");
                return null;
            }

            HalfEdge candidate;
            HalfEdge nextCandidate = null;
            bool b;
            int counter = 0;
            do {
                candidate = edgeB.next.next;

                if (Calc.ClockWiseCheck(edgeA.start,edgeB.start,candidate.start) >= 0) {
                    //pairMapA.Add(new EdgePair(prev.start,prev.next.start),prev);
                    //Utilities.debugStarPoint(edgeB.start,0.2f,Color.red);

                    return null;
                }

                // this case is if so many tris have been removed, that EdgeB is close to looping back onto the previous edgeA
                // meaning we have "ran out" of nextCandidate tris

                if (candidate.next.pair != null) {
                    nextCandidate = candidate.next.pair;
                } else {
                    return candidate;
                }

                //nextCandidate = edgeB.pair;
                               
                if (b = PointInsideCircumcircle(edgeA.start,edgeB.start,candidate.start,nextCandidate.start)) {

                    // DIRTY FIX
                    // CAN RESULT IN NON-DELAUNAY
                    if (edgeB.next.pair == null ) {
                        // uh oh
                        // we might "lose" a vert here
                        return candidate;
                        //Debug.DrawLine(edgeB.next.start,edgeB.next.next.start,Color.yellow);
                        //Debug.LogError("Edge is null? " + edgeB.next.start + " " + edgeB.next.next.start);
                    }

                    TryRemoveBFromMap();
                    //nextEdgeB = edgeB.next.pair.next;
                    nextEdgesB.Push(edgeB.next.pair.next);
                    edgeB = edgeB.pair.next;
                    UnlinkTri(edgeB.next.next.pair.face,false);
                }
                if (Utils.WhileCounterIncrementAndCheck(ref counter)) {
                    ;
                    break;
                }

            } while (b);

            return candidate;
        }

        void UnlinkTri(NavTri t, bool listA) {

            if(t == null) {
                Debug.LogError("trying to unlink null tri??");
                if (listA) {
                    Debug.DrawLine(edgeA.start,edgeA.next.start,Color.yellow);
                } else {
                    Debug.DrawLine(edgeB.start,edgeB.next.start,Color.yellow);
                }
            }

            if (draw) {
                Utils.debugStarPoint(t.centroid(),0.1f,Color.magenta);
                NavUtils.DrawLines(t,Color.magenta);
            }

            for (int i = 0; i < 3; i++) {
                if (t[i].pair != null)
                    t[i].pair.pair = null;
            }
            if (listA) {
                unlinkedA.Enqueue(t);
            } else {
                unlinkedB.Enqueue(t);
            }            
        }

        NavTri GetNewTri() {
            if(unlinkedA.Count != 0) {
                return unlinkedA.Dequeue();
            } else if (unlinkedB.Count != 0) {
                return unlinkedB.Dequeue();
            } else {
                NavTri t = new NavTri();
                t.e1 = new HalfEdge();
                t.e1.next = new HalfEdge();
                t.e1.next.next = new HalfEdge();
                t.e1.next.next.next = t.e1;

                t[0].face = t;
                t[1].face = t;
                t[2].face = t;

                newTri = t;

                trisA.Add(newTri);

                return t;
            }
        }

        void RemoveExtraUnlinkedTris() {
            if (unlinkedA.Count == 0 && unlinkedB.Count == 0) {
                return;
            }
            Debug.LogError("merging tri chunks ended with excessed unlinked tris at "+edgeA.start+" "+edgeB.start);
            Debug.DrawLine(edgeA.start,edgeB.start,Color.yellow);
            int counter = 0;
            while (unlinkedA.Count != 0) {
                trisA.Remove(unlinkedA.Dequeue());
                if (Utils.WhileCounterIncrementAndCheck(ref counter))
                    break;
            }                

            while (unlinkedB.Count != 0) {
                trisA.Remove(unlinkedB.Dequeue());
                if (Utils.WhileCounterIncrementAndCheck(ref counter))
                    break;
            }
        }
        
        void NewTriFromA(){
            newTri[0].start = edgeA.start;
            newTri[1].start = edgeB.start;
            newTri[2].start = edgeA.next.start;
            newTri[2].pair = edgeA;
            edgeA.pair = newTri[2];

            CheckPrevLink();

            prev = newTri[1];
            TryRemoveAFromMap();

            if (draw) {
                Debug.DrawLine(newTri[1].start,newTri[2].start,Color.green);
            }

            //if(nextEdgeA != null) {
            //    edgeA = nextEdgeA;
            //} else if(edgeA.next.pair == null) {
            //    edgeA = edgeA.next;                                
            //} else {
            //    edgeA = HalfEdge.NextNullPairForwards(edgeA);
            //} 

            if (nextEdgesA.Count != 0) {
                edgeA = nextEdgesA.Pop();
            } else if (edgeA.next.pair == null) {
                edgeA = edgeA.next;
            } else {
                edgeA = HalfEdge.NextNullPairForwards(edgeA);
            }

            //nextEdgeA = null;
        }

        void NewTriFromB() {
            
            newTri[0].start = edgeA.start;
            newTri[1].start = edgeB.start;

            newTri[1].pair = edgeB.next.next;
            edgeB.next.next.pair = newTri[1];

            newTri[2].start = edgeB.next.next.start;
            
            CheckPrevLink();

            prev = newTri[2];

            TryRemoveBFromMap();

            if (draw) {
                Debug.DrawLine(newTri[2].start,newTri[0].start,Color.green);
            }

            //if (nextEdgeB != null) {
            //    edgeB = nextEdgeB;
            //} else if (edgeB.next.pair == null) {
            //    edgeB = edgeB.next.next;
            //} else {
            //    HalfEdge e = HalfEdge.NextNullPairBackwards(edgeB.next.next);
            //    edgeB = e.next;
            //}

            if (nextEdgesB.Count != 0) {
                edgeB = nextEdgesB.Pop();
            } else if (edgeB.next.pair == null) {
                edgeB = edgeB.next.next;
            } else {
                HalfEdge e = HalfEdge.NextNullPairBackwards(edgeB.next.next);
                edgeB = e.next;
            }

            //nextEdgeB = null;
        }

        void CheckPrevLink() {
            newTri[0].pair = prev;

            if (prev != null) {                
                prev.pair = newTri[0];
            } else {
                pairMapA.Add(new EdgePair(newTri[0].start,newTri[1].start),newTri[0]);
            }            
        }

        void TryRemoveAFromMap() {
            if (pairMapA.ContainsKey(edgeA.GetEdgePair()))
                pairMapA.Remove(edgeA.GetEdgePair());
        }

        void TryRemoveBFromMap() {
            if (pairMapB.ContainsKey(edgeB.next.next.GetEdgePair()))
                pairMapB.Remove(edgeB.next.next.GetEdgePair());
        }
    }

    static bool Intersects(EdgePair ep, float a, bool xAxis) {
        throw new NotImplementedException();
    }

    static bool VertSetContainsEdge(HashSet<Vector2> set, EdgePair ep) {
        throw new NotImplementedException();
    }

    static NavTri MakeTri(EdgePair ep, HashSet<Vector2> points) {
        throw new NotImplementedException();
    }

    public class NavMeshData {

        const uint expandSize = 5000;

        Vector2[] edgeStart = new Vector2[expandSize];
        uint?[] pair = new uint?[expandSize];
        short[] type = new short[expandSize];
        //uint[] face;
        uint index;

        public void DeleteTri(uint tri) {
            if (tri >= index)
                Debug.LogError("trying to delete a tri with an index greater than mesh data index?");
            
            //if (tri % 3 != 0)
            //    Debug.LogError("Trying to delete a tri with an index not divisible by 3");

            tri *= 3;

            // set edge pair edges to have null pairs
            for(uint i = 0; i < 3; i++) {
                uint? pairEdge = pair[tri + i];
                if(pairEdge != null) {
                    pair[(uint)pairEdge] = null; 
                }
            }

            uint lastTri = (index - 1) * 3;

            // copy last tri into new spot
            for (uint i = 0; i < 3; i++) {
                // update last tri neighbours
                uint? pairEdge = pair[lastTri + i];
                pair[(uint)pairEdge] = tri + i;

                // move data
                edgeStart[tri + i] = edgeStart[lastTri + i];
                pair[tri + i] = pair[lastTri + i];
                type[tri + i] = type[lastTri + i];                
            }

            index--;
        }

        public void AddTri(Vector2[] edgeStartVal,uint?[] pairVal,short[] typeVal) {

            if (edgeStart.Length != 3 || pairVal.Length != 3 || typeVal.Length != 3)
                Debug.LogError("values arrays with non 3 size when making tri");

            if (ExpandCheck())
                ExpandArray();

            for(uint i = 0; i < 3; i++) {
                edgeStart[index + i] = edgeStartVal[i];
                pair[index + i] = pairVal[i];
                type[index + i] = typeVal[i];
            }

            index++;
        }

        bool ExpandCheck() {
            return index == edgeStart.Length;
        }

        void ExpandArray() {
            Vector2[] edgeStartNew = new Vector2[edgeStart.Length + expandSize];
            Array.Copy(edgeStart,edgeStartNew,edgeStart.Length);
            edgeStart = edgeStartNew;

            uint?[] pairNew = new uint?[edgeStart.Length + expandSize];
            Array.Copy(pair,pairNew,pair.Length);
            pair = pairNew;

            short[] typeNew = new short[type.Length + expandSize];
            Array.Copy(type,typeNew,type.Length);
            type = typeNew;
        }

        public Vector2 GetEdgeStart(uint edge) {
            return edgeStart[edge];
        }

        public void SetEdgeStart(uint edge, Vector2 value) {
            edgeStart[edge] = value;
        }

        public uint? GetEdgePair(uint edge) {
            return pair[edge];
        }

        public void SetEdgePair(uint edge, uint? value) {
            pair[edge] = value;
        }

        public short GetEdgeType(uint edge) {
            return type[edge];
        }

        public void SetEdgeType(uint edge, short value) {
            type[edge] = value;
        }

        public static uint Next(uint edge) {
            return (edge % 3 == 2) ? edge + 1 : edge - 2;
        }

        public static uint Prev(uint edge) {
            return (edge % 3 == 0) ? edge + 2 : edge - 1;
        }
    }

    class NavMeshDataAlt {
        HalfEdgeStruct[] edges;
        uint index;

        NavMeshDataAlt(HalfEdgeStruct[] e, uint i) {
            edges = e;
            index = i;
        }

        public void DeleteTri(uint tri) {
            if (tri >= index)
                Debug.LogError("trying to delete a tri with an index greater than mesh data index?");

            tri *= 3;
            // set edge pair edges to have null pairs
            for (uint i = 0; i < 3; i++) {
                uint? pairEdge = edges[tri + i].pair;
                if (pairEdge != null) {
                    edges[(uint)pairEdge].pair = null;
                }
            }
        }
    }

    struct HalfEdgeStruct {
        public Vector2 edgeStart;
        public uint? pair;
        public short type;
        public uint face;

        public HalfEdgeStruct(Vector2 v, uint? p, short t, uint f) {
            edgeStart = v;
            pair = p;
            type = t;
            face = f;
        }
    }

    class BowyerWatsonThread {
        Vector2 vert;
        HashSet<int> cavaityTris;

        private BowyerWatsonThread() { }

        public BowyerWatsonThread(Vector2 vertVal) {
            vert = vertVal;
        }
    }    
}

public class MeshChunk {

    MapChunked map;

    public const uint ChunkSize = 32;

    public Color c;

    uint x;
    uint y;

    const uint expandSize = 5000;

    Vector2[] edgeStart = new Vector2[expandSize];
    EdgeID[] pair = new EdgeID[expandSize];
    short[] type = new short[expandSize];
    //uint[] face;
    uint index;

    public List<NavTri> tris;
    //public Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2,HalfEdge>();
    public Dictionary<EdgePair,HalfEdge> edgeMap;


    private MeshChunk() { }

    public MeshChunk(MapChunked _map) {
        map = _map;
        edgeMap = new Dictionary<EdgePair,HalfEdge>();
        c = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f));
    }

    public void DeleteTri(uint tri) {
        if (tri >= index)
            Debug.LogError("trying to delete a tri with an index greater than mesh data index?");

        //if (tri % 3 != 0)
        //    Debug.LogError("Trying to delete a tri with an index not divisible by 3");

        tri *= 3;

        // set edge pair edges to have null pairs
        for (uint i = 0; i < 3; i++) {
            EdgeID pairEdge = pair[tri + i];
            if (pairEdge != null) {
                map[pairEdge.chunkX,pairEdge.chunkY].pair[pairEdge.edgeID] = null;
            }
        }

        ushort lastTri = (ushort)((index - 1) * 3);

        // copy last tri into new spot
        for (uint i = 0; i < 3; i++) {
            // update last tri neighbours
            EdgeID pairEdge = pair[lastTri + i];
            map[pairEdge.chunkX,pairEdge.chunkY].pair[pairEdge.edgeID].edgeID = tri + i;

            // move data
            edgeStart[tri + i] = edgeStart[lastTri + i];
            pair[tri + i] = pair[lastTri + i];
            type[tri + i] = type[lastTri + i];
        }

        index--;
    }

    public void AddTri(Vector2[] edgeStartVal,EdgeID[] pairVal,short[] typeVal) {

        if (edgeStart.Length != 3 || pairVal.Length != 3 || typeVal.Length != 3)
            Debug.LogError("values arrays with non 3 size when making tri");

        if (ExpandCheck())
            ExpandArray();

        for (uint i = 0; i < 3; i++) {
            edgeStart[index + i] = edgeStartVal[i];
            pair[index + i] = pairVal[i];
            type[index + i] = typeVal[i];
        }

        index++;
    }

    bool ExpandCheck() {
        return index == edgeStart.Length;
    }

    void ExpandArray() {
        Vector2[] edgeStartNew = new Vector2[edgeStart.Length + expandSize];
        Array.Copy(edgeStart,edgeStartNew,edgeStart.Length);
        edgeStart = edgeStartNew;

        EdgeID[] pairNew = new EdgeID[edgeStart.Length + expandSize];
        Array.Copy(pair,pairNew,pair.Length);
        pair = pairNew;

        short[] typeNew = new short[type.Length + expandSize];
        Array.Copy(type,typeNew,type.Length);
        type = typeNew;
    }

    public Vector2 GetEdgeStart(uint edge) {
        return edgeStart[edge];
    }

    public void SetEdgeStart(uint edge,Vector2 value) {
        edgeStart[edge] = value;
    }

    public EdgeID GetEdgePair(uint edge) {
        return pair[edge];
    }

    public void SetEdgePair(uint edge,EdgeID value) {
        pair[edge] = value;
    }

    public short GetEdgeType(uint edge) {
        return type[edge];
    }

    public void SetEdgeType(uint edge,short value) {
        type[edge] = value;
    }

    public static uint Next(uint edge) {
        return (edge % 3 == 2) ? edge + 1 : edge - 2;
    }

    public static uint Prev(uint edge) {
        return (edge % 3 == 0) ? edge + 2 : edge - 1;
    }
}

public class EdgeID {
    public byte chunkX;
    public byte chunkY;
    public uint edgeID;

    public EdgeID(byte _chunkX, byte _chunkY, uint _edgeID) {
        chunkX = _chunkX;
        chunkY = _chunkY;
        edgeID = _edgeID;
    }
}
