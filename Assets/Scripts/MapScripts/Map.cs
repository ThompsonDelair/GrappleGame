using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map
{

    public List<TerrainEdge> terrainEdges;

    List<Door> doors;

    HashSet<NavTri> pitTris;
    HashSet<NavTri> floorTris;
    HashSet<NavTri> roofTris;

    public List<Zone> zones;
    List<Zone> pitZones;
    List<Zone> floorZones;

    public KDNode TriKDMap { get { return triKDMap; } }
    public Dictionary<Vector2,HalfEdge> VertEdgeMap { get { return vertEdgeMap; } }

    public List<NavTri> navTris;
    public KDNode triKDMap;
    public Dictionary<Vector2,HalfEdge> vertEdgeMap = new Dictionary<Vector2,HalfEdge>();


    // Initializes the nav mesh
    // uses a triangulation algorithm and reinforces implicit edges if the tri algorithmn does not produce them
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

    // makes a list of edges based off the edges from the edgeGroup game object
    void LoadTerrainEdgeGroup(EdgeGroup eg,out HashSet<EdgePair> implicitEdges,out HashSet<Vector2> navVerts) {
        eg.drawEdges = false;
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
                Door d = new Door(ec.doorID,edge);
                doors.Add(d);
            }

            // add as implicit edge
            EdgePair implicitEdge = new EdgePair(posA,posB);
            implicitEdge.moveBlock = layer;
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

    // old function, used in old prototype maps
    // essentially does the same thing as LoadTerrainEdgeGroup()
    void FindTerrain(Transform terrainRoot,out HashSet<EdgePair> implicitEdges,out HashSet<Vector2> navVerts) {
        terrainEdges = new List<TerrainEdge>();
        implicitEdges = new HashSet<EdgePair>();
        navVerts = new HashSet<Vector2>();
        doors = new List<Door>();
        Transform[] pChainGroups = Utils.GetChildren(terrainRoot);

        for (int i = 0; i < pChainGroups.Length; i++) {
            Transform[] pChains = Utils.GetChildren(pChainGroups[i]);

            for (int j = 0; j < pChains.Length; j++) {
                PolygonalChain pChain = pChains[j].GetComponent<PolygonalChain>();
                Transform[] pChainTransforms = Utils.GetChildren(pChains[j]);

                int edges = (pChain.connectEnds) ? pChainTransforms.Length : pChainTransforms.Length - 1;

                for (int k = 0; k < edges; k++) {         

                    TerrainVertex vertA = pChainTransforms[k].GetComponent<TerrainVertex>();
                    Vector2 vertA_pos = Utils.Vector3ToVector2XZ(vertA.SnapPos);
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
                        edgePair.moveBlock = l;
                        if (!implicitEdges.Contains(edgePair)) {
                            implicitEdges.Add(edgePair);
                        } else {
                            Debug.LogWarning("implicit edges already contains this edge?");
                        }
                    }
                }
            }
            pChains[i].gameObject.SetActive(false);
        }
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

    // Iterate through area gameObjects and add them to the list
    public void FindAreas(Transform areaRoot,GameData data) {
        Transform[] children = Utils.GetChildren(areaRoot);
        for(int i = 0; i < children.Length; i++) {
            Area a = children[i].GetComponent<Area>();
            if (a != null) {
                data.areas.Add(a);
            }
        }
    }

    // Iterate through zoneFinder objects and get a list of zones
    public void FindZones() {

        zones = new List<Zone>();
        floorZones = new List<Zone>();
        pitZones = new List<Zone>();

        pitTris = new HashSet<NavTri>();
        floorTris = new HashSet<NavTri>();
        roofTris = new HashSet<NavTri>();

        HashSet<NavTri> takenTris = new HashSet<NavTri>();

        Transform[] zoners = Utils.GetChildren(GameObject.Find("Zoners").transform);
        for(int i = 0; i < zoners.Length; i++) {
            ZoneFinder finder = zoners[i].GetComponent<ZoneFinder>();
            if(finder == null) {
                continue;
            }

            Zone zone = finder.FindZone(this);
            zones.Add(zone);

            HashSet<NavTri> triSet = new HashSet<NavTri>(zone.tris);

            if (takenTris.Overlaps(triSet)) {
                Debug.LogError("two zones are overlapping?");
            }
            triSet.UnionWith(triSet);

            if(finder.zoneType == ZoneType.FLOOR) {
                floorTris.UnionWith(triSet);
                floorZones.Add(zone);
            } else if(finder.zoneType == ZoneType.PIT) {
                pitTris.UnionWith(triSet);
                pitZones.Add(zone);
            } else if(finder.zoneType == ZoneType.ROOF) {
                roofTris.UnionWith(triSet);
            }
        }
    }

    // sets the proper pathfinding value for door edges
    public void SetDoorHalfEdges() {
        for(int i = 0; i < doors.Count; i++) {
            Door d = doors[i];
            HalfEdge h = vertEdgeMap[d.terrainEdge.vertA_pos2D];
            bool foundH2 = false;
            foreach(HalfEdge h2 in MapProcessing.edgesAroundVert(h)) {
                if(h2.pair.start == d.terrainEdge.vertB_pos2D) {
                    d.halfEdge = h2;
                    foundH2 = true;
                    if (d.terrainEdge.layer == Layer.NONE) {
                        h2.moveBlock = Layer.NONE;
                        h2.pair.moveBlock = Layer.NONE;
                    } else {
                        h2.moveBlock = Layer.BLOCK_ALL;
                        h2.moveBlock = Layer.BLOCK_ALL;
                    }
                    break;
                }
            }
            if (!foundH2) {
                Debug.LogError("couldn't find H2 for door?");
            }
        }
    }

    // find what zone the point is in
    public Zone ZoneFromPoint(Vector2 point) {
        List<Zone> candidates = new List<Zone>();
        for(int i = 0; i < floorZones.Count; i++) {
            Zone z = floorZones[i];
            if (z.aabb.ContainsPoint(point)) {
                candidates.Add(z);
            }
        }
        for (int i = 0; i < pitZones.Count; i++) {
            Zone z = pitZones[i];
            if (z.aabb.ContainsPoint(point)) {
                candidates.Add(z);
            }
        }
        if(candidates.Count == 1) {
            return candidates[0];
        } else {
            for(int i = 0; i < candidates.Count; i++) {
                List<NavTri> tris = candidates[i].tris;
                for(int j = 0; j < tris.Count; j++) {
                    if (NavCalc.PointInOrOnTri(point,tris[j])) {
                        return candidates[i];
                    }
                }
            }
        }
        return null;
    }

    // is the point on a floor zone?
    public bool IsPointWalkable(Vector2 v) {
        foreach (NavTri t in floorTris) {
            if (NavCalc.PointInOrOnTri(v,t)) {
                return true;
            }
        }
        return false;
    }
    
    // Do these verts form a line that does not intersect a wall?
    public bool ClearSightLine(Vector2 a, Vector2 b) {
        AABB_2D aabb = new AABB_2D(a,b);
        for (int i = 0; i < terrainEdges.Count; i++) {
            TerrainEdge e = terrainEdges[i];

            if (!aabb.OverlapCheck(e.aabb)) {
                continue;
            }
            if (CollisionSystem.LayerOrCheck(e.layer,Layer.BLOCK_FLY) && Calc.DoLinesIntersect(a,b,e.vertA_pos2D,e.vertB_pos2D)) {

                return false;
            }
        }
        return true;
    }

    public void ToggleAllDoors() {
        for(int i = 0; i < doors.Count; i++) {
            doors[i].ToggleDoor();
        }
    }

    public void OpenDoor(int id) {
        for (int i = 0; i < doors.Count; i++) {
            Door d = doors[i];
            if (d.ID == id) {
                d.OpenDoor();
            }
        }
    }

    public void CloseDoor(int id) {
        for (int i = 0; i < doors.Count; i++) {
            Door d = doors[i];
            if (d.ID == id) {
                d.CloseDoor();
            }
        }
    }

    public void ToggleDoor(int id) {
        for(int i = 0; i < doors.Count; i++) {
            Door d = doors[i];
            if(d.ID == id) {
                d.ToggleDoor();
            }
        }
    }

    public void DrawTerrainEdgesGizmos() {
        for(int i = 0; i < terrainEdges.Count; i++) {
            TerrainEdge e = terrainEdges[i];
            if(Gizmos.color != ColorFromLayer(e.layer)) {
                Gizmos.color = ColorFromLayer(e.layer);
            }
            Gizmos.DrawLine(e.vertA_pos3D,e.vertB_pos3D);
        }
        for(int i = 0; i < doors.Count; i++) {
            Door d = doors[i];
            Color c = (d.terrainEdge.layer == Layer.NONE) ? Color.cyan : Color.blue;
            if (Gizmos.color != c)
                Gizmos.color = c;
            Gizmos.DrawLine(d.terrainEdge.vertA_pos3D,d.terrainEdge.vertB_pos3D);
        }
    }

    public Color ColorFromLayer(Layer l) {
        if (l == Layer.BLOCK_ALL) {
            return Color.black;
        } else if (l == Layer.BLOCK_WALK) {
            return CustomColors.darkRed;
        } else if (l == Layer.BLOCK_FLY) {
            return CustomColors.green;
        } else if (l == Layer.NONE) {
            return Color.cyan;
        }
        return Color.white;
    }
}

