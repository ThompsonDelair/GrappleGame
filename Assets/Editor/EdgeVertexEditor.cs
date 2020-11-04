using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EdgeVertex))]
public class EdgeVertexEditor : Editor
{

    public override void OnInspectorGUI() {
        EdgeVertex ev = (EdgeVertex)target;
        string s = "ID: " + ev.ID.ToString();
        EditorGUILayout.LabelField(s);
    }

    private void OnSceneGUI() {
        EdgeVertex ev = (EdgeVertex)target;
        if(ev.GetComponentInParent<EdgeGroup>() != null) {
            EdgeGroup eg = ev.GetComponentInParent<EdgeGroup>();
            foreach (Transform child in eg.transform) {
                EdgeVertex ev2 = child.GetComponent<EdgeVertex>();
                if(ev2 != null) {
                    Handles.Label(child.transform.position,ev2.ID.ToString());
                }         

            }
        }
    }

    //private void OnEnable() {
    //    // Remove delegate listener if it has previously
    //    // been assigned.
    //    SceneView.duringSceneGui -= this.DrawName;
    //    // Add (or re-add) the delegate.
    //    SceneView.duringSceneGui += this.DrawName;
    //}

    //private void OnDisable() {
    //    SceneView.duringSceneGui -= this.DrawName;
    //}

    //private void OnDestroy() {
    //    SceneView.duringSceneGui -= this.DrawName;
    //}

    //void DrawName(SceneView sceneView) {
    //    EdgeVertex ev = (EdgeVertex)target;
    //    Handles.Label(ev.transform.position,ev.gameObject.name);
    //    //Debug.Log("hello from scene gui");
    //}

    //[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    //static void RenderCustomGizmo(Transform objectTransform,GizmoType gizmoType) {
    //    //EdgeVertex ev = (EdgeVertex)target;
    //    Handles.Label(objectTransform.position,objectTransform.name);
    //}
    private void OnValidate() {
        EdgeVertex ev = (EdgeVertex)target;
        if (ev.ID == 0) {
            ev.SetID(ev.GetComponentInParent<EdgeGroup>().GetNewVertexID());
        }
        if (name != ev.ID.ToString()) {
            name = ev.ID.ToString();
        }
    }
}
