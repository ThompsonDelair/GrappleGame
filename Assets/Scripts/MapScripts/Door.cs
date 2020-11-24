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
        halfEdge.moveBlock = Layer.NONE;
        if (halfEdge.pair != null) {
            halfEdge.moveBlock = Layer.NONE;
        }
        terrainEdge.layer = Layer.NONE;
    }

    public void CloseDoor() {
        halfEdge.moveBlock = Layer.BLOCK_ALL;
        if(halfEdge.pair != null) {
            halfEdge.pair.moveBlock = Layer.BLOCK_ALL;
        }
        terrainEdge.layer = Layer.BLOCK_ALL;
    }

    public void ToggleDoor() {
        if(halfEdge.moveBlock == Layer.BLOCK_ALL) {
            OpenDoor();
        } else {
            CloseDoor();
        }
    }
    
}
