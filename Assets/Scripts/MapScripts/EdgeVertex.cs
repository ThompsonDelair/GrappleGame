using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
[CanEditMultipleObjects]
public class EdgeVertex : MonoBehaviour
{
    public int ID { get { return id; } }


    [HideInInspector]
    [SerializeField]
    private int id;

    private void Start() {
        
    }

    private void Update() {
        
    }

    private void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position,0.04f);
    }

    public void SetID(int newID) {
        id = newID;
    }


}
