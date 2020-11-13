using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EdgeGroupEditControls))]
public class EdgeGroupEditControlEditor : Editor
{
    bool addEdgeMode;

    bool edgeMode;

    int startingVertID;
    Vector3 startingVertPos;
    Vector3 currDragPos;



    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EdgeGroupEditControls ec = (EdgeGroupEditControls)target;
        EdgeGroup eg = ec.GetComponent<EdgeGroup>();
        if (GUILayout.Button("add vert")) {
            eg.AddNewVertex();
        }
    }

    //private void OnEnable() {
    //    // Remove delegate listener if it has previously
    //    // been assigned.
    //    SceneView.duringSceneGui -= this.DrawVerts;
    //    // Add (or re-add) the delegate.
    //    SceneView.duringSceneGui += this.DrawVerts;
    //}

    //private void OnDisable() {
    //    SceneView.duringSceneGui -= this.DrawVerts;
    //}

    //private void OnDestroy() {
    //    SceneView.duringSceneGui -= this.DrawVerts;
    //}


    

    private void OnSceneGUI() {
        EdgeGroupEditControls ec = (EdgeGroupEditControls)target;
        EdgeGroup eg = ec.GetComponent<EdgeGroup>();
        Transform[] children = Utils.GetChildren(eg.transform);

        if(ec.editMode == EdgeGroupEditControls.EditType.AddRemove) {
            if (addEdgeMode) {
                AddEdgeModeUpdate(eg,children);
            } else {
                AddRemoveUpdate(eg,children);
            }
        } else if(ec.editMode == EdgeGroupEditControls.EditType.ChangeEdgeType) {
            ChangeEdgeTypeUpdate(ec,eg,children);
        } else if(ec.editMode == EdgeGroupEditControls.EditType.MoveVert) {
            MoveVertModeUpdate(ec,eg,children);
        }

    }

    void DrawVertsAsWireCubes(Transform[] transforms, float size, Color color) {
        Handles.color = color;
        for(int i = 0; i < transforms.Length; i++) {
            float handleSize = HandleUtility.GetHandleSize(transforms[i].position);
            Handles.DrawWireCube(transforms[i].position,Vector3.one * handleSize * size);
        }
    }

    //public override bool RequiresConstantRepaint() {
    //    return addEdgeMode;
    //}

    int FindClosestVertext(Transform[] children, Vector3 point, out Vector3 closest) {
        float bestScore = Mathf.Infinity;
        closest = Vector3.positiveInfinity;
        int closestID = -1;
        for (int i = 0; i < children.Length; i++) {
            EdgeVertex ev = children[i].GetComponent<EdgeVertex>();
            if (ev == null)
                continue;

            float score = Vector3.SqrMagnitude(point - children[i].position);
            if (score < bestScore) {
                bestScore = score;
                closest = children[i].position;
                closestID = children[i].GetComponent<EdgeVertex>().ID;
            }
        }

        if(bestScore == Mathf.Infinity) {
            return -1;
        } else {
            return closestID;
        }

    }

    void AddRemoveUpdate(EdgeGroup eg,Transform[] children) {
        Dictionary<int,Transform> vertDict = new Dictionary<int,Transform>();

        // draw vertex handles
        for (int i = children.Length - 1; i >= 0; i--) {

            EdgeVertex ev;
            if ((ev = children[i].GetComponent<EdgeVertex>()) == null)
                continue;

            vertDict.Add(ev.ID,ev.transform);

            Vector3 pos = ev.transform.position;
            float size = HandleUtility.GetHandleSize(pos);

            float distSize = HandleUtility.GetHandleSize(pos) * 0.1f;
            float buttonSize = size * 0.2f;

            Handles.color = Color.red;
            if (Handles.Button(pos + Vector3.left * distSize * 1.3f,Quaternion.identity,buttonSize,buttonSize,Handles.SphereHandleCap)) {
                eg.RemoveConnectionsWithVertID(ev.ID);
                DestroyImmediate(ev.gameObject);
            }
            Handles.color = CustomColors.niceBlue;
            if (Handles.Button(pos + Vector3.right * distSize * 1.3f,Quaternion.identity,buttonSize,buttonSize,Handles.SphereHandleCap)) {
                startingVertID = ev.ID;
                startingVertPos = ev.transform.position;
                addEdgeMode = true;
            }

            Handles.Label(pos + Vector3.up * size * 0.3f,ev.ID.ToString());
        }


        // draw delete edge handles
        List<EdgeConnection> edgeList = eg.edgeList;
        for (int i = edgeList.Count - 1; i >= 0; i--) {
            EdgeConnection e = edgeList[i];

            if (e.vertA_ID == 0 || e.vertB_ID == 0) {
                continue;
            }

            if (!vertDict.ContainsKey(e.vertA_ID) || !vertDict.ContainsKey(e.vertB_ID)) {
                continue;
            }

            int vertA_ID = e.vertA_ID;
            int vertB_ID = e.vertB_ID;

            Vector3 posA = vertDict[vertA_ID].position;
            Vector3 posB = vertDict[vertB_ID].position;

            Vector3 A_to_B = posB - posA;
            Vector3 halfway = posA + (A_to_B / 2);

            float size = HandleUtility.GetHandleSize(halfway);
            Handles.color = Color.red;

            if (Handles.Button(halfway,Quaternion.identity,size * 0.1f,size * 0.1f,Handles.DotHandleCap)) {
                eg.RemoveConnection(i);
                SceneView.RepaintAll();
                HandleUtility.Repaint();
            }
        }
    }

    void AddEdgeModeUpdate(EdgeGroup eg, Transform[] children) {
        DrawVertsAsWireCubes(children,0.2f,Color.grey);

        //AddEdgeUpdate();
        Vector3 mouseWorldPos;
        if (EditorUtil.RaycastMousePosZeroYEditor(out mouseWorldPos)) {
            Vector3 closestVert;
            int closestID = FindClosestVertext(children,mouseWorldPos,out closestVert);
            float dist = Vector3.Distance(mouseWorldPos,closestVert);
            float size = HandleUtility.GetHandleSize(mouseWorldPos);
            Vector3 endPoint;
            bool snap = closestID != -1 && dist < 0.8f;
            if (snap) {
                // snap!
                endPoint = closestVert;
                Handles.color = Color.cyan;
                Handles.DrawWireCube(endPoint,Vector3.one * size * 0.3f);
            } else {
                endPoint = mouseWorldPos;
                Handles.color = CustomColors.niceBlue;
            }
            Handles.DrawLine(startingVertPos,endPoint);

            HandleUtility.Repaint();

            if (Handles.Button(mouseWorldPos,Quaternion.identity,size * 0.1f,size * 0.1f,Handles.SphereHandleCap)) {
                addEdgeMode = false;
                if (!snap || closestID != startingVertID) {

                    int vertA_ID = startingVertID;
                    int vertB_ID;

                    if (snap) {
                        vertB_ID = closestID;
                    } else {
                        EdgeVertex newVert = eg.AddNewVertex();
                        vertB_ID = newVert.ID;
                        newVert.transform.position = endPoint;
                    }
                    eg.AddConnection(vertA_ID,vertB_ID);
                }
            }
        } else {
            Debug.LogError("no mouse collision?");
        }
    }

    void ChangeEdgeTypeUpdate(EdgeGroupEditControls ec,EdgeGroup eg,Transform[] children) {
        Dictionary<int,Transform> vertDict = new Dictionary<int,Transform>();

        for(int i = 0; i < children.Length; i++) {
            EdgeVertex ev;
            if ((ev = children[i].GetComponent<EdgeVertex>()) == null)
                continue;

            vertDict.Add(ev.ID,ev.transform);
        }

        List<EdgeConnection> edgeList = eg.edgeList;
        for (int i = 0; i < edgeList.Count; i++) {
            EdgeConnection e = edgeList[i];

            if (e.vertA_ID == 0 || e.vertB_ID == 0) {
                continue;
            }

            int vertA_ID = e.vertA_ID;
            int vertB_ID = e.vertB_ID;

            Vector3 posA = vertDict[vertA_ID].position;
            Vector3 posB = vertDict[vertB_ID].position;

            Vector3 A_to_B = posB - posA;
            Vector3 halfway = posA + (A_to_B / 2);

            float size = HandleUtility.GetHandleSize(halfway);

            //Color c;

            //if(e.edgeType == EdgeType.Wall) {
            //    c = Color.black;
            //} else if(e.edgeType == EdgeType.Cliff) {
            //    c = CustomColors.darkRed;
            //} else if(e.edgeType == EdgeType.NonGrappleWall) {
            //    c = Color.red;
            //}

            Handles.color = EdgeGroup.ColorFromType(ec.set_edge_to);

            if(e.edgeType != ec.set_edge_to) {
                if (Handles.Button(halfway,Quaternion.identity,size * 0.1f,size * 0.1f,Handles.DotHandleCap)) {
                    //eg.RemoveConnection(i);
                    //SceneView.RepaintAll();
                    eg.SetEdgeType(i,ec.set_edge_to);
                    HandleUtility.Repaint();
                }
            }


        }

    }

    void MoveVertModeUpdate(EdgeGroupEditControls ec,EdgeGroup eg,Transform[] children) {
        List<EdgeConnection> edgeList = eg.edgeList;
        for (int i = 0; i < children.Length; i++) {
            Transform curr = children[i];
            float size = HandleUtility.GetHandleSize(curr.position);

            Handles.color = Color.red;
            // x axis
            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.Slider(
                curr.position,
                Vector3.right,
                size * 0.5f,
                Handles.ArrowHandleCap,
                0.2f                
                );
            if (EditorGUI.EndChangeCheck()) {
                curr.position = newTargetPosition;
            }

            Handles.color = CustomColors.niceBlue;
            // z axis
            EditorGUI.BeginChangeCheck();
            newTargetPosition = Handles.Slider(
                curr.position,
                Vector3.forward,
                size * 0.5f,
                Handles.ArrowHandleCap,
                0.2f
                );
            if (EditorGUI.EndChangeCheck()) {
                curr.position = newTargetPosition;
            }


            //Handles.color = Color.green;
            Handles.color = CustomColors.green;
            // free move
            EditorGUI.BeginChangeCheck();
            newTargetPosition = Handles.FreeMoveHandle(
                curr.position,
                Quaternion.identity,
                size * 0.15f,                
                Vector3.one * 0.2f,
                Handles.SphereHandleCap
                );
            if (EditorGUI.EndChangeCheck()) {
                newTargetPosition.y = 0;
                curr.position = newTargetPosition;
            }
        }
    }
}
