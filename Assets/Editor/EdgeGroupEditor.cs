using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EdgeGroup))]
public class EdgeGroupEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EdgeGroup eg = (EdgeGroup)target;
        if (GUILayout.Button("add vert")) {
            eg.AddNewVertex();
        }
    }
}
