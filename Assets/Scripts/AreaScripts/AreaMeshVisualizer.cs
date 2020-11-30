using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //mr.material = Resources.Load<Material>("Materials/Orange");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
