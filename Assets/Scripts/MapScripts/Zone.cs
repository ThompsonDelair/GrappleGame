using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A zone is an isolated area created by terrain edges
//      it can be a floor or pit or ceiling
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
