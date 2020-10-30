using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IEquatable<NavTri>, IComparable<NavTri>
public class NavTri {
    public HalfEdge e1;
    static uint idCounter;
    public uint ID { get { return id;  } }
    uint id;

    public NavTri() {
        id = idCounter++;
    }

    //public override bool Equals(object other) {
    //    if (other.GetType() != this.GetType()) { return false; }
    //    return Equals(other as NavTri);
    //}

    //public bool Equals(NavTri other) {
    //    return (e1.start == other.e1.start && e1.next.start == other.e1.next.start && e1.prev().start == other.e1.prev().start);
    //}

    //public override int GetHashCode() {
    //    unchecked {
    //        var hashCode = 17;
    //        hashCode = (hashCode * 397) + e1.start.GetHashCode();
    //        hashCode = (hashCode * 397) + e1.next.start.GetHashCode();
    //        hashCode = (hashCode * 397) + e1.next.next.start.GetHashCode();
    //        return hashCode;
    //    }
    //}
    

    public Vector2 centroid() {
        Vector2 v1 = e1.start;
        Vector2 v2 = e1.next.start;
        Vector2 v3 = e1.next.next.start;

        float cX = (v1.x + v2.x + v3.x) / 3;
        float cY = (v1.y + v2.y + v3.y) / 3;

        return new Vector2(cX,cY);
    }

    public IEnumerable<HalfEdge> Edges() {
        HalfEdge curr = e1;
        for (int i = 0; i < 3; i++) {
            yield return curr;
            curr = curr.next;
        }
    }

    public HalfEdge this[int i] {
        get {
            switch (i) {
                case 0:
                return e1;
                case 1:
                return e1.next;
                case 2:
                return e1.next.next;
                default:
                Debug.LogError("navtri halfedge indexer out of bounds");
                return null;
            }
        }
    }

    public IEnumerable<Vector2> Points() {
        yield return e1.start;
        yield return e1.next.start;
        yield return e1.next.next.start;
    }

    //public int CompareTo(NavTri other) {
    //    Vector2 aCent = centroid();
    //    Vector2 bCent = other.centroid();
    //    if (aCent.x != bCent.x) {
    //        return aCent.x.CompareTo(bCent.x);
    //    } else if (aCent.y != bCent.y) {
    //        return aCent.y.CompareTo(bCent.y);
    //    } else {
    //        return 0;
    //    }
    //}

    //static 

    
}