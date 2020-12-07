using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum VertType {
    WALL, 
    CLIFF,
    WALL_NO_GRAPPLE
}

[ExecuteInEditMode]
public class TerrainVertex : MonoBehaviour
{

    public VertType VertType { get { return vertType; } }

    //[SerializeField] private Layer layer;
    [SerializeField] private VertType vertType;

    public Vector3 SnapPos { get { return snapPos; } }

    [SerializeField] private Vector3 snapPos;
    [SerializeField] bool initSnap = false;


    // Start is called before the first frame update
    void Start()
    {
        if (!initSnap) {
            SnapCheck();
            initSnap = true;
        }            
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged) {

            Utils.debugStarPoint(snapPos,0.1f,Color.red);
            SnapCheck();
            transform.parent.GetComponent<PolygonalChain>().RenameChildren();
            transform.hasChanged = false;
        }       
    }

    void SnapCheck() {
        GameObject terrainRoot = transform.parent.parent.parent.gameObject;
        Transform closestVert = terrainRoot.GetComponent<TerrainEditorControl>().FindClosestVertToPoint(transform);
        TerrainVertex v = closestVert.GetComponent<TerrainVertex>();
        if (closestVert != null && Vector3.Distance(transform.position,v.SnapPos) < 1f) {
            snapPos = v.SnapPos;
        } else {
            snapPos = transform.position;
        }
    }
}


