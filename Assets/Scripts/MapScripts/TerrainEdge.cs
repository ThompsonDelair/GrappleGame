using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represents a wall or cliff or door
// used for collision
public class TerrainEdge
{
    Vector2 vertA_pos;
    Vector2 vertB_pos;
    public Layer layer;

    public Vector2 vertA_pos2D { get { return vertA_pos; } set { vertA_pos = value; } }
    public Vector2 vertB_pos2D { get { return vertB_pos; } set { vertB_pos = value; } }

    public Vector3 vertA_pos3D { get { return Utils.Vector2XZToVector3(vertA_pos); } }
    public Vector3 vertB_pos3D { get { return Utils.Vector2XZToVector3(vertB_pos); } }

    public AABB_2D aabb;

    public TerrainEdge(Vector2 posA,Vector3 posB,Layer type) {
        vertA_pos = posA;
        vertB_pos = posB;
        layer = type;
        aabb = new AABB_2D(posA,posB);
    }
}
