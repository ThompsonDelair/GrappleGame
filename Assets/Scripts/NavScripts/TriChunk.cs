using System;
using System.Collections.Generic;
using UnityEngine;

public class TriChunk {
    public List<NavTri> chunkTris = new List<NavTri>();
    public Dictionary<EdgePair,HalfEdge> border = new Dictionary<EdgePair,HalfEdge>();
    OldMap map;

    public TriChunk(OldMap map) {
        this.map = map;
    }

    public bool ContainsTri(NavTri t) {
        return chunkTris.Contains(t);
    }

    public void AddTri(NavTri t) {
        if (!chunkTris.Contains(t)) {
            chunkTris.Add(t);
        }
    }

    public void AddBorderPair(EdgePair inner,HalfEdge outer) {
        try {
            border.Add(inner,outer);
        } catch (Exception e) {
            Debug.DrawLine(inner.a,inner.b,Color.red);
            Debug.LogError("triChunk add border pair:  " + e);
        }
    }

    public void replaceTris(List<NavTri> tris) {
        //TestControl.main.current_map.navtris.ExceptWith(chunkTris);
        //TestControl.main.current_map.navtris.UnionWith(tris);

        foreach (NavTri t in chunkTris) {
            map.navtris.Remove(t);
            //.debugStarPoint(t.centroid(),0.10f,Color.yellow);
        }

        foreach (NavTri t in tris) {
            map.navtris.Add(t);
            //Utilities.debugStarPoint(t.centroid(),0.08f,Color.green);
        }

        removeVertsFromMap();
        tempSitchBorder(tris);
        //stitchBorder();
        addVertsToMap(tris);
    }

    private void tempSitchBorder(IEnumerable<NavTri> newTris) {
        //int counter = 0;
        bool noPair = false;
        foreach (KeyValuePair<EdgePair,HalfEdge> kvp in border) {
            if (kvp.Value == null) {
                continue;
            }
            bool pairFound = false;
            foreach (NavTri t in newTris) {
                foreach (HalfEdge h in t.Edges()) {
                    if (h.Equals(kvp.Key)) {
                        h.pair = kvp.Value;
                        kvp.Value.pair = h;

                        h.type = kvp.Value.type;

                        pairFound = true;
                        break;
                    }
                }
                if (pairFound) {
                    break;
                }
            }
            if (!pairFound) {
                Debug.DrawLine(kvp.Key.a,kvp.Key.b,CustomColors.orange);
                noPair = true;
            }
            ///counter++;
        }
        if (noPair) {
            Debug.LogError("No border pair found.");
        }
    }

    //void stitchBorder(TriChunk tc) {
    //    foreach(HalfEdge inner in tc.border.Keys) {
    //        inner.pair = border[inner];
    //    }
    //}

    private void removeVertsFromMap() {
        HashSet<Vector2> searched = new HashSet<Vector2>();
        foreach (NavTri t in chunkTris) {
            foreach (HalfEdge h in t.Edges()) {
                if (!searched.Contains(h.start)) {
                    map.vertEdgeMap.Remove(h.start);
                    searched.Add(h.start);
                }
            }
        }
    }

    private void addVertsToMap(IEnumerable<NavTri> tris) {
        MapProcessing.addToVertEdgeMap(tris,map.vertEdgeMap);

        //HashSet<Vector2> searched = new HashSet<Vector2>();
        //foreach (NavTri t in tris) {
        //    foreach (HalfEdge h in t.Edges()) {
        //        if (!searched.Contains(h.start)) {
        //            TestControl.main.current_map.vertEdgeMap.Add(h.start,h);
        //            searched.Add(h.start);
        //        }
        //    }
        //}
    }

    //void addVertsToMap(TriChunk tc) {
    //    HashSet<Vector2> searched = new HashSet<Vector2>();
    //    foreach (NavTri t in tc.chunkTris) {
    //        foreach (HalfEdge h in t.Edges()) {
    //            if (!searched.Contains(h.start)) {
    //                TestControl.main.current_map.vertEdgeMap.Add(h.start,h);
    //                searched.Add(h.start);
    //            }
    //        }
    //    }
    //}

    public void TrimWithBorder(TriChunk tc) {
        Triangulation.trimWithBorder(tc.chunkTris,border.Keys);
    }

    public HashSet<Vector2> getVerts() {
        HashSet<Vector2> verts = new HashSet<Vector2>();
        foreach (HalfEdge h in MapProcessing.EdgesInList(chunkTris)) {
            if (!verts.Contains(h.start)) {
                verts.Add(h.start);
            }
        }
        return verts;
    }
}