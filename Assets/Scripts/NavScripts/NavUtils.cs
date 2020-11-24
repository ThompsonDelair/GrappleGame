using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Vector2Mode { XY, XZ }
public static class NavUtils
{
    public static void DrawLines(IEnumerable<NavTri> tris,Color c) {
        foreach (NavTri t in tris) {
            foreach (HalfEdge h in t.Edges()) {
                Debug.DrawLine(h.start,h.next.start,c);
            }
        }
    }

    public static void DrawLines(NavTri t,Color c) {
        foreach (HalfEdge h in t.Edges()) {
            Debug.DrawLine(h.start,h.next.start,c);
        }
    }

    public static void DrawTriXZ(NavTri t,Color c) {
        foreach (HalfEdge h in t.Edges()) {
            Debug.DrawLine(Utils.Vector2XZToVector3(h.start),Utils.Vector2XZToVector3(h.next.start),c);
        }
    }

    public static Mesh LineMeshFromTris(List<NavTri> navTris,Vector2Mode mode = Vector2Mode.XY) {
        Mesh mesh = new Mesh();

        //mesh.top

        Dictionary<Vector2,int> vertMap = new Dictionary<Vector2,int>();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        //Vector2[] uvSet = { new Vector2(0,0),new Vector2(0,1),new Vector2(1,1) };

        foreach (NavTri t in navTris) {
            HalfEdge curr = t.e1;
            for (int i = 0; i < 3; i++) {

                Vector3 start = (mode == Vector2Mode.XY) ? new Vector3(curr.start.x,curr.start.y,0) : Utils.Vector2XZToVector3(curr.start);

                if (vertMap.ContainsKey(start)) {
                    tris.Add(vertMap[start]);
                } else {
                    vertMap.Add(start,vertMap.Count);
                    verts.Add(start);
                    Vector3 normal = (mode == Vector2Mode.XY) ? Vector3.forward : Vector3.up;
                    normals.Add(normal);
                    uv.Add(new Vector2(0,0));
                    tris.Add(vertMap.Count - 1);
                }
                curr = curr.prev();
            }
        }

        //mesh.setin
        //mesh.SetIndices(verts.ToArray(),MeshTopology.Lines,0);
        mesh.vertices = verts.ToArray();
        //mesh.uv = uv.ToArray();
        mesh.triangles = tris.ToArray();
        //mesh.normals = normals.ToArray();

        int[] triIndices = mesh.GetIndices(0);
        int[] lines = new int[triIndices.Length * 2];
        int index = 0;
        for (int n = 0; n < triIndices.Length; n += 3) {
            lines[index++] = tris[n + 0];
            lines[index++] = tris[n + 1];
            lines[index++] = tris[n + 1];
            lines[index++] = tris[n + 2];
            lines[index++] = tris[n + 2];
            lines[index++] = tris[n + 0];
        }
        mesh.SetIndices(lines,MeshTopology.Lines,0);

        return mesh;
    }

