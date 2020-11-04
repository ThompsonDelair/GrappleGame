using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeGroup))]
public class EdgeGroupEditControls : MonoBehaviour
{
    public enum EditType { None, AddRemove, ChangeEdgeType, MoveVert }
    public EditType editMode;
    public EdgeType set_edge_to;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
