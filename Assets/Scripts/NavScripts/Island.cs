using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Island {
    public HashSet<Vector2> cells = new HashSet<Vector2>();

    //public HashSet<EdgePair> edges = new HashSet<EdgePair>();

    //public Vector2 position;
    //public Vector2 size;

    public Vector2 topCorner;
    public Vector2 bottomCorner;

    //public List<EdgePair> implicits = new List<EdgePair>();
    //public HashSet<Island> adjacentIslands() {
    //    HashSet<Island> adj = new HashSet<Island>();
    //    foreach(Vector2 v in cells) {
    //        Vector2[] cellNeighbors = Utilities.cardinalNeighbors(v);
    //        foreach(Vector2 v2 in cellNeighbors) {
    //            if (!cells.Contains(v2)) {
    //                foreach(Island isl in TestControl.main.current_map.islands) {
    //                    if (isl.cells.Contains(v2)) {
    //                        adj.Add(isl);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return adj;
    //}
}