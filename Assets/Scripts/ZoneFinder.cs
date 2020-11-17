using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZoneType { FLOOR, PIT, ROOF}
public class ZoneFinder : MonoBehaviour
{
    List<NavTri> zoneTris;
    public ZoneType zoneType;
    public bool makeMeshFromZone;

    // Start is called before the first frame update
    void Start()
    {
        zoneTris = FindTris(Utils.Vector3ToVector2XZ(transform.position),GameManager.main.map);
        MakeMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<NavTri> FindTris(Vector2 startPoint,Map map) {
        NavTri startTri = NavCalc.TriFromPoint(startPoint,map.TriKDMap,map.vertEdgeMap);
        return NavUtils.FindConnectedTris(startTri);
    }

    void MakeMesh() {
        Vector3 offSet = transform.position;

        Mesh mesh = NavUtils.MeshFromTris(zoneTris,-offSet,Vector2Mode.XZ);
        GetComponent<MeshFilter>().mesh = mesh;

        if (zoneType == ZoneType.ROOF) {
            transform.Translate(Vector3.up * 2);
            MakeWalls(2.5f,1.5f,false);
        } else if (zoneType == ZoneType.PIT) {
            transform.Translate(Vector3.up * -2);
            MakeWalls(1.5f,-2.5f);
        } else if(zoneType == ZoneType.FLOOR) {
            transform.Translate(Vector2.down * 0.5f);
        }
    }

    void MakeWalls(float height,float yOffSet,bool faceInwards = true) {
        List<HalfEdge> border = ZoneBorderEdges(zoneTris);
        for(int i = 0; i < border.Count; i++) {
            HalfEdge e = border[i];
            float width = Vector2.Distance(e.start,e.next.start);
            Mesh mesh = Utils.NewQuadMesh(width,height);
            Vector2 halfway2D = e.start - (e.start - e.next.start) / 2;
            GameObject g = new GameObject();
            MeshFilter mf = g.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.material = GetComponent<MeshRenderer>().material;
            g.transform.position = Utils.Vector2XZToVector3(halfway2D)+Vector3.up * yOffSet/2;
            g.transform.SetParent(transform);

            float theta = Vector2.SignedAngle(e.next.start - e.start,Vector2.right);
            if (faceInwards) {
                theta += 180f;
            }
            g.transform.Rotate(Vector3.up*theta);
        }
    }

    List<HalfEdge> ZoneBorderEdges(List<NavTri> zoneTris) {
        List<HalfEdge> edges = new List<HalfEdge>();
        HashSet<NavTri> triSet = new HashSet<NavTri>(zoneTris);
        for(int i = 0; i < zoneTris.Count; i++) {
            for (int j = 0; j < 3; j++) {
                HalfEdge e = zoneTris[i][j];

                NavTri otherTri = (e.pair != null) ? e.pair.face : null;
                if (otherTri == null || !triSet.Contains(otherTri)) {
                    edges.Add(e);
                }
            }
        }
        return edges;
    }
}
