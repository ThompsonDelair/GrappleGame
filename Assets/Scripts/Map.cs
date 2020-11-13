using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{
    //public Vector2[][] PChains { get { return pChains; } }

    public List<TerrainEdge> terrainEdges;

    List<Door> doors;

    HashSet<NavTri> pitTris;
    HashSet<NavTri> floorTris;
    HashSet<NavTri> roofTris;

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

    // Added for object ability updates, needed enemy actors
    public List<EnemyActor> enemyObjects = new List<EnemyActor>();

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
        doors = new List<Door>();

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

            Vector2 posA = Utils.Vector3ToVector2XZ(vertMap[ec.vertA_ID].position);
            Vector2 posB = Utils.Vector3ToVector2XZ(vertMap[ec.vertB_ID].position);



            Layer layer = Layer.NONE;
            if(ec.edgeType == EdgeType.Wall || ec.edgeType == EdgeType.NonGrappleWall || ec.edgeType == EdgeType.DoorClosed) {
                layer = Layer.BLOCK_WALK | Layer.BLOCK_FLY;
            } else if(ec.edgeType == EdgeType.Cliff) {
                layer = Layer.BLOCK_WALK;
            } else if(ec.edgeType == EdgeType.DoorOpen){
                layer = Layer.NONE;
            } else {
                Debug.LogError("no proper edge type for edge?");
            }

            TerrainEdge edge = new TerrainEdge(posA,posB,layer);
            terrainEdges.Add(edge);

            if(ec.edgeType == EdgeType.DoorOpen || ec.edgeType == EdgeType.DoorClosed) {
                Door d = new Door();
                d.terrainEdge = edge;
            }



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
            return Layer.BLOCK_WALK | Layer.BLOCK_FLY;
        if (v == VertType.CLIFF)
            return Layer.BLOCK_WALK;
        if (v == VertType.WALL_NO_GRAPPLE)
            return Layer.BLOCK_WALK | Layer.BLOCK_FLY;
        ;
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

            if(stats.mainAttack == "TargetedShot") // Hacky way to segregate other objects from enemy sentry for now
            {
                EnemyActor enemyActor = new EnemyActor(children[i], stats.radius, Layer.ENEMIES, stats.maxHP, 0f , GameManager.main.AbilityStringToClass(stats.mainAttack));
                Debug.Log("THIS GUY: " + i + enemyActor.transform.gameObject.name);
                enemyObjects.Add(enemyActor);
                objects.Add(enemyActor);
            }
            else
            {
                Actor a = new Actor(children[i], stats.radius, Layer.ENEMIES, stats.maxHP, GameManager.main.AbilityStringToClass(stats.mainAttack));
                objects.Add(a);
            }
            
        }
    }

    public void FindZones() {

        pitTris = new HashSet<NavTri>();
        floorTris = new HashSet<NavTri>();
        roofTris = new HashSet<NavTri>();

        HashSet<NavTri> takenTris = new HashSet<NavTri>();

        Transform[] zoners = Utils.GetChildren(GameObject.Find("Zoners").transform);
        for(int i = 0; i < zoners.Length; i++) {
            ZoneFinder z = zoners[i].GetComponent<ZoneFinder>();
            if(z == null) {
                continue;
            }

            Vector2 startPoint = Utils.Vector3ToVector2XZ(z.transform.position);
            NavTri startTri = NavCalc.TriFromPoint(startPoint,TriKDMap,vertEdgeMap);
            List<NavTri> tris = NavUtils.FindConnectedTris(startTri);

            HashSet<NavTri> triSet = new HashSet<NavTri>(tris);

            if (takenTris.Overlaps(triSet)) {
                Debug.LogError("two zones are overlapping?");
            }
            triSet.UnionWith(triSet);

            if(z.zoneType == ZoneType.FLOOR) {
                floorTris.UnionWith(tris);
            } else if(z.zoneType == ZoneType.PIT) {
                pitTris.UnionWith(tris);
            } else if(z.zoneType == ZoneType.ROOF) {
                roofTris.UnionWith(tris);
            }
        }
    }

    public void FindDoorHalfEdges() {
        for(int i = 0; i < doors.Count; i++) {
            Door d = doors[i];
            HalfEdge h = vertEdgeMap[d.terrainEdge.vertA_pos2D];
            bool foundH2 = false;
            foreach(HalfEdge h2 in MapProcessing.edgesAroundVert(h)) {
                if(h2.start == d.terrainEdge.vertB_pos2D) {
                    // this is the halfedge
                    d.halfEdge = h2;
                    break;
                }
            }
            if (!foundH2) {
                Debug.LogError("couldn't find H2 for door?");
            }
        }
    }

    //public bool IsPointInPit(Vector2 v) {
    //    foreach(NavTri t in pitTris) {
    //        if (NavCalc.PointInOrOnTri(v,t)) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public bool IsPointWalkable(Vector2 v) {
        foreach (NavTri t in floorTris) {
            if (NavCalc.PointInOrOnTri(v,t)) {
                return true;
            }
        }
        return false;
    }
}

public class TerrainEdge
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