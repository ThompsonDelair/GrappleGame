using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone
{
    public Movement type;
    public List<NavTri> tris;
    public AABB_2D aabb;

    public Zone(Movement type, List<NavTri> tris, AABB_2D aabb) {
        this.type = type;
        this.tris = tris;
        this.aabb = aabb;
    }
}
