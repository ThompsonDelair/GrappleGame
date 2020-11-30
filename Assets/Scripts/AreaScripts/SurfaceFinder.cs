using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurfaceFinder : MonoBehaviour
{
    public float radius;
    public float spaceBetweenPoints;
    public bool setMesh;

    // Start is called before the first frame update
    void Start()
    {
        if (setMesh) {
            GetComponent<MeshFilter>().mesh = GetMesh();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Vector2> FindSurfacePoints() {
        List<Vector2> points = new List<Vector2>();
        HashSet<Vector2> set = new HashSet<Vector2>();

        int pointsInRad = (int)(radius / spaceBetweenPoints) + 1;
        
        // x levels?
        for(int i = -pointsInRad; i <= pointsInRad; i++) {
            {
                Vector2 upPos = Scan(i,-pointsInRad,Vector2.up,pointsInRad);
                if (!upPos.Equals(Vector2.negativeInfinity) && !ContainsV2(upPos)) {
                    points.Add(upPos);
                }
            }

            {
                Vector2 downPos = Scan(i,pointsInRad,Vector2.down,pointsInRad);

                if (!downPos.Equals(Vector2.negativeInfinity) && !ContainsV2(downPos)) {
                    points.Add(downPos);
                }
            }

        }

        // y levels
        //for (int i = -pointsInRad; i <= pointsInRad; i++) {
        //    {
        //        Vector2 leftPos = Scan(pointsInRad,i,Vector2.left,pointsInRad - 1);

        //        if (!leftPos.Equals(Vector2.negativeInfinity) && !ContainsV2(leftPos)) {
        //            points.Add(leftPos);
        //        }
        //    }

        //    {
        //        Vector2 rightPos = Scan(-pointsInRad,i,Vector2.right,pointsInRad - 1);

        //        if (!rightPos.Equals(Vector2.negativeInfinity) && !ContainsV2(rightPos)) {
        //            points.Add(rightPos);
        //        }
        //    }
        //}

        return points;

        bool ContainsV2(Vector2 v2) {
            float threshold = spaceBetweenPoints / 2;
            for(int i = 0; i < points.Count; i++) {
                if (Mathf.Abs(v2.x - points[i].x) < spaceBetweenPoints &&
                    Mathf.Abs(v2.y - points[i].y) < spaceBetweenPoints ) {
                    return true;
                }
            }
            return false;
        }


    }

    public Vector2 Scan(int yLevel,int xLevel,Vector2 dir,int steps) {
        Map m = GameManager.main.gameData.map;
        int i = 0;
        //endPos = Vector2.zero;
        for (; i < steps; i++) {
            Vector2 point = Utils.Vector3ToVector2XZ(transform.position);
            point += new Vector2(yLevel * spaceBetweenPoints,xLevel * spaceBetweenPoints);
            point += i * spaceBetweenPoints * dir;
            //endPos = point;
            if (m.IsPointWalkable(point) && Vector2.Distance(point,Utils.Vector3ToVector2XZ(transform.position)) <= radius)
                return point;
        }
        return Vector2.negativeInfinity;
    }

    public Mesh GetMesh() {
        List<Vector2> points = FindSurfacePoints();

        points.Sort((Vector2 a, Vector2 b)=> {
            if (a.x != b.x)
                return a.x.CompareTo(b.x);
            return a.y.CompareTo(b.y);
        });

        List<NavTri> tris = new List<NavTri>();
        if (points.Count > 2) {
            Dictionary<EdgePair,HalfEdge> dict;
            tris = Triangulation.BowyerWatson(points,out dict);

            Map m = GameManager.main.gameData.map;
            for (int i = tris.Count - 1; i >= 0; i--) {
                Vector2 centroid = tris[i].centroid();
                if (!m.IsPointWalkable(centroid)) {
                    for (int j = 0; j < 3; j++) {
                        if (tris[i][j].pair != null) {
                            tris[i][j].pair.pair = null;
                        }
                    }
                    tris.RemoveAt(i);
                }
            }

            return NavUtils.MeshFromTris(tris,-transform.position,Vector2Mode.XZ);
        }
        return null;
    }
}
