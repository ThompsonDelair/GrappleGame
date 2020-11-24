using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePair : IEquatable<EdgePair> {

    public Vector2 a;
    public Vector2 b;
    public Layer moveBlock;

    public EdgePair() { }

    public EdgePair(Vector2 _a,Vector2 _b) {
        AssignValues(_a,_b);
    }

    public EdgePair(HalfEdge e) {
        AssignValues(e.start,e.next.start);
    }

    public override bool Equals(object other) {
        if(other is HalfEdge) { return Equals(other as HalfEdge); }
        if (other.GetType() != this.GetType()) { return false; }
        return Equals(other as EdgePair);
    }

    public bool Equals(EdgePair other) {
        return (other.a == a && other.b == b);
    }

    public bool Equals(HalfEdge other) {
        return (a.Equals(other.start) && b.Equals(other.next.start)) || (a.Equals(other.next.start) && b.Equals(other.start));
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = 17;
            hashCode = (hashCode * 397) + a.GetHashCode();
            hashCode = (hashCode * 397) + b.GetHashCode();
            return hashCode;
        }
    }

    public void AssignValues(Vector2 v1,Vector2 v2) {
        if (v1.x < v2.x) {
            a = v1;
            b = v2;
        } else if (v1.x > v2.x) {
            a = v2;
            b = v1;
            // if x1.x == x2.x
        } else if (v1.y < v2.y) {
            a = v1;
            b = v2;
            // if v1.y >= v2.y
        } else {
            a = v2;
            b = v1;
        }
    }
}