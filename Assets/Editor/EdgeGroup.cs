using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EdgeGroup : MonoBehaviour
{
    public List<string> edgeList;
    //public List<EdgeConnection> edgeList2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DrawConnections();
    }

    void DrawConnections() {
        Transform[] children = Utils.GetChildren(transform);
        for(int i = 0; i < edgeList.Count; i++) {
            EdgeConnection e = EdgeConnectionFromID(edgeList[i]);
            int a = FindIDInArray(e.a,children);
            int b = FindIDInArray(e.b,children);
            if(a != -1 && b != -1) {
                Debug.DrawLine(children[a].position,children[b].position,Color.black);
            }
        }
    }

    int FindIDInArray(int id,Transform[] children) {
        for(int i = 0; i < children.Length; i++) {
            if(children[i].GetComponent<EdgeVertex>().id == id) {
                return i;
            }
        }
        return -1;
    }

    void SortList() {
        edgeList.Sort(delegate(string a, string b) {
            EdgeConnection ea = EdgeConnectionFromID(a);
            EdgeConnection eb = EdgeConnectionFromID(b);
            if(ea.a == 0 || ea.b == 0) {
                return -1;
            }
            if(eb.a == 0 || eb.b == 0) {
                return 1;
            }
            if(ea.a == eb.a) {
                return ea.b.CompareTo(eb.b);
            } else {
                return ea.a.CompareTo(eb.a);
            }
        });
    }

    void ValidateList() {
        for(int i = 0; i < edgeList.Count; i++) {

        }
    }

    public int HighestChildID() {
        int highest = 0;
        Transform[] children = Utils.GetChildren(transform);
        for (int i = 0; i < children.Length; i++) {
            EdgeVertex ev = children[i].GetComponent<EdgeVertex>();
            
            if(ev.id > highest) {
                highest = ev.id;
            }
        }
        return highest;
    }

    public void AddNewVertex() {
        int newID = HighestChildID() + 1;
        GameObject go = new GameObject();
        go.transform.parent = this.transform;
        EdgeVertex ev = go.AddComponent<EdgeVertex>();
        ev.id = newID;
    }

    EdgeConnection EdgeConnectionFromID(string s) {
        EdgeConnection e = new EdgeConnection();
        string[] ids = s.Split(',');
        if(ids.Length != 2) {
            return e;
        }
        int.TryParse(ids[0],out e.a);
        string[] b = s.Split('-');
        if(b.Length == 1) {
            int.TryParse(ids[1],out e.b);
        } else if(b.Length == 2) {
            int.TryParse(b[0],out e.b);
            int.TryParse(b[1],out e.bOtherGroup);
        }
        return e;
    }
}

[System.Serializable]
public struct EdgeConnection
{
    public int a;
    public int bOtherGroup;
    public int b;
}