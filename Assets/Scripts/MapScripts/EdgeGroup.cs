using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;


[ExecuteInEditMode]
public class EdgeGroup : MonoBehaviour
{


    //public List<string> edgeList;
    [SerializeField]
    public List<EdgeConnection> edgeList;
    //HashSet<int> doorIDs;
    public bool drawEdges;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate() {
        HashSet<int> ID_Set;
        CheckIDS(out ID_Set);
        ValidateEdges(ID_Set);
        SortList();
    }

    int FindIDInArray(int id,Transform[] children) {
        for(int i = 0; i < children.Length; i++) {
            if(children[i].GetComponent<EdgeVertex>().ID == id) {
                return i;
            }
        }
        return -1;
    }

    void SortList() {
        edgeList.Sort(delegate(EdgeConnection a,EdgeConnection b) {

            if(a.vertA_ID == 0 || a.vertB_ID == 0) {
                return 1;
            }
            if(b.vertA_ID == 0 || b.vertB_ID == 0) {
                return -1;
            }
            if(a.vertA_ID == b.vertA_ID) {
                return a.vertB_ID.CompareTo(b.vertB_ID);
            } else {
                return a.vertA_ID.CompareTo(b.vertA_ID);
            }
        });
    }

    void CheckIDS(out HashSet<int> IDs) {
        IDs = new HashSet<int>();
        Transform[] children = Utils.GetChildren(transform);
        for(int i = 0; i < children.Length; i++) {
            EdgeVertex ev = children[i].GetComponent<EdgeVertex>();
            if(ev != null) {
                if (ev.ID == 0) {
                    ev.SetID(GetNewVertexID());
                }
                if(ev.name != ev.ID.ToString()) {
                    ev.name = ev.ID.ToString();
                }
                IDs.Add(ev.ID);
            }
        }
    }



    void ValidateEdges(HashSet<int> IDs) {
        for(int i = edgeList.Count - 1; i >= 0; i--) {
            EdgeConnection e = edgeList[i];
            if(!IDs.Contains(e.vertA_ID) || !IDs.Contains(e.vertB_ID)) {
                edgeList.RemoveAt(i);
            }
            if((e.edgeType == EdgeType.DoorOpen || e.edgeType == EdgeType.DoorClosed) && e.doorID == 0) {
                e.doorID = GetNewDoorID();
            }
        }
    }

    public int GetNewVertexID() {
        HashSet<int> usedIds = new HashSet<int>();
        Transform[] children = Utils.GetChildren(transform);
        int highest = 0;
        for(int i = 0; i < children.Length; i++) {
            EdgeVertex ev = children[i].GetComponent<EdgeVertex>();
            if(ev != null) {
                usedIds.Add(ev.ID);
                if (ev.ID > highest) {
                    highest = ev.ID;
                }
            }
        }

        for (int i = 1; i <= highest + 1; i++) {
            if (!usedIds.Contains(i)) {
                return i;
            }
        }

        return -1;
    }

    //public int HighestChildID() {
    //    int highest = 0;
    //    Transform[] children = Utils.GetChildren(transform);
    //    for (int i = 0; i < children.Length; i++) {
    //        EdgeVertex ev = children[i].GetComponent<EdgeVertex>();
            
    //        if(ev.ID > highest) {
    //            highest = ev.ID;
    //        }
    //    }
    //    return highest;
    //}

    public EdgeVertex AddNewVertex() {
        int newID = GetNewVertexID();
        GameObject go = new GameObject();
        go.transform.parent = this.transform;
        EdgeVertex ev = go.AddComponent<EdgeVertex>();
        ev.SetID(newID);
        go.name = newID.ToString();
        return ev;
    }

    public void RemoveConnectionsWithVertID(int ID) {
        for(int i = edgeList.Count - 1; i >= 0 ; i--) {
            EdgeConnection ec = edgeList[i];
            if(ec.vertA_ID == ID || ec.vertB_ID == ID) {
                edgeList.RemoveAt(i);
            }
        }
    }

    public void AddConnection(int vertA_ID, int vertB_ID) {
        if (vertA_ID == vertB_ID) {
            Debug.LogError("edge cant have 2 same verts");
        }

        int first = (vertA_ID < vertB_ID) ? vertA_ID : vertB_ID;
        int second = (vertA_ID > vertB_ID) ? vertA_ID : vertB_ID;
        //string connection = first + "," + second;

        EdgeConnection e = new EdgeConnection(first,second);

        for(int i = 0; i < edgeList.Count; i++) {
            EdgeConnection other = edgeList[i];
            if(other.vertA_ID == e.vertA_ID && other.vertB_ID == e.vertB_ID) {
                return;
            }
        }

        edgeList.Add(e);
        //DrawConnections();
    }

    public void RemoveConnection(int i) {
        edgeList.RemoveAt(i);
        //DrawConnections();
    }

    private void OnDrawGizmos() {
        if (drawEdges) {
            GizmosDrawEdges();
        }     
    }

    int GetNewDoorID() {
        HashSet<int> usedIDs = new HashSet<int>();
        int highest = 0;
        for(int i = 0; i < edgeList.Count; i++) {
            EdgeConnection e = edgeList[i];
            if (e.doorID != 0){
                usedIDs.Add(e.doorID);
                if(e.doorID > highest) {
                    highest = e.doorID;
                }
            }
        }
        for(int i = 1; i <= highest + 1; i++) {
            if (!usedIDs.Contains(i)) {
                Debug.LogFormat("New door id: {0}",i);
                return i;
            }
        }
        return -1;
    }

