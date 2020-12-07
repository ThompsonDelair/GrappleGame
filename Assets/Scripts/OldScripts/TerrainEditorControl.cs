using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Old mad editor tool
public class TerrainEditorControl : MonoBehaviour
{
    //[SerializeField] private float vertSnapRange;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform FindClosestVertToPoint(Transform t) {
        Transform closest = null;
        float closestDist = Mathf.Infinity;

        Transform[] polygonGroups = Utils.GetChildren(transform);
        for (int i = 0; i < polygonGroups.Length; i++) {
            Transform[] polygons = Utils.GetChildren(polygonGroups[i]);
            for (int j = 0; j < polygons.Length; j++) {
                Transform[] verts = Utils.GetChildren(polygons[j]);
                for (int k = 0; k < verts.Length; k++) {
                    Transform vert = verts[k];

                    if (t == vert) {
                        continue;
                    }

                    Vector3 snapPosA = t.GetComponent<TerrainVertex>().SnapPos;
                    Vector3 snapPosB = vert.GetComponent<TerrainVertex>().SnapPos;

                    if (Vector3.Distance(snapPosA,snapPosB) < closestDist) {
                        closestDist = Vector3.Distance(snapPosA,snapPosB);
                        closest = vert;
                    }
                }
            }
        }

        return closest;
    }
}
