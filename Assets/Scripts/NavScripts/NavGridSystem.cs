using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NavGridSystem 
{
    public static void ChangeTerrain(OldMap map, Island areaToChange,HashSet<EdgePair> islandEdges,bool add = true) {

        Dictionary<uint,uint[]> xScans = new Dictionary<uint,uint[]>();
        Dictionary<uint,uint[]> yScans = new Dictionary<uint,uint[]>();

        HashSet<Vector2> finalPoints = new HashSet<Vector2>();
        TriChunk triChunk = FindIslandTriIntersect(map,areaToChange,islandEdges,xScans,yScans,finalPoints);

        if (triChunk == null) {
            return;
        }

        HashSet<Vector2> oldVerts = triChunk.getVerts();

        uint max = map.size;
        Vector2[] borderVerts = {
            new Vector2(0,0), new Vector2(0,max), new Vector2(max,0), new Vector2(max,max)
        };
        foreach (Vector2 v in borderVerts) {
            if (oldVerts.Contains(v)) {
                finalPoints.Add(v);

            }
        }

        HashSet<EdgePair> newEdges = new HashSet<EdgePair>();
        HashSet<Vector2> cells = new HashSet<Vector2>(map.cells);

        if (add) {
            cells.UnionWith(areaToChange.cells);

        } else {
            cells.ExceptWith(areaToChange.cells);
        }

        foreach (KeyValuePair<uint,uint[]> kvp in xScans) {
            HashSet<EdgePair> edges = MapProcessing.implicitLinearScan(map,kvp.Value[0],kvp.Value[1],kvp.Key);
            if (kvp.Key == 0 || kvp.Key == map.size) {
                foreach (EdgePair ep in edges) {
                    triChunk.border.Add(ep,null);
                }
            }
            newEdges.UnionWith(edges);
        }
        foreach (KeyValuePair<uint,uint[]> kvp in yScans) {
            HashSet<EdgePair> edges = MapProcessing.implicitLinearScan(map,kvp.Value[0],kvp.Value[1],kvp.Key,false);
            if (kvp.Key == 0 || kvp.Key == map.size) {
                foreach (EdgePair ep in edges) {
                    triChunk.border.Add(ep,null);
                }
            }
            newEdges.UnionWith(edges);
        }

        foreach (EdgePair ep in newEdges) {

            if (oldVerts.Contains(ep.a)) {
                oldVerts.Remove(ep.a);
            }

            if (oldVerts.Contains(ep.b)) {
                oldVerts.Remove(ep.b);
            }
        }

        foreach (EdgePair ep in triChunk.border.Keys) {
            if (oldVerts.Contains(ep.a))
                oldVerts.Remove(ep.a);

            if (oldVerts.Contains(ep.b))
                oldVerts.Remove(ep.b);

        }

        foreach (EdgePair e in newEdges) {
            if (!finalPoints.Contains(e.a)) {
                finalPoints.Add(e.a);
            }
            if (!finalPoints.Contains(e.b)) {
                finalPoints.Add(e.b);
            }
        }

        Dictionary<EdgePair,HalfEdge> dict;
        List<NavTri> newTris = Triangulation.BowyerWatson(finalPoints.ToList(), out dict);

        foreach (EdgePair e in islandEdges) {
            Debug.DrawLine(e.a,e.b,CustomColors.purple);
        }

        foreach (NavTri t in newTris) {
            foreach (HalfEdge h in t.Edges()) {
                Debug.DrawLine(h.start,h.next.start,Color.cyan);
            }
        }

        Triangulation.trimWithBorder(newTris,triChunk.border.Keys);

        foreach (EdgePair h in triChunk.border.Keys) {
            if (h == null) {
                continue;
            }
            Debug.DrawLine(h.a,h.b,Color.blue);
        }

        foreach (Vector2 v in finalPoints) {
            Utils.debugStarPoint(v,0.15f,Color.cyan);
        }

        if (Input.GetMouseButtonDown(0)) {

            if (add) {
                map.cells.UnionWith(areaToChange.cells);
            } else {
                map.cells.ExceptWith(areaToChange.cells);
            }

            triChunk.replaceTris(newTris);
            
            map.implicitEdges.UnionWith(newEdges);

            List<EdgePair> orphans = MapProcessing.ProcessMapImplicits(map.navtris,map.implicitEdges,map.vertEdgeMap);
            MapProcessing.fixMapOrphans(map.navtris,orphans,map.vertEdgeMap);
            orphans = MapProcessing.ProcessMapImplicits(map.navtris,map.implicitEdges,map.vertEdgeMap);

            if (orphans.Count != 0) {
                Debug.LogError("change terrain: persistant orphans");

                foreach (EdgePair e in orphans) {
                    HalfEdge h = map.vertEdgeMap[e.a];
                    Debug.DrawLine(h.start,h.next.start,Color.yellow);

                    Utils.debugStarPoint(e.a,0.08f,Color.red);

                    Debug.Log(e.a + " " + e.b);
                    Debug.DrawLine(e.a,e.b,Color.red);
                }
            }

            //Map.updateTilesGraphics(map);

            foreach (Vector2 v in oldVerts) {
                map.vertEdgeMap.Remove(v);
            }

            map.VertKDTree = new KDNode(map.vertEdgeMap.Keys.ToList());
            //TestControl.main.movementManager.TerrainChangePathCheck(triChunk.chunkTris);
        }
    }

    public static TriChunk FindIslandTriIntersect(OldMap map,Island isl,HashSet<EdgePair> islEdges,Dictionary<uint,uint[]> xScans,Dictionary<uint,uint[]> yScans,HashSet<Vector2> finalPoints) {

        //HashSet<Vector2> oldVerts = new HashSet<Vector2>();
        //Dictionary<int,int[]> yScans = new Dictionary<int,int[]>();
        //Dictionary<int,int[]> xScans = new Dictionary<int,int[]>();
        HashSet<HalfEdge> intersectEdges = new HashSet<HalfEdge>();


        TriChunk tc = new TriChunk(map);

        foreach (EdgePair ep in islEdges) {
            addToScanList(ep.a,ep.b,xScans,yScans,map);
        }

        //HashSet<NavTri> exploredTri = new HashSet<NavTri>();
        HashSet<NavTri> frontier = new HashSet<NavTri>();

        //HashSet<Island> adjacentIslands = new HashSet<Island>();

        //Dictionary<HalfEdge,HalfEdge> border = new Dictionary<HalfEdge,HalfEdge>();

        EdgePair first = islEdges.ElementAt(0);

        NavTri firstTri = map.triFromPoint(first.a + new Vector2(0.0013f,0.00079f));

        //if (firstTri == null && current_map.vertEdgeMap.ContainsKey(first.a)) {
        //    firstTri = current_map.vertEdgeMap[first.a].face;
        //}

        if (firstTri == null) {




            //Debug.Log("No first tri found");
            Utils.debugStarPoint(first.a,0.15f,Color.cyan);
            return null;
        }



        frontier.Add(firstTri);

        int counter = 0;
        while (frontier.Count != 0) {
            NavTri curr = frontier.First();

            foreach (HalfEdge h in MapProcessing.TriEdges(curr)) {

                if (h.pair == null) {

                    Utils.debugStarPoint(h.face.centroid(),0.15f,CustomColors.orange);


                    if (h.start.x == h.next.start.x) {
                        if (h.start.x != 0 && h.start.x != map.size) {
                            Debug.LogError("null pair edge not on edge of map?");
                        }
                    } else if (h.start.y == h.next.start.y) {
                        if (h.start.y != 0 && h.start.y != map.size) {
                            Debug.LogError("null pair edge not on edge of map?");
                        }
                    } else {
                        Debug.LogError("Map edge that isn't aligned on grid?");
                        Debug.DrawLine(h.start,h.next.start,Color.red);
                        Utils.debugStarPoint(h.start,0.20f,Color.red);
                        Utils.debugStarPoint(h.next.start,0.20f,Color.red);
                    }
                                                                          
                    //tc.AddBorderPair(h,h.pair);
                    addToScanList(h,xScans,yScans,map);
                    if (h.type == 1) {
                        map.implicitEdges.Remove(new EdgePair(h.start,h.next.start));
                    }
                    continue;
                }

                if (h.pair != null && tc.ContainsTri(h.pair.face)) {
                    continue;
                }

                bool intersect = false;
                bool terrain = false;

                foreach (EdgePair e in islEdges) {

                    // || e.a == h.start || e.a == h.next.start || e.b == h.start || e.b == h.next.start
                    if (Calc.DoesLineIntersect(e.a,e.b,h.start,h.next.start)) {
                        intersect = true;
                        if (h.type == 1) {
                            terrain = true;
                            break;
                        }
                    }
                }

                if (intersect) {
                    intersectEdges.Add(h);
                    if (h.pair != null) {
                        frontier.Add(h.pair.face);
                    }

                    if (terrain) {
                        addToScanList(h,xScans,yScans,map);
                        map.implicitEdges.Remove(new EdgePair(h.start,h.next.start));
                    }

                } else {
                    tc.AddBorderPair(h.GetEdgePair(),h.pair);
                    if (!finalPoints.Add(h.start)) { }
                    if (!finalPoints.Add(h.next.start)) { }
                    //Debug.DrawLine(h.start,h.next.start,Color.red);
                    //border.Add(h,h.pair);
                }

            }
            tc.AddTri(curr);
            //exploredTri.Add(curr);
            frontier.Remove(curr);

            if (Utils.WhileCounterIncrementAndCheck(ref counter)) 
                break;            
        }

        return tc;
    }
    
    static void addToScanList(HalfEdge h,Dictionary<uint,uint[]> xScans,Dictionary<uint,uint[]> yScans,OldMap map) {
        addToScanList(h.start,h.next.start,xScans,yScans,map);
    }

    static void addToScanList(EdgePair ep,Dictionary<uint,uint[]> xScans,Dictionary<uint,uint[]> yScans,OldMap map) {
        addToScanList(ep.a,ep.b,xScans,yScans,map);
    }

    static void addToScanList(Vector2 v1,Vector2 v2,Dictionary<uint,uint[]> xScans,Dictionary<uint,uint[]> yScans,OldMap map) {
        if (v1.y == v2.y) {

            if (v1.y > map.size || v1.y < 0) {
                return;
            }

            uint yLevel = (uint)v1.y;

            uint xLow = (uint)((v1.x < v2.x) ? v1.x : v2.x);
            uint xHigh = (uint)((v1.x > v2.x) ? v1.x : v2.x);

            if (xLow < 0)
                xLow = 0;

            if (xHigh > map.size)
                xHigh = map.size;

            if (xScans.ContainsKey(yLevel)) {
                if (xScans[yLevel][0] > xLow) {
                    xScans[yLevel][0] = xLow;
                }
                if (xScans[yLevel][1] < xHigh) {
                    xScans[yLevel][1] = xHigh;
                }
            } else {
                xScans.Add(yLevel,new uint[2] { xLow,xHigh });
            }

        } else if (v1.x == v2.x) {

            if (v1.x > map.size || v1.x < 0) {
                return;
            }

            uint xLevel = (uint)v1.x;

            uint yLow = (uint)((v1.y < v2.y) ? v1.y : v2.y);
            uint yHigh = (uint)((v1.y > v2.y) ? v1.y : v2.y);

            if (yLow < 0)
                yLow = 0;

            if (yHigh > map.size)
                yHigh = map.size;

            if (yScans.ContainsKey(xLevel)) {
                if (yScans[xLevel][0] > yLow) {
                    yScans[xLevel][0] = yLow;
                }
                if (yScans[xLevel][1] < yHigh) {
                    yScans[xLevel][1] = yHigh;
                }
            } else {
                yScans.Add(xLevel,new uint[2] { yLow,yHigh });
            }
        } else {
            Debug.LogError("add to scan list: coords not in line with grid?");
        }
    }

    public static void addEdgePointsToSet(EdgePair e,HashSet<Vector2> set) {
        if (!set.Contains(e.a)) {
            set.Add(e.a);
        }
        if (!set.Contains(e.b)) {
            set.Add(e.b);
        }
    }
}
