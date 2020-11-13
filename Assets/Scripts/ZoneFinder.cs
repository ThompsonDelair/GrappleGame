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
        } else if (zoneType == ZoneType.PIT) {
            transform.Translate(Vector3.up * -2);
        } else if(zoneType == ZoneType.FLOOR) {
            transform.Translate(Vector2.down * 0.5f);
        }
    }
}
