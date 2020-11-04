using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utils 
{
    const int loopLimit = 500000;
    public const int smallLimit = 5000;

    public static void ListSwap<T>(List<T> list,int a,int b) {
        T temp = list[a];
        list[a] = list[b];
        list[b] = temp;
    }

    public static bool WhileCounterIncrementAndCheck(ref int i, int limit = loopLimit) {
        if (i > limit) {
            Debug.LogError("Loop limit reached");
            return true;
        }
        i++;
        return false;


    }

    public static Vector3[] crossOnPoint(Vector3 v,float f) {
        Vector3[] r = new Vector3[4];
        r[0] = new Vector3(v.x,v.y + f,v.z);
        r[1] = new Vector3(v.x,v.y - f,v.z);
        r[2] = new Vector3(v.x + f,v.y,v.z);
        r[3] = new Vector3(v.x - f,v.y,v.z);
        return r;
    }

    public static Vector3[] xOnPoint(Vector3 v,float f) {
        float sqr = Mathf.Sqrt(2);
        float val = Mathf.Sqrt(Mathf.Pow(f,2) / 2);
        Vector3[] r = new Vector3[4];
        r[0] = new Vector3(v.x + val,v.y + val,v.z);
        r[1] = new Vector3(v.x - val,v.y - val,v.z);
        r[2] = new Vector3(v.x + val,v.y - val,v.z);
        r[3] = new Vector3(v.x - val,v.y + val,v.z);

        return r;
    }

    public static List<Vector3> starOnPoint(Vector3 v,float f) {
        List<Vector3> r = new List<Vector3>();
        r.AddRange(crossOnPoint(v,f));
        r.AddRange(xOnPoint(v,f));
        return r;
    }

    public static void debugStarPoint(Vector3 v,float f,Color c,float duration = 0) {
        List<Vector3> points = starOnPoint(v,f);
        for (int i = 0; i < 8; i += 2) {
            Debug.DrawLine(points[i],points[i + 1],c,duration);
        }
    }

    public static void DisplayMessage(string message) {
        if (GameObject.Find("Messager") == null)
            return;
        GameObject.Find("Messager").GetComponent<Text>().text = message;
    }

    public static void DrawLinesPoly(List<Vector2> poly, Color c) {
        for(int i = 0; i < poly.Count; i++) {
            Debug.DrawLine(poly[i],poly[(i + 1) % poly.Count],c);
        }
    }

    public static Vector2[] cardinalNeighbors(Vector2 v) {
        Vector2[] r = {
            new Vector2(v.x+1,v.y),
            new Vector2(v.x-1,v.y),
            new Vector2(v.x,v.y+1),
            new Vector2(v.x,v.y-1)
        };
        return r;
    }

    public static void DebugLinePercentage(Vector2 a,Vector2 b,float percentage,Color c) {
        Vector2 midpoint = (b - a) * percentage + a;
        Debug.DrawLine(a,midpoint,c);
    }

    public static Mesh NewQuadMesh(float width,float height) {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        /* 0,0
         * 0,1
         * 1,1
         * 1,0
         */

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        vertices[0] = new Vector3(-halfWidth,-halfHeight);
        vertices[1] = new Vector3(-halfWidth,+halfHeight);
        vertices[2] = new Vector3(+halfWidth,+halfHeight);
        vertices[3] = new Vector3(+halfWidth,-halfHeight);

        uv[0] = new Vector2(0,0);
        uv[1] = new Vector2(0,1);
        uv[2] = new Vector2(1,1);
        uv[3] = new Vector2(1,0);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 3;

        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }

    public static Mesh LineMeshFromPoints(List<Vector3> points) {
        List<int> indicies = new List<int>();

        for(int i = 0; i < points.Count; i++) {
            indicies.Add(i);
        }

        Mesh mesh = new Mesh();

        mesh.vertices = points.ToArray();
        mesh.SetIndices(indicies.ToArray(),MeshTopology.Lines,0);

        return mesh;
    }

    public static Tuple<Vector2,Vector2> GetBoundingBox(IEnumerable<Vector2> points) {
        Vector2 min = new Vector2();
        Vector2 max = new Vector2();
        foreach(Vector2 v in points) {
            if (v.x < min.x)
                min.x = v.x;
            if (v.y < min.y)
                min.y = v.y;

            if (v.x > max.x)
                max.x = v.x;
            if (v.y > max.y)
                max.y = v.y;
        }
        return new Tuple<Vector2,Vector2>(min,max);
    }

    public static Transform[] GetChildren(Transform t) {
        Transform[] children = new Transform[t.childCount];
        int i = 0;
        foreach (Transform child in t.transform) {
            children[i] = child;
            i++;
        }
        return children;
    }

    public static Vector2 Vector3ToVector2XZ(Vector3 v) {
        return new Vector2(v.x,v.z);
    }

    public static Vector3 Vector2XZToVector3(Vector2 v) {
        return new Vector3(v.x,0,v.y);
    }


    // Authored Extensions - Kurtis Lawson
    public static Vector3 DirectionToTarget(this Transform transform, Vector3 target) {
        return (target - transform.position).normalized;
    }

    public static Vector3 DirectionFromTarget(this Transform transform, Vector3 target) {
        return (transform.position - target).normalized;
    }

    public static Vector2[] Vector2XZsFromTransforms(Transform[] transforms) {
        Vector2[] verts = new Vector2[transforms.Length];
        for(int i = 0; i < verts.Length; i++) {
            verts[i] = Utils.Vector3ToVector2XZ(transforms[i].position);
        }
        return verts;
    }
    
    public static void DrawTransformsAsPolygon(Transform[] transforms, Color c) {
        for(int i = 0; i < transforms.Length; i++) {
            Vector3 a = transforms[i].position;
            Vector3 b = transforms[(i + 1) % transforms.Length].position;
            Debug.DrawLine(a,b,c);
        }
    }

    public static bool RaycastMousePosZeroY(Camera camera, out Vector3 worldPos) {

        // We create an invisible plane at world pos 0,0,0
        Plane groundPlane = new Plane(Vector3.up,Vector3.zero);
        float rayLength;

        // Here's the ray. We cast from the camera to the mouse position.
        Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);

        // If we intersect with the ground plane, we can get it's point of intersection and return that.
        if (groundPlane.Raycast(cameraRay,out rayLength)) {
            worldPos = cameraRay.GetPoint(rayLength);
            return true;
        }

        worldPos = Vector3.negativeInfinity;
        return false;

    }


}
