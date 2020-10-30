using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NavCalc
{
    public static bool PointInTri(Vector3 p,NavTri tri) {
        Vector2 p0 = tri.e1.start;
        Vector2 p1 = tri.e1.next.start;
        Vector2 p2 = tri.e1.next.next.start;

        float area = 0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
        float s = 1 / (2 * area) * (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y);
        float t = 1 / (2 * area) * (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y);

        return (s >= 0 && t >= 0 && 1 - s - t >= 0);
    }

    public static bool PointInOrOnTri(Vector3 p,NavTri tri) {
        foreach (HalfEdge h in tri.Edges()) {
            if (Calc.ClockWiseCheck(h.start,h.next.start,p) > 0)
                return false;
        }
        return true;
    }

    public static NavTri TriFromPoint(Vector2 point, KDNode root,Dictionary<Vector2,HalfEdge> vertEdgeMap) {

        Vector2 vert = root.ClosestPoint(point);
        foreach (HalfEdge h in MapProcessing.edgesAroundVert(vert,vertEdgeMap)) {
            if (NavCalc.PointInTri(point,h.face)) {
                return h.face;
            }
        }

        //Debug.LogError("Cannot find tri from point");

        NavTri t = vertEdgeMap[vert].face;

        int counter = 0;
        while (counter < Utils.smallLimit) {
            foreach (HalfEdge h in t.Edges()) {
                if (h.pair != null && Calc.ClockWiseCheck(h.start,h.next.start,point) > 0) {
                    t = h.pair.face;
                    break;
                }
            }

            if (NavCalc.PointInOrOnTri(point,t))
                return t;
            counter++;
        }

        if (counter >= Utils.smallLimit) {
            Debug.LogError("loop limit reached, position: " + point);
            Utils.debugStarPoint(point,1f,Color.red);
        }


        return null;
    }

}

