using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EdgeVertex))]
public class EdgeVertexEditor : Editor
{

    public override void OnInspectorGUI() {
        EdgeVertex ev = (EdgeVertex)target;
        string s = "ID: " + ev.id.ToString();
        EditorGUILayout.LabelField(s);
    }

    

    private void OnEnable() {
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.duringSceneGui -= this.DrawName;
        // Add (or re-add) the delegate.
        SceneView.duringSceneGui += this.DrawName;
    }

    private void OnDisable() {
        SceneView.duringSceneGui -= this.DrawName;
    }

    private void OnDestroy() {
        SceneView.duringSceneGui -= this.DrawName;
    }

    void DrawName(SceneView sceneView) {
        EdgeVertex ev = (EdgeVertex)target;
        Handles.Label(ev.transform.position,ev.gameObject.name);
        //Debug.Log("hello from scene gui");
    }

    //[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    //static void RenderCustomGizmo(Transform objectTransform,GizmoType gizmoType) {
    //    //EdgeVertex ev = (EdgeVertex)target;
    //    Handles.Label(objectTransform.position,objectTransform.name);
    //}
    private void OnValidate() {
        EdgeVertex ev = (EdgeVertex)target;
        if (ev.id == 0) {
            ev.id = ev.GetComponentInParent<EdgeGroup>().HighestChildID() + 1;
        }
        if (name != ev.id.ToString()) {
            name = ev.id.ToString();
        }
    }
}
