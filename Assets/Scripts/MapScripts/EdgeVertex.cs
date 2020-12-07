using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// this component is used on child game objects of an edgeGroup game object
// it is assigned a unique ID as is used as a vertex for edges
[ExecuteInEditMode]
public class EdgeVertex : MonoBehaviour
{
    public int ID { get { return id; } }

    [HideInInspector]
    [SerializeField]
    private int id;

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position,0.04f);
    }

    public void SetID(int newID) {
        id = newID;
    }
}
