using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainVertex))]
public class TerrainVertexEditor : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos() {
        TerrainVertex vt = (TerrainVertex)target;
        if (EditorApplication.isPlaying) {
            Handles.Label(vt.SnapPos,vt.gameObject.name);
            //Gizmos.DrawSphere(snapPos,gizmoSize);
        }



    }
}