    public static Mesh MeshFromTris(List<NavTri> navTris,Vector3 offset, Vector2Mode mode = Vector2Mode.XY) {
        Mesh mesh = new Mesh();

        //mesh.top

        Dictionary<Vector2,int> vertMap = new Dictionary<Vector2,int>();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        //Vector2[] uvSet = { new Vector2(0,0),new Vector2(0,1),new Vector2(1,1) };

        foreach (NavTri t in navTris) {
            HalfEdge curr = t.e1;
            for (int i = 0; i < 3; i++) {

                Vector3 start = (mode == Vector2Mode.XY) ? new Vector3(curr.start.x,curr.start.y,0) : Utils.Vector2XZToVector3(curr.start);
                start += offset;

                //if (vertMap.ContainsKey(start)) {
                //    tris.Add(vertMap[start]);
                //} else {
                //    vertMap.Add(start,vertMap.Count);
                //    verts.Add(start);
                //    Vector3 normal = (mode == Vector2Mode.XY) ? Vector3.forward : Vector3.up;
                //    normals.Add(Vector3.forward);
                //    uv.Add(new Vector2(0,0));
                //    tris.Add(vertMap.Count - 1);
                //}

                //vertMap.Add(start,vertMap.Count);
                verts.Add(start);
                Vector3 normal = (mode == Vector2Mode.XY) ? Vector3.forward : Vector3.up;
                normals.Add(Vector3.forward);
                uv.Add(new Vector2(curr.start.x,curr.start.y));
                tris.Add(verts.Count - 1);


                curr = curr.prev();
            }
        }

        //Debug.LogFormat("Made a mesh with {0} tris ",navTris.Count);

        //mesh.setin
        //mesh.SetIndices(verts.ToArray(),MeshTopology.Lines,0);
        mesh.vertices = verts.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = tris.ToArray();
        //mesh.normals = normals.ToArray();
        mesh.RecalculateNormals();

        //int[] triIndices = mesh.GetIndices(0);
        //int[] lines = new int[triIndices.Length * 3];
        //int index = 0;
        //for (int n = 0; n < triIndices.Length; n += 3) {
        //    lines[index++] = tris[n + 0];
        //    lines[index++] = tris[n + 1];
        //    lines[index++] = tris[n + 1];
        //    lines[index++] = tris[n + 2];
        //    lines[index++] = tris[n + 2];
        //    lines[index++] = tris[n + 0];
        //}
        //mesh.SetIndices(lines,MeshTopology.Triangles,0);

        return mesh;
    }

    public static List<NavTri> TrisFromPoints(List<Vector2> points) {
        HashSet<EdgePair> implicitEdges = new HashSet<EdgePair>();
        Dictionary<EdgePair,HalfEdge> edgeMap;
        Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2, HalfEdge>();

        List<NavTri> navTris = Triangulation.BowyerWatson(points,out edgeMap);
        MapProcessing.addToVertEdgeMap(navTris,vertEdgeMap);

        List<EdgePair> orphans = MapProcessing.ProcessMapImplicits(navTris,implicitEdges,vertEdgeMap);
        //Debug.Log("nav mesh gen had " + orphans.Count + " orphan edges");
        MapProcessing.fixMapOrphans(navTris,orphans,vertEdgeMap);
        MapProcessing.ProcessMapImplicits(navTris,implicitEdges,vertEdgeMap);
        
        return navTris;
    }

    public static List<NavTri> FindConnectedTris(NavTri start) {
        List<NavTri> tris = new List<NavTri>();
        HashSet<NavTri> visited = new HashSet<NavTri>();
        Stack<NavTri> frontier = new Stack<NavTri>();

        frontier.Push(start);
        int counter;

        while (frontier.Count != 0) {
            NavTri curr = frontier.Pop();

            foreach (HalfEdge e in curr.Edges()) {
                if (e.moveBlock == Layer.NONE && e.pair != null && !visited.Contains(e.pair.face)) {
                    frontier.Push(e.pair.face);
                }
            }

            visited.Add(curr);
            tris.Add(curr);
        }

        return tris;
    }

    public static List<NavTri> FindConnectedTrisWithAABB(NavTri start, out AABB_2D aabb) {
        Vector2 bottomLeft = Vector2.positiveInfinity;
        Vector2 topRight = Vector2.negativeInfinity;
        List<NavTri> tris = new List<NavTri>();
        HashSet<NavTri> visited = new HashSet<NavTri>();
        Stack<NavTri> frontier = new Stack<NavTri>();

        frontier.Push(start);
        int counter;

        while (frontier.Count != 0) {
            NavTri curr = frontier.Pop();

            foreach (HalfEdge e in curr.Edges()) {
                if (e.moveBlock == Layer.NONE && e.pair != null && !visited.Contains(e.pair.face)) {
                    frontier.Push(e.pair.face);
                }

                if(e.start.x < bottomLeft.x) {
                    bottomLeft.x = e.start.x;
                } else if(e.start.x > topRight.x) {
                    topRight.x = e.start.x;
                }

                if (e.start.y < bottomLeft.y) {
                    bottomLeft.y = e.start.y;
                } else if (e.start.y > topRight.y) {
                    topRight.y = e.start.y;
                }
            }

            visited.Add(curr);
            tris.Add(curr);
        }
        aabb = new AABB_2D(bottomLeft,topRight);
        return tris;
    }
}
