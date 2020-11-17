using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door 
{
    public HalfEdge halfEdge;
    public TerrainEdge terrainEdge;
    public int ID;

    public Door(int id, TerrainEdge e) {
        terrainEdge = e;
        ID = id;
    }

    public void OpenDoor() {
        halfEdge.type = 0;
        if (halfEdge.pair != null) {
            halfEdge.pair.type = 0;
        }
        terrainEdge.layer = Layer.NONE;
    }

    public void CloseDoor() {
        halfEdge.type = 1;
        if(halfEdge.pair != null) {
            halfEdge.pair.type = 1;
        }
        terrainEdge.layer = Layer.BLOCK_ALL;
    }

    public void ToggleDoor() {
        if(halfEdge.type == 1) {
            OpenDoor();
        } else {
            CloseDoor();
        }
    }
    
}
