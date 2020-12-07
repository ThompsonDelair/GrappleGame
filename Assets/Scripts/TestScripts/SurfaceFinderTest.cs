using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SurfaceFinder))]
public class SurfaceFinderTest : MonoBehaviour
{
    List<NavTri> tris;
    List<Vector2> points;
    // Start is called before the first frame update
    void Start()
    {
        SurfaceFinder sf = GetComponent<SurfaceFinder>();
        points = sf.FindSurfacePoints();

        if(points.Count > 2) {
            Dictionary<EdgePair,HalfEdge> dict;
            tris = Triangulation.BowyerWatson(points,out dict);

            Map m = GameManager.main.GameData.map;
            for (int i = tris.Count -1; i >= 0; i--) {
                Vector2 centroid = tris[i].centroid();
                if (!m.IsPointWalkable(centroid)){
                    for(int j = 0; j < 3; j++) {
                        if(tris[i][j].pair != null) {
                            tris[i][j].pair.pair = null;
                        }                        
                    }
                    tris.RemoveAt(i);
                }
            }

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if(mr == null) {
                mr = gameObject.AddComponent<MeshRenderer>();
            }

            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) {
                mf = gameObject.AddComponent<MeshFilter>();
            }

            mf.mesh = NavUtils.MeshFromTris(tris,-transform.position,Vector2Mode.XZ);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos() {
        //if(tris != null) {
        //    for(int i = 0; i < tris.Count; i++) {
        //        NavUtils.DrawTriXZ(tris[i],CustomColors.orange);
        //    }
        //}
        //if(points != null) {
        //    for(int i = 0; i < points.Count; i++) {
        //        Utils.debugStarPoint(Utils.Vector2XZToVector3(points[i]),0.1f,CustomColors.orange);
        //    }
        //}
    }
}
