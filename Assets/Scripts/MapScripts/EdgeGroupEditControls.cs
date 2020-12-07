using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to control editor controls for an edge group
[RequireComponent(typeof(EdgeGroup))]
public class EdgeGroupEditControls : MonoBehaviour
{
    public enum EditType { None, AddRemove, ChangeEdgeType, MoveVert }
    public EditType editMode;
    public EdgeType set_edge_to;
}
