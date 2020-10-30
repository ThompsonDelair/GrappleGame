using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{
    public Vector2[][] PChains { get { return pChains; } }
    public Layer[][] VertTypes { get { return vertTypes; } }
    public KDNode TriKDMap { get { return triKDMap; } }
    public Dictionary<Vector2,HalfEdge> VertEdgeMap { get { return vertEdgeMap; } }

    Vector2[][] pChains;
    Layer[][] vertTypes;
    public List<NavTri> navTris;
    public KDNode triKDMap;
    public Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2,HalfEdge>();

    public List<Area> areas = new List<Area>();
    public List<Actor> objects = new List<Actor>();

    public void InitNavMesh(Transform terrainRoot) {
        HashSet<Vector2> navVerts;
        HashSet<EdgePair> implicitEdges;
        Dictionary<EdgePair,HalfEdge> edgeMap;

        FindTerrain(terrainRoot,out implicitEdges,out navVerts);
        navTris = Triangulation.BowyerWatson(navVerts.ToList(),out edgeMap);
        MapProcessing.addToVertEdgeMap(navTris,vertEdgeMap);

        List<EdgePair> orphans = MapProcessing.ProcessMapImplicits(navTris,implicitEdges,vertEdgeMap);
        Debug.Log("nav mesh gen had " + orphans.Count + " orphan edges");
        MapProcessing.fixMapOrphans(navTris,orphans,vertEdgeMap);
        MapProcessing.ProcessMapImplicits(navTris,implicitEdges,vertEdgeMap);

        triKDMap = new KDNode(vertEdgeMap.Keys.ToList());
    }

   void FindTerrain(Transform terrainRoot,out HashSet<EdgePair> implicitEdges,out HashSet<Vector2> navVerts) {

        List<Vector2[]> pChainList = new List<Vector2[]>();
        List<Layer[]> vertTypesList = new List<Layer[]>();
        implicitEdges = new HashSet<EdgePair>();
        navVerts = new HashSet<Vector2>();
        Transform[] pChainGroups = Utils.GetChildren(terrainRoot);

        for (int i = 0; i < pChainGroups.Length; i++) {
            Transform[] pChains = Utils.GetChildren(pChainGroups[i]);

            for (int j = 0; j < pChains.Length; j++) {
                PolygonalChain pChain = pChains[j].GetComponent<PolygonalChain>();
                Transform[] pChainTransforms = Utils.GetChildren(pChains[j]);
                Vector2[] pChainVerts = (pChain.connectEnds) ?
                    new Vector2[pChainTransforms.Length + 1] : new Vector2[pChainTransforms.Length];
                Layer[] pChainVertTypes = (pChain.connectEnds) ?
                    new Layer[pChainTransforms.Length + 1] : new Layer[pChainTransforms.Length];

                for (int k = 0; k < pChainTransforms.Length; k++) {

                    TerrainVertex vertA = pChainTransforms[k].GetComponent<TerrainVertex>();
                    pChainVerts[k] = Utils.Vector3ToVector2XZ(vertA.SnapPos);
                    pChainVertTypes[k] = LayerFromVertType(vertA.VertType);

                    Vector2 vertPosXZ = Utils.Vector3ToVector2XZ(vertA.SnapPos);

                    if (!navVerts.Contains(vertPosXZ)) {
                        navVerts.Add(vertPosXZ);
                    } else {
                        Debug.LogWarning("vert already added to list?");
                    }

                    TerrainVertex vertB = pChainTransforms[(k + 1) % pChainTransforms.Length].GetComponent<TerrainVertex>();
                    Vector2 nextVertPosXZ = Utils.Vector3ToVector2XZ(vertB.SnapPos);

                    // IMPORTANT FOR PCHAINS
                    if (k == pChainTransforms.Length - 1 && !pChain.connectEnds) {
                        continue;
                    } else {
                        pChainVerts[k + 1] = nextVertPosXZ;
                        pChainVertTypes[k + 1] = LayerFromVertType(vertB.VertType);
                    }

                    // add as implicit edge
                    if (k != pChainTransforms.Length - 1 || pChain.connectEnds) {
                        EdgePair edge = new EdgePair(vertPosXZ,nextVertPosXZ);
                        if (!implicitEdges.Contains(edge)) {
                            implicitEdges.Add(edge);
                        } else {
                            Debug.LogWarning("implicit edges already contains this edge?");
                        }
                    }
                }
                pChainList.Add(pChainVerts);
                vertTypesList.Add(pChainVertTypes);
            }
            pChains[i].gameObject.SetActive(false);
        }
        pChains = pChainList.ToArray();
        vertTypes = vertTypesList.ToArray();
   }

    Layer LayerFromVertType(VertType v) {
        if (v == VertType.WALL)
            return Layer.WALLS;
        if (v == VertType.CLIFF)
            return Layer.CLIFFS;
        if (v == VertType.WALL_NO_GRAPPLE)
            return Layer.NO_GRAPPLE_WALL;
        return Layer.NONE;
    }

    public void FindAreas(Transform areaRoot) {
        Transform[] children = Utils.GetChildren(areaRoot);
        for(int i = 0; i < children.Length; i++) {
            Area a = children[i].GetComponent<Area>();
            if (a != null) {
                areas.Add(a);
            }
        }
    }

    public void FindObjects(Transform objectRoot) {
        Transform[] children = Utils.GetChildren(objectRoot);
        for (int i = 0; i < children.Length; i++) {
            //ActorNameComponent name = children[i].GetComponent<ActorNameComponent>();
            ActorStats stats = children[i].GetComponent<ActorInfo>().stats;
            Actor a = new Actor(children[i],stats.radius,Layer.ENEMIES,stats.maxHP);
            objects.Add(a);
        }
    }
}
