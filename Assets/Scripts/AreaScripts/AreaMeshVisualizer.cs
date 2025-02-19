﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// this component creates a 3D mesh from the shape of an polygon area collider

[RequireComponent(typeof(AreaPolygonCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class AreaMeshVisualizer : MonoBehaviour
{    
    // Start is called before the first frame update
    void Start()
    {
        AreaPolygonCollider a = GetComponent<AreaPolygonCollider>();
        PolygonCollider p = a.PolygonCollider;
        List<NavTri> tris = NavUtils.TrisFromPoints(new List<Vector2>(p.Verts));
        Mesh mesh = NavUtils.MeshFromTris(tris,transform.position * -1,Vector2Mode.XZ);
        MeshFilter mf = GetComponent<MeshFilter>();

        mf.mesh = mesh;
    }
}
