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
    //static float gizmoSize = 0.2f;

    public VertType VertType { get { return vertType; } }

    //[SerializeField] private Layer layer;
    [SerializeField] private VertType vertType;

    public Vector3 SnapPos { get { return snapPos; } }

    [SerializeField] private Vector3 snapPos;
    [SerializeField] bool initSnap = false;


    // Start is called before the first frame update
    void Start()
    {
        if(vertType == VertType.CLIFF) {
            //layer = Layer.CLIFFS;
        } else if(vertType == VertType.WALL) {
            //layer = Layer.WALLS;
        }
        //snapPos = transform.position;
        //transform.hasChanged = false;
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

            // check all other verts and snap to other verts within range
            // communicate with terrain?
            
            //Debug.Log("parent:"+transform.parent.name);
            //Debug.Log("parent.parent:"+transform.parent.parent.name);
            //Debug.Log("parent.parent.parent:"+transform.parent.parent.parent.name);
            SnapCheck();

            transform.parent.GetComponent<PolygonalChain>().RenameChildren();
            //transform.parent.GetComponent<PolygonalChain>().CenterOnChildren();

            //if(closestVert != null) {
            //    Utils.debugStarPoint(closestVert.position,0.1f,Color.green);
            //} else {
            //    Utils.debugStarPoint(snapPos,0.1f,Color.grey);
            //}

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

    private void OnDrawGizmos() {
        //TerrainVertex vt = (TerrainVertex)target;
        //TerrainVertex vt = this;
        //if (EditorApplication.isPlaying) {
            //Handles.Label(vt.SnapPos,vt.gameObject.name);
            //Gizmos.DrawSphere(snapPos,gizmoSize);
        //}
    }
}


