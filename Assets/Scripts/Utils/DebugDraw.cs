using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugDraw
{


    public static void DrawTransformsAsPolygon(Transform[] transforms, Color c) {
        for(int i = 0; i < transforms.Length; i++) {
            Debug.DrawLine(transforms[i].position,transforms[(i+1)%transforms.Length].position,c);
        }
    }

    public static void DrawPath3D(List<Vector2> path,Color c) {

        for(int i = 0; i < path.Count - 1; i++) {
            Vector3 a = Utils.Vector2XZToVector3(path[i]);
            Vector3 b = Utils.Vector2XZToVector3(path[i+1]);
            Debug.DrawLine(a,b,c);
        }
    }
}
