using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this component searches the navmesh at game start to find the triangles that make up a zone
//      it can also generate a mesh for a mesh renderer, creating the visual map geometry for the zone
public enum ZoneType { FLOOR, PIT, ROOF}
public class ZoneFinder : MonoBehaviour
{
    Zone zone;
    public ZoneType zoneType;
    public bool makeMeshFromZone;
    public Material wallMat;

    public float wallHeight = 2f;
    const float floorLevel = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        if(zone == null) {
            zone = FindZone(GameManager.main.GameData.map);
        }
        MakeMesh();
    }

    Movement MovementFromZoneType(ZoneType z) {
        if(z == ZoneType.FLOOR) {
            return Movement.WALKING;    
        } else if(z == ZoneType.PIT) {
            return Movement.FLYING;
        }
        return Movement.NONE;
    }

    public Zone FindZone(Map map) {
        Vector2 startPoint = Utils.Vector3ToVector2XZ(transform.position);
        NavTri startTri = NavCalc.TriFromPoint(startPoint,map.TriKDMap,map.vertEdgeMap);
        AABB_2D aabb;
        List<NavTri> navTris = NavUtils.FindConnectedTrisWithAABB(startTri,out aabb);
        zone = new Zone(MovementFromZoneType(zoneType),navTris,aabb);
        return zone;
    }

    void MakeMesh() {
        Vector3 offSet = transform.position;

        Mesh mesh = NavUtils.MeshFromTris(zone.tris,-offSet,Vector2Mode.XZ);
        GetComponent<MeshFilter>().mesh = mesh;

        if (zoneType == ZoneType.ROOF) {
            transform.Translate(Vector3.up * wallHeight);
            MakeWalls(wallHeight + floorLevel,wallHeight-floorLevel,false);
        } else if (zoneType == ZoneType.PIT) {
            transform.Translate(Vector3.up * -wallHeight);
            MakeWalls(wallHeight - floorLevel,-wallHeight-floorLevel);
        } else if(zoneType == ZoneType.FLOOR) {
            transform.Translate(Vector2.down * floorLevel);
        }
    }

    void MakeWalls(float height,float yOffSet,bool faceInwards = true) {
        List<HalfEdge> border = ZoneBorderEdges(zone.tris);
        for(int i = 0; i < border.Count; i++) {
            HalfEdge e = border[i];
            float width = Vector2.Distance(e.start,e.next.start);
            Mesh mesh = Utils.NewQuadMesh(width,height);
            Vector2 halfway2D = e.start - (e.start - e.next.start) / 2;
            GameObject g = new GameObject();
            MeshFilter mf = g.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.material = (wallMat == null) ? GetComponent<MeshRenderer>().material : wallMat;
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

    private void OnDrawGizmos() {
        Color c = Color.white;

        if (zoneType == ZoneType.ROOF) {
            c = Color.black;
        } else if (zoneType == ZoneType.PIT) {
            c = CustomColors.darkRed;
        } else if (zoneType == ZoneType.FLOOR) {
            c = Color.blue;
        }

        Gizmos.color = c;

        Gizmos.DrawSphere(transform.position,1f);
    }
}
