using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{
    //public Vector2[][] PChains { get { return pChains; } }

    public List<TerrainEdge> terrainEdges;

    //public Layer[][] VertTypes { get { return vertTypes; } }
    public KDNode TriKDMap { get { return triKDMap; } }
    public Dictionary<Vector2,HalfEdge> VertEdgeMap { get { return vertEdgeMap; } }

    //Vector2[][] pChains;
    //Layer[][] vertTypes;
    public List<NavTri> navTris;
    public KDNode triKDMap;
    public Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2,HalfEdge>();

    public List<Area> areas = new List<Area>();
    public List<Actor> objects = new List<Actor>();

    public void InitNavMesh(Transform terrainRoot) {
        HashSet<Vector2> navVerts;
        HashSet<EdgePair> implicitEdges;
        Dictionary<EdgePair,HalfEdge> edgeMap;

        if (terrainRoot.GetComponentInChildren<EdgeGroup>() != null) {
            EdgeGroup eg = terrainRoot.GetComponentInChildren<EdgeGroup>();
            LoadTerrainEdgeGroup(eg,out implicitEdges,out navVerts);
        } else {
            FindTerrain(terrainRoot,out implicitEdges,out navVerts);
        }

        navTris = Triangulation.BowyerWatson(navVerts.ToList(),out edgeMap);
        MapProcessing.addToVertEdgeMap(navTris,vertEdgeMap);

        List<EdgePair> orphans = MapProcessing.ProcessMapImplicits(navTris,implicitEdges,vertEdgeMap);
        Debug.Log("nav mesh gen had " + orphans.Count + " orphan edges");
        MapProcessing.fixMapOrphans(navTris,orphans,vertEdgeMap);
        MapProcessing.ProcessMapImplicits(navTris,implicitEdges,vertEdgeMap);

        triKDMap = new KDNode(vertEdgeMap.Keys.ToList());
    }

    void LoadTerrainEdgeGroup(EdgeGroup eg,out HashSet<EdgePair> implicitEdges,out HashSet<Vector2> navVerts) {
        Dictionary<int,Transform> vertMap = eg.GetVertMap();
        implicitEdges = new HashSet<EdgePair>();
        navVerts = new HashSet<Vector2>();
        terrainEdges = new List<TerrainEdge>();

        List<EdgeConnection> edgeList = eg.edgeList;
        for(int i = 0; i < edgeList.Count; i++) {
            EdgeConnection ec = edgeList[i];
            if (!vertMap.ContainsKey(ec.vertA_ID)) {
                Debug.LogError("vert A not in vert map when loading edge groups?");
                continue;
            }
            if (!vertMap.ContainsKey(ec.vertB_ID)) {
                Debug.LogError("vert B not in vert map when loading edge groups?");
                continue;
            }

            Layer layer = Layer.NONE;
            if(ec.edgeType == EdgeType.Wall) {
                layer = Layer.WALLS;
            } else if(ec.edgeType == EdgeType.Cliff) {
                layer = Layer.CLIFFS;
            } else if(ec.edgeType == EdgeType.NonGrappleWall) {
                layer = Layer.NO_GRAPPLE_WALL;
            } else {
                Debug.LogError("no proper edge type for edge?");
            }

            Vector2 posA = Utils.Vector3ToVector2XZ(vertMap[ec.vertA_ID].position);
            Vector2 posB = Utils.Vector3ToVector2XZ(vertMap[ec.vertB_ID].position);

            TerrainEdge edge = new TerrainEdge(posA,posB,layer);
            terrainEdges.Add(edge);

            // add as implicit edge
            EdgePair implicitEdge = new EdgePair(posA,posB);
            if (!implicitEdges.Contains(implicitEdge)) {
                implicitEdges.Add(implicitEdge);
            } else {
                Debug.LogWarning("implicit edges already contains this edge?");
            }
        }

        foreach(KeyValuePair<int,Transform> kvp in vertMap) {
            navVerts.Add(Utils.Vector3ToVector2XZ(kvp.Value.position));
        }
    }

   void FindTerrain(Transform terrainRoot,out HashSet<EdgePair> implicitEdges,out HashSet<Vector2> navVerts) {
        terrainEdges = new List<TerrainEdge>();
        //List<Vector2[]> pChainList = new List<Vector2[]>();
        //List<Layer[]> vertTypesList = new List<Layer[]>();
        implicitEdges = new HashSet<EdgePair>();
        navVerts = new HashSet<Vector2>();
        Transform[] pChainGroups = Utils.GetChildren(terrainRoot);

        for (int i = 0; i < pChainGroups.Length; i++) {
            Transform[] pChains = Utils.GetChildren(pChainGroups[i]);

            for (int j = 0; j < pChains.Length; j++) {
                PolygonalChain pChain = pChains[j].GetComponent<PolygonalChain>();
                Transform[] pChainTransforms = Utils.GetChildren(pChains[j]);
                //Vector2[] pChainVerts = (pChain.connectEnds) ?
                //    new Vector2[pChainTransforms.Length + 1] : new Vector2[pChainTransforms.Length];
                //Layer[] pChainVertTypes = (pChain.connectEnds) ?
                //    new Layer[pChainTransforms.Length + 1] : new Layer[pChainTransforms.Length];

                int edges = (pChain.connectEnds) ? pChainTransforms.Length : pChainTransforms.Length - 1;

                for (int k = 0; k < edges; k++) {         

                    TerrainVertex vertA = pChainTransforms[k].GetComponent<TerrainVertex>();
                    Vector2 vertA_pos = Utils.Vector3ToVector2XZ(vertA.SnapPos);
                    //pChainVerts[k] = Utils.Vector3ToVector2XZ(vertA.SnapPos);
                    Layer l = LayerFromVertType(vertA.VertType);

                    TerrainVertex vertB = pChainTransforms[(k + 1) % pChainTransforms.Length].GetComponent<TerrainVertex>();
                    Vector2 vertB_pos = Utils.Vector3ToVector2XZ(vertB.SnapPos);

                    TerrainEdge e = new TerrainEdge(vertA_pos,vertB_pos,l);
                    terrainEdges.Add(e);

                    // nav verts
                    if (!navVerts.Contains(vertA_pos)) {
                        navVerts.Add(vertA_pos);
                    }
                    if (!navVerts.Contains(vertB_pos)) {
                        navVerts.Add(vertB_pos);
                    }

                    // add as implicit edge
                    if (k != pChainTransforms.Length - 1 || pChain.connectEnds) {
                        EdgePair edgePair = new EdgePair(vertA_pos,vertB_pos);
                        if (!implicitEdges.Contains(edgePair)) {
                            implicitEdges.Add(edgePair);
                        } else {
                            Debug.LogWarning("implicit edges already contains this edge?");
                        }
                    }
                }
                //pChainList.Add(pChainVerts);
                //vertTypesList.Add(pChainVertTypes);
            }
            pChains[i].gameObject.SetActive(false);
        }
        //pChains = pChainList.ToArray();
        //vertTypes = vertTypesList.ToArray();
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

public struct TerrainEdge
{
    Vector2 vertA_pos;
    Vector2 vertB_pos;
    public Layer layer;

    public Vector2 vertA_pos2D { get { return vertA_pos; } set { vertA_pos = value; } }
    public Vector2 vertB_pos2D { get { return vertB_pos; } set { vertB_pos = value; } }

    public Vector3 vertA_pos3D { get { return Utils.Vector2XZToVector3(vertA_pos); } }
    public Vector3 vertB_pos3D { get { return Utils.Vector2XZToVector3(vertB_pos); } }

    public TerrainEdge(Vector2 posA,Vector3 posB,Layer type) {
        vertA_pos = posA;
        vertB_pos = posB;
        layer = type;
    }
}