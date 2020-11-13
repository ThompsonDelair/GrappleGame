using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class EdgeGroup : MonoBehaviour
{


    //public List<string> edgeList;
    [SerializeField]
    public List<EdgeConnection> edgeList;

    

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

    public static EdgeConnection EdgeConnectionFromID(string s) {
        EdgeConnection e = new EdgeConnection();
        string[] ids = s.Split(',');
        if(ids.Length != 2) {
            return e;
        }
        int.TryParse(ids[0],out e.vertA_ID);
        string[] b = s.Split('-');
        if(b.Length == 1) {
            int.TryParse(ids[1],out e.vertB_ID);
        } else if(b.Length == 2) {
            int.TryParse(b[0],out e.vertB_ID);
            //int.TryParse(b[1],out e.bOtherGroup);
        }
        return e;
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
        GizmosDrawEdges();        
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
        e.edgeType = type;
        edgeList[edgeIndex] = e;
    }

    void GizmosDrawEdges() {
        Transform[] children = Utils.GetChildren(transform);
        for (int i = 0; i < edgeList.Count; i++) {
            EdgeConnection e = edgeList[i];
            int a = FindIDInArray(e.vertA_ID,children);
            int b = FindIDInArray(e.vertB_ID,children);
            if (a != -1 && b != -1) {
                Gizmos.color = ColorFromType(e.edgeType);

                //Gizmos.color = Color.black;
                Gizmos.DrawLine(children[a].position,children[b].position);
                //Debug.DrawLine(children[a].position,children[b].position,Color.black);
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
            return Color.cyan;
        } else if(type == EdgeType.DoorOpen) {
            return Color.blue;
        }

        return Color.white;
    }
}

[System.Serializable]
public struct EdgeConnection
{
    public int vertA_ID;
    public int vertB_ID;
    public EdgeType edgeType;
    int doorID;

    public EdgeConnection(int aID, int bID, EdgeType type = EdgeType.Wall) {

        if (aID == bID) {
            Debug.LogError("edge cant have 2 same verts");
        }

        int first = (aID < bID) ? aID : bID;
        int second = (aID > bID) ? aID : bID;

        vertA_ID = first;
        vertB_ID = second;
        edgeType = type;
        doorID = -1;
    }

}

public enum EdgeType { Wall, Cliff, NonGrappleWall, DoorClosed, DoorOpen }