    public void SetEdgeType(int vertA_ID, int vertB_ID, EdgeType type) {

        if (vertA_ID == vertB_ID) {
            Debug.LogError("edge cant have 2 same verts");
        }

        int first = (vertA_ID < vertB_ID) ? vertA_ID : vertB_ID;
        int second = (vertA_ID > vertB_ID) ? vertA_ID : vertB_ID;

        bool foundEdge = false;

        for (int i = 0; i < edgeList.Count; i++) {
            EdgeConnection e = edgeList[i];
            if(e.vertA_ID == first && e.vertB_ID == second) {

                if (e.edgeType != EdgeType.DoorClosed && e.edgeType != EdgeType.DoorOpen) {
                    if (type == EdgeType.DoorClosed || type == EdgeType.DoorOpen) {
                        // wasn't a door and now is a door
                        e.doorID = GetNewDoorID();
                    }
                } else {
                    if (type != EdgeType.DoorClosed && type != EdgeType.DoorOpen) {
                        // was a door, now isn't
                        e.doorID = 0;
                    }
                }

                e.edgeType = type;
                foundEdge = true;

                break;
            }
        }



        if (!foundEdge) {
            Debug.LogError("Couldn't find edge to change");
        }        
    }

    public void SetEdgeType(int edgeIndex, EdgeType type) {
        EdgeConnection e = edgeList[edgeIndex];
        if (e.edgeType != EdgeType.DoorClosed && e.edgeType != EdgeType.DoorOpen) {
            if (type == EdgeType.DoorClosed || type == EdgeType.DoorOpen) {
                // wasn't a door and now is a door
                e.doorID = GetNewDoorID();
            }
        } else {
            if (type != EdgeType.DoorClosed && type != EdgeType.DoorOpen) {
                // was a door, now isn't
                e.doorID = 0;
            }
        }
        e.edgeType = type;
    }

    // found this online from a stack exchange
    //static void DrawString(string text,Vector3 worldPos,Color? colour = null) {
    //    UnityEditor.Handles.BeginGUI();
    //    if (colour.HasValue)
    //        GUI.color = colour.Value;
    //    var view = UnityEditor.SceneView.currentDrawingSceneView;
    //    Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
    //    Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
    //    GUI.Label(new Rect(screenPos.x - (size.x / 2),-screenPos.y + view.position.height + 4,size.x,size.y),text);
    //    UnityEditor.Handles.EndGUI();
    //}

    void GizmosDrawEdges() {

        //Debug.Log("drawing edges");
        GUI.color = Color.white;

        Transform[] children = Utils.GetChildren(transform);
        //Dictionary<int,Transform> vertMap = GetVertMap();
        for (int i = 0; i < edgeList.Count; i++) {
            EdgeConnection e = edgeList[i];
            int a = FindIDInArray(e.vertA_ID,children);
            int b = FindIDInArray(e.vertB_ID,children);
            if (a != -1 && b != -1) {
                Gizmos.color = ColorFromType(e.edgeType);

                //Gizmos.color = Color.black;
                Gizmos.DrawLine(children[a].position,children[b].position);
                //Debug.DrawLine(children[a].position,children[b].position,Color.black);


                if (e.doorID != 0) {
                    //Debug.Log("Doing door stuff");
                    Vector3 halfway = children[a].position - (children[a].position - children[b].position) / 2 + Vector3.up;

                    #if UNITY_EDITOR

                    //UnityEditor.Handles.BeginGUI();

                    //var view = UnityEditor.SceneView.currentDrawingSceneView;
                    //Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
                    //Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
                    //GUI.Label(new Rect(screenPos.x - (size.x / 2),-screenPos.y + view.position.height + 4,size.x,size.y),text);
                    Handles.Label(halfway,e.doorID.ToString());

                    //UnityEditor.Handles.EndGUI();

                    #endif
                    //DrawString(e.doorID.ToString(),halfway,Color.white);
                } else {

                }
            }
        }
    }

    public Dictionary<int,Transform> GetVertMap() {
        Dictionary<int,Transform> vertMap = new Dictionary<int,Transform>();
        Transform[] children = Utils.GetChildren(transform);
        for (int i = 0; i < children.Length;i++) {
            EdgeVertex ev = children[i].GetComponent<EdgeVertex>();
            if(ev == null) {
                continue;
            }
            vertMap.Add(ev.ID,ev.transform);
        }
        return vertMap;
    }

    public static Color ColorFromType(EdgeType type) {
        if (type == EdgeType.Wall) {
            return Color.black;
        } else if(type == EdgeType.Cliff) {
            return CustomColors.darkRed;
        } else if(type == EdgeType.NonGrappleWall) {
            return Color.red;
        } else if(type == EdgeType.DoorClosed) {
            return Color.blue;
        } else if(type == EdgeType.DoorOpen) {
            return Color.cyan;
        }

        return Color.white;
    }
}

[System.Serializable]
public class EdgeConnection
{
    public int vertA_ID;
    public int vertB_ID;
    public EdgeType edgeType;
    public int doorID;

    public EdgeConnection(int aID, int bID, EdgeType type = EdgeType.Wall) {

        if (aID == bID) {
            Debug.LogError("edge cant have 2 same verts");
        }

        int first = (aID < bID) ? aID : bID;
        int second = (aID > bID) ? aID : bID;

        vertA_ID = first;
        vertB_ID = second;
        edgeType = type;
        doorID = 0;
    }

}

public enum EdgeType { Wall, Cliff, NonGrappleWall, DoorClosed, DoorOpen }